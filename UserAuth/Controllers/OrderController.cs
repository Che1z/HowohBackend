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
using System.IO;
using iTextSharp.text.pdf;

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
                        ///todo: 刪除預約
                        //var appointments = db.AppointmentsEntities.Where(x => x.houseId == houseToAddOrder.id).ToList();
                        //db.AppointmentsEntities.RemoveRange(appointments);
                    }

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

        // TODO 待修正Contract填入內容與個別欄位判別
        [HttpPost]
        [JwtAuthFilters]
        [Route("api/order/landloard/createContract")]
        public IHttpActionResult CreateContract(string id)
        {
            // 取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            // 取得JWT內部資料
            var role = (UserRoleType)jwtObject["Role"];
            var userId = (int)jwtObject["Id"];

            try
            {
                if (role == UserRoleType.租客) // 檢查角色
                {
                    return Content(HttpStatusCode.BadRequest, "身分錯誤");
                }
                else
                {
                    //string filePath = @"C:\Users\KHUser\Desktop\20240603 好窩房屋租賃合約.pdf";
                    string filePath = @"C:\Users\Howoh\Desktop\20240603_.pdf";
                    //string tempPDF = @"C:\Users\KHUser\Desktop\輸出契約.pdf";
                    string tempPDF = @"C:\Users\Howoh\Desktop\Contract\NewContract.pdf";

                    using (PdfReader reader = new PdfReader(filePath))
                    {
                        using (FileStream fileStream = new FileStream(tempPDF, FileMode.Create, FileAccess.Write))
                        {
                            using (PdfStamper stamper = new PdfStamper(reader, fileStream))
                            {
                                AcroFields form = stamper.AcroFields;

                                using (var db = new DBModel())
                                {

                                    int orderId = Convert.ToInt32(id);
                                    var query = from order in db.OrdersEntities.AsQueryable()
                                                join house in db.HouseEntities on order.houseId equals house.id
                                                where order.id == orderId
                                                join user in db.UserEntities on house.userId equals user.Id
                                                select new
                                                {
                                                    house,
                                                    order,
                                                    landlordId = house.userId,
                                                    tenant = user
                                                };
                                    var orderContent = query.FirstOrDefault();

                                    if (query == null)
                                    {
                                        return Content(HttpStatusCode.NotFound, "房屋未找到");
                                    }

                                    // 驗證數據是否正確
                                    string userName = orderContent.house.userIdFK.firstName + orderContent.house.userIdFK.lastName;
                                    string userCity = orderContent.house.city.ToString();

                                    // 寫入表單欄位
                                    form.SetField("fill_1", userName);
                                    form.SetField("fill_2", userCity);
                                }
                                stamper.FormFlattening = true;
                            }
                        }
                    }

                    byte[] fileBytes = System.IO.File.ReadAllBytes(tempPDF);
                    System.IO.File.Delete(tempPDF); // 清理臨時文件

                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(fileBytes)
                    };

                    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "Contract.pdf"
                    };

                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                    return ResponseMessage(response);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
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