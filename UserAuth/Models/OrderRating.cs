using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserAuth.Models
{
    public class OrderRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Display(Name = "訂單編號")]
        public int orderId { get; set; }

        [JsonIgnore]
        [ForeignKey("orderId")]
        [Display(Name = "訂單編號")]
        public virtual Order orderIdFK { get; set; }
        [Display(Name = "評價人員編號")]
        [Required]
        public int UserId { get; set; }

        [Display(Name = "評價人員編號")]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Display(Name = "評分評論")]
        public string Comment { get; set; }

        [Display(Name = "評分")]
        public int Rating { get; set; }
        public DateTime RatingDate { get; set; }
        public ICollection<ReplyRating> Replies { get; set; }
    }
}