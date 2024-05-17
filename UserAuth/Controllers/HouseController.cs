using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
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

        [HttpPost]
        [Route("api/myHouse")]
        [JwtAuthFilters]
        public IHttpActionResult createListing()
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

        [HttpPatch]
        [Route("api/myHouse/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult updateListing(int id, HouseInput houseInput)
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
                    type houseType = houseInput.type;
                    string ping = houseInput.ping;
                    string houseRoomNumbers = houseInput.roomNumbers;
                    string houseLivingRoomNumbers = houseInput.livingRoomNumbers;
                    string houseBathRoomNumbers = houseInput.bathRoomNumbers;
                    string houseBalconyNumbers = houseInput.balconyNumbers;
                    string houseParkingSpaceNumbers = houseInput.parkingSpaceNumbers;
                    bool houseIsRentSubsidy = houseInput.isRentSubsidy;
                    bool houseIsPetAllowed = houseInput.isPetAllowed;
                    bool houseIsCookAllowed = houseInput.isCookAllowed;
                    bool houseIsSTRAllowed = houseInput.isSTRAllowed;
                    bool houuseIsNearByDepartmentStore = houseInput.isNearByDepartmentStore;
                    bool houseIsNearBySchool = houseInput.isNearBySchool;
                    bool houseIsNearByMorningMarket = houseInput.isNearByMorningMarket;
                    bool houseIsNearByNightMarket = houseInput.isNearByNightMarket;
                    bool houseIsNearByConvenientStore = houseInput.isNearByConvenientStore;
                    bool houseIsNearByPark = houseInput.isNearByPark;
                    bool houseHasGarbageDisposal = houseInput.hasGarbageDisposal;
                    bool houseHasWindowInBathroom = houseInput.hasWindowInBathroom;
                    bool houseHasElevator = houseInput.hasElevator;
                    bool houseIsNearMRT = houseInput.isNearMRT;
                    string houseKmAwayMRT = houseInput.kmAwayMRT;
                    bool houseIsNearLRT = houseInput.isNearLRT;
                    string houseKmAwayLRT = houseInput.kmAwayLRT;
                    bool houseIsNearBusStation = houseInput.isNearBusStation;
                    string houseKmAwayBusStation = houseInput.kmAwayBusStation;
                    bool houseIsNearHSR = houseInput.isNearHSR;
                    string houseKmAwayHSR = houseInput.kmAwayHSR;
                    bool houseIsNearTrainStation = houseInput.isNearTrainStation;
                    string houseKmAwayTrainStation = houseInput.kmAwayTrainStation;
                    bool houseHasAirConditioner = houseInput.hasAirConditioner;
                    bool houseHasWashingMachine = houseInput.hasWashingMachine;
                    bool houseHasRefrigerator = houseInput.hasRefrigerator;
                    bool houseHasCloset = houseInput.hasCloset;
                    bool houseHasTableAndChair = houseInput.hasTableAndChair;
                    bool houseHasWaterHeater = houseInput.hasWaterHeater;
                    bool houseHasInternet = houseInput.hasInternet;
                    bool houseHasBed = houseInput.hasBed;
                    bool houseHasTV = houseInput.hasTV;
                    paymentTypeOfWaterBill housePaymentMethodOfWaterBill = houseInput.paymentMethodOfWaterBill;
                    string houseWaterBillPerMonth = houseInput.waterBillPerMonth;
                    paymentTypeOfElectricBill houseElectricBill = houseInput.electricBill;
                    paymentMethodOfElectricBill housePaymentMethodOfElectricBill = houseInput.paymentMethodOfElectricBill;
                    paymentMethodOfManagementFee housePaymentMethodOfManagementFee = houseInput.paymentMethodOfManagementFee;
                    string houseManagementFeePerMonth = houseInput.managementFeePerMonth;
                    string houseRent = houseInput.rent;
                    securityDepositType houseSecurityDeposit = houseInput.securityDeposit;
                    string houseDescription = houseInput.description;
                    bool houseHasTenantRestrictions = houseInput.hasTenantRestrictions;
                    genderRestrictionType houseGenderRestriction = houseInput.genderRestriction;
                    string houseJobRestriction = houseInput.jobRestriction;
                    statusType houseStatus = houseInput.status;

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
                    updateHouse.type = houseType;
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
                    if (houseIsRentSubsidy)
                    {
                        updateHouse.isRentSubsidy = houseIsRentSubsidy;
                    }
                    if (houseIsPetAllowed)
                    {
                        updateHouse.isPetAllowed = houseIsPetAllowed;
                    }
                    if (houseIsCookAllowed)
                    {
                        updateHouse.isCookAllowed = houseIsCookAllowed;
                    }
                    if (houseIsSTRAllowed)
                    {
                        updateHouse.isSTRAllowed = houseIsSTRAllowed;
                    }
                    if (houuseIsNearByDepartmentStore)
                    {
                        updateHouse.isNearByDepartmentStore = houuseIsNearByDepartmentStore;
                    }
                    if (houseIsNearBySchool)
                    {
                        updateHouse.isNearBySchool = houseIsNearBySchool;
                    }
                    if (houseIsNearByMorningMarket)
                    {
                        updateHouse.isNearByMorningMarket = houseIsNearByMorningMarket;
                    }
                    if (houseIsNearByNightMarket)
                    {
                        updateHouse.isNearByNightMarket = houseIsNearByNightMarket;
                    }
                    if (houseIsNearByConvenientStore)
                    {
                        updateHouse.isNearByConvenientStore = houseIsNearByConvenientStore;
                    }
                    if (houseIsNearByPark)
                    {
                        updateHouse.isNearByPark = houseIsNearByPark;
                    }
                    if (houseHasGarbageDisposal)
                    {
                        updateHouse.hasGarbageDisposal = houseHasGarbageDisposal;
                    }
                    if (houseHasWindowInBathroom)
                    {
                        updateHouse.hasWindowInBathroom = houseHasWindowInBathroom;
                    }
                    if (houseHasElevator)
                    {
                        updateHouse.hasElevator = houseHasElevator;
                    }
                    if (houseIsNearMRT)
                    {
                        updateHouse.isNearMRT = houseIsNearMRT;
                    }
                    if (houseKmAwayMRT != null)
                    {
                        updateHouse.kmAwayMRT = houseKmAwayMRT;
                    }
                    if (houseIsNearLRT)
                    {
                        updateHouse.isNearLRT = houseIsNearLRT;
                    }
                    if (houseKmAwayLRT != null)
                    {
                        updateHouse.kmAwayLRT = houseKmAwayLRT;
                    }
                    if (houseIsNearBusStation)
                    {
                        updateHouse.isNearBusStation = houseIsNearBusStation;
                    }
                    if (houseKmAwayBusStation != null)
                    {
                        updateHouse.kmAwayBusStation = houseKmAwayBusStation;
                    }
                    if (houseIsNearHSR)
                    {
                        updateHouse.isNearHSR = houseIsNearHSR;
                    }
                    if (houseKmAwayHSR != null)
                    {
                        updateHouse.kmAwayHSR = houseKmAwayHSR;
                    }
                    if (houseIsNearTrainStation)
                    {
                        updateHouse.isNearTrainStation = houseIsNearTrainStation;
                    }
                    if (houseKmAwayTrainStation != null)
                    {
                        updateHouse.kmAwayTrainStation = houseKmAwayTrainStation;
                    }
                    if (houseHasAirConditioner)
                    {
                        updateHouse.hasAirConditioner = houseHasAirConditioner;
                    }
                    if (houseHasWashingMachine)
                    {
                        updateHouse.hasWashingMachine = houseHasWashingMachine;
                    }
                    if (houseHasRefrigerator)
                    {
                        updateHouse.hasRefrigerator = houseHasRefrigerator;
                    }
                    if (houseHasCloset)
                    {
                        updateHouse.hasCloset = houseHasCloset;
                    }
                    if (houseHasTableAndChair)
                    {
                        updateHouse.hasTableAndChair = houseHasTableAndChair;
                    }
                    if (houseHasWaterHeater)
                    {
                        updateHouse.hasWaterHeater = houseHasWaterHeater;
                    }
                    if (houseHasInternet)
                    {
                        updateHouse.hasInternet = houseHasInternet;
                    }
                    if (houseHasBed)
                    {
                        updateHouse.hasBed = houseHasBed;
                    }
                    if (houseHasTV)
                    {
                        updateHouse.hasTV = houseHasTV;
                    }
                    updateHouse.paymentMethodOfWaterBill = housePaymentMethodOfWaterBill;
                    if (houseWaterBillPerMonth != null)
                    {
                        updateHouse.waterBillPerMonth = houseWaterBillPerMonth;
                    }
                    updateHouse.electricBill = houseElectricBill;
                    updateHouse.paymentMethodOfElectricBill = housePaymentMethodOfElectricBill;
                    updateHouse.paymentMethodOfManagementFee = housePaymentMethodOfManagementFee;
                    if (houseManagementFeePerMonth != null)
                    {
                        updateHouse.managementFeePerMonth = houseManagementFeePerMonth;
                    }
                    if (houseRent != null)
                    {
                        updateHouse.rent = houseRent;
                    }
                    updateHouse.securityDeposit = houseSecurityDeposit;
                    if (houseDescription != null)
                    {
                        updateHouse.description = houseDescription;
                    }
                    if (houseHasTenantRestrictions)
                    {
                        updateHouse.hasTenantRestrictions = houseHasTenantRestrictions;
                    }
                    updateHouse.genderRestriction = houseGenderRestriction;
                    updateHouse.status = houseStatus;

                    db.SaveChanges();

                    return Content(HttpStatusCode.OK, "已成功修改房源");
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Route("api/myHouseImg/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult creatingHouseImg(int id, HouseImgInput houseImgInput)
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
                        InsertNewHouseImg.name = houseImgObject.name;
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

        [HttpDelete]
        [Route("api/myHouse/{id}")]
        [JwtAuthFilters]
        public IHttpActionResult deleteMyHouse(int id)
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

        [HttpGet]
        [Route("api/myHouse/getHomePageHouse")]
        public IHttpActionResult getHomePageHouse()
        {
            DBModel db = new DBModel();
            var query = db.HouseEntities.Where(h => h.isRentSubsidy && h.isCookAllowed && h.isPetAllowed && h.isSTRAllowed)
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
                    .Where(h => (h.isRentSubsidy && h.isCookAllowed) || (h.isPetAllowed && h.isSTRAllowed) || (h.isRentSubsidy && h.isPetAllowed) || h.isRentSubsidy || h.isCookAllowed || h.isPetAllowed || h.isSTRAllowed)
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

        //房東取得各狀態的房源內容
        [HttpGet]
        [Route("api/myHouse/info/{id}")]
        public IHttpActionResult getMyHouseInfo(int id)
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
                            if (houseEnter.status == statusType.已完成 || houseEnter.status == statusType.刊登中)
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
                                ///todo: 房東取得各狀態的房源內容:已承租
                                throw new Exception("已承租還沒做");
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
                                    kmAwayMRT = "", //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = "", //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = "", //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = "", //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = "", //距離火車公里

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
                                    kmAwayMRT = "", //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = "", //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = "", //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = "", //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = "", //距離火車公里
                                    rent = "", //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = "", //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = "", //管理費每月價錢

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
                                    kmAwayMRT = "", //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = "", //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = "", //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = "", //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = "", //距離火車公里
                                    rent = "", //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = "", //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = "", //管理費每月價錢
                                    description = "", //房源介紹

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
                            else
                            {
                                string jobRestriction = "";
                                string[] jobRestrictions = houseEnter.jobRestriction.Split(',');
                                for (int i = 0; i < jobRestrictions.Length; i++)
                                {
                                    jobRestrictions[i] = jobRestrictions[i].Trim();
                                    jobRestriction += Enum.GetName(typeof(UserJob), Convert.ToInt32(jobRestrictions[i])) + ", ";
                                }
                                char[] trimArr = { ',', ' ' };
                                jobRestriction = jobRestriction.Trim(trimArr);
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
                                    kmAwayMRT = "", //距離捷運公里
                                    isNearLRT = houseEnter.isNearLRT, //交通: 輕軌
                                    kmAwayLRT = "", //距離輕軌公里
                                    isNearBusStation = houseEnter.isNearBusStation, //交通: 公車
                                    kmAwayBusStation = "", //距離公車公里
                                    isNearHSR = houseEnter.isNearHSR, //交通: 高鐵
                                    kmAwayHSR = "", //距離高鐵公里
                                    isNearTrainStation = houseEnter.isNearTrainStation, //交通: 火車
                                    kmAwayTrainStation = "", //距離火車公里
                                    rent = "", //每月租金
                                    securityDeposit = Enum.GetName(typeof(securityDepositType), houseEnter.securityDeposit), //押金幾個月 Enum
                                    paymentMethodOfWaterBill = Enum.GetName(typeof(paymentTypeOfWaterBill), houseEnter.paymentMethodOfWaterBill), //水費繳納方式 Enum
                                    waterBillPerMonth = "", //水費每月價錢
                                    electricBill = Enum.GetName(typeof(paymentTypeOfElectricBill), houseEnter.electricBill), //電費計價方式 Enum
                                    paymentMethodOfElectricBill = Enum.GetName(typeof(paymentMethodOfElectricBill), houseEnter.paymentMethodOfElectricBill), //電費繳納方式 Enum
                                    paymentMethodOfManagementFee = Enum.GetName(typeof(paymentMethodOfManagementFee), houseEnter.paymentMethodOfManagementFee), //管理費方式 Enum
                                    managementFeePerMonth = "", //管理費每月價錢
                                    description = "", //房源介紹
                                    hasTenantRestrictions = true, //是否有租客限制
                                    genderRestriction = Enum.GetName(typeof(genderRestrictionType), houseEnter.genderRestriction), //男or女or性別友善
                                    jobRestriction = jobRestriction, //排除職業

                                    pictures = pictureObject
                                };
                                var result = new
                                {
                                    statusCode = 200,
                                    status = "success",
                                    message = "page: 完成",
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