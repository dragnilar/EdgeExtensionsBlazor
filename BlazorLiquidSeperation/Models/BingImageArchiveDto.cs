using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorLiquidSeperation.Models
{
    public class BingImageArchiveDto
    {
        public string title { get; set; }
        public Data data { get; set; }
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
    }
}
