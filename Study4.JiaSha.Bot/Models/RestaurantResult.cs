using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Models
{
    public class RestaurantResult
    {
        public RestaurantResult()
        {

        }

        public IEnumerable<RestaurantDetail> Results { get; set; }
    }
}
