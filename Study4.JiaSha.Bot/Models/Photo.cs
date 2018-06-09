using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study4.JiaSha.Bot.Models
{
    public class Photo
    {
        [JsonProperty("Photo_Reference")]
        public string PhotoReference { get; set; }
    }
}
