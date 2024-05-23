using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserAuth.Models.OrderEnumList
{
    public class OrderEnum
    {
    }
    public enum OrderStatus
    {
        待租客回覆租約 = 1,
        租客已確認租約 = 2,
        租客已拒絕租約 = 3,
        租客非系統用戶 = 4
    }
}