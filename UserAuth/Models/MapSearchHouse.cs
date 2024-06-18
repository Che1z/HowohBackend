using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UserAuth.Models
{
    public class MapSearchHouse
    {
        [Required]
        public double latitude { get; set; }
        [Required]
        public double longitude { get; set; }

        public double distance { get; set; }
        public int pageNumber { get; set; }
    }
}