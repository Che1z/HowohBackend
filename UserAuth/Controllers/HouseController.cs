using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UserAuth.Models;
using UserAuth.Models.HouseEnumList;
using UserAuth.Models.UserEnumList;
using UserAuth.Security;

namespace UserAuth.Controllers
{
    public class HouseController : ApiController
    {
        private DBModel db = new DBModel();

        [HttpPost]
        [Route("api/createListing")]
        [JwtAuthFilters]
        public IHttpActionResult createListing()
        {
            //檢查是否為房東
            //取得使用者JWT
            var jwtObject = JwtAuthFilters.GetToken(Request.Headers.Authorization.Parameter);

            //取得JWT內部資料
            int UserId = (int)jwtObject["Id"];
            UserRoleType UserRole = (UserRoleType)jwtObject["Role"];

            try
            {
                if (UserRole != UserRoleType.房東)
                {
                    throw new Exception("該使用者不是房東，不可使用此功能");
                }
                //if (!ModelState.IsValid || house == null)
                //{
                //    throw new Exception("錯誤資訊不符合規範");
                //}
                using (DBModel db = new DBModel())
                {
                    //int houseUserId = house.userId;
                    //statusType houseStatus = house.status;
                    House InsertNewAccount = new House();
                    InsertNewAccount.userId = UserId;
                    InsertNewAccount.status = statusType.未完成步驟1;

                    db.HouseEntities.Add(InsertNewAccount);
                    db.SaveChanges();
                    return Content(HttpStatusCode.OK, "已成功新增房源");
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // GET: api/House
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/House/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/House
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/House/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/House/5
        //public void Delete(int id)
        //{
        //}
    }
}