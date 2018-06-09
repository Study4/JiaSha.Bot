using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Models
{
    public class RestaurantDetail
    {
        public string Name { get; set; }
        public double Rating { get; set; }
        public List<string> Types { get; set; }
        public List<Photo> Photos { get; set; }
        public string Vicinity { get; set; }
    }
}
