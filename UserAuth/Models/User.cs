using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using UserAuth.Models.UserEnumList;


namespace UserAuth.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "名")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "姓")]
        public string lastName { get; set; }

        [MaxLength(int.MaxValue)]
        [Display(Name = "信箱")]
        public string email { get; set; }

        [MaxLength(int.MaxValue)]
        public string salt { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(int.MaxValue)]
        public string password { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "手機號碼")]
        public string telphone { get; set; }

        [Display(Name = "性別")]
        public UserSexType gender { get; set; }

        [Display(Name = "職業")]
        public UserJob job { get; set; }

        [Display(Name = "照片")]
        public string photo { get; set; }

        [Display(Name = "角色")]
        [Required(ErrorMessage = "{0}必填")]
        public UserRoleType role { get; set; }

        [Display(Name = "平均分數")]
        public float averageRating { get; set; }

        [Display(Name = "評分筆數")]
        public int ratingCount { get; set;}
       
        public DateTime CreateAt { get; set; }
    }
}