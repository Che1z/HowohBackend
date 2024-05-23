using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;

namespace UserAuth.Controllers
{
    public class HouseListController : ApiController
    {
        /// <summary>
        ///
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
                    image = h.HouseImgs,
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
            // 隨機性:使用OrderBy Guid
            var filteredHouses = query.ToList().OrderBy(h => Guid.NewGuid()).Take(8).ToList();
            if (filteredHouses.Count != 8)
            {
                var additionalHouses = db.HouseEntities

                    .Where(h => (h.status == (statusType)10 && h.isRentSubsidy && h.isCookAllowed) || (h.status == (statusType)10 && h.isPetAllowed && h.isSTRAllowed) || (h.status == (statusType)10 && h.isRentSubsidy && h.isPetAllowed) || h.status == (statusType)10 && h.isRentSubsidy || h.status == (statusType)10 && h.isCookAllowed || h.status == (statusType)10 && h.isPetAllowed || h.status == (statusType)10 && h.isSTRAllowed)

                    .Select(h => new
                    {
                        Id = h.id,
                        image = h.HouseImgs,
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

        [HttpGet]
        [Route("api/house/common/list/search")]
        public IHttpActionResult SearchHouse(int city, string districts = null,
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
                          .Where(house => house.userId == h.userId)
                          .Select(house => house.id)
                          .Contains(o.houseId))
                      .SelectMany(o => o.orderRatings)
                      .Count() == 0 ? 0 :
                        db.OrdersEntities
                     .Where(o => db.HouseEntities
                         .Where(house => house.userId == h.userId)
                         .Select(house => house.id)
                         .Contains(o.houseId))
                     .SelectMany(o => o.orderRatings)
                     .Select(r => r.Rating)
                     .DefaultIfEmpty(0)
                     .Sum() /
                        db.OrdersEntities
                      .Where(o => db.HouseEntities
                          .Where(house => house.userId == h.userId)
                          .Select(house => house.id)
                          .Contains(o.houseId))
                      .SelectMany(o => o.orderRatings)
                      .Count(),
                },

                image = h.HouseImgs,
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
    }
}