using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;
using UserAuth.Models.ViewModel;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class UserAuthController : ApiController
    {
        //註冊
        private DBModel db = new DBModel();

        [HttpPost]
        [Route("api/signup")]
        public IHttpActionResult SignUp(User user)
        {
            if (!ModelState.IsValid || user == null)
            {
                return Content(HttpStatusCode.BadRequest, "錯誤資訊不符合規範");
            }
            else
            {
                using (DBModel db = new DBModel())
                    try
                    {
                        //檢查是否重複手機號碼
                        string userTel = user.telphone;
                        var existUser = db.UserEntities.Where(x => x.telphone == userTel).FirstOrDefault();
                        if (existUser != null)
                        {
                            return Content(HttpStatusCode.BadRequest, "手機號碼已被使用");
                        }
                        else
                        {
                            string userLastName = user.lastName;
                            string userFirstName = user.firstName;
                            string userEmail = user.email;
                            string password = user.password;
                            //加鹽
                            var salt = CreateSalt();
                            string userSalt = Convert.ToBase64String(salt); //將 byte 改回字串存回資料表
                            var hash = HashPassword(password, salt);
                            string hashPassword = Convert.ToBase64String(hash);
                            string userIntro = $"我是 {userLastName} {userFirstName}, 職業是 {user.job}";
                            string userPassword = user.password;
                            string userPhoto = user.photo;
                            // new一個User物件
                            User InsertNewAccount = new User();
                            InsertNewAccount.lastName = userLastName;
                            InsertNewAccount.firstName = userFirstName;
                            InsertNewAccount.email = userEmail;
                            InsertNewAccount.salt = userSalt;
                            InsertNewAccount.password = hashPassword;
                            InsertNewAccount.telphone = userTel;
                            InsertNewAccount.photo = userPhoto;
                            InsertNewAccount.CreateAt = DateTime.Now;
                            InsertNewAccount.job = user.job;
                            InsertNewAccount.gender = user.gender;
                            InsertNewAccount.role = user.role;
                            InsertNewAccount.userIntro = userIntro;

                            db.UserEntities.Add(InsertNewAccount);
                            db.SaveChanges();
                            return Content(HttpStatusCode.OK, "已成功註冊");
                        }
                    }
                    catch (Exception ex)
                    {
                        return Content(HttpStatusCode.BadRequest, ex);
                    }
            }
        }

        [HttpPost]
        [Route("api/phoneNumberVerifi")]
        public IHttpActionResult VerifyPhone([FromBody]string phoneNumber)
        {
            if (phoneNumber == null)
            {
                return Content(HttpStatusCode.BadRequest, "錯誤資訊不符合規範");
            }
            else
            {
                try
                {
                    // 檢查是否重複手機號碼
                    var existData = db.UserEntities.FirstOrDefault(x => x.telphone == phoneNumber);
                    if (existData == null)
                    {
                        return Content(HttpStatusCode.OK, "尚未註冊手機號碼");
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, "已註冊手機號碼");
                    }

                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.BadRequest, ex.Message);
                }
            }
        }


        [HttpPost]
        [Route("api/login")]
        public IHttpActionResult LogIn(LogInInput loginput)
        {
            if (!ModelState.IsValid || loginput == null)
            {
                return Content(HttpStatusCode.BadRequest, "錯誤資訊不符合規範");
            }
            else
            {
                using (DBModel db = new DBModel())
                    try
                    {
                        //檢查是否重複手機號碼
                        string inputTel = loginput.telphone;
                        string password = loginput.password;
                        var existData = db.UserEntities.Where(x => x.telphone == inputTel).FirstOrDefault();
                        if (existData == null)
                        {
                            return Content(HttpStatusCode.BadRequest, "尚未註冊手機號碼");
                        }
                        else
                        {
                            byte[] hash = Convert.FromBase64String(existData.password.ToString());
                            byte[] salt = Convert.FromBase64String(existData.salt.ToString());
                            bool success = VerifyHash(password, salt, hash);

                            if (success)
                            {
                                // 產生JWT Token
                                JwtAuthUtil jwtAuthUtil = new JwtAuthUtil();
                                string jwtToken = jwtAuthUtil.GenerateToken(existData.Id);
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "登入成功", // token失效時間:一天
                                    token = jwtToken,  // 登入成功時，回傳登入成功順便夾帶 JwtToken
                                    data = new
                                    {
                                        lastName = existData.lastName,
                                        firstName = existData.firstName,
                                        telphone = existData.telphone,
                                        photo = existData.photo,
                                    }
                                };

                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                return Content(HttpStatusCode.BadRequest, "密碼錯誤");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return Content(HttpStatusCode.BadRequest, ex);
                    }
            }
        }



        private byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        // Hash 處理加鹽的密碼功能
        private byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            //底下這些數字會影響運算時間，而且驗證時要用一樣的值
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8; // 4 核心就設成 8
            argon2.Iterations = 4; // 迭代運算次數
            argon2.MemorySize = 1024 * 1024; // 1 GB
            return argon2.GetBytes(16);
        }

        //驗證密碼
        private bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash); // LINEQ
        }

        // GET: api/UserAuth
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/UserAuth/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/UserAuth
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/UserAuth/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/UserAuth/5
        public void Delete(int id)
        {
        }
    }
}