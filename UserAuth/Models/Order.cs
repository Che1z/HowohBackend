using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using UserAuth.Models.OrderEnumList;

namespace UserAuth.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "房子")]
        public int houseId { get; set; }

        [JsonIgnore] 
        [ForeignKey("houseId")]
        [Display(Name = "房子")]
        public virtual House houseIdFK { get; set; }

        [Display(Name = "租客")]
        public int userId { get; set; }

        [JsonIgnore]
        [ForeignKey("userId")]
        [Display(Name = "租客")]
        public virtual User userIdFK { get; set; }

        [Display(Name = "租期開始時間")]
        public DateTime leaseStartTime { get; set; }

        [Display(Name = "租期結束時間")]
        public DateTime leaseEndTime { get; set; }

        [Display(Name = "租客電話")]
        [StringLength(10, MinimumLength = 10)]
        public string tenantTelphone { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "訂單狀態")]
        public OrderStatus status { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreateAt { get; set; } = DateTime.Now;

        public ICollection<OrderRating> orderRatings { get; set; }

    }
}