using System;
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
        [JwtAuthFilters]
        [Route("api/order/landlord/userInfo")]
        public IHttpActionResult PostUserInfoToOrder(OrderInfoInput orderInfoInput)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            var role = (UserRoleType)jwtObject["Role"];
            var userId = (int)jwtObject["Id"];

            try
            {
                if (role == UserRoleType.租客) //檢查角色
                {
                    throw new Exception("使用者角色不符，不得使用此功能");
                }
                if (!ModelState.IsValid || orderInfoInput == null)
                {
                    throw new Exception("錯誤資訊不符合規範");
                }
                using (DBModel db = new DBModel())
                {
                    var houseToAddOrder = db.HouseEntities.Where(x => x.id == orderInfoInput.houseId).FirstOrDefault();
                    var userToAdd = db.UserEntities.Where(x => x.Id == orderInfoInput.userId).FirstOrDefault();

                    if (houseToAddOrder.userId != userId)
                    {
                        throw new Exception("該房源不屬於此使用者，無法變更房源狀態");
                    }

                    if (orderInfoInput.userId == null && String.IsNullOrEmpty(orderInfoInput.tenantTelphone))
                    {
                        throw new Exception("租客Id及手機號碼均未輸入，無法設定租客資訊");
                    }

                    if (houseToAddOrder == null) //檢查房源是否存在
                    {
                        throw new Exception("此房源不存在，無法設定租客資訊");
                    }
                    if (houseToAddOrder.status != statusType.刊登中) //檢查房源狀態
                    {
                        throw new Exception("此房源狀態非刊登中，無法設定租客資訊");
                    }
                    if (userToAdd == null) //檢查租客是否存在
                    {
                        throw new Exception("此租客不存在，無法設定租客資訊");
                    }

                    var order = new Order();
                    if (orderInfoInput.userId != null)
                    {
                        order.userId = orderInfoInput.userId.Value;
                        order.status = OrderStatus.待租客回覆租約;
                    }
                    else
                    {
                        order.tenantTelphone = orderInfoInput.tenantTelphone;
                        order.status = OrderStatus.租客非系統用戶;
                        houseToAddOrder.status = statusType.已承租;
                        //var appointments = db.AppointmentsEntities.Where(x => x.houseId == houseToAddOrder.id).ToList();
                    }
                    ///todo: 刪除預約
                    order.houseId = orderInfoInput.houseId;
                    order.leaseStartTime = orderInfoInput.leaseStartTime;
                    order.leaseEndTime = orderInfoInput.leaseEndTime;
                    db.OrdersEntities.Add(order);
                    db.SaveChanges();

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