using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using UserAuth.Models.HouseEnumList;

namespace UserAuth.Models.ViewModel
{
    public class HouseImgInput
    {
        [Display(Name = "房源狀態")]
        public statusType status { get; set; }

        public List<houseImgObject> files { get; set; }
    }

    //public class files
    //{
    //    public houseImgObject[] houseImgObjects;
    //}

    public class houseImgObject
    {
        //[Required]
        //[Display(Name = "圖片名稱")]
        //public string name { get; set; }

        [Required]
        [Display(Name = "圖片路徑")]
        public string path { get; set; }

        [Required]
        [Display(Name = "是否為封面")]
        public bool isCover { get; set; }
    }
}