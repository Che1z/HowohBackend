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

namespace UserAuth.Controllers
{
    public class UserAuthController : ApiController
    {
        DBModel db = new DBModel();
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
                        else { 
                        string userLastName = user.lastName;
                        string userFirstName = user.firstName;
                        string userEmail = user.email;
                        string password = user.password;
                        //加鹽
                        var salt = CreateSalt();
                        string userSalt = Convert.ToBase64String(salt); //將 byte 改回字串存回資料表
                        var hash = HashPassword(password, salt);
                        string hashPassword = Convert.ToBase64String(hash);

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
