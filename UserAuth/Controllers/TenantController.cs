using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.UserEnumList;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class TenantController : ApiController
    {
        /// <summary>
        /// [FTU-1]取得登入的租客使用者資訊
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/userInfo")]
        [JwtAuthFilters]
        public IHttpActionResult GetUserInfo()
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];

            //確認user是租客
            try
            {
                if (UserRole != UserRoleType.租客)
                {
                    throw new Exception("使用者角色並非租客");
                }
                else
                {
                    int ratingCount = 0;
                    string ratingAvg = "新用戶，尚未被評價過";
                    using (DBModel db = new DBModel())
                    {
                        //租客的過去訂單
                        var orderListOfUser = db.OrdersEntities.Where(x => x.userId == UserId).ToList();
                        //有找到訂單
                        if (orderListOfUser != null)
                        {
                            var beenRatingList = new List<OrderRating>();
                            foreach (var order in orderListOfUser)
                            {
                                //過去訂單中房東給的評價
                                var beenRatingItems = db.OrdersRatingEntities.Where(x => x.orderId == order.id).Where(x => x.UserId != UserId).ToList();
                                beenRatingList.AddRange(beenRatingItems);
                            }
                            if (beenRatingList.Count() != 0)
                            {
                                //計算評價則數及平均星數
                                int ratingTotal = 0;
                                foreach (var beenRatingItem in beenRatingList)
                                {
                                    ratingTotal += beenRatingItem.Rating;
                                }

                                ratingCount = beenRatingList.Count();
                                ratingAvg = (ratingTotal / ratingCount).ToString();
                            }
                        }
                    }
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功獲取租客資訊",
                        data = new
                        {
                            firstName = jwtObject["FirstName"],
                            lastName = jwtObject["LastName"],
                            telphone = jwtObject["Telphone"],
                            gender = jwtObject["Gender"],
                            job = jwtObject["Job"],
                            photo = jwtObject["Photo"],
                            userIntro = jwtObject["userIntro"],
                            ratingAvg = ratingAvg, //平均星數
                            ratingCount = ratingCount //被評價則數
                        }
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [FTU-2]確認提供登入的使用者資訊供系統比對是否符合預約房源條件
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/user/tenant/info/match/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult PostUserInfo(int id)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];
            UserSexType UserGender = (UserSexType)jwtObject["Gender"];
            UserJob UserJob = (UserJob)jwtObject["Job"];
            try
            {
                if (UserRole != UserRoleType.租客)
                {
                    throw new Exception("使用者角色並非租客");
                }

                using (DBModel db = new DBModel())
                {
                    var houseForMatching = db.HouseEntities.Where(x => x.id == id).FirstOrDefault();
                    if (houseForMatching == null)
                    {
                        throw new Exception("該房源不存在");
                    }
                    if (houseForMatching.hasTenantRestrictions == true)
                    {
                        var genderRestriction = Enum.GetName(typeof(genderRestrictionType), houseForMatching.genderRestriction);
                        if (genderRestriction == "排除男性" && UserGender == UserSexType.男)
                        {
                            throw new Exception("使用者不符此房源之租客限制");
                        }
                        if (genderRestriction == "排除女性" && UserGender == UserSexType.女)
                        {
                            throw new Exception("使用者不符此房源之租客限制");
                        }
                        if (!String.IsNullOrEmpty(houseForMatching.jobRestriction))
                        {
                            string[] jobRestriction = houseForMatching.jobRestriction.Split(',');
                            for (int i = 0; i < jobRestriction.Length; i++)
                            {
                                jobRestriction[i] = jobRestriction[i].Trim();
                                if (jobRestriction[i] == UserJob.ToString())
                                {
                                    throw new Exception("使用者不符此房源之租客限制");
                                }
                            }
                        }
                    }

                    Appointment appointment = new Appointment();
                    appointment.houseId = id;
                    appointment.userId = UserId;
                    appointment.hidden = false;
                    db.AppointmentsEntities.Add(appointment);
                    db.SaveChanges();

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功預約看房",
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex) { return Content(HttpStatusCode.BadRequest, ex); }
        }

        //GET: api/Tenant
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Tenant/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Tenant
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/Tenant/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE: api/Tenant/5
        //public void Delete(int id)
        //{
        //}
    }
}