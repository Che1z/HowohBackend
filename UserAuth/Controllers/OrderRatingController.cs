using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.OrderEnumList;
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

                    if (UserRole == UserRoleType.租客)
                    {
                        var tenantQuery = from order in db.OrdersEntities.AsQueryable()
                                          where order.id == orderId && order.userId == UserId && expiredDate <= order.leaseEndTime && order.status == OrderStatus.租客已確認租約
                                          join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                          from orderRating in orderRatingGroup.DefaultIfEmpty()
                                          select new
                                          {
                                              order,
                                              userComment = orderRatingGroup.FirstOrDefault(or => or.UserId == UserId) ?? null,
                                          };

                        var queryResult = tenantQuery.FirstOrDefault();
                        if (queryResult != null)
                        {
                            if (queryResult.userComment == null)
                            {
                                if (orderRatingInput.rating > 5) { orderRatingInput.rating = 5; }
                                if (orderRatingInput.rating < 1) { orderRatingInput.rating = 1; }
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
                    else
                    {
                        var landlordQuery = from house in db.HouseEntities.AsQueryable()
                                            where house.userId == UserId
                                            join order in db.OrdersEntities on house.id equals order.houseId
                                            where expiredDate <= order.leaseEndTime && order.id == orderId && order.status == OrderStatus.租客已確認租約
                                            join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                            from orderRating in orderRatingGroup.DefaultIfEmpty()
                                            select new
                                            {
                                                order,
                                                userComment = orderRatingGroup.FirstOrDefault(or => or.UserId == UserId) ?? null,
                                            };

                        var queryResult = landlordQuery.FirstOrDefault();
                        if (queryResult != null)
                        {
                            if (queryResult.userComment == null)
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

        /// <summary>
        /// [ACC-6]回覆對方的評價
        /// </summary>
        /// <param name="orderRatingId">orderRatingId</param>
        /// <param name="replyRatingInput">回覆內容</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/comment/common/reply/{orderRatingId}")]
        [JwtAuthFilters]
        public IHttpActionResult PostMyReply(int orderRatingId, ReplyRatingInput replyRatingInput)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];

            try
            {
                if (!ModelState.IsValid || replyRatingInput == null)
                {
                    throw new Exception("錯誤資訊不符合規範");
                }
                using (DBModel db = new DBModel())
                {
                    if (UserRole == UserRoleType.租客)
                    {
                        var query = from order in db.OrdersEntities.AsQueryable()
                                    where order.userId == UserId
                                    join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId
                                    where orderRating.id == orderRatingId && orderRating.UserId != UserId
                                    //var query = from orderRating in db.OrdersRatingEntities.AsQueryable()
                                    //            where orderRating.id == orderRatingId
                                    select new
                                    {
                                        orderRating,
                                        reply = db.ReplyRatingEntities.FirstOrDefault(rr => rr.UserId == UserId && rr.orderRatingId == orderRating.id) ?? null
                                    };
                        var queryResult = query.FirstOrDefault();
                        if (queryResult != null)
                        {
                            if (queryResult.reply == null)
                            {
                                var replyRating = new ReplyRating
                                {
                                    orderRatingId = orderRatingId,
                                    UserId = UserId,
                                    ReplyComment = replyRatingInput.replyComment
                                };
                                db.ReplyRatingEntities.Add(replyRating);
                                db.SaveChanges();
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "已成功進行回覆",
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                return Content(HttpStatusCode.Forbidden, "使用者已進行回覆");
                            }
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "找不到相關評論");
                        }
                    }
                    else
                    {
                        var query = from house in db.HouseEntities.AsQueryable()
                                    where house.userId == UserId
                                    join order in db.OrdersEntities on house.id equals order.houseId
                                    join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId
                                    where orderRating.id == orderRatingId && orderRating.UserId != UserId
                                    select new
                                    {
                                        orderRating,
                                        reply = db.ReplyRatingEntities.FirstOrDefault(rr => rr.UserId == UserId && rr.orderRatingId == orderRating.id) ?? null
                                    };
                        var queryResult = query.FirstOrDefault();
                        if (queryResult != null)
                        {
                            if (queryResult.reply == null)
                            {
                                var replyRating = new ReplyRating
                                {
                                    orderRatingId = orderRatingId,
                                    UserId = UserId,
                                    ReplyComment = replyRatingInput.replyComment
                                };
                                db.ReplyRatingEntities.Add(replyRating);
                                db.SaveChanges();
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "已成功進行回覆",
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                return Content(HttpStatusCode.Forbidden, "使用者已進行回覆");
                            }
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, "找不到相關評論");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ACC-1]顯示全部評價
        /// </summary>
        /// <param name="page">頁數</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/comment/common/list/all")]
        [JwtAuthFilters]
        public IHttpActionResult GetMyCommentList(string page = "1")
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];
            try
            {
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
                    var today = DateTime.Today;
                    if (UserRole == UserRoleType.租客)
                    {
                        var query = from order in db.OrdersEntities.AsQueryable()
                                    where order.userId == UserId && order.status == OrderStatus.租客已確認租約 && today > order.leaseEndTime
                                    join house in db.HouseEntities on order.houseId equals house.id
                                    join user in db.UserEntities on house.userId equals user.Id
                                    join houseImg in db.HouseImgsEntities on house.id equals houseImg.houseId
                                    where houseImg.isCover == true
                                    select new
                                    {
                                        order,
                                        house,
                                        photo = houseImg.path,
                                        landlord = user,
                                        myComment = (from orderRating in db.OrdersRatingEntities
                                                     join reply in db.ReplyRatingEntities on orderRating.id equals reply.orderRatingId into replyGroup
                                                     from reply in replyGroup.DefaultIfEmpty()
                                                     where orderRating.UserId == UserId && orderRating.orderId == order.id
                                                     select new
                                                     {
                                                         comment = orderRating,
                                                         reply
                                                     }).FirstOrDefault(),
                                        landlordComment = (from orderRating in db.OrdersRatingEntities
                                                           join reply in db.ReplyRatingEntities on orderRating.id equals reply.orderRatingId into replyGroup
                                                           from reply in replyGroup.DefaultIfEmpty()
                                                           where orderRating.UserId != UserId && orderRating.orderId == order.id
                                                           select new
                                                           {
                                                               comment = orderRating,
                                                               reply
                                                           }).FirstOrDefault()
                                    };
                        // 計算資料總筆數
                        int totalRecords = query.Count();
                        // 分頁
                        var queryResult = query.OrderByDescending(o => o.order.leaseEndTime).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                        //var queryResult = query.ToList();
                        var orderList = new List<object>();
                        //var expiredDate = DateTime.Today.AddDays(-14);
                        foreach (var i in queryResult)
                        {
                            bool canComment = false;
                            if (today > i.order.leaseEndTime.Date
                                    && today <= i.order.leaseEndTime.Date.AddDays(14)
                                    && i.myComment == null)
                            {
                                canComment = true;
                            }
                            var order = new
                            {
                                orderInfo = new
                                {
                                    orderId = i.order.id,
                                    photo = i.photo,
                                    name = i.house.name,
                                    landlord = i.landlord.lastName + i.landlord.firstName,
                                    leaseStartTime = i.order.leaseStartTime,
                                    leaseEndTime = i.order.leaseEndTime
                                },
                                commentInfo = new
                                {
                                    canComment = canComment,
                                    myComment = i.myComment == null ? null : new
                                    {
                                        commentId = i.myComment.comment.id,
                                        rating = i.myComment.comment.Rating,
                                        comment = i.myComment.comment.Comment,
                                        commentTime = i.myComment.comment.RatingDate,
                                        reply = i.myComment.reply?.ReplyComment,
                                        replyTime = i.myComment.reply?.ReplyDate
                                    },
                                    landlordComment = i.landlordComment == null ? null : new
                                    {
                                        commentId = i.landlordComment.comment.id,
                                        rating = i.landlordComment.comment.Rating,
                                        comment = i.landlordComment.comment.Comment,
                                        commentTime = i.landlordComment.comment.RatingDate,
                                        reply = i.landlordComment.reply?.ReplyComment,
                                        replyTime = i.landlordComment.reply?.ReplyDate
                                    },
                                }
                            };
                            orderList.Add(order);
                        }
                        var result = new
                        {
                            statusCode = 200,
                            status = "success",
                            message = "已成功回傳租客的評價列表",
                            data = new
                            {
                                page = pageNumber,
                                totalCount = totalRecords,
                                orderList = orderList
                            }
                        };
                        return Content(HttpStatusCode.OK, result);
                    }
                    else
                    {
                        var query = from house in db.HouseEntities.AsQueryable()
                                    where house.userId == UserId
                                    join order in db.OrdersEntities on house.id equals order.houseId
                                    where order.status == OrderStatus.租客已確認租約 && today > order.leaseEndTime
                                    //where (order.status == OrderStatus.租客已確認租約 || order.status == OrderStatus.租客非系統用戶) && today > order.leaseEndTime
                                    join user in db.UserEntities on order.userId equals user.Id into userGroup
                                    from user in userGroup.DefaultIfEmpty()
                                    join houseImg in db.HouseImgsEntities on house.id equals houseImg.houseId
                                    where houseImg.isCover == true
                                    select new
                                    {
                                        order,
                                        house,
                                        photo = houseImg.path,
                                        tenant = user,
                                        //tenant = order.status == OrderStatus.租客非系統用戶 ? null : user,
                                        myComment = (from orderRating in db.OrdersRatingEntities
                                                     join reply in db.ReplyRatingEntities on orderRating.id equals reply.orderRatingId into replyGroup
                                                     from reply in replyGroup.DefaultIfEmpty()
                                                     where orderRating.UserId == UserId && orderRating.orderId == order.id
                                                     select new
                                                     {
                                                         comment = orderRating,
                                                         reply
                                                     }).FirstOrDefault(),
                                        tenantComment = (from orderRating in db.OrdersRatingEntities
                                                         join reply in db.ReplyRatingEntities on orderRating.id equals reply.orderRatingId into replyGroup
                                                         from reply in replyGroup.DefaultIfEmpty()
                                                         where orderRating.UserId != UserId && orderRating.orderId == order.id
                                                         select new
                                                         {
                                                             comment = orderRating,
                                                             reply
                                                         }).FirstOrDefault()
                                    };
                        // 計算資料總筆數
                        int totalRecords = query.Count();
                        // 分頁
                        var queryResult = query.OrderByDescending(o => o.order.leaseEndTime).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                        //var queryResult = query.ToList();
                        var orderList = new List<dynamic>();
                        //var today = DateTime.Today;
                        //var expiredDate = DateTime.Today.AddDays(-14);
                        foreach (var i in queryResult)
                        {
                            bool canComment = false;
                            if (today > i.order.leaseEndTime.Date
                                   && today <= i.order.leaseEndTime.Date.AddDays(14)
                                   && i.myComment == null)
                            {
                                canComment = true;
                            }
                            var order = new
                            {
                                orderInfo = new
                                {
                                    orderId = i.order.id,
                                    photo = i.photo,
                                    name = i.house.name,
                                    tenant = i.tenant == null ? null : i.tenant.lastName + i.tenant.firstName,
                                    leaseStartTime = i.order.leaseStartTime,
                                    leaseEndTime = i.order.leaseEndTime
                                },
                                commentInfo = new
                                {
                                    canComment = canComment,
                                    myComment = i.myComment == null ? null : new
                                    {
                                        commentId = i.myComment.comment.id,
                                        rating = i.myComment.comment.Rating,
                                        comment = i.myComment.comment.Comment,
                                        commentTime = i.myComment.comment.RatingDate,
                                        reply = i.myComment.reply?.ReplyComment,
                                        replyTime = i.myComment.reply?.ReplyDate
                                    },
                                    tenantComment = i.tenantComment == null ? null : new
                                    {
                                        commentId = i.tenantComment.comment.id,
                                        rating = i.tenantComment.comment.Rating,
                                        comment = i.tenantComment.comment.Comment,
                                        commentTime = i.tenantComment.comment.RatingDate,
                                        reply = i.tenantComment.reply?.ReplyComment,
                                        replyTime = i.tenantComment.reply?.ReplyDate
                                    },
                                }
                            };
                            orderList.Add(order);
                        }
                        orderList = orderList.OrderByDescending(o => o.commentInfo.canComment ? 1 : 0).ThenByDescending(o => o.orderInfo.leaseEndTime).ToList();
                        var result = new
                        {
                            statusCode = 200,
                            status = "success",
                            message = "已成功回傳房東的評價列表",
                            data = new
                            {
                                page = pageNumber,
                                totalCount = totalRecords,
                                orderList = orderList
                            }
                        };
                        return Content(HttpStatusCode.OK, result);
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