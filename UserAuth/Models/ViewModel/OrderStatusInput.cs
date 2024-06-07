using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UserAuth.Models.ViewModel
{
    public class OrderStatusInput
    {
        /// <summary>
        /// 是否接受租約邀請
        /// </summary>
        [Display(Name = "接受租約邀請")]
        public bool acceptOrder { get; set; }
    }
}