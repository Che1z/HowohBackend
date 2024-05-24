using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using UserAuth.Models.OrderEnumList;

namespace UserAuth.Models.ViewModel
{
    public class OrderInfoInput
    {
        /// <summary>
        /// 房源Id
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "房子")]
        public int houseId { get; set; }

        /// <summary>
        /// 租客Id
        /// </summary>
        [Display(Name = "租客")]
        public int? userId { get; set; }

        /// <summary>
        /// 租期開始時間
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "租期開始時間")]
        public DateTime leaseStartTime { get; set; }

        /// <summary>
        /// 租期結束時間
        /// </summary>
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "租期結束時間")]
        public DateTime leaseEndTime { get; set; }

        /// <summary>
        /// 租客電話
        /// </summary>
        [Display(Name = "租客電話")]
        [StringLength(10, MinimumLength = 10)]
        public string tenantTelphone { get; set; }

        /// <summary>
        /// 訂單狀態
        /// </summary>
        //[Required(ErrorMessage = "{0}必填")]
        //[Display(Name = "訂單狀態")]
        //public OrderStatus status { get; set; }
    }
}