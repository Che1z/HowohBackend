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
        public IHttpActionResult GetLandlordExpiredOrderList(string page = "1")
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
                    // 假設 page 是傳入的頁碼參數，pageSize 是每頁顯示的筆數
                    int pageNumber;
                    // 嘗試將 page 轉換成 int，如果失敗則設置為預設值 1
                    if (!int.TryParse(page, out pageNumber) || pageNumber < 1)
                    {
                        pageNumber = 1;
                    }
                    int pageSize = 12; // 每頁顯示的筆數
                    //var query = from house in db.HouseEntities.AsQueryable()
                    //            where house.userId == UserId
                    //            join order in db.OrdersEntities on house.id equals order.houseId into orderGroup
                    //            from order in orderGroup.DefaultIfEmpty()
                    //            where order.leaseEndTime < DateTime.Today //過期的order
                    //            join houseImg in db.HouseImgsEntities on house.id equals houseImg.houseId
                    //            where houseImg.isCover == true
                    //            join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                    //            from orderRating in orderRatingGroup.DefaultIfEmpty()
                    //            select new
                    //            {
                    //                order,
                    //                house,
                    //                photo = houseImg.path,
                    //                tenant = order.userId == null ? null : db.UserEntities.FirstOrDefault(u => u.Id == order.userId),
                    //                ratingByLandlord = orderRating == null ? null : orderRatingGroup.FirstOrDefault(o => o.UserId == UserId)
                    //            };

                    var query = from order in db.OrdersEntities.AsQueryable()
                                where order.leaseEndTime < DateTime.Today //過期的order
                                join house in db.HouseEntities on order.houseId equals house.id
                                where house.userId == UserId //使用者的房子
                                join houseImg in db.HouseImgsEntities on house.id equals houseImg.houseId
                                where houseImg.isCover == true
                                //join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                //from orderRating in orderRatingGroup.DefaultIfEmpty()
                                //where orderRating.UserId == UserId
                                select new
                                {
                                    order,
                                    house,
                                    photo = houseImg.path,
                                    tenant = order.userId == null ? null : db.UserEntities.FirstOrDefault(u => u.Id == order.userId),
                                    ratingByLandlord = db.OrdersRatingEntities.FirstOrDefault(or => or.UserId == UserId && or.orderId == order.id) == null ? null : db.OrdersRatingEntities.FirstOrDefault(or => or.UserId == UserId && or.orderId == order.id)
                                };
                    // 計算資料總筆數
                    int totalRecords = query.Count();
                    // 分頁
                    var paginatedResult = query.OrderByDescending(q => q.order.leaseEndTime).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    //var queryResult = query.ToList();
                    var resultList = new List<object>();
                    if (paginatedResult.Count > 0)
                    {
                        foreach (var item in paginatedResult)
                        {
                            //可評價的狀況
                            bool canComment = false;
                            if (item.order.userId != null && DateTime.Now < item.order.leaseEndTime.AddDays(14) && item.ratingByLandlord == null)
                            {
                                canComment = true;
                            }
                            var data = new
                            {
                                orderId = item.order.id,
                                photo = item.photo,
                                name = item.house.name,
                                tenant = item.tenant == null ? "" : (item.tenant.lastName + item.tenant.firstName),
                                tenantTel = item.tenant == null ? item.order.tenantTelphone : item.tenant.telphone,
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
                        data = new
                        {
                            page = pageNumber,
                            totalCount = totalRecords,
                            orderList = resultList
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
        /// [ATH-1, ATH-2]取得租客的租賃列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/order/tenant/list")]
        [JwtAuthFilters]
        public IHttpActionResult GetTenantOrderList(string page = "1")
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];
            try
            {
                //驗證角色
                if (UserRole != UserRoleType.租客)
                {
                    return Content(HttpStatusCode.Forbidden, "使用者角色非租客");
                }
                using (DBModel db = new DBModel())
                {
                    // 假設 page 是傳入的頁碼參數，pageSize 是每頁顯示的筆數
                    int pageNumber;
                    // 嘗試將 page 轉換成 int，如果失敗則設置為預設值 1
                    if (!int.TryParse(page, out pageNumber) || pageNumber < 1)
                    {
                        pageNumber = 1;
                    }
                    int pageSize = 12; // 每頁顯示的筆數
                    var query = from order in db.OrdersEntities.AsQueryable()
                                where order.userId == UserId //使用者的訂單
                                join house in db.HouseEntities on order.houseId equals house.id
                                join houseImg in db.HouseImgsEntities on house.id equals houseImg.houseId
                                where houseImg.isCover == true
                                join user in db.UserEntities on house.userId equals user.Id
                                join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                from orderRating in orderRatingGroup.DefaultIfEmpty()
                                orderby order.leaseEndTime descending
                                select new
                                {
                                    order,
                                    house,
                                    photo = houseImg.path,
                                    landlord = user,
                                    ratingByUser = orderRatingGroup.FirstOrDefault(o => o.UserId == UserId) != null ? orderRatingGroup.FirstOrDefault(o => o.UserId == UserId) : null,
                                    //ratingByOthers = orderRatingGroup.FirstOrDefault(o => o.UserId != UserId) != null ? orderRatingGroup.FirstOrDefault(o => o.UserId != UserId) : null
                                };
                    // 計算資料總筆數
                    int totalRecords = query.Count();
                    // 分頁
                    var paginatedResult = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    var queryOfRatingsByOthers = (from order in db.OrdersEntities.AsQueryable()
                                                  where order.userId == UserId
                                                  join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                                  from orderRating in orderRatingGroup.DefaultIfEmpty()
                                                  where orderRating.UserId != UserId
                                                  select orderRating.Rating)
                                                 .GroupBy(r => 1)
                                                 .Select(g => new
                                                 {
                                                     Count = g.Count(),
                                                     Average = g.Average()
                                                 });
                    var queryOfRatingsByOthersResult = queryOfRatingsByOthers.FirstOrDefault();
                    //double landlordRatingAvg = 0;
                    //if (queryOfRatingsByOthersResult.Count > 0)
                    //{
                    //    //queryOfRatingsByOthersResult.
                    //}
                    var resultList = new List<object>();
                    if (paginatedResult.Count > 0)
                    {
                        foreach (var item in paginatedResult)
                        {
                            //可評價的狀況
                            string orderStatus = "";
                            if (DateTime.Now < item.order.leaseEndTime)
                            {
                                orderStatus = "已承租";
                            }
                            else
                            {
                                if (item.ratingByUser == null && DateTime.Now < item.order.leaseEndTime.AddDays(14))
                                {
                                    orderStatus = "待評價";
                                }
                                else
                                {
                                    orderStatus = "已完成";
                                }
                            }

                            var data = new
                            {
                                orderInfo = new
                                {
                                    orderId = item.order.id,
                                    orderStatus = orderStatus,
                                    leaseStartTime = item.order.leaseStartTime,
                                    leaseEndTime = item.order.leaseEndTime,
                                },
                                houseInfo = new
                                {
                                    photo = item.photo,
                                    name = item.house.name,
                                    rent = item.house.rent,
                                    securityDeposit = item.house.securityDeposit.ToString()
                                },
                                landlordInfo = new
                                {
                                    lastName = item.landlord.lastName,
                                    gender = item.landlord.gender.ToString(),
                                    tel = item.landlord.telphone,
                                    description = item.landlord.userIntro,
                                    ratingCount = queryOfRatingsByOthersResult.Count,
                                    ratingAvg = queryOfRatingsByOthersResult.Average
                                }
                            };
                            resultList.Add(data);
                        }
                    }

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功回傳租客的租賃列表",
                        data = new
                        {
                            page = pageNumber,
                            totalCount = totalRecords,
                            orderList = resultList
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