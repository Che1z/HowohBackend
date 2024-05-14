using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserAuth.Models
{
    [Table("HouseImg")]

    public class HouseImg
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "房屋")]
        public int houseId { get; set; }

        [JsonIgnore]
        [ForeignKey("houseId")]
        [Display(Name = "房屋")]
        public virtual House houseIdFK { get; set; }

        [Required]
        [Display(Name = "圖片名稱")]
        public string name { get; set; }

        [Required]
        [Display(Name = "圖片路徑")]
        public string path { get; set; }

        [Required]
        [Display(Name = "是否為封面")]
        public bool isCover { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreateAt { get; set; } = DateTime.Now;


    }
}