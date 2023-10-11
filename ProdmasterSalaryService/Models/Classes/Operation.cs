using System.ComponentModel.DataAnnotations;
using ProdmasterSalaryService.Models.Interfaces;

namespace ProdmasterSalaryService.Models.Classes
{
    public class Operation : IDisanModel
    {
        [Key]
        public long Id { get; set; }
        [Newtonsoft.Json.JsonProperty("number")]
        public long DisanId { get; set; }
        [Newtonsoft.Json.JsonProperty("sum")]
        public double Sum { get; set; }
        [Newtonsoft.Json.JsonProperty("date")]
        public DateTime Date { get; set; }
        [Newtonsoft.Json.JsonProperty("saled")]
        public DateTime Saled { get; set; }
        [Newtonsoft.Json.JsonProperty("paid")]
        public DateTime Paid { get; set; }
        [Newtonsoft.Json.JsonProperty("object")]
        public long? Object { get; set; }
        [Newtonsoft.Json.JsonProperty("rem1")]
        public string? Note { get; set; } = string.Empty;
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Custom? Custom { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime? Created { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime? Modified { get; set; }
    }
}
