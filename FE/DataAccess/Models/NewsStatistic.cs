using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class NewsStatistic
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("dateString")]
        public string? DateString { get; set; }

        [JsonPropertyName("totalArticles")]
        public int TotalArticles { get; set; }

        [JsonPropertyName("activeArticles")]
        public int ActiveArticles { get; set; }

        [JsonPropertyName("inactiveArticles")]
        public int InactiveArticles { get; set; }
    }
}
