using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.UserEnumList;

namespace UserAuth.Models
{
    [Table("Houses")]
    public class House
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "房東")]
        public int userId { get; set; }

        [JsonIgnore]
        [ForeignKey("userId")]
        [Display(Name = "房東")]
        public virtual User userIdFK { get; set; }

        [JsonIgnore]
        [ForeignKey("userId")]
        [Display(Name = "訂單")]
        public virtual ICollection<Order> orderIdFK { get; set; }

        [MaxLength(100)]
        [Display(Name = "名稱")]
        public string name { get; set; }

        [Display(Name = "縣市")]
        public CityType city { get; set; }

        [Display(Name = "區域鄉鎮")]
        public DistrictType district { get; set; }

        [Display(Name = "路街")]
        public string road { get; set; }

        [Display(Name = "巷")]
        public string lane { get; set; }

        [Display(Name = "弄")]
        public string alley { get; set; }

        [Display(Name = "號")]
        public string number { get; set; }

        [Display(Name = "樓")]
        public string floor { get; set; }

        [Display(Name = "總樓層")]
        public string floorTotal { get; set; }

        [Display(Name = "類型")]
        public type type { get; set; }

        [Display(Name = "坪數")]
        public string ping { get; set; }

        [Display(Name = "房數")]
        public string roomNumbers { get; set; }

        [Display(Name = "廳數")]
        public string livingRoomNumbers { get; set; }

        [Display(Name = "衛浴數")]
        public string bathRoomNumbers { get; set; }

        [Display(Name = "陽台數")]
        public string balconyNumbers { get; set; }

        [Display(Name = "車位數")]
        public string parkingSpaceNumbers { get; set; }

        [Display(Name = "是否可申請租屋補助")]
        public bool isRentSubsidy { get; set; } = false;

        [Display(Name = "是否寵物友善")]
        public bool isPetAllowed { get; set; } = false;

        [Display(Name = "是否可開伙")]
        public bool isCookAllowed { get; set; } = false;

        [Display(Name = "是否可短租")]
        public bool isSTRAllowed { get; set; } = false;

        [Display(Name = "是否鄰近百貨商場")]
        public bool isNearByDepartmentStore { get; set; } = false;

        [Display(Name = "是否鄰近學校")]
        public bool isNearBySchool { get; set; } = false;

        [Display(Name = "是否鄰近早市")]
        public bool isNearByMorningMarket { get; set; } = false;

        [Display(Name = "是否鄰近夜市")]
        public bool isNearByNightMarket { get; set; } = false;

        [Display(Name = "是否鄰近超商")]
        public bool isNearByConvenientStore { get; set; } = false;

        [Display(Name = "是否鄰近公園綠地")]
        public bool isNearByPark { get; set; } = false;

        [Display(Name = "是否垃圾集中處理")]
        public bool hasGarbageDisposal { get; set; } = false;

        [Display(Name = "是否浴室開窗")]
        public bool hasWindowInBathroom { get; set; } = false;

        [Display(Name = "是否有電梯")]
        public bool hasElevator { get; set; } = false;

        [Display(Name = "是否鄰近捷運")]
        public bool isNearMRT { get; set; } = false;

        [Display(Name = "鄰近捷運公里")]
        public string kmAwayMRT { get; set; }

        [Display(Name = "是否鄰近輕軌")]
        public bool isNearLRT { get; set; } = false;

        [Display(Name = "鄰近輕軌公里")]
        public string kmAwayLRT { get; set; }

        [Display(Name = "是否鄰近公車站")]
        public bool isNearBusStation { get; set; } = false;

        [Display(Name = "鄰近公車站公里")]
        public string kmAwayBusStation { get; set; }

        [Display(Name = "是否鄰近高鐵")]
        public bool isNearHSR { get; set; } = false;

        [Display(Name = "鄰近高鐵公里")]
        public string kmAwayHSR { get; set; }

        [Display(Name = "是否鄰近火車")]
        public bool isNearTrainStation { get; set; } = false;

        [Display(Name = "鄰近火車公里")]
        public string kmAwayTrainStation { get; set; }

        [Display(Name = "是否提供冷氣")]
        public bool hasAirConditioner { get; set; } = false;

        [Display(Name = "是否提供洗衣機")]
        public bool hasWashingMachine { get; set; } = false;

        [Display(Name = "是否提供冰箱")]
        public bool hasRefrigerator { get; set; } = false;

        [Display(Name = "是否提供衣櫃")]
        public bool hasCloset { get; set; } = false;

        [Display(Name = "是否提供桌椅")]
        public bool hasTableAndChair { get; set; } = false;

        [Display(Name = "是否提供熱水器")]
        public bool hasWaterHeater { get; set; } = false;

        [Display(Name = "是否提供網路")]
        public bool hasInternet { get; set; } = false;

        [Display(Name = "是否提供床")]
        public bool hasBed { get; set; } = false;

        [Display(Name = "是否提供電視")]
        public bool hasTV { get; set; } = false;

        [Display(Name = "水費繳納方式")]
        public paymentTypeOfWaterBill paymentMethodOfWaterBill { get; set; }

        [Display(Name = "每月水費價錢")]
        public string waterBillPerMonth { get; set; }

        [Display(Name = "電費計價方式")]
        public paymentTypeOfElectricBill electricBill { get; set; }

        [Display(Name = "電費繳納方式")]
        public paymentMethodOfElectricBill paymentMethodOfElectricBill { get; set; }

        [Display(Name = "管理費繳納方式")]
        public paymentMethodOfManagementFee paymentMethodOfManagementFee { get; set; }

        [Display(Name = "每月管理費價錢")]
        public string managementFeePerMonth { get; set; }

        [Display(Name = "每月租金")]
        public string rent { get; set; }

        [Display(Name = "押金")]
        public securityDepositType securityDeposit { get; set; }

        [Display(Name = "房源介紹")]
        public string description { get; set; }

        [Display(Name = "是否有租客限制")]
        public bool hasTenantRestrictions { get; set; } = false;

        [Display(Name = "設定租客性別限制")]
        public genderRestrictionType genderRestriction { get; set; }

        [Display(Name = "設定租客工作限制")]
        public string jobRestriction { get; set; }

        [Required]
        [Display(Name = "房源狀態")]
        public statusType status { get; set; } = statusType.未完成步驟1;

        [Display(Name = "建立時間")]
        public DateTime CreateAt { get; set; } = DateTime.Now;

        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<HouseImg> HouseImgs { get; set; }

    }
}