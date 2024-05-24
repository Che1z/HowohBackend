﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.OrderEnumList;
using UserAuth.Models.UserEnumList;
using UserAuth.Models.ViewModel;
using UserAuth.Models;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class OrderController : ApiController
    {
        /// <summary>
        /// [ALO-5, ALO-8]新增租客資料成立訂單
        /// </summary>
        /// <param name="orderInfoInput">訂單成立資訊</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/order/landlord/userInfo")]
        public IHttpActionResult PostUserInfoToOrder(OrderInfoInput orderInfoInput)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            var role = (UserRoleType)jwtObject["Role"];
            try
            {
                if (role == UserRoleType.租客)
                {
                    throw new Exception("使用者角色不符，不得使用此功能");
                }
                using (DBModel db = new DBModel())
                {
                    var order = new Order();
                    if (orderInfoInput.userId != null)
                    {
                        order.userId = orderInfoInput.userId.Value;
                        order.status = OrderStatus.待租客回覆租約;
                    }
                    else if (!String.IsNullOrEmpty(orderInfoInput.tenantTelphone))
                    {
                        order.tenantTelphone = orderInfoInput.tenantTelphone;
                        order.status = OrderStatus.租客非系統用戶;
                        var houseToChangeStatus = db.HouseEntities.Where(x => x.id == orderInfoInput.houseId).FirstOrDefault();
                        houseToChangeStatus.status = statusType.已承租;
                    }
                    else
                    {
                        throw new Exception("租客Id及手機號碼均未輸入，無法設定租客資訊");
                    }
                    order.houseId = orderInfoInput.houseId;
                    order.leaseStartTime = orderInfoInput.leaseStartTime;
                    order.leaseEndTime = orderInfoInput.leaseEndTime;
                    db.OrdersEntities.Add(order);
                    db.SaveChanges();
                    //if (!String.IsNullOrEmpty(orderInfoInput.tenantTelphone))
                    //{
                    //    var houseToChangeStatus = db.HouseEntities.Where(x => x.id == orderInfoInput.houseId).FirstOrDefault();
                    //    houseToChangeStatus.status = statusType.已承租;
                    //}
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "成功設定租客資訊"
                        //data = new
                        //{
                        //}
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // GET: api/Order
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Order/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Order
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/Order/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE: api/Order/5
        //public void Delete(int id)
        //{
        //}
    }
}