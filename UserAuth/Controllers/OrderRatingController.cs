using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.UserEnumList;
using UserAuth.Models.ViewModel;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class OrderRatingController : ApiController
    {
        /// <summary>
        /// [ACC-5]送出我的評價
        /// </summary>
        /// <param name="orderId">orderId</param>
        /// <param name="orderRatingInput">評價內容</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/comment/common/{orderId}")]
        [JwtAuthFilters]
        public IHttpActionResult PostMyComment(int orderId, OrderRatingInput orderRatingInput)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];

            try
            {
                if (!ModelState.IsValid || orderRatingInput == null)
                {
                    throw new Exception("錯誤資訊不符合規範");
                }
                using (DBModel db = new DBModel())
                {
                    var expiredDate = DateTime.Today.AddDays(-14);
                    var query = from order in db.OrdersEntities.AsQueryable()
                                where order.id == orderId
                                select order;
                    if (UserRole == UserRoleType.租客)
                    {
                        var tenantQuery = from order in query
                                          where order.userId == UserId && expiredDate <= order.leaseEndTime
                                          join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                          from orderRating in orderRatingGroup.DefaultIfEmpty()
                                          where orderRating.UserId == UserId
                                          select new
                                          {
                                              order,
                                              orderRating
                                          };

                        var queryResult = tenantQuery.FirstOrDefault();
                        if (queryResult != null)
                        {
                            if (queryResult.orderRating == null)
                            {
                                OrderRating orderRating = new OrderRating();

                                orderRating.orderId = orderId;
                                orderRating.UserId = UserId;
                                orderRating.Comment = orderRatingInput.comment;
                                orderRating.Rating = orderRatingInput.rating;

                                db.OrdersRatingEntities.Add(orderRating);
                                db.SaveChanges();
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "已成功進行評價",
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                return Content(HttpStatusCode.Forbidden, "使用者已進行評論");
                            }
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "無可供進行評論的order");
                        }
                    }
                    else
                    {
                        var landlordQuery = from order in query
                                            join house in db.HouseEntities on order.houseId equals house.id
                                            where house.userId == UserId && expiredDate <= order.leaseEndTime
                                            join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                            from orderRating in orderRatingGroup.DefaultIfEmpty()
                                            where orderRating.UserId == UserId
                                            select new
                                            {
                                                order,
                                                orderRating
                                            };
                        var queryResult = landlordQuery.FirstOrDefault();
                        if (queryResult != null)
                        {
                            if (queryResult.orderRating == null)
                            {
                                OrderRating orderRating = new OrderRating
                                {
                                    orderId = orderId,
                                    UserId = UserId,
                                    Comment = orderRatingInput.comment,
                                    Rating = orderRatingInput.rating
                                };
                                db.OrdersRatingEntities.Add(orderRating);
                                db.SaveChanges();
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "已成功進行評價",
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                return Content(HttpStatusCode.Forbidden, "使用者已進行評論");
                            }
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "無可供進行評論的order");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // GET: api/OrderRating
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/OrderRating/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/OrderRating
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/OrderRating/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE: api/OrderRating/5
        //public void Delete(int id)
        //{
        //}
    }
}