using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserAuth.Models
{
    public class ReplyRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Display(Name = "訂單編號")]
        public int orderRatingId { get; set; }

        [JsonIgnore]
        [ForeignKey("orderRatingId")]
        [Display(Name = "訂單編號")]
        public virtual OrderRating orderRatingIdFK { get; set; }

        [Display(Name = "評論人員編號")]
        public int UserId { get; set; }

        [Display(Name = "評論人員編號")]
        [JsonIgnore]
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Display(Name = "評論內容")]
        public string ReplyComment { get; set; }

        public DateTime ReplyDate { get; set; } = DateTime.Now;
    }
}