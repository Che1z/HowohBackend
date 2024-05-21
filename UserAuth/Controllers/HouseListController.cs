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
        [HttpGet]
        [Route("api/house/list")]
        public IHttpActionResult getHomePageHouse()
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
        [Route("api/house/list")]
        public IHttpActionResult searchHouse(int city, string districts = null, int pageNumber = 1)
        {
            DBModel db = new DBModel();
            var query = db.HouseEntities.AsQueryable();
            var cityType = (CityType)city;

            // 刊登中 = 10
            query = query.Where(h => h.city == cityType && h.status == (statusType)10);

            // -----開始條件判斷----

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

            // 2.

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
        [Route("api/myHouse/searchHouseTotal")]
        public IHttpActionResult searchHouseTotal(int city, string districts = null)
        {
            DBModel db = new DBModel();
            var query = db.HouseEntities.AsQueryable();
            var cityType = (CityType)city;


            query = query.Where(h => h.city == cityType);


            if (!string.IsNullOrEmpty(districts))
            {
                // 將逗號分隔的districts字符串轉換為整數列表
                var districtList = districts.Split(',')
                                            .Select(int.Parse)
                                            .Select(d => (DistrictType)d)
                                            .ToList();

                // 使用Contains方法來篩選符合districtList中的區域的房屋 && 篩選刊登中房源(10)

                query = query.Where(h => districtList.Contains(h.district) && h.status == (statusType)10);
            }


            int totalCount = query.Count();
            var result = new
            {
                totalNumber = totalCount
            };
            return Content(HttpStatusCode.OK, result);
        }


    }
}
