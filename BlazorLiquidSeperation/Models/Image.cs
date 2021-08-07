using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorLiquidSeperation.Models
{
    public class Image
    {
        public string caption { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string copyright { get; set; }
        public string date { get; set; }
        public string clickUrl { get; set; }
        public Imageurls imageUrls { get; set; }
        public string descriptionPara2 { get; set; }
        public string descriptionPara3 { get; set; }
        public string isoDate { get; set; }



    }
}
