using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UserAuth.Models.ViewModel
{
    public class LogInInput
    {

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(10)]
        [Display(Name = "手機號碼")]
        public string telphone { get; set; }

        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(int.MaxValue)]
        public string password { get; set; }
    }
}