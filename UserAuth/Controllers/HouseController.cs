using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.OrderEnumList;
using UserAuth.Models.UserEnumList;
using UserAuth.Models.ViewModel;
using UserAuth.Security;
using HttpPatchAttribute = System.Web.Http.HttpPatchAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace UserAuth.Controllers
{
    public class HouseController : ApiController
    {
        //private DBModel db = new DBModel();
        /// <summary>
        /// [ALO-2]新增房源
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/house/landlord")]
        [JwtAuthFilters]
        public IHttpActionResult CreateListing()
        {
            //檢查是否為房東
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];

            try
            {
                if (UserRole != UserRoleType.房東)
                {
                    throw new Exception("該使用者不是房東，不可使用此功能");
                }
                using (DBModel db = new DBModel())
                {
                    //int houseUserId = house.userId;
                    //statusType houseStatus = house.status;
                    House InsertNewHouse = new House();
                    InsertNewHouse.userId = UserId;
                    InsertNewHouse.status = statusType.未完成步驟1;

                    db.HouseEntities.Add(InsertNewHouse);

                    db.SaveChanges();
                    var createHouse = db.HouseEntities.Where(x => x.userId == UserId).OrderByDescending(x => x.id).FirstOrDefault();

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功新增房源",
                        data = new
                        {
                            houseId = createHouse.id,
                        }
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-3]修改房源內容
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <param name="houseInput">房源內容</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("api/house/landlord/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult UpdateListing(int id, HouseInput houseInput)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];
            try
            {
                //檢查是否為房東
                if (UserRole != UserRoleType.房東)
                {
                    throw new Exception("該使用者不是房東，不可使用此功能");
                }
                if (!ModelState.IsValid || houseInput == null)
                {
                    throw new Exception("錯誤資訊不符合規範");
                }
                using (DBModel db = new DBModel())
                {
                    //int houseId = houseInput.id;

                    var updateHouse = db.HouseEntities.Where(x => x.id == id).FirstOrDefault();

                    //檢查該房源的房東是不是使用者
                    if (updateHouse.userId != UserId)
                    {
                        throw new Exception("該物件不屬於此使用者，不可修改房源內容");
                    }

                    string houseName = houseInput.name;
                    string houseCity = houseInput.city;
                    string houseDistrict = houseInput.city + houseInput.district;
                    string houseRoad = houseInput.road;
                    string houseLane = houseInput.lane;
                    string houseAlley = houseInput.alley;
                    string houseNumber = houseInput.number;
                    string houseFloor = houseInput.floor;
                    string houseFloorTotal = houseInput.floorTotal;
                    string ping = houseInput.ping;
                    string houseRoomNumbers = houseInput.roomNumbers;
                    string houseLivingRoomNumbers = houseInput.livingRoomNumbers;
                    string houseBathRoomNumbers = houseInput.bathRoomNumbers;
                    string houseBalconyNumbers = houseInput.balconyNumbers;
                    string houseParkingSpaceNumbers = houseInput.parkingSpaceNumbers;
                    string houseKmAwayMRT = houseInput.kmAwayMRT;
                    string houseKmAwayLRT = houseInput.kmAwayLRT;
                    string houseKmAwayBusStation = houseInput.kmAwayBusStation;
                    string houseKmAwayHSR = houseInput.kmAwayHSR;
                    string houseKmAwayTrainStation = houseInput.kmAwayTrainStation;
                    string houseWaterBillPerMonth = houseInput.waterBillPerMonth;
                    string houseManagementFeePerMonth = houseInput.managementFeePerMonth;
                    string houseRent = houseInput.rent;
                    string houseDescription = houseInput.description;
                    string houseJobRestriction = houseInput.jobRestriction;

                    CityType houseCityType;
                    DistrictType houseDistrictType;
                    UserJob userJob;

                    //存入房東的租客職業限制
                    if (houseJobRestriction != null)
                    {
                        string hjrOutput = "";
                        string[] hjr = houseJobRestriction.Split(',');
                        for (int i = 0; i < hjr.Length; i++)
                        {
                            hjr[i] = hjr[i].Trim(' ');
                            if (Enum.TryParse(hjr[i], out userJob))
                            {
                                int userJobToInt = (int)userJob;
                                hjrOutput += $"{userJobToInt.ToString()}, ";
                            }
                        }
                        char[] trimArr = { ',', ' ' };
                        hjrOutput = hjrOutput.Trim(trimArr);
                        updateHouse.jobRestriction = hjrOutput;
                    }

                    //存入縣市
                    if (houseCity != null)
                    {
                        if (Enum.TryParse(houseCity, out houseCityType))
                        {
                            updateHouse.city = houseCityType;
                        }
                        else
                        {
                            throw new Exception("該縣市不在系統中，請洽網站管理員");
                        }
                    }

                    //存入鄉鎮區域
                    if (houseInput.district != null)
                    {
                        if (Enum.TryParse(houseDistrict, out houseDistrictType))
                        {
                            updateHouse.district = houseDistrictType;
                        }
                        else
                        {
                            throw new Exception("該鄉鎮區域不在系統中，請洽網站管理員");
                        }
                    }

                    if (houseName != null)
                    {
                        updateHouse.name = houseName;
                    }
                    if (houseRoad != null)
                    {
                        updateHouse.road = houseRoad;
                    }
                    if (houseLane != null)
                    {
                        updateHouse.lane = houseLane;
                    }
                    if (houseAlley != null)
                    {
                        updateHouse.alley = houseAlley;
                    }
                    if (houseNumber != null)
                    {
                        updateHouse.number = houseNumber;
                    }
                    if (houseFloor != null)
                    {
                        updateHouse.floor = houseFloor;
                    }
                    if (houseFloorTotal != null)
                    {
                        updateHouse.floorTotal = houseFloorTotal;
                    }
                    if (ping != null)
                    {
                        updateHouse.ping = ping;
                    }
                    if (houseRoomNumbers != null)
                    {
                        updateHouse.roomNumbers = houseRoomNumbers;
                    }
                    if (houseInput.type.HasValue)
                    {
                        updateHouse.type = houseInput.type.Value;
                    }
                    if (houseLivingRoomNumbers != null)
                    {
                        updateHouse.livingRoomNumbers = houseLivingRoomNumbers;
                    }
                    if (houseBathRoomNumbers != null)
                    {
                        updateHouse.bathRoomNumbers = houseBathRoomNumbers;
                    }
                    if (houseBalconyNumbers != null)
                    {
                        updateHouse.balconyNumbers = houseBalconyNumbers;
                    }
                    if (houseParkingSpaceNumbers != null)
                    {
                        updateHouse.parkingSpaceNumbers = houseParkingSpaceNumbers;
                    }
                    if (houseInput.isRentSubsidy.HasValue)
                    {
                        updateHouse.isRentSubsidy = houseInput.isRentSubsidy.Value;
                    }
                    if (houseInput.isPetAllowed.HasValue)
                    {
                        updateHouse.isPetAllowed = houseInput.isPetAllowed.Value;
                    }
                    if (houseInput.isCookAllowed.HasValue)
                    {
                        updateHouse.isCookAllowed = houseInput.isCookAllowed.Value;
                    }
                    if (houseInput.isSTRAllowed.HasValue)
                    {
                        updateHouse.isSTRAllowed = houseInput.isSTRAllowed.Value;
                    }
                    if (houseInput.isNearByDepartmentStore.HasValue)
                    {
                        updateHouse.isNearByDepartmentStore = houseInput.isNearByDepartmentStore.Value;
                    }
                    if (houseInput.isNearBySchool.HasValue)
                    {
                        updateHouse.isNearBySchool = houseInput.isNearBySchool.Value;
                    }
                    if (houseInput.isNearByMorningMarket.HasValue)
                    {
                        updateHouse.isNearByMorningMarket = houseInput.isNearByMorningMarket.Value;
                    }
                    if (houseInput.isNearByNightMarket.HasValue)
                    {
                        updateHouse.isNearByNightMarket = houseInput.isNearByNightMarket.Value;
                    }
                    if (houseInput.isNearByConvenientStore.HasValue)
                    {
                        updateHouse.isNearByConvenientStore = houseInput.isNearByConvenientStore.Value;
                    }
                    if (houseInput.isNearByPark.HasValue)
                    {
                        updateHouse.isNearByPark = houseInput.isNearByPark.Value;
                    }
                    if (houseInput.hasGarbageDisposal.HasValue)
                    {
                        updateHouse.hasGarbageDisposal = houseInput.hasGarbageDisposal.Value;
                    }
                    if (houseInput.hasWindowInBathroom.HasValue)
                    {
                        updateHouse.hasWindowInBathroom = houseInput.hasWindowInBathroom.Value;
                    }
                    if (houseInput.hasElevator.HasValue)
                    {
                        updateHouse.hasElevator = houseInput.hasElevator.Value;
                    }
                    if (houseInput.isNearMRT.HasValue)
                    {
                        updateHouse.isNearMRT = houseInput.isNearMRT.Value;
                    }
                    if (houseKmAwayMRT != null)
                    {
                        updateHouse.kmAwayMRT = houseKmAwayMRT;
                    }
                    if (houseInput.isNearLRT.HasValue)
                    {
                        updateHouse.isNearLRT = houseInput.isNearLRT.Value;
                    }
                    if (houseKmAwayLRT != null)
                    {
                        updateHouse.kmAwayLRT = houseKmAwayLRT;
                    }
                    if (houseInput.isNearBusStation.HasValue)
                    {
                        updateHouse.isNearBusStation = houseInput.isNearBusStation.Value;
                    }
                    if (houseKmAwayBusStation != null)
                    {
                        updateHouse.kmAwayBusStation = houseKmAwayBusStation;
                    }
                    if (houseInput.isNearHSR.HasValue)
                    {
                        updateHouse.isNearHSR = houseInput.isNearHSR.Value;
                    }
                    if (houseKmAwayHSR != null)
                    {
                        updateHouse.kmAwayHSR = houseKmAwayHSR;
                    }
                    if (houseInput.isNearTrainStation.HasValue)
                    {
                        updateHouse.isNearTrainStation = houseInput.isNearTrainStation.Value;
                    }
                    if (houseKmAwayTrainStation != null)
                    {
                        updateHouse.kmAwayTrainStation = houseKmAwayTrainStation;
                    }
                    if (houseInput.hasAirConditioner.HasValue)
                    {
                        updateHouse.hasAirConditioner = houseInput.hasAirConditioner.Value;
                    }
                    if (houseInput.hasWashingMachine.HasValue)
                    {
                        updateHouse.hasWashingMachine = houseInput.hasWashingMachine.Value;
                    }
                    if (houseInput.hasRefrigerator.HasValue)
                    {
                        updateHouse.hasRefrigerator = houseInput.hasRefrigerator.Value;
                    }
                    if (houseInput.hasCloset.HasValue)
                    {
                        updateHouse.hasCloset = houseInput.hasCloset.Value;
                    }
                    if (houseInput.hasTableAndChair.HasValue)
                    {
                        updateHouse.hasTableAndChair = houseInput.hasTableAndChair.Value;
                    }
                    if (houseInput.hasWaterHeater.HasValue)
                    {
                        updateHouse.hasWaterHeater = houseInput.hasWaterHeater.Value;
                    }
                    if (houseInput.hasInternet.HasValue)
                    {
                        updateHouse.hasInternet = houseInput.hasInternet.Value;
                    }
                    if (houseInput.hasBed.HasValue)
                    {
                        updateHouse.hasBed = houseInput.hasBed.Value;
                    }
                    if (houseInput.hasTV.HasValue)
                    {
                        updateHouse.hasTV = houseInput.hasTV.Value;
                    }
                    if (houseInput.paymentMethodOfWaterBill.HasValue)
                    {
                        updateHouse.paymentMethodOfWaterBill = houseInput.paymentMethodOfWaterBill.Value;
                    }
                    if (houseWaterBillPerMonth != null)
                    {
                        updateHouse.waterBillPerMonth = houseWaterBillPerMonth;
                    }
                    if (houseInput.electricBill.HasValue)
                    {
                        updateHouse.electricBill = houseInput.electricBill.Value;
                    }
                    if (houseInput.electricBillPerDegree != null)
                    {
                        updateHouse.electricBillPerDegree = houseInput.electricBillPerDegree;
                    }
                    if (houseInput.paymentMethodOfElectricBill.HasValue)
                    {
                        updateHouse.paymentMethodOfElectricBill = houseInput.paymentMethodOfElectricBill.Value;
                    }
                    if (houseInput.paymentMethodOfManagementFee.HasValue)
                    {
                        updateHouse.paymentMethodOfManagementFee = houseInput.paymentMethodOfManagementFee.Value;
                    }
                    if (houseManagementFeePerMonth != null)
                    {
                        updateHouse.managementFeePerMonth = houseManagementFeePerMonth;
                    }
                    if (houseRent != null)
                    {
                        updateHouse.rent = houseRent;
                    }
                    if (houseInput.securityDeposit.HasValue)
                    {
                        updateHouse.securityDeposit = houseInput.securityDeposit.Value;
                    }
                    if (houseDescription != null)
                    {
                        updateHouse.description = houseDescription;
                    }
                    if (houseInput.hasTenantRestrictions.HasValue)
                    {
                        updateHouse.hasTenantRestrictions = houseInput.hasTenantRestrictions.Value;
                    }
                    if (houseInput.genderRestriction.HasValue)
                    {
                        updateHouse.genderRestriction = houseInput.genderRestriction.Value;
                    }
                    if (houseInput.status.HasValue)
                    {
                        if (houseInput.status.Value > updateHouse.status)
                        {
                            updateHouse.status = houseInput.status.Value;
                        }
                    }
                    //updateHouse.status = houseStatus;

                    db.SaveChanges();

                    return Content(HttpStatusCode.OK, "已成功修改房源");
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-4]新增房源照片
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <param name="houseImgInput">房源照片</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/house/landlord/img/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult CreatingHouseImg(int id, HouseImgInput houseImgInput)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            try
            {
                if (!ModelState.IsValid || houseImgInput == null)
                {
                    throw new Exception("錯誤資訊不符合規範");
                }
                using (DBModel db = new DBModel())
                {
                    //檢查有沒有舊照片，有的話先刪掉
                    var houseImgsOfUser = db.HouseImgsEntities.Where(x => x.houseId == id).ToList();
                    if (houseImgsOfUser != null)
                    {
                        foreach (var img in houseImgsOfUser)
                        {
                            db.HouseImgsEntities.Remove(img);
                        }
                        db.SaveChanges();
                    }

                    //新增照片
                    List<houseImgObject> newHouseImgsOfUser = houseImgInput.files;
                    foreach (var houseImgObject in newHouseImgsOfUser)
                    {
                        HouseImg InsertNewHouseImg = new HouseImg();
                        InsertNewHouseImg.houseId = id;
                        InsertNewHouseImg.path = houseImgObject.path;
                        InsertNewHouseImg.isCover = houseImgObject.isCover;

                        db.HouseImgsEntities.Add(InsertNewHouseImg);
                    }
                    db.SaveChanges();

                    //修改房源狀態
                    var updateHouse = db.HouseEntities.Where(x => x.id == id).FirstOrDefault();
                    updateHouse.status = houseImgInput.status;
                    db.SaveChanges();

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功新增房源照片",
                        //data = new
                        //{
                        //    //houseId = createHouse.id,
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

        /// <summary>
        /// [ALO-11]刪除房源
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/house/landlord/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult DeleteMyHouse(int id)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];

            try
            {
                using (DBModel db = new DBModel())
                {
                    var houseEnter = db.HouseEntities.Where(x => x.id == id).FirstOrDefault();
                    if (houseEnter == null)
                    {
                        throw new Exception("查無此房源");
                    }
                    if (houseEnter.userId == UserId)
                    {
                        var houseImgsOfUser = db.HouseImgsEntities.Where(x => x.houseId == id).ToList();
                        if (houseImgsOfUser != null)
                        {
                            foreach (var img in houseImgsOfUser)
                            {
                                db.HouseImgsEntities.Remove(img);
                            }
                            db.SaveChanges();
                        }
                        db.HouseEntities.Remove(houseEnter);
                        db.SaveChanges();

                        var result = new
                        {
                            statusCode = 200,
                            status = "success",
                            message = "已成功刪除房源",
                            //data = new
                            //{
                            //    //houseId = createHouse.id,
                            //}
                        };
                        return Content(HttpStatusCode.OK, result);
                    }
                    else
                    {
                        throw new Exception("該房源擁有者非使用者");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-12, ALO-13, ALO-14, ALO-15]取得各房源狀態的單一房源內容
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/house/landlord/info/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult GetMyHouseInfo(int id)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];

            try
            {
                using (DBModel db = new DBModel())
                {
                    var houseEnter = db.HouseEntities.Where(x => x.id == id).FirstOrDefault();
                    //檢查房源是否存在
                    if (houseEnter == null)
                    {
                        throw new Exception("查無此房源");
                    }

                    //檢查房源擁有者是否為使用者
                    if (houseEnter.userId == UserId)
                    {
                        //狀態為未完成步驟1及完成步驟1
                        if (houseEnter.status == statusType.未完成步驟1 || houseEnter.status == statusType.完成步驟1)
                        {
                            if (houseEnter.status == statusType.未完成步驟1)
                            {
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 基本資訊"
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 照片",
                                    data = data
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                        }
                        else
                        {   //狀態非步驟0或步驟1的一定有照片
                            var houseImgsOfUser = db.HouseImgsEntities.Where(x => x.houseId == id).ToList();

                            //找到首圖的item
                            var firstPicture = houseImgsOfUser.Where(x => x.isCover == true).FirstOrDefault();
                            //找到非首圖的items
                            var restOfPicture = houseImgsOfUser.Where(x => x.isCover == false).ToList();
                            List<string> restOfPicsList = new List<string>();
                            foreach (var h in restOfPicture)
                            {
                                restOfPicsList.Add(h.path);
                            }
                            //狀態為已完成或刊登中
                            if (houseEnter.status == statusType.已完成)
                            {
                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    pictures = pictureObject
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
                            //狀態為已承租
                            else if (houseEnter.status == statusType.已承租)
                            {
                                var today = DateTime.Now;
                                var order = db.OrdersEntities.FirstOrDefault(x => x.leaseStartTime <= today && x.leaseEndTime >= today);
                                var tenant = db.UserEntities.FirstOrDefault(x => x.Id == order.userId);
                                //租客過去的order list
                                var pastOrderOfTenant = db.OrdersEntities.Where(x => x.userId == order.userId).Select(x => x.id).ToList();
                                //別人對租客的評價list
                                var ratingsToTenant = db.OrdersRatingEntities.Where(x => pastOrderOfTenant.Contains(x.orderId) && x.UserId != order.userId).ToList();
                                //平均評價與評價則數
                                string ratingAvg = "新租客";
                                int ratingCount = 0;

                                if (ratingsToTenant.Count != 0)
                                {
                                    ratingCount = ratingsToTenant.Count;
                                    double ratingScore = 0;
                                    foreach (var rating in ratingsToTenant)
                                    {
                                        ratingScore += rating.Rating;
                                    }
                                    ratingAvg = Convert.ToString(ratingScore / ratingCount);
                                }

                                var tenantInfo = new
                                {
                                    leaseStartTime = order.leaseStartTime,
                                    leaseEndTime = order.leaseEndTime,
                                    name = tenant.lastName + tenant.firstName,
                                    gender = Enum.GetName(typeof(UserSexType), tenant.gender),
                                    job = Enum.GetName(typeof(UserJob), tenant.job),
                                    tel = tenant.telphone,
                                    description = tenant.userIntro,
                                    ratingCount = ratingCount,
                                    ratingAvg = ratingAvg
                                };

                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                //租客限制
                                string jobRestriction = "";
                                string[] jobRestrictions = houseEnter.jobRestriction.Split(',');

                                for (int i = 0; i < jobRestrictions.Length; i++)
                                {
                                    jobRestrictions[i] = jobRestrictions[i].Trim();
                                    jobRestriction += Enum.GetName(typeof(UserJob), Convert.ToInt32(jobRestrictions[i])) + ",";
                                }
                                char[] trimArr = { ',', ' ' };
                                jobRestriction = jobRestriction.Trim(trimArr);
                                var houseInfo = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                                    isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                                    isCookAllowed = houseEnter.isCookAllowed, //可開伙
                                    isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                                    isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                                    hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = houseEnter.hasElevator, //有電梯
                                    hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                                    hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                                    hasInternet = houseEnter.hasInternet, //設備: 網路
                                    hasBed = houseEnter.hasBed, //設備: 床
                                    hasTV = houseEnter.hasTV, //設備: 電視
                                    isNearMRT = houseEnter.isNearMRT, //交通: 捷運
                                    kmAwayMRT = houseEnter.kmAwayMRT, //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = houseEnter.kmAwayLRT, //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = houseEnter.kmAwayBusStation, //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = houseEnter.kmAwayHSR, //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = houseEnter.kmAwayTrainStation, //距離火車公里
                                    rent = houseEnter.rent, //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = houseEnter.waterBillPerMonth, //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    electricBillPerDegree = houseEnter.electricBillPerDegree,
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = houseEnter.managementFeePerMonth, //管理費每月價錢
                                    description = houseEnter.description, //房源介紹
                                    hasTenantRestrictions = houseEnter.hasTenantRestrictions.ToString().ToLower(), //是否有租客限制
                                    genderRestriction = Enum.GetName(typeof(genderRestrictionType), houseEnter.genderRestriction), //男or女or性別友善
                                    jobRestriction = jobRestriction, //排除職業

                                    pictures = pictureObject
                                };
                                var data = new
                                {
                                    tenantInfo = tenantInfo,
                                    houseInfo = houseInfo
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
                            else if (houseEnter.status == statusType.完成步驟2)
                            {
                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位

                                    pictures = pictureObject
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 設備設施",
                                    data = data
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else if (houseEnter.status == statusType.完成步驟3)
                            {
                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                                    isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                                    isCookAllowed = houseEnter.isCookAllowed, //可開伙
                                    isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                                    isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                                    hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = houseEnter.hasElevator, //有電梯
                                    hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                                    hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                                    hasInternet = houseEnter.hasInternet, //設備: 網路
                                    hasBed = houseEnter.hasBed, //設備: 床
                                    hasTV = houseEnter.hasTV, //設備: 電視
                                    isNearMRT = houseEnter.isNearMRT, //交通: 捷運
                                    kmAwayMRT = houseEnter.kmAwayMRT, //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = houseEnter.kmAwayLRT, //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = houseEnter.kmAwayBusStation, //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = houseEnter.kmAwayHSR, //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = houseEnter.kmAwayTrainStation, //距離火車公里

                                    pictures = pictureObject
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 費用",
                                    data = data
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else if (houseEnter.status == statusType.完成步驟4)
                            {
                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                                    isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                                    isCookAllowed = houseEnter.isCookAllowed, //可開伙
                                    isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                                    isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                                    hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = houseEnter.hasElevator, //有電梯
                                    hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                                    hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                                    hasInternet = houseEnter.hasInternet, //設備: 網路
                                    hasBed = houseEnter.hasBed, //設備: 床
                                    hasTV = houseEnter.hasTV, //設備: 電視
                                    isNearMRT = houseEnter.isNearMRT, //交通: 捷運
                                    kmAwayMRT = houseEnter.kmAwayMRT, //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = houseEnter.kmAwayLRT, //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = houseEnter.kmAwayBusStation, //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = houseEnter.kmAwayHSR, //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = houseEnter.kmAwayTrainStation, //距離火車公里
                                    rent = houseEnter.rent, //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = houseEnter.waterBillPerMonth, //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    electricBillPerDegree = houseEnter.electricBillPerDegree,
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = houseEnter.managementFeePerMonth, //管理費每月價錢

                                    pictures = pictureObject
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 介紹",
                                    data = data
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else if (houseEnter.status == statusType.完成步驟5)
                            {
                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                                    isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                                    isCookAllowed = houseEnter.isCookAllowed, //可開伙
                                    isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                                    isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                                    hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = houseEnter.hasElevator, //有電梯
                                    hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                                    hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                                    hasInternet = houseEnter.hasInternet, //設備: 網路
                                    hasBed = houseEnter.hasBed, //設備: 床
                                    hasTV = houseEnter.hasTV, //設備: 電視
                                    isNearMRT = houseEnter.isNearMRT, //交通: 捷運
                                    kmAwayMRT = houseEnter.kmAwayMRT, //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = houseEnter.kmAwayLRT, //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = houseEnter.kmAwayBusStation, //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = houseEnter.kmAwayHSR, //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = houseEnter.kmAwayTrainStation, //距離火車公里
                                    rent = houseEnter.rent, //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = houseEnter.waterBillPerMonth, //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    electricBillPerDegree = houseEnter.electricBillPerDegree,
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = houseEnter.managementFeePerMonth, //管理費每月價錢
                                    description = houseEnter.description, //房源介紹

                                    pictures = pictureObject
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 限制",
                                    data = data
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else if (houseEnter.status == statusType.完成步驟6)
                            {
                                string jobRestriction = "";
                                if (!String.IsNullOrEmpty(houseEnter.jobRestriction))
                                {
                                    string[] jobRestrictions = houseEnter.jobRestriction.Split(',');
                                    for (int i = 0; i < jobRestrictions.Length; i++)
                                    {
                                        jobRestrictions[i] = jobRestrictions[i].Trim();
                                        jobRestriction += Enum.GetName(typeof(UserJob), Convert.ToInt32(jobRestrictions[i])) + ",";
                                    }
                                    char[] trimArr = { ',', ' ' };
                                    jobRestriction = jobRestriction.Trim(trimArr);
                                }

                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                                    isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                                    isCookAllowed = houseEnter.isCookAllowed, //可開伙
                                    isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                                    isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                                    hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = houseEnter.hasElevator, //有電梯
                                    hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                                    hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                                    hasInternet = houseEnter.hasInternet, //設備: 網路
                                    hasBed = houseEnter.hasBed, //設備: 床
                                    hasTV = houseEnter.hasTV, //設備: 電視
                                    isNearMRT = houseEnter.isNearMRT, //交通: 捷運
                                    kmAwayMRT = houseEnter.kmAwayMRT, //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = houseEnter.kmAwayLRT, //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = houseEnter.kmAwayBusStation, //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = houseEnter.kmAwayHSR, //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = houseEnter.kmAwayTrainStation, //距離火車公里
                                    rent = houseEnter.rent, //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = houseEnter.waterBillPerMonth, //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    electricBillPerDegree = houseEnter.electricBillPerDegree,
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = houseEnter.managementFeePerMonth, //管理費每月價錢
                                    description = houseEnter.description, //房源介紹
                                    hasTenantRestrictions = houseEnter.hasTenantRestrictions.ToString().ToLower(), //是否有租客限制
                                    genderRestriction = Enum.GetName(typeof(genderRestrictionType), houseEnter.genderRestriction), //男or女or性別友善
                                    jobRestriction = jobRestriction, //排除職業

                                    pictures = pictureObject
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 確認",
                                    data = data
                                };
                                return Content(HttpStatusCode.OK, result);
                            }
                            else
                            {
                                string jobRestriction = "";
                                if (!String.IsNullOrEmpty(houseEnter.jobRestriction))
                                {
                                    string[] jobRestrictions = houseEnter.jobRestriction.Split(',');
                                    for (int i = 0; i < jobRestrictions.Length; i++)
                                    {
                                        jobRestrictions[i] = jobRestrictions[i].Trim();
                                        jobRestriction += Enum.GetName(typeof(UserJob), Convert.ToInt32(jobRestrictions[i])) + ",";
                                    }
                                    char[] trimArr = { ',', ' ' };
                                    jobRestriction = jobRestriction.Trim(trimArr);
                                }
                                var appointmentCount = db.AppointmentsEntities.Count(x => x.houseId == houseEnter.id && x.isValid == true);

                                var pictureObject = new
                                {
                                    firstPic = firstPicture.path,
                                    restOfPic = restOfPicsList
                                };
                                var data = new
                                {
                                    appointmentCount = appointmentCount,
                                    name = houseEnter.name, //名稱
                                    city = Enum.GetName(typeof(CityType), houseEnter.city), //縣市 Enum
                                    district = Enum.GetName(typeof(DistrictType), houseEnter.district).Remove(0, 3), //市區鄉鎮 Enum
                                    road = houseEnter.road, //路街
                                    lane = houseEnter.lane, //巷
                                    alley = houseEnter.alley, //弄
                                    number = houseEnter.number, //號
                                    floor = houseEnter.floor, //樓層
                                    floorTotal = houseEnter.floorTotal, //總樓數
                                    type = Enum.GetName(typeof(type), houseEnter.type), //類型 Enum
                                    ping = houseEnter.ping, //承租坪數
                                    roomNumbers = houseEnter.roomNumbers, //房
                                    livingRoomNumbers = houseEnter.livingRoomNumbers, //廳
                                    bathRoomNumbers = houseEnter.bathRoomNumbers, //衛浴
                                    balconyNumbers = houseEnter.balconyNumbers, //陽台
                                    parkingSpaceNumbers = houseEnter.parkingSpaceNumbers, //車位
                                    isRentSubsidy = houseEnter.isRentSubsidy, //可申請租屋補助
                                    isPetAllowed = houseEnter.isPetAllowed, //寵物友善
                                    isCookAllowed = houseEnter.isCookAllowed, //可開伙
                                    isSTRAllowed = houseEnter.isSTRAllowed, //可短租
                                    isNearByDepartmentStore = houseEnter.isNearByDepartmentStore, //附近機能: 百貨商場
                                    isNearBySchool = houseEnter.isNearBySchool, //附近機能: 學校
                                    isNearByMorningMarket = houseEnter.isNearByMorningMarket, //附近機能: 早市
                                    isNearByNightMarket = houseEnter.isNearByNightMarket, //附近機能: 夜市
                                    isNearByConvenientStore = houseEnter.isNearByConvenientStore, //附近機能: 超商
                                    isNearByPark = houseEnter.isNearByPark, //附近機能: 公園綠地
                                    hasGarbageDisposal = houseEnter.hasGarbageDisposal, //屋源特色: 垃圾集中處理
                                    hasWindowInBathroom = houseEnter.hasWindowInBathroom, //屋源特色: 浴室開窗
                                    hasElevator = houseEnter.hasElevator, //有電梯
                                    hasAirConditioner = houseEnter.hasAirConditioner, //設備: 冷氣
                                    hasWashingMachine = houseEnter.hasWashingMachine, //設備: 洗衣機
                                    hasRefrigerator = houseEnter.hasRefrigerator, //設備: 冰箱
                                    hasCloset = houseEnter.hasCloset, //設備: 衣櫃
                                    hasTableAndChair = houseEnter.hasTableAndChair, //設備: 桌椅
                                    hasWaterHeater = houseEnter.hasWaterHeater, //設備: 熱水器
                                    hasInternet = houseEnter.hasInternet, //設備: 網路
                                    hasBed = houseEnter.hasBed, //設備: 床
                                    hasTV = houseEnter.hasTV, //設備: 電視
                                    isNearMRT = houseEnter.isNearMRT, //交通: 捷運
                                    kmAwayMRT = houseEnter.kmAwayMRT, //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = houseEnter.kmAwayLRT, //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = houseEnter.kmAwayBusStation, //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = houseEnter.kmAwayHSR, //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = houseEnter.kmAwayTrainStation, //距離火車公里
                                    rent = houseEnter.rent, //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = houseEnter.waterBillPerMonth, //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    electricBillPerDegree = houseEnter.electricBillPerDegree,
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = houseEnter.managementFeePerMonth, //管理費每月價錢
                                    description = houseEnter.description, //房源介紹
                                    hasTenantRestrictions = houseEnter.hasTenantRestrictions.ToString().ToLower(), //是否有租客限制
                                    genderRestriction = Enum.GetName(typeof(genderRestrictionType), houseEnter.genderRestriction), //男or女or性別友善
                                    jobRestriction = jobRestriction, //排除職業

                                    pictures = pictureObject
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
                    else
                    {
                        throw new Exception("該房源擁有者非使用者");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-9]查詢使用者 (via 手機號碼)
        /// </summary>
        /// <param name="tel">手機號碼</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/house/landlord/userInfo")]
        [JwtAuthFilters]
        public IHttpActionResult GetTenantUserId([FromBody] string tel)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            var role = (UserRoleType)jwtObject["Role"];
            try
            {
                if (role == UserRoleType.租客)
                {
                    throw new Exception("使用者角色不符，不得使用此功能");
                }
                using (DBModel db = new DBModel())
                {
                    var telEnter = db.UserEntities.Where(x => x.telphone == tel).FirstOrDefault();
                    if (telEnter == null)
                    {
                        throw new Exception("該電話號碼用戶非系統用戶");
                    }
                    if (telEnter.role == UserRoleType.房東)
                    {
                        throw new Exception("該電話號碼用戶角色為房東");
                    }
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "成功找到該系統用戶",
                        data = new
                        {
                            userId = telEnter.Id,
                            name = telEnter.lastName + telEnter.firstName,
                            photo = telEnter.photo
                        }
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-6]變更房源狀態為已完成
        /// </summary>
        /// <param name="id">房源Id</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("api/house/landlord/status/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult PatchHouseStatusToDone(int id)
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            var role = (UserRoleType)jwtObject["Role"];
            var userId = (int)jwtObject["Id"];
            try
            {
                if (role == UserRoleType.租客)
                {
                    throw new Exception("使用者角色不符，不得使用此功能");
                }
                using (DBModel db = new DBModel())
                {
                    //檢查房源是否存在
                    var houseEnter = db.HouseEntities.Where(x => x.id == id).FirstOrDefault() ?? throw new Exception("該房源不存在，無法變更房源狀態");

                    if (houseEnter.userId != userId) //檢查房源是否屬於該使用者
                    {
                        throw new Exception("該房源不屬於此使用者，無法變更房源狀態");
                    }
                    if (houseEnter.status != statusType.刊登中) //檢查房源狀態是否屬於該使用者
                    {
                        throw new Exception("該房源狀態非刊登中，無法變更房源狀態");
                    }

                    houseEnter.status = statusType.已完成;
                    db.SaveChanges();

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "成功修改房源狀態為已完成"
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-1]取得房東所有房源狀態的列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/house/landlord/list")]
        [JwtAuthFilters]
        public IHttpActionResult GetLandlordHouseList()
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            var role = (UserRoleType)jwtObject["Role"];
            var userId = (int)jwtObject["Id"];

            try
            {
                if (role == UserRoleType.租客)
                {
                    throw new Exception("使用者角色不符，不得使用此功能");
                }
                using (DBModel db = new DBModel())
                {
                    //房東所有的房子
                    var landlordEnter = db.HouseEntities.Where(x => x.userId == userId).ToList();
                    //只要id list
                    var houseIds = landlordEnter.Select(x => x.id).ToList();
                    //只要id list裡面有的houseId, 只要封面照
                    var houseImages = db.HouseImgsEntities
                                        .Where(img => houseIds.Contains(img.houseId) && img.isCover)
                                        .ToList();
                    //只要id list裡面有的houseId
                    var houseOrders = db.OrdersEntities
                                        .Where(order => houseIds.Contains(order.houseId))
                                        .ToList();

                    List<EditingHouse> housesEditing = new List<EditingHouse>(); //未完成房源列表
                    List<ForRentHouse> housesForRent = new List<ForRentHouse>(); //刊登中房源列表
                    List<LeasingHouse> housesLeasing = new List<LeasingHouse>(); //已承租房源列表
                    List<DiscontinuedHouse> housesDiscontinued = new List<DiscontinuedHouse>(); //已完成房源列表
                    foreach (var house in landlordEnter)
                    {
                        if (house.status != statusType.未完成步驟1)
                        {
                            switch (house.status)
                            {
                                case statusType.已完成:
                                    var discontinuedHouse = new DiscontinuedHouse();
                                    discontinuedHouse.houseId = house.id;
                                    discontinuedHouse.name = house.name;
                                    discontinuedHouse.photo = houseImages.FirstOrDefault(x => x.houseId == house.id)?.path;
                                    housesDiscontinued.Add(discontinuedHouse);
                                    break;

                                case statusType.已承租:
                                    var leasingHouse = new LeasingHouse();
                                    leasingHouse.houseId = house.id;
                                    leasingHouse.name = house.name;
                                    leasingHouse.photo = houseImages.FirstOrDefault(x => x.houseId == house.id)?.path;
                                    var latestLeasingOrder = houseOrders.Where(x => x.houseId == house.id)
                                                        .OrderByDescending(x => x.id)
                                                        .FirstOrDefault();
                                    leasingHouse.leaseStartTime = latestLeasingOrder?.leaseStartTime ?? DateTime.MinValue;
                                    leasingHouse.leaseEndTime = latestLeasingOrder?.leaseEndTime ?? DateTime.MinValue;
                                    housesLeasing.Add(leasingHouse);
                                    break;

                                case statusType.刊登中:
                                    //只要id list裡面有的houseId
                                    var houseAppointments = db.AppointmentsEntities
                                                        .Where(appointment => houseIds.Contains(appointment.houseId))
                                                        .ToList();
                                    var forRentHouse = new ForRentHouse();
                                    forRentHouse.houseId = house.id;
                                    forRentHouse.name = house.name;
                                    forRentHouse.photo = houseImages.FirstOrDefault(x => x.houseId == house.id)?.path;
                                    var latestPendingOrder = houseOrders.Where(x => x.houseId == house.id && (x.status == OrderStatus.待租客回覆租約 || x.status == OrderStatus.租客已拒絕租約))
                                                        .OrderByDescending(x => x.id)
                                                        .FirstOrDefault();
                                    if (latestPendingOrder == null)
                                    {
                                        forRentHouse.status = "申請預約看房";
                                        forRentHouse.reservationCount = houseAppointments.Where(x => x.houseId == house.id).Count();
                                    }
                                    else
                                    {
                                        var orderUserName = db.UserEntities.FirstOrDefault(x => x.Id == latestPendingOrder.userId);
                                        forRentHouse.userName = orderUserName.lastName + orderUserName.firstName;
                                        if (latestPendingOrder.status == OrderStatus.待租客回覆租約)
                                        {
                                            forRentHouse.status = "租約邀請已送出";
                                        }
                                        else if (latestPendingOrder.status == OrderStatus.租客已拒絕租約)
                                        {
                                            forRentHouse.status = "租約邀請已拒絕";
                                        }
                                    }

                                    housesForRent.Add(forRentHouse);
                                    break;

                                default:
                                    var editingHouse = new EditingHouse();
                                    editingHouse.houseId = house.id;
                                    editingHouse.name = house.name;
                                    editingHouse.photo = houseImages.FirstOrDefault(x => x.houseId == house.id)?.path;
                                    housesEditing.Add(editingHouse);
                                    break;
                            }
                        }
                    }
                    var houseResult = new
                    {
                        未完成 = housesEditing,
                        刊登中 = housesForRent,
                        已承租 = housesLeasing,
                        已完成 = housesDiscontinued
                    };
                    // 將 List 物件轉換成 JSON 字串
                    //string jsonEditing = JsonConvert.SerializeObject(housesEditing, Formatting.Indented);
                    //string jsonForRent = JsonConvert.SerializeObject(housesForRent, Formatting.Indented);
                    //string jsonLeasing = JsonConvert.SerializeObject(housesLeasing, Formatting.Indented);
                    //string jsonDiscontinued = JsonConvert.SerializeObject(housesDiscontinued, Formatting.Indented);
                    //string output = JsonConvert.SerializeObject(Object變數)

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "成功找到房源列表",
                        data = houseResult
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        /// <summary>
        /// [ALO-10]房東取得待評價的房源筆數
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/house/landlord/count/unrated")]
        [JwtAuthFilters]
        public IHttpActionResult GetLandlordUnratedHouseCount()
        {
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];
            try
            {
                //驗證角色
                if (UserRole != UserRoleType.房東)
                {
                    return Content(HttpStatusCode.Forbidden, "使用者角色非房東");
                }
                using (DBModel db = new DBModel())
                {
                    var query = from order in db.OrdersEntities.AsQueryable()
                                where order.leaseEndTime < DateTime.Today //過期的order
                                join house in db.HouseEntities on order.houseId equals house.id
                                where house.userId == UserId //使用者的房子
                                join user in db.UserEntities on order.userId equals user.Id
                                join orderRating in db.OrdersRatingEntities on order.id equals orderRating.orderId into orderRatingGroup
                                from orderRating in orderRatingGroup.DefaultIfEmpty()
                                select new
                                {
                                    order,
                                    house,
                                    tenant = user,
                                    ratingByLandlord = orderRatingGroup.FirstOrDefault(o => o.UserId == UserId) != null ? orderRatingGroup.FirstOrDefault(o => o.UserId == UserId) : null,
                                    ratingByTenant = orderRatingGroup.FirstOrDefault(o => o.UserId != UserId) != null ? orderRatingGroup.FirstOrDefault(o => o.UserId != UserId) : null
                                };
                    var queryResult = query.ToList();
                    int canCommentCount = 0;
                    if (queryResult.Count > 0)
                    {
                        foreach (var item in queryResult)
                        {
                            //可評價的狀況

                            if (DateTime.Now < item.order.leaseEndTime.AddDays(14) && item.ratingByLandlord == null)
                            {
                                canCommentCount++;
                            }
                        }
                    }

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "已成功回傳房東待評價的房源筆數",
                        data = new
                        {
                            count = canCommentCount
                        }
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

    // GET: api/House
    //public IEnumerable<string> Get()
    //{
    //    return new string[] { "value1", "value2" };
    //}

    //// GET: api/House/5
    //public string Get(int id)
    //{
    //    return "value";
    //}

    //// POST: api/House
    //public void Post([FromBody]string value)
    //{
    //}

    //// PUT: api/House/5
    //public void Put(int id, [FromBody]string value)
    //{
    //}

    //// DELETE: api/House/5
    //public void Delete(int id)
    //{
    //}
}