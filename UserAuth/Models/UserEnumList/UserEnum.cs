using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserAuth.Models.UserEnumList
{
    public class UserEnum
    {
    }
    public enum UserRoleType
    {
        房東 = 0,
        租客 = 1
    }

    public enum UserSexType
    {
        男 = 0,
        女 = 1,        
    }

    public enum UserJob 
    {
        一般職員 = 0,
        農牧業 = 1,
        漁業 = 2,
        木材森林業 = 3,
        礦業採石業 = 4,
        交通運輸業 = 5,
        餐旅業 = 6,
        建築工程業 = 7,
        製造業 = 8,
        新聞廣告業 = 9,
        衛生保健 = 10,
        娛樂業 = 11,
        特種營業人員 = 12,
        文教機關 = 13,
        宗教團體 = 14,
        公共事業 =  15,
        一般商業 = 16,
        服務業 = 17,
        家庭管理 = 18,
        資訊業 = 19,
        軍人 = 20,
        運動人員 = 21,    
    }
}