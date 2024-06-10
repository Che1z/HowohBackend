using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UserAuth.Models.ViewModel
{
    public class OrderRatingInput
    {
        //[Display(Name = "訂單編號")]
        //public int orderId { get; set; }

        //[Display(Name = "評價人員編號")]
        //[Required]
        //public int userId { get; set; }

        [Display(Name = "評分評論")]
        public string comment { get; set; }

        [Display(Name = "評分")]
        public int rating { get; set; }
    }
}