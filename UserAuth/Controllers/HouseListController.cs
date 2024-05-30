using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Routing;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.UserEnumList;

namespace UserAuth.Controllers
{
    public class HouseListController : ApiController
    {
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
            int pageNumber = 1, string price = null, string type = null, string feature = null, string content = "")
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
                     .Select(r => r.Rating)
                     .DefaultIfEmpty(0)
                     .Sum(),

                    Count = db.OrdersEntities
                      .Where(o => db.HouseEntities
                          .Where(house => house.userId == h.userId)
                          .Select(house => house.id)
                          .Contains(o.houseId))
                      .SelectMany(o => o.orderRatings)
                      .Count(),

                    AverageRating = db.OrdersEntities
                     .Where(o => db.HouseEntities
                         .Where(house => house.userId == h.userId) //House表格中找屬於房東的所有房子
                         .Select(house => house.id) //選擇這些house id
                         .Contains(o.houseId)) // 篩選:檢查 OrderEntities中訂單的houseId是否在上述id清單中，若是則保留該訂單
                     .SelectMany(o => o.orderRatings)
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
            try
            {
                using (DBModel db = new DBModel())
                {
                    var houseEnter = db.HouseEntities.FirstOrDefault(x => x.id != id);
                    //var houseOwner = db.UserEntities.FirstOrDefault(x => x.Id == houseEnter.userId);
                    //house id list
                    //var housesOflandlord = db.HouseEntities.Where(x => x.userId == houseOwner.Id).Select(x => x.id).ToList();
                    //order id list
                    //var ordersOfHouses = db.OrdersEntities.Where(x => housesOflandlord.Contains(x.houseId)).Select(x => x.id).ToList();
                    //rating list by others
                    //var ratingsFromtenant = db.OrdersRatingEntities.Where(x => ordersOfHouses.Contains(x.orderId) && x.UserId != houseOwner.Id).ToList();
                    if (houseEnter == null)
                    {
                        throw new Exception("此房源不存在");
                    }
                    if (houseEnter.status != statusType.刊登中)
                    {
                        throw new Exception("此房源狀態非刊登中，不可查看");
                    }
                    var imgsOfHouse = db.HouseImgsEntities.Where(x => x.houseId == id).ToList();
                    //找到首圖的item
                    var firstPicture = imgsOfHouse.FirstOrDefault(x => x.isCover == true);
                    //找到非首圖的items
                    var restOfPicture = imgsOfHouse.Where(x => x.isCover == false).ToList();
                    List<string> restOfPicsList = new List<string>();
                    foreach (var h in restOfPicture)
                    {
                        restOfPicsList.Add(h.path);
                    }
                    // 房東資訊
                    var queryOfLandlord = from house in db.HouseEntities.AsQueryable()
                                          join user in db.UserEntities on house.userId equals user.Id
                                          select new
                                          {
                                              lastName = user.lastName,
                                              gender = user.gender,
                                              photo = user.photo,
                                              userIntro = user.userIntro
                                          };
                    //計算房東評價則數及平均分數
                    var queryOfRatingForLandlord = from house in db.HouseEntities.AsQueryable()
                                                   join user in db.UserEntities on house.userId equals user.Id
                                                   join order in db.OrdersEntities on house.id equals order.houseId into orderGroup
                                                   from order in orderGroup.DefaultIfEmpty()
                                                   join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                                   from orderRating in orderRatingGroup.DefaultIfEmpty()
                                                   select new
                                                   {
                                                       orderRating
                                                   };
                    var resultOfQueryOfRatingForLandlord = queryOfRatingForLandlord.ToList();
                    // 計算 orderRating 的筆數
                    var orderRatingCount = resultOfQueryOfRatingForLandlord.Count(r => r.orderRating != null);
                    // 計算 rating 的總和和平均值
                    var totalRating = resultOfQueryOfRatingForLandlord.Where(r => r.orderRating != null).Sum(r => r.orderRating.Rating);
                    var averageRating = orderRatingCount > 0 ? (double)totalRating / orderRatingCount : 0;

                    // 評價
                    var queryOfRatingAndReplyForLandlord = from orderRating in db.OrdersRatingEntities.AsQueryable()
                                                           join user in db.UserEntities on orderRating.UserId equals user.Id
                                                           join order in db.OrdersEntities on orderRating.orderId equals order.id
                                                           join house in db.HouseEntities on order.houseId equals house.id
                                                           join replyRating in db.ReplyRatingEntities on orderRating.id equals replyRating.orderRatingId into replyRatingGroup
                                                           from replyRating in replyRatingGroup.DefaultIfEmpty()
                                                           where house.id == id
                                                           where orderRating.UserId != house.userId
                                                           select new
                                                           {
                                                               tenantName = user.lastName,
                                                               tenantGender = Enum.GetName(typeof(UserSexType), user.gender),
                                                               houseName = house.name,
                                                               leaseStartTime = order.leaseStartTime,
                                                               leaseEndTime = order.leaseEndTime,
                                                               ratingScore = orderRating.Rating,
                                                               comment = orderRating.Comment,
                                                               landlordReply = replyRating.ReplyComment != null ? replyRating.ReplyComment : null
                                                           };
                    //var result = queryOfRatingAndReplyForLandlord.ToList();
                    //var ratingList = result.Select(r => new
                    //{
                    //    tenantName = r.tenantName,
                    //    tenantGender = Enum.GetName(typeof(UserSexType), r.tenantGender),
                    //    houseName = r.houseName,
                    //    leaseStartTime = r.leaseStartTime,
                    //    leaseEndTime = r.leaseEndTime,
                    //    ratingScore = r.ratingScore ?? 0, // 如果 ratingScore 為空，設置默認值 5
                    //    comment = r.comment,
                    //    landlordReply = r.landlordReply
                    //}).ToList();
                    var query = from house in db.HouseEntities.AsQueryable()
                                join user in db.UserEntities on house.userId equals user.Id
                                join order in db.OrdersEntities on house.id equals order.houseId into orderGroup
                                from order in orderGroup.DefaultIfEmpty()
                                join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                from orderRating in orderRatingGroup.DefaultIfEmpty()
                                join replyRating in db.ReplyRatingEntities on orderRating.id equals replyRating.orderRatingId into replyRatingGroup
                                from replyRating in replyRatingGroup.DefaultIfEmpty()
                                where house.id == id
                                where orderRating.UserId != house.userId
                                group orderRating by new { house, user } into grouped
                                select new
                                {
                                    photos = new
                                    {
                                        firstPic = grouped.Key.house.HouseImgs
                                                    .Where(i => i.isCover)
                                                    .Select(i => i.path)
                                                    .FirstOrDefault(),
                                        restOfPic = grouped.Key.house.HouseImgs
                                                    .Select(i => i.path)
                                                    .ToList(),
                                    },
                                    name = grouped.Key.house.name,
                                    features = new
                                    {
                                        isRentSubsidy = grouped.Key.house.isRentSubsidy,
                                        isPetAllowed = grouped.Key.house.isPetAllowed, //寵物友善
                                        isCookAllowed = grouped.Key.house.isCookAllowed, //可開伙
                                        isSTRAllowed = grouped.Key.house.isSTRAllowed, //可短租
                                    },
                                    basicInfo = new
                                    {
                                        address = Enum.GetName(typeof(CityType), grouped.Key.house.city)
                                                + Enum.GetName(typeof(DistrictType), grouped.Key.house.district).Remove(0, 3)
                                                + grouped.Key.house.road,
                                        ping = grouped.Key.house.ping,
                                        floor = $"{grouped.Key.house.floor}/{grouped.Key.house.floorTotal}",
                                        type = Enum.GetName(typeof(type), grouped.Key.house.type),
                                        roomNumbers = grouped.Key.house.roomNumbers, //房
                                        livingRoomNumbers = grouped.Key.house.livingRoomNumbers, //廳
                                        bathRoomNumbers = grouped.Key.house.bathRoomNumbers, //衛浴
                                        balconyNumbers = grouped.Key.house.balconyNumbers, //陽台
                                        parkingSpaceNumbers = grouped.Key.house.parkingSpaceNumbers, //車位
                                    },
                                    description = grouped.Key.house.description,
                                    facilities = new
                                    {
                                        lifeFunctions = new
                                        {
                                            isNearByDepartmentStore = grouped.Key.house.isNearByDepartmentStore, //附近機能: 百貨商場
                                            isNearBySchool = grouped.Key.house.isNearBySchool, //附近機能: 學校
                                            isNearByMorningMarket = grouped.Key.house.isNearByMorningMarket, //附近機能: 早市
                                            isNearByNightMarket = grouped.Key.house.isNearByNightMarket, //附近機能: 夜市
                                            isNearByConvenientStore = grouped.Key.house.isNearByConvenientStore, //附近機能: 超商
                                            isNearByPark = grouped.Key.house.isNearByPark, //附近機能: 公園綠地
                                        },
                                        otherFeatures = new
                                        {
                                            hasGarbageDisposal = grouped.Key.house.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                            hasWindowInBathroom = grouped.Key.house.hasWindowInBathroom, //屋源特色: 浴室開窗
                                            hasElevator = grouped.Key.house.hasElevator, //有電梯
                                        },
                                        furnitures = new
                                        {
                                            hasAirConditioner = grouped.Key.house.hasAirConditioner, //設備: 冷氣
                                            hasWashingMachine = grouped.Key.house.hasWashingMachine, //設備: 洗衣機
                                            hasRefrigerator = grouped.Key.house.hasRefrigerator, //設備: 冰箱
                                            hasCloset = grouped.Key.house.hasCloset, //設備: 衣櫃
                                            hasTableAndChair = grouped.Key.house.hasTableAndChair, //設備: 桌椅
                                            hasWaterHeater = grouped.Key.house.hasWaterHeater, //設備: 熱水器
                                            hasInternet = grouped.Key.house.hasInternet, //設備: 網路
                                            hasBed = grouped.Key.house.hasBed, //設備: 床
                                            hasTV = grouped.Key.house.hasTV, //設備: 電視
                                        }
                                    },
                                    cost = new
                                    {
                                        waterBill = new
                                        {
                                            paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), grouped.Key.house.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                            waterBillPerMonth = grouped.Key.house.waterBillPerMonth, //水費每月價錢
                                        },
                                        electricBill = new
                                        {
                                            electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), grouped.Key.house.electricBill), //電費計價方式 Enum
                                            paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), grouped.Key.house.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                        },
                                        managementFee = new
                                        {
                                            paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), grouped.Key.house.paymentMethodOfManagementFee), //管理費方式 Enum
                                            managementFeePerMonth = grouped.Key.house.managementFeePerMonth, //管理費每月價錢
                                        }
                                    },
                                    price = new
                                    {
                                        rent = grouped.Key.house.rent,
                                        securityDeposit = grouped.Key.house.securityDeposit
                                    },
                                    landlord = new
                                    {
                                        lastName = grouped.Key.user.lastName,
                                        gender = Enum.GetName(typeof(UserSexType), grouped.Key.user.gender),
                                        photo = grouped.Key.user.photo,
                                        description = grouped.Key.user.userIntro,
                                        ratingAvg = grouped.Where(gr => gr != null).Average(gr => gr.Rating),
                                        ratingCount = grouped.Count(gr => gr != null),
                                    },
                                    rating = queryOfRatingAndReplyForLandlord.ToList()
                                };

                    //var data1 = query.Select(h => new
                    //{
                    //    photos = new
                    //    {
                    //        firstPic = db.HouseImgsEntities.Where(i => i.houseId == h.id && i.isCover == true).Select(i => i.path).FirstOrDefault(),
                    //        restOfPic = db.HouseImgsEntities.Where(i => i.houseId == h.id).Select(i => i.path).ToList()
                    //    },
                    //    name = h.name,
                    //    features = new
                    //    {
                    //        isRentSubsidy = h.isRentSubsidy, //可申請租屋補助
                    //        isPetAllowed = h.isPetAllowed, //寵物友善
                    //        isCookAllowed = h.isCookAllowed, //可開伙
                    //        isSTRAllowed = h.isSTRAllowed, //可短租
                    //    },
                    //    basicInfo = new
                    //    {
                    //        address = Enum.GetName(typeof(CityType), h.city)
                    //             + Enum.GetName(typeof(DistrictType), h.district).Remove(0, 3)
                    //             + h.road,
                    //        ping = h.ping,
                    //        floor = $"{h.floor}/{h.floorTotal}",
                    //        type = Enum.GetName(typeof(type), h.type),
                    //        roomNumbers = h.roomNumbers, //房
                    //        livingRoomNumbers = h.livingRoomNumbers, //廳
                    //        bathRoomNumbers = h.bathRoomNumbers, //衛浴
                    //        balconyNumbers = h.balconyNumbers, //陽台
                    //        parkingSpaceNumbers = h.parkingSpaceNumbers, //車位
                    //    },
                    //    description = h.description,
                    //    facilities = new
                    //    {
                    //        lifeFunctions = new
                    //        {
                    //            isNearByDepartmentStore = h.isNearByDepartmentStore, //附近機能: 百貨商場
                    //            isNearBySchool = h.isNearBySchool, //附近機能: 學校
                    //            isNearByMorningMarket = h.isNearByMorningMarket, //附近機能: 早市
                    //            isNearByNightMarket = h.isNearByNightMarket, //附近機能: 夜市
                    //            isNearByConvenientStore = h.isNearByConvenientStore, //附近機能: 超商
                    //            isNearByPark = h.isNearByPark, //附近機能: 公園綠地
                    //        },
                    //        otherFeatures = new
                    //        {
                    //            hasGarbageDisposal = h.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                    //            hasWindowInBathroom = h.hasWindowInBathroom, //屋源特色: 浴室開窗
                    //            hasElevator = h.hasElevator, //有電梯
                    //        },
                    //        furnitures = new
                    //        {
                    //            hasAirConditioner = h.hasAirConditioner, //設備: 冷氣
                    //            hasWashingMachine = h.hasWashingMachine, //設備: 洗衣機
                    //            hasRefrigerator = h.hasRefrigerator, //設備: 冰箱
                    //            hasCloset = h.hasCloset, //設備: 衣櫃
                    //            hasTableAndChair = h.hasTableAndChair, //設備: 桌椅
                    //            hasWaterHeater = h.hasWaterHeater, //設備: 熱水器
                    //            hasInternet = h.hasInternet, //設備: 網路
                    //            hasBed = h.hasBed, //設備: 床
                    //            hasTV = h.hasTV, //設備: 電視
                    //        }
                    //    },
                    //    cost = new
                    //    {
                    //        waterBill = new
                    //        {
                    //            paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), h.paymentMethodOfWaterBill), //水費繳納方式 Enum
                    //            waterBillPerMonth = h.waterBillPerMonth, //水費每月價錢
                    //        },
                    //        electricBill = new
                    //        {
                    //            electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), h.electricBill), //電費計價方式 Enum
                    //            paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), h.paymentMethodOfElectricBill), //電費繳納方式 Enum
                    //        },
                    //        managementFee = new
                    //        {
                    //            paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), h.paymentMethodOfManagementFee), //管理費方式 Enum
                    //            managementFeePerMonth = h.managementFeePerMonth, //管理費每月價錢
                    //        }
                    //    },
                    //    price = new
                    //    {
                    //        rent = h.rent,
                    //        securityDeposit = h.securityDeposit
                    //    },
                    //    landlord =
                    //}).FirstOrDefault();
                    //var data = new
                    //{
                    //    photos = new
                    //    {
                    //        firstPic = firstPicture.path,
                    //        restOfPic = restOfPicsList
                    //    },
                    //    name = houseEnter.name,
                    //    features = new
                    //    {
                    //        isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                    //        isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                    //        isCookAllowed = houseEnter.isCookAllowed, //可開伙
                    //        isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                    //    },
                    //    basicInfo = new
                    //    {
                    //        address = Enum.GetName(typeof(CityType), houseEnter.city)
                    //             + Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3)
                    //             + houseEnter.road,
                    //        ping = houseEnter.ping,
                    //        floor = $"{houseEnter.floor}/{houseEnter.floorTotal}",
                    //        type = Enum.GetName(typeof(type), houseEnter.type),
                    //        roomNumbers = houseEnter.roomNumbers, //房
                    //        livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                    //        bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                    //        balconyNumbers = houseEnter.balconyNumbers, //陽台
                    //        parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                    //    },
                    //    description = houseEnter.description,
                    //    facilities = new
                    //    {
                    //        lifeFunctions = new
                    //        {
                    //            isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                    //            isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                    //            isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                    //            isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                    //            isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                    //            isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                    //        },
                    //        otherFeatures = new
                    //        {
                    //            hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                    //            hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                    //            hasElevator = houseEnter.hasElevator, //有電梯
                    //        },
                    //        furnitures = new
                    //        {
                    //            hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                    //            hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                    //            hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                    //            hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                    //            hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                    //            hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                    //            hasInternet = houseEnter.hasInternet, //設備: 網路
                    //            hasBed = houseEnter.hasBed, //設備: 床
                    //            hasTV = houseEnter.hasTV, //設備: 電視
                    //        }
                    //    },
                    //    cost = new
                    //    {
                    //        waterBill = new
                    //        {
                    //            paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                    //            waterBillPerMonth = houseEnter.waterBillPerMonth, //水費每月價錢
                    //        },
                    //        electricBill = new
                    //        {
                    //            electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                    //            paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                    //        },
                    //        managementFee = new
                    //        {
                    //            paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                    //            managementFeePerMonth = houseEnter.managementFeePerMonth, //管理費每月價錢
                    //        }
                    //    },
                    //    price = new
                    //    {
                    //        rent = houseEnter.rent,
                    //        securityDeposit = houseEnter.securityDeposit
                    //    },
                    //    landlord = queryOfLandlord.Select(l => new
                    //    {
                    //        lastName = l.lastName,
                    //        gender = Enum.GetName(typeof(UserSexType), l.gender),
                    //        photo = l.photo,
                    //        description = l.userIntro,
                    //        ratingAvg = averageRating,
                    //        ratingCount = orderRatingCount
                    //    })
                    //};
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功回傳房源內容",
                        data = query.FirstOrDefault()
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}