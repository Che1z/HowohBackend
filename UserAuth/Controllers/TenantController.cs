using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.UserEnumList;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class TenantController : ApiController
    {
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
                            ///todo: userIntro加到token裡後記得取消comment
                            //userIntro = jwtObject["userIntro"],
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

        // GET: api/Tenant
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