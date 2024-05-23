using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.ViewModel;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class AppointmentController : ApiController
    {
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/common/list")]
        public IHttpActionResult getAppointmentList(string houseId = "null")
        {
            // 檢查 houseId 是否可以轉換為整數
            int houseIdInt = 0;
            bool isHouseIdValid = int.TryParse(houseId, out houseIdInt);

            // 取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            // 取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            using (DBModel db = new DBModel())
            {
                var query = db.AppointmentsEntities.AsQueryable();
                // 房東查詢
                if (isHouseIdValid)
                {
                    query = query.Where(a => a.houseId == houseIdInt);
                    var result = query.Select(r => new
                    {
                        userId = r.userId,
                        descrption = new
                        {
                            tenantInfo = db.UserEntities
                                     .Where(u => u.Id == r.userId)
                                      .Select(u => new
                                      {
                                          firstName = u.firstName,
                                          lastName = u.lastName,
                                          email = u.email,
                                          gender = u.gender.ToString(),
                                          job = u.job.ToString(),
                                          phoneNumber = u.telphone,
                                      }).FirstOrDefault(),
                            orderInfo = db.OrdersEntities.Where(o => o.houseId == houseIdInt && o.userId == r.userId).Select(o => new
                            {
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
                // 租客查詢
                else
                {
                    query = query.Where(a => a.userId == UserId);
                    var result = query.Select(r => new
                    {
                        houseId = r.houseId,
                        createTime = r.CreateAt,
                        descrption = new
                        {
                            detail = db.HouseEntities
                                     .Where(h => h.id == r.houseId)
                                     .Select(hi => new
                                     {
                                         landlordId = hi.userId,
                                         houseTitle = hi.name,
                                         houseImage = db.HouseImgsEntities.Where(h => h.isCover == true && h.houseId == r.houseId).Select(z => new
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
                                                    }).FirstOrDefault(),
                                         orderInfo = db.OrdersEntities.Where(o => o.houseId == r.houseId && o.userId == r.userId).Select(o => new
                                         {
                                             status = o.status.ToString(),
                                             createTime = o.CreateAt
                                         })
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


    }
}
