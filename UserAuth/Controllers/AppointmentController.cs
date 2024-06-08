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
        /// 未隱藏且為舊到新 = 1, 未隱藏且為新至舊 = 2, 只看隱藏 = 3
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/common/list")]
        public IHttpActionResult getAppointmentList(string houseId = "null", int houseStatus = 10, int orderMethod = 1, int pageNumber = 1)
        {
            try
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
                    if (houseStatus != 10 && houseStatus != 20)
                    {
                        return Content(HttpStatusCode.BadRequest, "傳入資料錯誤");
                    }
                    // 房東查詢
                    if (isHouseIdValid)
                    {
                        if (role == 1)
                        {
                            return Content(HttpStatusCode.BadRequest, "傳入資料錯誤");
                        }
                        //若傳入HouseId和JWT Token中UserId沒有符合

                        var house = db.HouseEntities.FirstOrDefault(i => i.id == houseIdInt);

                        if (house == null || house.userId != UserId)
                        {
                            return Content(HttpStatusCode.BadRequest, "ID資料錯誤");
                        }
                        else
                        {
                            houseStatus = 10;
                            query = query.Where(a => a.houseId == houseIdInt && a.isValid == true);

                            if (orderMethod == 1)
                            {
                                query = query.Where(h => h.hidden == false && h.isValid == true).OrderBy(h => h.CreateAt);
                            }
                            if (orderMethod == 2)
                            {
                                query = query.Where(h => h.hidden == false && h.isValid == true).OrderByDescending(h => h.CreateAt);
                            }
                            if (orderMethod == 3)
                            {
                                query = query.Where(h => h.hidden == true && h.isValid == true).OrderBy(h => h.CreateAt);
                            }



                            query = query.Skip(init).Take(12);
                            var result = query.Select(r => new
                            {
                                appointmentId = r.id,
                                userId = r.userId,
                                appointmentTime = r.CreateAt,
                                description = new
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
                        if (houseStatus == 10)
                        {
                            query = query.OrderBy(a => a.CreateAt).Where(a => a.userId == UserId && db.HouseEntities.Any(h => h.id == a.houseId && h.status == (statusType)houseStatus)).Skip(init).Take(12);
                        }
                        // Appointment的isValid若為False，則判定房子為已承租
                        if (houseStatus == 20)
                        {
                            query = query.OrderBy(a => a.CreateAt).Where(a => a.userId == UserId && db.HouseEntities.Any(h => h.id == a.houseId && a.isValid == false)).Skip(init).Take(12);
                        }

                        var result = query.Select(r => new
                        {
                            appointmentId = r.id,
                            appointmentCreateTime = r.CreateAt,
                            description = new
                            {
                                detail = db.HouseEntities
                                         .Where(h => h.id == r.houseId && h.status == (statusType)houseStatus)
                                         .Select(hi => new
                                         {
                                             houseId = hi.id,
                                             landlordId = hi.userId,
                                             houseTitle = hi.name,
                                             houseRent = hi.rent,
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
            catch (Exception ex)
            {
                // 記錄錯誤日志或處理錯誤
                return Content(HttpStatusCode.InternalServerError, "伺服器內部錯誤: " + ex.Message);
            }
        }

        /// <summary>
        /// [ACA-2]取得預約看房列表查詢數量
        /// </summary>
        /// <param name="houseId"></param>
        /// <param name="houseStatus"></param>
        /// 刊登中 = 10,已承租 = 20,
        /// <param name="orderMethod"></param>
        /// 未隱藏且為舊到新 = 1, 未隱藏且為新至舊 = 2, 只看隱藏 = 3
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/common/totalNumber")]
        public IHttpActionResult getAppointmentTotalNumber(string houseId = "null", int houseStatus = 10, int orderMethod = 1)
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
                var query = db.AppointmentsEntities.AsQueryable();
                // 房東查詢
                if (isHouseIdValid)
                {
                    if (role == 1)
                    {
                        return Content(HttpStatusCode.BadRequest, "傳入資料錯誤");
                    }
                    //若傳入HouseId和JWT Token中UserId沒有符合

                    var house = db.HouseEntities.FirstOrDefault(i => i.id == houseIdInt);

                    if (house == null || house.userId != UserId)
                    {
                        return Content(HttpStatusCode.BadRequest, "ID資料錯誤");
                    }
                    else
                    {
                        houseStatus = 10;
                        query = query.Where(a => a.houseId == houseIdInt);

                        if (orderMethod == 1)
                        {
                            query = query.Where(h => h.hidden == false).OrderBy(h => h.CreateAt);
                        }
                        if (orderMethod == 2)
                        {
                            query = query.Where(h => h.hidden == false).OrderByDescending(h => h.CreateAt);
                        }
                        if (orderMethod == 3)
                        {
                            query = query.Where(h => h.hidden == true).OrderBy(h => h.CreateAt);
                        }
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
                    if (houseStatus == 10)
                    {
                        query = query.OrderBy(a => a.CreateAt).Where(a => a.userId == UserId && db.HouseEntities.Any(h => h.id == a.houseId && h.status == (statusType)houseStatus));
                    }
                    // Appointment的isValid若為False，則判定房子為已承租
                    if (houseStatus == 20)
                    {
                        query = query.OrderBy(a => a.CreateAt).Where(a => a.userId == UserId && db.HouseEntities.Any(h => h.id == a.houseId && a.isValid == false));
                    }


                    int dataTotalNumber = query.Count();
                    var finalresult = new
                    {
                        totalNumber = dataTotalNumber
                    };
                    return Content(HttpStatusCode.OK, finalresult);
                }
            }
        }
        /// <summary>
        /// [ALA-1] 取得預約看房的租客資訊
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/landlord/{appointmentId}")]
        public IHttpActionResult landloardGetAppointmentDetail(string appointmentId)
        {
            // 檢查 appointmentId 是否可以轉換為整數
            int appointmentIdInt = 0;
            bool isAppointmentIdValid = int.TryParse(appointmentId, out appointmentIdInt);
            // 取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            // 取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            int role = (int)jwtObject["Role"];
            using (DBModel db = new DBModel())
            {
                // 可轉換成int後，確認JWT是否為房東
                if (isAppointmentIdValid)
                {
                    if (role == 1)
                    {
                        return Content(HttpStatusCode.BadRequest, "角色錯誤");
                    }
                    else
                    {
                        var query = db.AppointmentsEntities.AsQueryable();

                        var firstQuery = query.Where(a => a.id == appointmentIdInt && a.houseIdFK.userId == UserId).FirstOrDefault();
                        if (firstQuery == null)
                        {
                            return Content(HttpStatusCode.BadRequest, "輸入ID錯誤");
                        }
                        else
                        {
                            var result = query.Where(a => a.id == appointmentIdInt).Select(q => new
                            {
                                tenantId = q.userId,
                                tenantInfo = db.UserEntities.Where(u => u.Id == q.userId).Select(r => new
                                {
                                    lastName = r.lastName,
                                    firstName = r.firstName,
                                    job = r.job.ToString(),
                                    gender = r.gender.ToString(),
                                    phonenumber = r.telphone,
                                    intro = r.userIntro,
                                    photo = r.photo
                                }),
                                tenantRatingInfo = new
                                {


                                    Sum = db.OrdersEntities.Where(o => o.userId == q.userId).SelectMany(o => o.orderRatings).Where(o => o.UserId != q.userId)
                                    .Select(r => r.Rating)
                                    .DefaultIfEmpty(0)
                                    .Sum(),

                                    Count = db.OrdersEntities.Where(o => o.userId == q.userId)
                                    .SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                    .Where(o => o.UserId != q.userId)
                                    .Count(),

                                    Average = db.OrdersEntities
                                    .Where(o => o.userId == q.userId)
                                     .SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                    .Where(o => o.UserId != q.userId)
                                    .Select(r => (double)r.Rating) //需轉成double類型
                                    .DefaultIfEmpty(0)
                                    .Average()
                                },
                                orderList = new
                                {
                                    orderInfo = db.OrdersEntities.Where(o => o.userId == q.userId).Select(a => new
                                    {
                                        orderId = a.id,
                                        ratingList = db.OrdersRatingEntities.Where(or => or.orderId == a.id && or.UserId != q.userId).Select(e => new
                                        {
                                            orderRatingId = e.id,
                                            ratingRole = e.UserId == q.userId ? "租客評分" : "房東評分",
                                            orderRating = e.Rating,
                                            ratingDate = e.RatingDate,
                                            ratingComment = e.Comment,
                                            ratingUserInfo = db.UserEntities.Where(ur => ur.Id == e.UserId).Select(uri => new
                                            {
                                                userLastName = uri.lastName,
                                                userFirstName = uri.firstName,
                                                userGender = uri.gender.ToString(),
                                                userJob = uri.job.ToString(),
                                            }),
                                            replyInfo = db.ReplyRatingEntities.Where(rp => rp.orderRatingId == e.id).Select(ri => new
                                            {
                                                orderRatingId = ri.orderRatingId,
                                                //userid = ri.UserId,                                            
                                                commentUserRole = ri.UserId == q.userId ? "租客評語" : "房東評語",
                                                // 房東評語才需回傳，詳細房東資料 (租客評語只需渲染租客回覆)
                                                userInfo = ri.UserId == q.userId ? null : db.UserEntities.Where(u => u.Id == ri.UserId).Select(g => new
                                                {
                                                    lastName = g.lastName,
                                                    firstName = g.firstName,
                                                    gender = g.gender.ToString(),
                                                }).FirstOrDefault(),
                                                replyComment = ri.ReplyComment,
                                                date = ri.ReplyDate,
                                            })
                                        }).ToList()
                                    })
                                },

                            }).ToList();
                            return Content(HttpStatusCode.OK, result);
                        }
                    }
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "錯誤參數");
                }
            }
        }

        /// <summary>
        /// [ATA-3] 取得預約看房的房東資訊
        /// </summary>
        /// <param name="appointmentId"></param>
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/tenant/{appointmentId}")]
        public IHttpActionResult tenantGetAppointment(string appointmentId)
        {
            // 檢查 appointmentId 是否可以轉換為整數
            int appointmentIdInt = 0;
            bool isAppointmentIdValid = int.TryParse(appointmentId, out appointmentIdInt);
            // 取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            // 取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            int role = (int)jwtObject["Role"];

            using (DBModel db = new DBModel())
            {
                // 可轉換成int後，再確認JWT是否為租客
                if (isAppointmentIdValid)
                {
                    if (role == 0)
                    {
                        return Content(HttpStatusCode.BadRequest, "角色錯誤");
                    }
                    else
                    {
                        var query = db.AppointmentsEntities.AsQueryable();

                        var firstQuery = query.Where(a => a.id == appointmentIdInt && a.userId == UserId).FirstOrDefault();
                        if (firstQuery == null)
                        {
                            return Content(HttpStatusCode.BadRequest, "輸入ID錯誤");
                        }
                        else
                        {

                            var result = query.Where(a => a.id == appointmentIdInt).Select(q => new
                            {
                                appointmentId = q.id,
                                houseId = q.houseId,
                                info = db.HouseEntities.Where(h => h.id == q.houseId).Select(a => new
                                {
                                    userId = a.userId,                                    
                                    info = db.UserEntities.Where(u => u.Id == a.userId).Select(b => new
                                    {
                                        lastName = b.lastName,
                                        firstName = b.firstName,
                                        image = b.photo,
                                        intro = b.userIntro,
                                        gender = b.gender,
                                    }),
                                    landlordRatingInfo = new
                                    {
                                        Sum = db.OrdersEntities.Where(o => db.HouseEntities
                                    .Where(h => h.userId == a.userId)
                                    .Select(h => h.id)
                                    .Contains(o.houseId)).SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                    .Where(o => o.UserId != a.userId)
                                    .Select(r => r.Rating)
                                    .DefaultIfEmpty(0)
                                    .Sum(),

                                        Count = db.OrdersEntities.Where(o => db.HouseEntities
                                    .Where(h => h.userId == a.userId)
                                    .Select(h => h.id)
                                    .Contains(o.houseId)).SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                    .Where(o => o.UserId != a.userId)
                                    .Count(),

                                        Average = db.OrdersEntities
                                         .Where(o => db.HouseEntities
                                         .Where(h => h.userId == a.userId)
                                         .Select(h => h.id)
                                         .Contains(o.houseId))
                                         .SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                         .Where(o => o.UserId != a.userId)
                                         .Select(r => (double)r.Rating)
                                         .DefaultIfEmpty(0)
                                         .Average()
                                    },

                                    tagList = new
                                    {
                                        rentSubsity = a.isRentSubsidy,
                                        petAllowed = a.isPetAllowed,
                                        cookAllowed = a.isCookAllowed,
                                        STRAllowed = a.isSTRAllowed,
                                    },
                                    basicInfo = new
                                    {
                                        city = a.city,
                                        district = a.district.ToString(),
                                        road = a.road,
                                        lane = a.lane,
                                        alley = a.alley,
                                        number = a.number,
                                        ping = a.ping,
                                        floor = a.floor,
                                        floorTotal = a.floorTotal,
                                        type = a.type.ToString(),
                                        roomNumbers = a.roomNumbers,
                                        livingRoomNumbers = a.livingRoomNumbers,
                                        bathroomNumbers = a.bathRoomNumbers,
                                        balconyNumbers = a.balconyNumbers,
                                        parkingSpaceNumbers = a.parkingSpaceNumbers,
                                        description = a.description,
                                    },
                                    facility = new
                                    {
                                        nearFacilty = new
                                        {
                                            nearDepartmentStore = a.isNearByDepartmentStore,
                                            nearSchool = a.isNearBySchool,
                                            nearMorningMarket = a.isNearByMorningMarket,
                                            nearNightMarket = a.isNearByNightMarket,
                                            nearConvenientStore = a.isNearByConvenientStore,
                                            nearPark = a.isNearByPark,
                                        },
                                        houseFeature = new
                                        {
                                            hasGarbageDisposal = a.hasGarbageDisposal,
                                            hasWindowBathroom = a.hasWindowInBathroom,
                                            hasElevator = a.hasElevator,
                                        },
                                        equipment = new
                                        {
                                            hasAirConditioner = a.hasAirConditioner,
                                            hasWashingMachine = a.hasWashingMachine,
                                            hasRefrigerator = a.hasRefrigerator,
                                            hasCloset = a.hasCloset,
                                            hasTableAndChair = a.hasTableAndChair,
                                            hasWaterHeater = a.hasWaterHeater,
                                            hasInternet = a.hasInternet,
                                            hasBed = a.hasBed,
                                            hasTV = a.hasTV,
                                        },
                                        commute = new
                                        {
                                            nearMRT = a.isNearMRT,
                                            kmAwayMRT = a.kmAwayMRT,
                                            nearLRT = a.isNearLRT,
                                            kmAwayLRT = a.kmAwayLRT,
                                            nearBusStation = a.isNearBusStation,
                                            kmAwayBusStation = a.kmAwayBusStation,
                                            nearTrainStation = a.isNearTrainStation,
                                            kmAwayTrainStation = a.kmAwayTrainStation,
                                            nearHSR = a.isNearHSR,
                                            kmAwayHSR = a.kmAwayHSR,
                                        },
                                        payment = new
                                        {
                                            waterBillPayment = a.paymentMethodOfWaterBill.ToString(),
                                            waterBillPerMonth = a.waterBillPerMonth,
                                            electricBillPayment = a.paymentMethodOfElectricBill.ToString(),
                                            electricBillPerMonth = a.electricBillPerDegree,
                                            managementFeePayment = a.paymentMethodOfManagementFee.ToString(),
                                            managementFeePerMonth = a.managementFeePerMonth,
                                        },
                                        replyComment = new
                                        {
                                            detail = db.OrdersEntities.Where(o => db.HouseEntities
                                        .Where(h => h.userId == a.userId)
                                        .Select(h => h.id)
                                        .Contains(o.houseId)).Select(de => new
                                        {
                                            orderId = de.id,
                                            //
                                            ratingList = db.OrdersRatingEntities.Where(or => or.orderId == de.id && or.UserId != a.userId).Select(e => new
                                            {
                                                orderRatingId = e.id,
                                                ratingRole = e.UserId == a.userId ? "房東評分" : "租客評分",
                                                orderRating = e.Rating,
                                                ratingDate = e.RatingDate,
                                                ratingComment = e.Comment,
                                                ratingUserInfo = db.UserEntities.Where(ur => ur.Id == e.UserId).Select(uri => new
                                                {
                                                    userLastName = uri.lastName,
                                                    userFirstName = uri.firstName,
                                                    userGender = uri.gender.ToString(),
                                                    userJob = uri.job.ToString(),
                                                }),
                                                replyInfo = db.ReplyRatingEntities.Where(rp => rp.orderRatingId == e.id).Select(ri => new
                                                {
                                                    orderRatingId = ri.orderRatingId,
                                                    //userid = ri.UserId,                                            
                                                    commentUserRole = ri.UserId == a.userId ? "房東回應" : "租客回應",
                                                    userInfo = ri.UserId == a.userId ? null : db.UserEntities.Where(u => u.Id == ri.UserId).Select(g => new
                                                    {
                                                        lastName = g.lastName,
                                                        firstName = g.firstName,
                                                        gender = g.gender.ToString(),
                                                    }).FirstOrDefault(),
                                                    replyComment = ri.ReplyComment,
                                                    date = ri.ReplyDate,
                                                })
                                            }).ToList()
                                        })
                                        }
                                    }
                                }).FirstOrDefault()
                            }).ToList();
                            return Content(HttpStatusCode.OK, result);
                        }
                    }
                }
                return Content(HttpStatusCode.BadRequest, "輸入ID錯誤");
            }
        }

        /// <summary>
        /// [ATA-1] 取得租約邀請的列表
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/tenant/list/invited")]
        public IHttpActionResult getInvitedList(string pageNumber = "1")
        {
            try
            {
                // 取得使用者JWT
                var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

                // 取得JWT內部資料
                int UserId = (int)jwtObject["Id"];
                int role = (int)jwtObject["Role"];

                if (role != 1)
                {
                    return Content(HttpStatusCode.BadRequest, "身分錯誤");
                }
                else
                {
                    using (DBModel db = new DBModel())
                    {
                        var query = db.OrdersEntities.AsQueryable();
                        var firstQuery = query.Where(o => o.status.ToString() == "待租客回覆租約" && o.userId == UserId);
                        int skip = (Convert.ToInt32(pageNumber) - 1) * 12;
                        firstQuery = firstQuery.OrderBy(z => z.CreateAt).Skip(skip).Take(12);
                        var result = firstQuery.Select(a => new
                        {
                            orderId = a.id,
                            orderCreateTime = a.CreateAt,
                            leaseStartTime = a.leaseStartTime,
                            leaseEndTime = a.leaseEndTime,

                            houseId = a.houseId,
                            houseInfo = new
                            {
                                houseDetail = db.HouseEntities.Where(h => h.id == a.houseId).Select(b => new
                                {
                                    landlordId = b.userId,
                                    housePhoto = b.HouseImgs.Where(z => z.isCover == true).Select(i => new
                                    {
                                        photoPath = i.path,
                                    }),
                                    landlordInfo = db.UserEntities.Where(d => d.Id == b.userId).Select(e => new
                                    {
                                        lastName = e.lastName,
                                        firstName = e.firstName,
                                        gender = e.gender.ToString(),
                                        intro = e.userIntro,
                                        phoneNumber = e.telphone,
                                    }).FirstOrDefault(),
                                    title = b.name,
                                    rent = b.rent,
                                }).FirstOrDefault()
                            }
                        }).ToList();
                        return Content(HttpStatusCode.OK, result);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ATA-2] 取得租約邀請的房源資訊
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>

        [HttpGet]
        [JwtAuthFilters]
        [Route("api/appointment/tenant/invited/{orderId}")]
        public IHttpActionResult getInvitedOrder(string orderId)
        {
            try
            {
                // 取得使用者JWT
                var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

                // 取得JWT內部資料
                int UserId = (int)jwtObject["Id"];
                int role = (int)jwtObject["Role"];

                if (role != 1)
                {
                    return Content(HttpStatusCode.BadRequest, "身分錯誤");
                }
                else
                {
                    using (DBModel db = new DBModel())
                    {
                        var query = db.OrdersEntities.AsQueryable();
                        int orderNumber = Convert.ToInt32(orderId);
                        var tryQuery = query.Where(o => o.userId == UserId && o.id == orderNumber).FirstOrDefault();
                        if (tryQuery == null)
                        {
                            return Content(HttpStatusCode.BadRequest, "ID錯誤");
                        }
                        else
                        {
                            var firstQuery = query.Where(o => o.userId == UserId && o.id == orderNumber);

                            var result = firstQuery.Select(a => new
                            {
                                orderId = a.id,
                                orderCreateTime = a.CreateAt,
                                leaseStartTime = a.leaseStartTime,
                                leaseEndTime = a.leaseEndTime,
                                houseId = a.houseId,
                                landlord = db.HouseEntities.Where(b => b.id == a.houseId).Select(c => new
                                {
                                    landlordId = c.userId,
                                    rent = c.rent,
                                    depositType = c.securityDeposit.ToString(),
                                    landlordInfo = db.UserEntities.Where(d => d.Id == c.userId).Select(e => new
                                    {
                                        lastName = e.lastName,
                                        firstName = e.firstName,
                                        gender = e.gender.ToString(),
                                        intro = e.userIntro,
                                        photo = e.photo,
                                    }).FirstOrDefault(),
                                    landlordRatingInfo = new
                                    {
                                        Sum = db.OrdersEntities.Where(o => db.HouseEntities
                                    .Where(h => h.userId == c.userId)
                                    .Select(h => h.id)
                                    .Contains(o.houseId)).SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                        .Where(o => o.UserId != c.userId)
                                        .Select(r => r.Rating)
                                        .DefaultIfEmpty(0)
                                        .Sum(),

                                        Count = db.OrdersEntities.Where(o => db.HouseEntities
                                    .Where(h => h.userId == c.userId)
                                    .Select(h => h.id)
                                    .Contains(o.houseId)).SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                        .Where(o => o.UserId != c.userId)
                                        .Count(),

                                        Average = db.OrdersEntities
                                             .Where(o => db.HouseEntities
                                             .Where(h => h.userId == c.userId)
                                             .Select(h => h.id)
                                             .Contains(o.houseId))
                                             .SelectMany(o => o.orderRatings.Where(r => r.orderId == o.id))
                                             .Where(o => o.UserId != c.userId)
                                             .Select(r => (double)r.Rating)
                                             .DefaultIfEmpty(0)
                                             .Average()
                                    },
                                }).FirstOrDefault(),
                            }).ToList();
                            return Content(HttpStatusCode.OK, result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }


        }
    }
}
