using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UserAuth.Models.ViewModel
{
    public class ContractInput
    {
        [Required(ErrorMessage = "{0}必填")]
        [Display(Name = "訂單編號")]
        public string orderId { get; set; }

        [Display(Name = "出租人")]
        public string landlordName { get; set; }

        [Display(Name = "承租人")]
        public string tenantName { get; set; }

        [Display(Name = "完整地址")]
        public string address { get; set; }

        [Display(Name = "定期付款日期")]
        public string contractPaymentBeforeDate { get; set; }

        [Display(Name = "終止合約需提前幾月告知")]
        public string contractTerminationNoticeMonth { get; set; }

        [Display(Name = "終止合約需支付幾月罰金")]
        public string contractTerminationPenaltyMonth { get; set; }
    }
}