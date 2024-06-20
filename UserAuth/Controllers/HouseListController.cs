using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.UserEnumList;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class HouseListController : ApiController
    {
        private static readonly string GoogleMapsApiKey = "AIzaSyDJ4Dpw3mS88UWh2HpRDAwZm5IcOB5w4gM"; // 替換為你的 Google Maps API 金鑰

        /// <summary>
        /// [FCO-1]取得系統推薦的房源列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/house/common/list")]
        public IHttpActionResult GetHomePageHouse()
        {
            DBModel db = new DBModel();

            var query = db.HouseEntities.Where(h => h.status == (statusType)10 && h.isRentSubsidy && h.isCookAllowed && h.isPetAllowed && h.isSTRAllowed)

                .Select(h => new
                {
                    Id = h.id,
                    image = h.HouseImgs.Where(hi => hi.isCover == true).Select(da => new
                    {
                        imageId = da.id,
                        imagePath = da.path,
                        isCover = da.isCover,
                    }).FirstOrDefault(),
                    title = h.name,
                    city = h.city,
                    district = h.district,
                    road = h.road,
                    lane = h.lane,
                    alley = h.alley,
                    number = h.number,
                    floor = h.floor,
                    floorTotal = h.floorTotal,
                    type = h.type,
                    ping = h.ping,
                    roomNumber = h.roomNumbers,
                    livingRoomNumbers = h.livingRoomNumbers,
                    bathRoomNumbers = h.bathRoomNumbers,
                    balconyNumbers = h.balconyNumbers,
                    rent = h.rent,
                    isRentSubsidy = h.isRentSubsidy,
                    isCookAllowd = h.isCookAllowed,
                    isPetAllowd = h.isPetAllowed,
                    isSTRAllowed = h.isSTRAllowed,
                }); ;
            // 隨機性:使用OrderBy Guid
            var filteredHouses = query.ToList().OrderBy(h => Guid.NewGuid()).Take(8).ToList();
            if (filteredHouses.Count != 8)
            {
                var additionalHouses = db.HouseEntities

                    .Where(h => (h.status == (statusType)10 && h.isRentSubsidy && h.isCookAllowed) || (h.status == (statusType)10 && h.isPetAllowed && h.isSTRAllowed) || (h.status == (statusType)10 && h.isRentSubsidy && h.isPetAllowed) || h.status == (statusType)10 && h.isRentSubsidy || h.status == (statusType)10 && h.isCookAllowed || h.status == (statusType)10 && h.isPetAllowed || h.status == (statusType)10 && h.isSTRAllowed)

                    .Select(h => new
                    {
                        Id = h.id,
                        image = h.HouseImgs.Where(hi => hi.isCover == true).Select(da => new
                        {
                            imageId = da.id,
                            imagePath = da.path,
                            isCover = da.isCover,
                        }).FirstOrDefault(),
                        title = h.name,
                        city = h.city,
                        district = h.district,
                        road = h.road,
                        lane = h.lane,
                        alley = h.alley,
                        number = h.number,
                        floor = h.floor,
                        floorTotal = h.floorTotal,
                        type = h.type,
                        ping = h.ping,
                        roomNumber = h.roomNumbers,
                        livingRoomNumbers = h.livingRoomNumbers,
                        bathRoomNumbers = h.bathRoomNumbers,
                        balconyNumbers = h.balconyNumbers,
                        rent = h.rent,
                        isRentSubsidy = h.isRentSubsidy,
                        isCookAllowd = h.isCookAllowed,
                        isPetAllowd = h.isPetAllowed,
                        isSTRAllowed = h.isSTRAllowed,
                    });
                filteredHouses.AddRange(additionalHouses);
                // 兩次結果相加後，再篩選 (為了讓符合全條件的物件優先被找到)
                // 隨機性:使用OrderBy Guid, 獨特性:使用Groupby Id
                var combinedHouses = filteredHouses.OrderBy(h => Guid.NewGuid()).GroupBy(h => h.Id).Select(group => group.FirstOrDefault()).Take(8).ToList();
                if (combinedHouses.Count == 8)
                {
                    return Ok(combinedHouses);
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, combinedHouses);
                }
            }
            else
            {
                return Ok(filteredHouses);
            }
        }

        /// <summary>
        /// [FCO-2]取得搜尋結果的房源列表
        /// </summary>
        /// <param name="city"></param>
        /// <param name="districts"></param>
        /// <param name="pageNumber"></param>
        /// <param name="price"></param>
        /// <param name="type"></param>
        /// <param name="feature"></param>
        /// <param name="content"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("api/house/common/list/search")]
        public IHttpActionResult searchHouse(int city, string districts = null,
            int pageNumber = 1, string price = null, string type = null, string feature = null, string content = "", string rating = "")
        {
            DBModel db = new DBModel();
            var query = db.HouseEntities.AsQueryable();

            // 篩選城市 (必填)
            var cityType = (CityType)city;

            // 刊登中 => 10
            query = query.Where(h => h.city == cityType && h.status == (statusType)10);

            // -----非必填條件，判斷篩選----

            // 1.區域 (Districts)
            if (!string.IsNullOrEmpty(districts))
            {
                // 將逗號分隔的districts字符串轉換為整數列表
                var districtList = districts.Split(',')
                                            .Select(int.Parse)
                                            .Select(d => (DistrictType)d)
                                            .ToList();

                // 使用Contains方法來篩選符合districtList中的區域的房屋 && 篩選刊登中房源(10)
                query = query.Where(h => districtList.Contains(h.district));
            }

            // 2.房間類別
            if (!string.IsNullOrEmpty(type))
            {
                var typeList = type.Split(',').Select(int.Parse).Select(d => (type)d).ToList();
                query = query.Where(h => typeList.Contains(h.type));
            }

            // 3. 價格(Price) Note: 最大值需輸入-1
            if (!string.IsNullOrEmpty(price))
            {
                var range = price.Split(',');
                var minValue = new List<int>();
                var maxValue = new List<int>();

                foreach (var rangePiece in range)
                {
                    var bound = rangePiece.Split('_');
                    if (bound.Length == 2)
                    {
                        if (int.TryParse(bound[0], out int min))
                        {
                            minValue.Add(min);
                        }
                        int max;
                        if (bound[1] == "-1")
                        {
                            max = int.MaxValue;
                        }
                        else if (int.TryParse(bound[1], out max))
                        {
                            maxValue.Add(max);
                        }
                    }
                }
                if (minValue.Count > 0 && maxValue.Count > 0)
                {
                    int overallMin = minValue.Min();
                    int overallMax = maxValue.Max();

                    var houseList = query.ToList().Where(h =>
                    {
                        if (!string.IsNullOrEmpty(h.rent) && int.TryParse(h.rent, out int rent))
                        {
                            return rent >= overallMin && rent <= overallMax;
                        }
                        return false;
                    }).ToList();
                    query = houseList.AsQueryable();
                }
            }

            // 4.房間特色
            if (!string.IsNullOrEmpty(feature))
            {
                string[] featureList = feature.Split(',');
                int featureListLength = featureList.Length;
                for (int i = 0; i < featureListLength; i++)
                {
                    if (featureList[i] == "rentSubsidy")
                    {
                        query = query.Where(h => h.isRentSubsidy == true);
                    }
                    if (featureList[i] == "petAllowed")
                    {
                        query = query.Where(h => h.isPetAllowed == true);
                    }
                    if (featureList[i] == "cookAllowed")
                    {
                        query = query.Where(h => h.isCookAllowed == true);
                    }
                    if (featureList[i] == "STRAllowd")
                    {
                        query = query.Where(h => h.isSTRAllowed == true);
                    }
                }
            };

            // 5. 內容 (Content)
            if (!string.IsNullOrEmpty(content))
            {
                query = query.Where(h => h.name != null && h.name.Contains(content));
            }

            // 6. 評分 (Rating)
            if (!string.IsNullOrEmpty(rating))
            {
                // 將逗號分隔的rating字符串轉換為整數列表
                var ratings = rating.Split(',').Select(int.Parse).ToList();

                // 取出最小的評分值
                int rate = ratings.Min();

                // 篩選房東
                var role1Users = db.UserEntities.Where(u => u.role.ToString() == "房東").Select(u => u.Id);

                // 獲取這些用戶的房子
                var houses = db.HouseEntities.Where(h => role1Users.Contains(h.userId));

                // 獲取房子的訂單及其評分
                var houseRatings = from h in houses
                                   join o in db.OrdersEntities on h.id equals o.houseId
                                   join r in db.OrdersRatingEntities on o.id equals r.orderId
                                   where !role1Users.Contains(r.UserId)
                                   select new { h.userId, r.Rating };

                // 計算每個房東的平均評分
                var userAverageRatings = from hr in houseRatings
                                         group hr by hr.userId into userGroup
                                         select new
                                         {
                                             userId = userGroup.Key,
                                             AverageRating = userGroup.Average(x => x.Rating)
                                         };

                // 篩取平均評分
                var filteredUserIds = userAverageRatings.Where(u => u.AverageRating >= rate).Select(u => u.userId);

                // 獲取符合條件的房子
                query = query.Where(h => filteredUserIds.Contains(h.userId));
            }

            // ----結束條件判斷----
            int totalCount = query.Count();

            //初始點

            // 一頁12筆資料
            int init = (pageNumber - 1) * 12;

            //使用skip前，需先排序
            var paginatedQuery = query.OrderByDescending(h => h.CreateAt).Skip(init).Take(12);

            //選取並返回篩選後的結果
            var result = paginatedQuery.Select(h => new
            {
                houseId = h.id,
                landlordId = h.userId,
                landlordFirstName = h.userIdFK.firstName,
                landlordlastName = h.userIdFK.lastName,
                landlordgender = h.userIdFK.gender,
                landlordDisplayName = h.userIdFK.lastName + (h.userIdFK.gender == 0 ? "先生" : "小姐"),

                // 列舉所有data
                //ratingsData = db.OrdersEntities
                //    .Where(o => db.HouseEntities
                //        .Where(house => house.userId == h.userId)
                //        .Select(house => house.id)
                //        .Contains(o.houseId))
                //    .SelectMany(o => o.orderRatings)
                //    .Select(r => r.Rating)
                //    .DefaultIfEmpty(0) // Handle case when there are no ratings
                //    .ToList(),

                ratingDetails = new
                {
                    Sum = db.OrdersEntities
                     .Where(o => db.HouseEntities
                         .Where(house => house.userId == h.userId) //House表格中找屬於房東的所有房子
                         .Select(house => house.id) //選擇這些house id
                         .Contains(o.houseId)) // 篩選:檢查 OrderEntities中訂單的houseId是否在上述id清單中，若是則保留該訂單
                     .SelectMany(o => o.orderRatings)
                     .Where(z => z.UserId != h.userId)
                     .Select(r => r.Rating)
                     .DefaultIfEmpty(0)
                     .Sum(),

                    Count = db.OrdersEntities
                      .Where(o => db.HouseEntities
                          .Where(house => house.userId == h.userId)
                          .Select(house => house.id)
                          .Contains(o.houseId))
                      .SelectMany(o => o.orderRatings)
                      .Where(z => z.UserId != h.userId)
                      .Count(),

                    AverageRating = db.OrdersEntities
                     .Where(o => db.HouseEntities
                         .Where(house => house.userId == h.userId) //House表格中找屬於房東的所有房子
                         .Select(house => house.id) //選擇這些house id
                         .Contains(o.houseId)) // 篩選:檢查 OrderEntities中訂單的houseId是否在上述id清單中，若是則保留該訂單
                     .SelectMany(o => o.orderRatings)
                    .Where(z => z.UserId != h.userId)
                     .Select(r => (double)r.Rating)
                     .DefaultIfEmpty(0).Average()
                },

                image = db.HouseImgsEntities.Where(z => z.houseId == h.id && z.isCover == true).Select(s => new
                {
                    imgId = s.id,
                    coverIamgePath = s.path
                }),
                title = h.name,
                city = h.city,
                district = h.district,
                road = h.road,
                lane = h.lane,
                alley = h.alley,
                number = h.number,
                parkingSpaceNumbers = h.parkingSpaceNumbers,
                floor = h.floor,
                floorTotal = h.floorTotal,
                type = h.type,
                ping = h.ping,
                roomNumber = h.roomNumbers,
                livingRoomNumbers = h.livingRoomNumbers,
                bathRoomNumbers = h.bathRoomNumbers,
                balconyNumbers = h.balconyNumbers,
                rent = h.rent,
                isRentSubsidy = h.isRentSubsidy,
                isCookAllowd = h.isCookAllowed,
                isPetAllowd = h.isPetAllowed,
                isSTRAllowed = h.isSTRAllowed,
            }).ToList();

            var response = new
            {
                Houses = result,
            };
            return Content(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [Route("api/house/common/totalNumber")]
        public IHttpActionResult SearchHouseTotal(int city, string districts = null, string price = null, string type = null, string feature = null, string content = "")
        {
            DBModel db = new DBModel();
            var query = db.HouseEntities.AsQueryable();

            // 篩選城市 (必填)
            var cityType = (CityType)city;

            // 刊登中 => 10
            query = query.Where(h => h.city == cityType && h.status == (statusType)10);

            // -----非必填條件，判斷篩選----

            // 1.區域 (Districts)
            if (!string.IsNullOrEmpty(districts))
            {
                // 將逗號分隔的districts字符串轉換為整數列表
                var districtList = districts.Split(',')
                                            .Select(int.Parse)
                                            .Select(d => (DistrictType)d)
                                            .ToList();

                // 使用Contains方法來篩選符合districtList中的區域的房屋 && 篩選刊登中房源(10)
                query = query.Where(h => districtList.Contains(h.district));
            }

            // 2.房間類別
            if (!string.IsNullOrEmpty(type))
            {
                var typeList = type.Split(',').Select(int.Parse).Select(d => (type)d).ToList();
                query = query.Where(h => typeList.Contains(h.type));
            }

            // 3. 價格(Price) Note: 最大值需輸入-1
            if (!string.IsNullOrEmpty(price))
            {
                var range = price.Split(',');
                var minValue = new List<int>();
                var maxValue = new List<int>();

                foreach (var rangePiece in range)
                {
                    var bound = rangePiece.Split('_');
                    if (bound.Length == 2)
                    {
                        if (int.TryParse(bound[0], out int min))
                        {
                            minValue.Add(min);
                        }
                        int max;
                        if (bound[1] == "-1")
                        {
                            max = int.MaxValue;
                        }
                        else if (int.TryParse(bound[1], out max))
                        {
                            maxValue.Add(max);
                        }
                    }
                }
                if (minValue.Count > 0 && maxValue.Count > 0)
                {
                    int overallMin = minValue.Min();
                    int overallMax = maxValue.Max();

                    var houseList = query.ToList().Where(h =>
                    {
                        if (!string.IsNullOrEmpty(h.rent) && int.TryParse(h.rent, out int rent))
                        {
                            return rent >= overallMin && rent <= overallMax;
                        }
                        return false;
                    }).ToList();
                    query = houseList.AsQueryable();
                }
            }

            // 4.房間特色
            if (!string.IsNullOrEmpty(feature))
            {
                string[] featureList = feature.Split(',');
                int featureListLength = featureList.Length;
                for (int i = 0; i < featureListLength; i++)
                {
                    if (featureList[i] == "rentSubsidy")
                    {
                        query = query.Where(h => h.isRentSubsidy == true);
                    }
                    if (featureList[i] == "petAllowed")
                    {
                        query = query.Where(h => h.isPetAllowed == true);
                    }
                    if (featureList[i] == "cookAllowed")
                    {
                        query = query.Where(h => h.isCookAllowed == true);
                    }
                    if (featureList[i] == "STRAllowd")
                    {
                        query = query.Where(h => h.isSTRAllowed == true);
                    }
                }
            };

            // 5. 內容 (Content)
            if (!string.IsNullOrEmpty(content))
            {
                query = query.Where(h => h.name != null && h.name.Contains(content));
            }

            // ----結束條件判斷----
            int totalCount = query.Count();
            var result = new
            {
                totalNumber = totalCount
            };
            return Content(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// [FCO-3]取得單一房源內容
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/house/common/info/{id}")]
        public IHttpActionResult GetHouseInfo(int id)
        {
            // 檢查是否傳入了 Token
            var authHeader = Request.Headers.Authorization;
            bool hasToken = authHeader != null && authHeader.Scheme == "Bearer";

            // 定義 UserId 變數
            int? UserId = null;
            UserRoleType? UserRole = null;
            // 如果有 Token，解析 Token 並取得 UserId
            if (hasToken)
            {
                try
                {
                    var jwtObject = JwtAuthFilters.GetToken(authHeader.Parameter);
                    UserId = (int)jwtObject["Id"];
                    UserRole = (UserRoleType)jwtObject["Role"];
                }
                catch
                {
                    // Token 解析失敗的處理邏輯
                    //return Content(HttpStatusCode.Unauthorized, "無效的 Token");
                }
            }
            try
            {
                using (DBModel db = new DBModel())
                {
                    //預約
                    var queryOfAppointment = from appointment in db.AppointmentsEntities.AsQueryable()
                                             where appointment.houseId == id && appointment.isValid == true && appointment.userId == UserId
                                             select appointment;
                    //try
                    var queryOfTry = from house in db.HouseEntities.AsQueryable()
                                     where house.id == id && house.status == statusType.刊登中
                                     join user in db.UserEntities on house.userId equals user.Id
                                     select new
                                     {
                                         house,
                                         landlord = user,
                                         rating = (from h in db.HouseEntities
                                                   where h.userId == user.Id
                                                   join o in db.OrdersEntities on h.id equals o.houseId
                                                   join or in db.OrdersRatingEntities on o.id equals or.orderId
                                                   where or.UserId != user.Id
                                                   join u in db.UserEntities on or.UserId equals u.Id
                                                   select new
                                                   {
                                                       h,
                                                       o,
                                                       or,
                                                       u,
                                                       r = db.ReplyRatingEntities.FirstOrDefault(r => r.orderRatingId == or.id)
                                                   }).ToList()
                                     };
                    // 評價
                    //var queryOfRatingAndReplyForLandlord = from orderRating in db.OrdersRatingEntities.AsQueryable()
                    //                                       join user in db.UserEntities on orderRating.UserId equals user.Id
                    //                                       join order in db.OrdersEntities on orderRating.orderId equals order.id
                    //                                       join house in db.HouseEntities on order.houseId equals house.id
                    //                                       join replyRating in db.ReplyRatingEntities on orderRating.id equals replyRating.orderRatingId into replyRatingGroup
                    //                                       from replyRating in replyRatingGroup.DefaultIfEmpty()
                    //                                       where house.id == id
                    //                                       where house.status == statusType.刊登中
                    //                                       where orderRating.UserId != house.userId
                    //                                       select new
                    //                                       {
                    //                                           tenant = user,
                    //                                           house,
                    //                                           order,
                    //                                           rating = orderRating,
                    //                                           reply = replyRating,
                    //                                       };
                    //房源
                    //var query = from house in db.HouseEntities.AsQueryable()
                    //            join user in db.UserEntities on house.userId equals user.Id
                    //            join order in db.OrdersEntities on house.id equals order.houseId into orderGroup
                    //            from order in orderGroup.DefaultIfEmpty()
                    //            join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                    //            from orderRating in orderRatingGroup.DefaultIfEmpty()
                    //            join replyRating in db.ReplyRatingEntities on orderRating.id equals replyRating.orderRatingId into replyRatingGroup
                    //            from replyRating in replyRatingGroup.DefaultIfEmpty()
                    //            where house.id == id
                    //            where house.status == statusType.刊登中
                    //            where orderRating.UserId != house.userId
                    //            group orderRating by new { house, user } into grouped
                    //            select new
                    //            {
                    //                house = grouped.Key.house,
                    //                landlord = grouped.Key.user,
                    //                rating = grouped,
                    //            };
                    // 執行查詢並保存結果
                    //房源
                    var queryResult = queryOfTry.FirstOrDefault() ?? null;
                    if (queryResult == null)
                    {
                        return Content(HttpStatusCode.NotFound, "此房源不存在");
                    }
                    else
                    {
                        //預約
                        bool? anyAppointment = null;
                        if (UserId != null)
                        {
                            anyAppointment = queryOfAppointment.Any() ? false : true;
                            if (UserRole == UserRoleType.房東)
                            {
                                anyAppointment = false;
                            }
                        }
                        //評價
                        //var queryOfRatingAndReplyForLandlordResult = queryOfRatingAndReplyForLandlord.ToList();
                        var ratings = new List<object>();
                        if (queryResult.rating.Count() > 0)
                        {
                            foreach (var i in queryResult.rating)
                            {
                                ratings.Add(new
                                {
                                    tenantName = i.u.lastName,
                                    tenantGender = i.u.gender.ToString(),
                                    houseName = i.h.name,
                                    leaseStartTime = i.o.leaseStartTime,
                                    leaseEndTime = i.o.leaseEndTime,
                                    ratingScore = i.or.Rating,
                                    comment = i.or.Comment,
                                    landlordReply = i.r == null ? null : i.r.ReplyComment
                                });
                            }
                        }
                        var data = new
                        {
                            appointmentAvailable = anyAppointment,
                            photos = new
                            {
                                firstPic = queryResult.house.HouseImgs?
                                                        .Where(i => i.isCover)
                                                        .Select(i => i.path)
                                                        .FirstOrDefault(),
                                restOfPic = queryResult.house.HouseImgs?
                                                        .Select(i => i.path)
                                                        .ToList(),
                            },
                            name = queryResult.house.name,
                            features = new
                            {
                                isRentSubsidy = queryResult.house.isRentSubsidy,
                                isPetAllowed = queryResult.house.isPetAllowed, //寵物友善
                                isCookAllowed = queryResult.house.isCookAllowed, //可開伙
                                isSTRAllowed = queryResult.house.isSTRAllowed, //可短租
                            },
                            basicInfo = new
                            {
                                address = Enum.GetName(typeof(CityType), queryResult.house.city)
                                                    + Enum.GetName(typeof(DistrictType), queryResult.house.district).Remove(0, 3)
                                                    + queryResult.house.road,
                                ping = queryResult.house.ping,
                                floor = $"{queryResult.house.floor}/{queryResult.house.floorTotal}",
                                type = Enum.GetName(typeof(type), queryResult.house.type),
                                roomNumbers = queryResult.house.roomNumbers, //房
                                livingRoomNumbers = queryResult.house.livingRoomNumbers, //廳
                                bathRoomNumbers = queryResult.house.bathRoomNumbers, //衛浴
                                balconyNumbers = queryResult.house.balconyNumbers, //陽台
                                parkingSpaceNumbers = queryResult.house.parkingSpaceNumbers, //車位
                            },
                            description = queryResult.house.description,
                            facilities = new
                            {
                                lifeFunctions = new
                                {
                                    isNearByDepartmentStore = queryResult.house.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = queryResult.house.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = queryResult.house.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = queryResult.house.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = queryResult.house.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = queryResult.house.isNearByPark, //附近機能: 公園綠地
                                },
                                otherFeatures = new
                                {
                                    hasGarbageDisposal = queryResult.house.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = queryResult.house.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = queryResult.house.hasElevator, //有電梯
                                },
                                furnitures = new
                                {
                                    hasAirConditioner = queryResult.house.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = queryResult.house.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = queryResult.house.hasRefrigerator, //設備: 冰箱
                                    hasCloset = queryResult.house.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = queryResult.house.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = queryResult.house.hasWaterHeater, //設備: 熱水器
                                    hasInternet = queryResult.house.hasInternet, //設備: 網路
                                    hasBed = queryResult.house.hasBed, //設備: 床
                                    hasTV = queryResult.house.hasTV, //設備: 電視
                                },
                                transportation = new
                                {
                                    isNearMRT = queryResult.house.isNearMRT, //是否鄰近捷運
                                    kmAwayMRT = queryResult.house.kmAwayMRT, //鄰近捷運公里
                                    isNearLRT = queryResult.house.isNearLRT, //是否鄰近輕軌
                                    kmAwayLRT = queryResult.house.kmAwayLRT, //鄰近輕軌公里
                                    isNearBusStation = queryResult.house.isNearBusStation, //是否鄰近公車站
                                    kmAwayBusStation = queryResult.house.kmAwayBusStation, //鄰近公車站公里
                                    isNearHSR = queryResult.house.isNearHSR, //是否鄰近高鐵
                                    kmAwayHSR = queryResult.house.kmAwayHSR, //鄰近高鐵公里
                                    isNearTrainStation = queryResult.house.isNearTrainStation, //是否鄰近火車
                                    kmAwayTrainStation = queryResult.house.kmAwayTrainStation //鄰近火車公里
                                }
                            },
                            cost = new
                            {
                                waterBill = new
                                {
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), queryResult.house.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = queryResult.house.waterBillPerMonth, //水費每月價錢
                                },
                                electricBill = new
                                {
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), queryResult.house.electricBill), //電費計價方式 Enum
                                    electricBillPerDegree = queryResult.house.electricBillPerDegree, //電費每度元
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), queryResult.house.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                },
                                managementFee = new
                                {
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), queryResult.house.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = queryResult.house.managementFeePerMonth, //管理費每月價錢
                                }
                            },
                            price = new
                            {
                                rent = queryResult.house.rent,
                                securityDeposit = queryResult.house.securityDeposit
                            },
                            landlord = new
                            {
                                lastName = queryResult.landlord.lastName,
                                gender = Enum.GetName(typeof(UserSexType), queryResult.landlord.gender),
                                photo = queryResult.landlord.photo,
                                description = queryResult.landlord.userIntro,
                                ratingAvg = queryResult.rating.Count() > 0 ? queryResult.rating.Where(r => r != null).Average(r => (double?)r.or.Rating) : 0.0,
                                //ratingAvg = queryResult.rating.Where(gr => gr != null).Any() ? queryResult.rating.Average(gr => gr.Rating) : 0,
                                //queryResult.rating.Where(gr => gr != null).Average(gr => gr.Rating),
                                ratingCount = queryResult.rating.Count() > 0 ? queryResult.rating.Count() : 0,
                            },
                            ratings = ratings
                        };

                        var result = new
                        {
                            statusCode = 200,
                            status = "success",
                            message = "已成功回傳房源內容",
                            data = data
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

        /// <summary>
        /// [FCO-5] 取得地圖搜尋結果的房源列表
        /// </summary>
        /// <param name="houseinput"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("api/house/common/map/list")]
        public async Task<IHttpActionResult> searchMapHouse([FromBody] MapSearchHouse houseinput)
        {
            try
            {
                using (DBModel db = new DBModel())
                {
                    var query = db.HouseEntities.AsQueryable();

                    var houses = query.Where(a => a.status == (statusType)10);
                    var distances = new List<object>();

                    if (houseinput.distance == null || houseinput.distance == 0)
                    {
                        houseinput.distance = 1000;
                    }
                    if (houseinput.pageNumber == null || houseinput.pageNumber == 0)
                    {
                        houseinput.pageNumber = 1;
                    }

                    int itemsPerPage = 12;
                    int skipCount = (houseinput.pageNumber - 1) * itemsPerPage;
                    int addedCount = 0;

                    foreach (var house in houses)
                    {
                        if (house.latitude == null || house.longitude == null)
                        {
                            continue;
                        }
                        double latitude;
                        double longitude;

                        try
                        {
                            latitude = Convert.ToDouble(house.latitude);
                            longitude = Convert.ToDouble(house.longitude);
                        }
                        catch (FormatException)
                        {
                            continue;
                        }

                        double distance;
                        try
                        {
                            distance = await GetDistanceAsync(houseinput.latitude, houseinput.longitude, latitude, longitude);
                        }
                        catch (Exception)
                        {
                            // 如果距離有問題，則跳過該房屋
                            continue;
                        }

                        if (distance < houseinput.distance) // 只返回距離小於傳入參數的結果
                        {
                            if (skipCount > 0)
                            {
                                skipCount--;
                                continue;
                            }

                            distances.Add(new
                            {
                                houseId = house.id,
                                title = house.name,
                                city = house.city.ToString(),
                                district = house.district.ToString(),
                                latitude = house.latitude,
                                longitude = house.longitude,
                                isCookAllowed = house.isCookAllowed,
                                isPetAllowed = house.isPetAllowed,
                                isRentSubsidy = house.isRentSubsidy,
                                isSTRAllowed = house.isSTRAllowed,
                                road = house.road,
                                alley = house.alley,
                                number = house.number,
                                roomNumbers = house.roomNumbers,
                                livingRoomNumbers = house.livingRoomNumbers,
                                bathRoomNumbers = house.bathRoomNumbers,
                                balconyNumbers = house.balconyNumbers,
                                rent = house.rent,
                                type = house.type.ToString(),
                                floor = house.floor,
                                floorTotal = house.floorTotal,
                                ping = house.ping,
                                coverImage = house.HouseImgs.FirstOrDefault(a => a.isCover)?.path,
                                distance = distance + " 公尺",
                            });

                            addedCount++;
                            if (addedCount == itemsPerPage) // 只取當前頁的資料
                            {
                                break;
                            }
                        }
                    }
                    return Ok(distances);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static async Task<double> GetDistanceAsync(double lat1, double lng1, double lat2, double lng2)
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"https://maps.googleapis.com/maps/api/distancematrix/json?units=metric&origins={lat1},{lng1}&destinations={lat2},{lng2}&key={GoogleMapsApiKey}";
                var response = await client.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Error fetching distance from Google Maps API");
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var status = json["status"].ToString();
                if (status != "OK")
                {
                    throw new Exception($"Error fetching distance: {status}");
                }

                var element = json["rows"][0]["elements"][0];
                var elementStatus = element["status"].ToString();
                if (elementStatus != "OK")
                {
                    throw new Exception($"Error fetching distance element: {elementStatus}");
                }

                // 使用 "value" 來取得距離，確保單位為公尺
                var distance = element["distance"]["value"].ToObject<double>();
                return distance;
            }
        }

        [HttpPost]
        [Route("api/house/common/map/count")]
        public async Task<IHttpActionResult> GetHouseCount([FromBody] MapSearchHouse houseInput)
        {
            try
            {
                using (DBModel db = new DBModel())
                {
                    var query = db.HouseEntities.AsQueryable();

                    var houses = query.Where(a => a.status == (statusType)10);
                    if (houseInput.distance == null || houseInput.distance == 0)
                    {
                        houseInput.distance = 1000;
                    }
                    int houseCount = 0;

                    foreach (var house in houses)
                    {
                        if (house.latitude == null || house.longitude == null)
                        {
                            continue;
                        }
                        double latitude;
                        double longitude;

                        try
                        {
                            latitude = Convert.ToDouble(house.latitude);
                            longitude = Convert.ToDouble(house.longitude);
                        }
                        catch (FormatException)
                        {
                            continue;
                        }

                        double distance;
                        try
                        {
                            distance = await GetDistanceAsync(houseInput.latitude, houseInput.longitude, latitude, longitude);
                        }
                        catch (Exception)
                        {
                            // 如果距離有問題，則跳過該房屋
                            continue;
                        }
                        if (distance < houseInput.distance)
                        {
                            houseCount++;
                        }
                    }

                    return Ok(houseCount);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}