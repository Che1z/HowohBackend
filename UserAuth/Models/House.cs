using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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

        [MaxLength(100)]
        [Display(Name = "名稱")]
        public string name { get; set; }
    }
}