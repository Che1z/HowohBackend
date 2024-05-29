using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserAuth.Models
{
    [Table("Appointment")]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [Display(Name = "房子")]
        public int houseId { get; set; }

        [JsonIgnore]
        [ForeignKey("houseId")]
        [Display(Name = "房子")]
        public virtual House houseIdFK { get; set; }

        [Required]
        [Display(Name = "租客")]
        public int userId { get; set; }

        [Display(Name = "隱藏")]
        public Boolean hidden { get; set; }

        [JsonIgnore]
        [ForeignKey("userId")]
        [Display(Name = "租客")]
        public virtual User userIdFK { get; set; }

        // 0 : 預約房屋已變成為出租中(歷史預約)
        // 1 : 上架中房屋的預約資料
        [Display(Name = "是否有效")]
        public Boolean isValid { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}