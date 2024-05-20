using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace UserAuth.Controllers
{
    public class TenantController : ApiController
    {
        // GET: api/Tenant
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Tenant/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Tenant
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Tenant/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Tenant/5
        public void Delete(int id)
        {
        }
    }
}
