using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
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
                    House InsertNewAccount = new House();
                    InsertNewAccount.userId = UserId;
                    InsertNewAccount.status = statusType.未完成步驟1;

                    db.HouseEntities.Add(InsertNewAccount);

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
}