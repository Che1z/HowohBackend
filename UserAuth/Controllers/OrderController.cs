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
using iTextSharp.text;
using System.Web;

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
            var UserId = (int)jwtObject["Id"];

            try
            {
                if (role == UserRoleType.租客) //檢查角色
                {
                    return Content(HttpStatusCode.Forbidden, "使用者角色不符，不得使用此功能");
                }
                if (!ModelState.IsValid || orderInfoInput == null)
                {
                    throw new Exception("錯誤資訊不符合規範");
                }
                using (DBModel db = new DBModel())
                {
                    var houseToAddOrder = db.HouseEntities.Where(x => x.id == orderInfoInput.houseId).FirstOrDefault();

                    //進行房源相關檢查
                    if (houseToAddOrder == null) //檢查房源是否存在
                    {
                        throw new Exception("此房源不存在，無法設定租客資訊");
                    }
                    if (houseToAddOrder.userId != UserId)
                    {
                        return Content(HttpStatusCode.Forbidden, "該房源不屬於此使用者，無法變更房源狀態");
                    }
                    if (houseToAddOrder.status != statusType.刊登中) //檢查房源狀態
                    {
                        return Content(HttpStatusCode.Forbidden, "此房源狀態非刊登中，無法設定租客資訊");
                    }

                    //進行租客相關檢查
                    if (orderInfoInput.userId == null && String.IsNullOrEmpty(orderInfoInput.tenantTelphone))
                    {
                        throw new Exception("租客Id及手機號碼均未輸入，無法設定租客資訊");
                    }
                    var order = new Order();
                    if (orderInfoInput.userId != null)
                    {
                        var userToAdd = db.UserEntities.Where(x => x.Id == orderInfoInput.userId).FirstOrDefault();
                        if (userToAdd == null) //檢查租客是否存在
                        {
                            throw new Exception("此租客不存在，無法設定租客資訊");
                        }
                        order.userId = orderInfoInput.userId;
                        order.status = OrderStatus.待租客回覆租約;
                    }
                    else
                    {
                        order.tenantTelphone = orderInfoInput.tenantTelphone;
                        order.status = OrderStatus.租客非系統用戶;
                        houseToAddOrder.status = statusType.已承租;
                        var appointments = db.AppointmentsEntities.Where(x => x.houseId == houseToAddOrder.id && x.isValid == true).ToList();
                        foreach (var appointment in appointments)
                        {
                            appointment.isValid = false;
                        }
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
                    string filePath = Path.Combine((HttpContext.Current.Server.MapPath("~/Contracts")), "20240604 好窩房屋租賃合約.pdf");
                    string tempPDF = Path.Combine((HttpContext.Current.Server.MapPath("~/Contracts/NewContracts")), "輸出契約.pdf"); ;

                    using (PdfReader reader = new PdfReader(filePath))
                    {
                        using (FileStream fileStream = new FileStream(tempPDF, FileMode.Create, FileAccess.Write))
                        {
                            using (PdfStamper stamper = new PdfStamper(reader, fileStream))
                            {
                                string path = Path.Combine((HttpContext.Current.Server.MapPath("~/fonts")), "msjh.ttc");
                                BaseFont chBaseFont = BaseFont.CreateFont($"{path},0", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                                reader.AcroFields.AddSubstitutionFont(chBaseFont);

                                AcroFields form = stamper.AcroFields;
                                foreach (var ele in form.Fields.Keys)
                                {
                                    form.SetFieldProperty(ele, "textfont", chBaseFont, null);
                                }

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

                                    if (orderContent == null)
                                    {
                                        return Content(HttpStatusCode.NotFound, "房屋未找到");
                                    }

                                    if (orderContent.house.userIdFK.Id != userId)
                                    {
                                        return Content(HttpStatusCode.NotFound, "房東ID錯誤");
                                    }

                                    // 驗證數據是否正確
                                    string userName = orderContent.order.contractLandlordName;
                                    string tenentName = orderContent.order.contracttenantName;
                                    string houseLocation = orderContent.order.contractAddress;
                                    string housePing = orderContent.house.ping;
                                    string userCity = orderContent.house.city.ToString();

                                    int leaseDuraionYearCal = (orderContent.order.leaseEndTime - orderContent.order.leaseStartTime).Days / 365;
                                    string leaseDurationYear = leaseDuraionYearCal.ToString();

                                    int leaseDuraionMonthCal = (orderContent.order.leaseEndTime - orderContent.order.leaseStartTime).Days / 30;
                                    int leaseDurationMonthMinusYear = leaseDuraionMonthCal - leaseDuraionYearCal * 12;
                                    string leaseDurationMonth = leaseDurationMonthMinusYear.ToString();

                                    string contractStart = orderContent.order.leaseStartTime.ToString("yyyy年M月d日");
                                    string contractEnd = orderContent.order.leaseEndTime.ToString("yyyy年M月d日");

                                    int rentInt = Convert.ToInt32(orderContent.house.rent);
                                    string rent = rentInt.ToString();

                                    string paymentBeforeDate = orderContent.order.contractRentPaymentBeforeDate;

                                    string securityDeposit = orderContent.house.securityDeposit.ToString();
                                    int securityDepositAmount = 0;
                                    if (securityDeposit == "一個月")
                                    {
                                        securityDepositAmount = 1;
                                    }
                                    else if (securityDeposit == "二個月")
                                    {
                                        securityDepositAmount = 2;
                                    }
                                    securityDepositAmount = securityDepositAmount * Convert.ToInt32(rent);

                                    //付款方式
                                    List<string> paymentMethod = new List<string> { };
                                    if (orderContent.house.paymentMethodOfWaterBill != (paymentTypeOfWaterBill)2)
                                    {
                                        paymentMethod.Add("水費");
                                    }
                                    if (orderContent.house.paymentMethodOfManagementFee == (paymentMethodOfManagementFee)3 || orderContent.house.paymentMethodOfManagementFee == (paymentMethodOfManagementFee)2)
                                    {
                                        paymentMethod.Add("管理費");
                                    }
                                    paymentMethod.Add("電費");

                                    string paymentMethods = string.Join(", ", paymentMethod);

                                    // 寫入表單欄位

                                    //fill_1 : 出租者
                                    form.SetField("fill_1", userName);
                                    //fill_2 : 承租者
                                    form.SetField("fill_2", tenentName);

                                    //fill_3 : 房屋座落
                                    form.SetField("fill_3", houseLocation);

                                    //fill_4 : 承租面積
                                    form.SetField("fill_4", $"{housePing}坪");

                                    //fill_5 : 合約 (計 X 年)
                                    form.SetField("fill_5", leaseDurationYear);

                                    //fill_6 : 合約 (計 X 月)
                                    form.SetField("fill_6", leaseDurationMonth);

                                    //fill_7 : 合約 (起 - 西元)
                                    form.SetField("fill_7", contractStart);

                                    //fill_8 : 合約 (訖 - 西元)
                                    form.SetField("fill_8", contractEnd);

                                    //fill_9 : 租金 (新台幣)
                                    form.SetField("fill_9", rentInt.ToString("N0"));

                                    //fill_10 : 每月 (幾日給付甲方)
                                    form.SetField("fill_10", paymentBeforeDate);

                                    //fill_11 : 押金 (新台幣)
                                    form.SetField("fill_11", securityDepositAmount.ToString("N0"));

                                    //fill_12 : 什麼費用要由 (乙方)負擔
                                    form.SetField("fill_12", paymentMethods.ToString());

                                    //fill_13 : 水費
                                    //fill_14 : 水費 (繳費方式)
                                    //fill_15 : 電費
                                    //fill_16 : 電費 (繳費方式)
                                    //fill_17 : 管理費
                                    //fill_18 : 管理費 (繳費方式)

                                    //只需繳交電費
                                    if (paymentMethod.Count == 1)
                                    {
                                        if (orderContent.house.electricBill == (paymentTypeOfElectricBill)1)
                                        {
                                            //自行繳納
                                            form.SetField("fill_13", "電費依台電計價");
                                            form.SetField("fill_14", orderContent.house.paymentMethodOfElectricBill.ToString());
                                        }
                                        else
                                        {
                                            //隨房租繳納
                                            form.SetField("fill_13", $"電費每度{orderContent.house.electricBillPerDegree}元計價");
                                            form.SetField("fill_14", orderContent.house.paymentMethodOfElectricBill.ToString());
                                        }
                                    }

                                    //若只繳兩種 (電費 + 水費 || 管理費)
                                    if (paymentMethod.Count == 2)
                                    {
                                        if (paymentMethod.Contains("水費"))
                                        {
                                            if (orderContent.house.paymentMethodOfWaterBill == (paymentTypeOfWaterBill)1)
                                            {
                                                form.SetField("fill_13", "水費依台水計價");
                                                form.SetField("fill_14", "自行繳納");
                                            }
                                            else if (orderContent.house.paymentMethodOfWaterBill == (paymentTypeOfWaterBill)3)
                                            {
                                                form.SetField("fill_13", $"水費自訂，每人每月{orderContent.house.waterBillPerMonth}元");
                                                form.SetField("fill_14", "隨房租繳納");
                                            }
                                            if (orderContent.house.electricBill == (paymentTypeOfElectricBill)1)
                                            {
                                                //依台電計價
                                                form.SetField("fill_15", "電費依台電計價");
                                                form.SetField("fill_16", orderContent.house.paymentMethodOfElectricBill.ToString());
                                            }
                                            else
                                            {
                                                //自訂
                                                form.SetField("fill_15", $"電費每度{orderContent.house.electricBillPerDegree}元計價");
                                                form.SetField("fill_16", orderContent.house.paymentMethodOfElectricBill.ToString());
                                            }
                                        }
                                        if (paymentMethod.Contains("管理費"))
                                        {
                                            int managementFee = Convert.ToInt32(orderContent.house.managementFeePerMonth);
                                            form.SetField("fill_13", $"管理費每月{managementFee.ToString("N0")}元");
                                            form.SetField("fill_14", orderContent.house.paymentMethodOfManagementFee.ToString());
                                            if (orderContent.house.electricBill == (paymentTypeOfElectricBill)1)
                                            {
                                                //依台電計價
                                                form.SetField("fill_15", "電費依台電計價");
                                                form.SetField("fill_16", orderContent.house.paymentMethodOfElectricBill.ToString());
                                            }
                                            else
                                            {
                                                //自訂
                                                form.SetField("fill_15", $"電費每度{orderContent.house.electricBillPerDegree}元計價");
                                                form.SetField("fill_16", orderContent.house.paymentMethodOfElectricBill.ToString());
                                            }
                                        }
                                    }
                                    if (paymentMethod.Count == 3)
                                    {
                                        if (orderContent.house.paymentMethodOfWaterBill == (paymentTypeOfWaterBill)1)
                                        {
                                            form.SetField("fill_13", "水費依台水計價");
                                            form.SetField("fill_14", "自行繳納");
                                        }
                                        else if (orderContent.house.paymentMethodOfWaterBill == (paymentTypeOfWaterBill)3)
                                        {
                                            form.SetField("fill_13", $"水費自訂，每人每月{orderContent.house.waterBillPerMonth}元");
                                            form.SetField("fill_14", "隨房租繳納");
                                        }
                                        int managementFee = Convert.ToInt32(orderContent.house.managementFeePerMonth);
                                        form.SetField("fill_15", $"管理費每月{managementFee.ToString("N0")}元");
                                        form.SetField("fill_16", orderContent.house.paymentMethodOfManagementFee.ToString());

                                        if (orderContent.house.electricBill == (paymentTypeOfElectricBill)1)
                                        {
                                            //自行繳納
                                            form.SetField("fill_17", "電費依台電計價");
                                            form.SetField("fill_18", orderContent.house.paymentMethodOfElectricBill.ToString());
                                        }
                                        else
                                        {
                                            //隨房租繳納
                                            form.SetField("fill_17", $"電費每度{orderContent.house.electricBillPerDegree}元計價");
                                            form.SetField("fill_18", orderContent.house.paymentMethodOfElectricBill.ToString());
                                        }
                                    }

                                    //fill_1_2 : 提前幾個月通知他方
                                    form.SetField("fill_1_2", orderContent.order.contractTerminationNoticeMonths);

                                    //fill_2_2 : 賠償幾個月的租金
                                    form.SetField("fill_2_2", orderContent.order.contractTerminationPenaltyMonths);

                                    //fill_3_2 : 特別約定事項2 (不用填)
                                    //fill_4_2 : 特別約定事項3 (不用填)
                                    //fill_5_2 : 生活公約1 (不用填)
                                    //fill_6_2 : 生活公約2 (不用填)
                                    //fill_7_2 : 設備包含
                                    List<string> equipments = new List<string> { };
                                    if (orderContent.house.hasAirConditioner == true)
                                    {
                                        equipments.Add("冷氣");
                                    }
                                    if (orderContent.house.hasWashingMachine == true)
                                    {
                                        equipments.Add("洗衣機");
                                    }
                                    if (orderContent.house.hasRefrigerator == true)
                                    {
                                        equipments.Add("冰箱");
                                    }
                                    if (orderContent.house.hasCloset == true)
                                    {
                                        equipments.Add("衣櫃");
                                    }
                                    if (orderContent.house.hasTableAndChair == true)
                                    {
                                        equipments.Add("桌椅");
                                    }
                                    if (orderContent.house.hasWaterHeater == true)
                                    {
                                        equipments.Add("熱水器");
                                    }
                                    if (orderContent.house.hasInternet == true)
                                    {
                                        equipments.Add("網路");
                                    }
                                    if (orderContent.house.hasBed == true)
                                    {
                                        equipments.Add("床");
                                    }
                                    if (orderContent.house.hasTV == true)
                                    {
                                        equipments.Add("電視");
                                    }
                                    string equipmentString = string.Join(", ", equipments);
                                    form.SetField("fill_7_2", equipmentString);

                                    //fill_8_2 : 備註 (不用填)
                                    //fill_9_2 : 甲方 (不用填)
                                    //fill_10_2 : 乙方 (不用填)
                                    //fill_11_2 : 身分證字號(甲方) (不用填)
                                    //fill_12_2 : 身分證字號(乙方) (不用填)
                                    //fill_13_2 : 合約 (西元) (不用填)
                                    //fill_14_2 : 合約 (月) (不用填)
                                    //fill_15_2 : 合約 (日) (不用填)
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