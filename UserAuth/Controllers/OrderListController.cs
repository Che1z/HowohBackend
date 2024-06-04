using Microsoft.Ajax.Utilities;
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
    public class OrderListController : ApiController
    {
        /// <summary>
        /// [ALH-1]取得房東租期已結束的租賃列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/order/landlord/list/expired")]
        [JwtAuthFilters]
        public IHttpActionResult GetLandlordExpiredOrderList()
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];
            try
            {
                //驗證角色
                if (UserRole != UserRoleType.房東)
                {
                    return Content(HttpStatusCode.Forbidden, "使用者角色非房東");
                }
                using (DBModel db = new DBModel())
                {
                    var query = from order in db.OrdersEntities.AsQueryable()
                                where order.leaseEndTime < DateTime.Today //過期的order
                                join house in db.HouseEntities on order.houseId equals house.id
                                where house.userId == UserId //使用者的房子
                                join houseImg in db.HouseImgsEntities on house.id equals houseImg.houseId
                                where houseImg.isCover == true
                                join user in db.UserEntities on order.userId equals user.Id
                                join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                from orderRating in orderRatingGroup.DefaultIfEmpty()
                                select new
                                {
                                    order,
                                    house,
                                    photo = houseImg.path,
                                    tenant = user,
                                    ratingByLandlord = orderRatingGroup.FirstOrDefault(o => o.UserId == UserId) != null ? orderRatingGroup.FirstOrDefault(o => o.UserId == UserId) : null,
                                    ratingByTenant = orderRatingGroup.FirstOrDefault(o => o.UserId != UserId) != null ? orderRatingGroup.FirstOrDefault(o => o.UserId != UserId) : null
                                };
                    var queryResult = query.ToList();
                    var resultList = new List<object>();
                    if (queryResult.Count > 0)
                    {
                        foreach (var item in queryResult)
                        {
                            //可評價的狀況
                            bool canComment = false;
                            if (DateTime.Now < item.order.leaseEndTime.AddDays(14) && item.ratingByLandlord == null)
                            {
                                canComment = true;
                            }
                            var data = new
                            {
                                orderId = item.order.id,
                                photo = item.photo,
                                tenant = item.tenant.lastName + item.tenant.firstName,
                                tenantTel = item.tenant.telphone,
                                leaseStartTime = item.order.leaseStartTime,
                                leaseEndTime = item.order.leaseEndTime,
                                canComment = canComment
                            };
                            resultList.Add(data);
                        }
                    }

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功回傳房東租期已結束的租賃列表",
                        data = resultList
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // GET: api/OrderList
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/OrderList/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/OrderList
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/OrderList/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE: api/OrderList/5
        //public void Delete(int id)
        //{
        //}
    }
}