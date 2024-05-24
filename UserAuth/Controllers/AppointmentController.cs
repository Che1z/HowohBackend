using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.ViewModel;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class AppointmentController : ApiController
    {
        /// <summary>
        /// [ACA-1]取得預約看房列表
        /// </summary>
        /// <param name="houseId"></param>
        /// <param name="houseStatus"></param>         
        /// 刊登中 = 10,已承租 = 20,
        /// <param name="orderMethod"></param>
        /// 舊到新 = 1, 新至舊 = 2, 只看隱藏 = 3, 只看沒有隱藏 & 舊到新 = 4
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/common/list")]
        public IHttpActionResult getAppointmentList(string houseId = "null", int houseStatus = 10, int orderMethod = 1, int pageNumber = 1)
        {
            // 檢查 houseId 是否可以轉換為整數
            int houseIdInt = 0;
            bool isHouseIdValid = int.TryParse(houseId, out houseIdInt);

            // 取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            // 取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            int role = (int)jwtObject["Role"];
            using (DBModel db = new DBModel())
            {
                int init = (pageNumber - 1) * 12;
                var query = db.AppointmentsEntities.AsQueryable();
                // 房東查詢
                if (isHouseIdValid)
                {
                    if (role == 1 )
                    {
                        return Content(HttpStatusCode.BadRequest, "傳入資料錯誤");
                    }
                    else
                    {
                        houseStatus = 10;
                        query = query.Where(a => a.houseId == houseIdInt);

                        if (orderMethod == 1)
                        {
                            query = query.OrderBy(h => h.CreateAt);
                        }
                        if (orderMethod == 2)
                        {
                            query = query.OrderByDescending(h => h.CreateAt);
                        }
                        if (orderMethod == 3)
                        {
                            query = query.Where(h => h.hidden == true).OrderBy(h => h.CreateAt);
                        }
                        if (orderMethod == 4)
                        {
                            query = query.Where(h => h.hidden == false).OrderBy(h => h.CreateAt);
                        }


                        query = query.Skip(init).Take(12);
                        var result = query.Select(r => new
                        {
                            userId = r.userId,
                            appointmentTime = r.CreateAt,
                            descrption = new
                            {
                                tenantInfo = db.UserEntities
                                         .Where(u => u.Id == r.userId)
                                          .Select(u => new
                                          {
                                              firstName = u.firstName,
                                              lastName = u.lastName,
                                              photo = u.photo,
                                              email = u.email,
                                              gender = u.gender.ToString(),
                                              job = u.job.ToString(),
                                              phoneNumber = u.telphone,
                                          }).FirstOrDefault(),
                                orderInfo = db.OrdersEntities.Where(o => o.houseId == houseIdInt && o.userId == r.userId).Select(o => new
                                {
                                    orderId = o.id,
                                    status = o.status.ToString(),
                                    createTime = o.CreateAt
                                })
                            }
                        }).ToList();

                        var finalresult = new
                        {
                            houseId = houseIdInt,
                            result = result
                        };
                        return Content(HttpStatusCode.OK, finalresult);
                    }
                }
                // 租客查詢
                else
                {
                    query = query.OrderBy(a => a.CreateAt).Where(a => a.userId == UserId && db.HouseEntities.Any(h => h.id == a.houseId && h.status == (statusType)houseStatus)).Skip(init).Take(12);
                   
                    var result = query.Select(r => new
                    {
                        appointmentCreateTime = r.CreateAt,
                        descrption = new
                        {
                            detail = db.HouseEntities
                                     .Where(h => h.id == r.houseId &&  h.status == (statusType)houseStatus)
                                     .Select(hi => new
                                     {
                                         houseId = hi.id,
                                         landlordId = hi.userId,
                                         houseTitle = hi.name,
                                         houseImage = db.HouseImgsEntities.Where(h => h.isCover == true && h.houseId == hi.id).Select(z => new
                                         {
                                             houseImagePath = z.path
                                         }),
                                         landlordInfo = db.UserEntities
                                                    .Where(u => u.Id == hi.userId)
                                                    .Select(u => new
                                                    {
                                                        firstName = u.firstName,
                                                        lastName = u.lastName,
                                                        gender = u.gender.ToString(),
                                                        phoneNumber = u.telphone,
                                                        lineId = u.LineId,
                                                    }).FirstOrDefault(),                                       
                                     })
                        }
                    }).ToList();
                    var finalresult = new
                    {
                        userId = UserId,
                        result = result
                    };
                    return Content(HttpStatusCode.OK, finalresult);
                }
            }
        }

        /// <summary>
        /// [ACA-2]取得預約看房列表查詢數量
        /// </summary>
        /// <param name="houseId"></param>
        /// <param name="houseStatus"></param>
        /// <param name="orderMethod"></param>
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/common/totalNumber")]
        public IHttpActionResult getAppointmentTotalNumber(string houseId = "null", int houseStatus = 10, int orderMethod = 1) {


            // 檢查 houseId 是否可以轉換為整數
            int houseIdInt = 0;
            bool isHouseIdValid = int.TryParse(houseId, out houseIdInt);

            // 取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            // 取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            int role = (int)jwtObject["Role"];
            using (DBModel db = new DBModel())
            {
                var query = db.AppointmentsEntities.AsQueryable();
                // 房東查詢
                if (isHouseIdValid)
                {
                    if (role == 1)
                    {
                        return Content(HttpStatusCode.BadRequest, "傳入資料錯誤");
                    }
                    else
                    {
                        houseStatus = 10;
                        query = query.Where(a => a.houseId == houseIdInt);

                        if (orderMethod == 1)
                        {
                            query = query.OrderBy(h => h.CreateAt);
                        }
                        if (orderMethod == 2)
                        {
                            query = query.OrderByDescending(h => h.CreateAt);
                        }
                        if (orderMethod == 3)
                        {
                            query = query.Where(h => h.hidden == true).OrderBy(h => h.CreateAt);
                        }
                        if (orderMethod == 4)
                        {
                            query = query.Where(h => h.hidden == false).OrderBy(h => h.CreateAt);
                        }


                        int dataTotalNumber = query.Count();

                        var finalresult = new
                        {
                            totalNumber = dataTotalNumber
                        
                        };
                        return Content(HttpStatusCode.OK, finalresult);
                    }
                }
                // 租客查詢
                else
                {
                    query = query.OrderBy(a => a.CreateAt).Where(a => a.userId == UserId && db.HouseEntities.Any(h => h.id == a.houseId && h.status == (statusType)houseStatus));

                    int dataTotalNumber = query.Count();
                    var finalresult = new
                    {
                        totalNumber = dataTotalNumber
                    };
                    return Content(HttpStatusCode.OK, finalresult);
                }
            }
        }


    }
}
