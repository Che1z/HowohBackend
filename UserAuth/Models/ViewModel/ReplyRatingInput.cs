using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UserAuth.Models.ViewModel
{
    public class ReplyRatingInput
    {
        [Display(Name = "評論內容")]
        public string replyComment { get; set; }
    }
}