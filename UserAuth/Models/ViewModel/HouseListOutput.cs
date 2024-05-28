using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserAuth.Models.HouseEnumList;

namespace UserAuth.Models.ViewModel
{
    public class HouseListOutput
    {
    }

    public class EditingHouse //未完成
    {
        public int houseId;
        public string name;
        public string photo;
    }

    public class ForRentHouse //刊登中
    {
        public int houseId;
        public string name;
        public string photo;
        public string status = "";
        public string userName = "";
        public int? reservationCount;
    }

    public class LeasingHouse //已承租
    {
        public int houseId;
        public string name;
        public string photo;
        public DateTime leaseStartTime;
        public DateTime leaseEndTime;
    }

    public class DiscontinuedHouse //已完成
    {
        public int houseId;
        public string name;
        public string photo;
    }
}