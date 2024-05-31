using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.UserEnumList;

namespace UserAuth.Models.ViewModel
{
    public class HouseInput
    {
        //[Required]
        //public int id { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        /// <summary>
        /// 房源名稱
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "名稱")]
        public string name { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "縣市")]
        public string city { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "區域鄉鎮")]
        public string district { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "路街")]
        public string road { get; set; }

        [Display(Name = "巷")]
        public string lane { get; set; }

        [Display(Name = "弄")]
        public string alley { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "號")]
        public string number { get; set; }

        [Display(Name = "樓")]
        public string floor { get; set; }

        [Display(Name = "總樓層")]
        public string floorTotal { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "類型")]
        public type? type { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "坪數")]
        public string ping { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "房數")]
        public string roomNumbers { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "廳數")]
        public string livingRoomNumbers { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "衛浴數")]
        public string bathRoomNumbers { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "陽台數")]
        public string balconyNumbers { get; set; }

        //[Required(ErrorMessage = "{0}必填")]
        [Display(Name = "車位數")]
        public string parkingSpaceNumbers { get; set; }

        [Display(Name = "是否可申請租屋補助")]
        public bool? isRentSubsidy { get; set; }

        [Display(Name = "是否寵物友善")]
        public bool? isPetAllowed { get; set; }

        [Display(Name = "是否可開伙")]
        public bool? isCookAllowed { get; set; }

        [Display(Name = "是否可短租")]
        public bool? isSTRAllowed { get; set; }

        [Display(Name = "是否鄰近百貨商場")]
        public bool? isNearByDepartmentStore { get; set; }

        [Display(Name = "是否鄰近學校")]
        public bool? isNearBySchool { get; set; }

        [Display(Name = "是否鄰近早市")]
        public bool? isNearByMorningMarket { get; set; }

        [Display(Name = "是否鄰近夜市")]
        public bool? isNearByNightMarket { get; set; }

        [Display(Name = "是否鄰近超商")]
        public bool? isNearByConvenientStore { get; set; }

        [Display(Name = "是否鄰近公園綠地")]
        public bool? isNearByPark { get; set; }

        [Display(Name = "是否垃圾集中處理")]
        public bool? hasGarbageDisposal { get; set; }

        [Display(Name = "是否浴室開窗")]
        public bool? hasWindowInBathroom { get; set; }

        [Display(Name = "是否有電梯")]
        public bool? hasElevator { get; set; }

        [Display(Name = "是否鄰近捷運")]
        public bool? isNearMRT { get; set; }

        [Display(Name = "鄰近捷運公里")]
        public string kmAwayMRT { get; set; }

        [Display(Name = "是否鄰近輕軌")]
        public bool? isNearLRT { get; set; }

        [Display(Name = "鄰近輕軌公里")]
        public string kmAwayLRT { get; set; }

        [Display(Name = "是否鄰近公車站")]
        public bool? isNearBusStation { get; set; }

        [Display(Name = "鄰近公車站公里")]
        public string kmAwayBusStation { get; set; }

        [Display(Name = "是否鄰近高鐵")]
        public bool? isNearHSR { get; set; }

        [Display(Name = "鄰近高鐵公里")]
        public string kmAwayHSR { get; set; }

        [Display(Name = "是否鄰近火車")]
        public bool? isNearTrainStation { get; set; }

        [Display(Name = "鄰近火車公里")]
        public string kmAwayTrainStation { get; set; }

        [Display(Name = "是否提供冷氣")]
        public bool? hasAirConditioner { get; set; }

        [Display(Name = "是否提供洗衣機")]
        public bool? hasWashingMachine { get; set; }

        [Display(Name = "是否提供冰箱")]
        public bool? hasRefrigerator { get; set; }

        [Display(Name = "是否提供衣櫃")]
        public bool? hasCloset { get; set; }

        [Display(Name = "是否提供桌椅")]
        public bool? hasTableAndChair { get; set; }

        [Display(Name = "是否提供熱水器")]
        public bool? hasWaterHeater { get; set; }

        [Display(Name = "是否提供網路")]
        public bool? hasInternet { get; set; }

        [Display(Name = "是否提供床")]
        public bool? hasBed { get; set; }

        [Display(Name = "是否提供電視")]
        public bool? hasTV { get; set; }

        [Display(Name = "水費繳納方式")]
        public paymentTypeOfWaterBill? paymentMethodOfWaterBill { get; set; }

        [Display(Name = "每月水費價錢")]
        public string waterBillPerMonth { get; set; }

        [Display(Name = "電費計價方式")]
        public paymentTypeOfElectricBill? electricBill { get; set; }

        [Display(Name = "電費每度幾元")]
        public string electricBillPerDegree { get; set; }

        [Display(Name = "電費繳納方式")]
        public paymentMethodOfElectricBill? paymentMethodOfElectricBill { get; set; }

        [Display(Name = "管理費繳納方式")]
        public paymentMethodOfManagementFee? paymentMethodOfManagementFee { get; set; }

        [Display(Name = "每月管理費價錢")]
        public string managementFeePerMonth { get; set; }

        [Display(Name = "每月租金")]
        public string rent { get; set; }

        [Display(Name = "押金")]
        public securityDepositType? securityDeposit { get; set; }

        [Display(Name = "房源介紹")]
        public string description { get; set; }

        [Display(Name = "是否有租客限制")]
        public bool? hasTenantRestrictions { get; set; }

        [Display(Name = "設定租客性別限制")]
        public genderRestrictionType? genderRestriction { get; set; }

        [Display(Name = "設定租客工作限制")]
        public string jobRestriction { get; set; }

        [Display(Name = "房源狀態")]
        public statusType? status { get; set; }
    }
}