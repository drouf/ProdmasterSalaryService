using ProdmasterSalaryService.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ProdmasterSalaryService.Models.Classes
{
    public class Shift : IDisanModel
    {
        [Key]
        public long Id { get; set; }
        [Newtonsoft.Json.JsonProperty("number")]
        public long DisanId { get; set; }
        [Newtonsoft.Json.JsonProperty("object")]
        public long Object {  get; set; }
        [Newtonsoft.Json.JsonProperty("timebeg")]
        public DateTime? Start {  get; set; }
        [Newtonsoft.Json.JsonProperty("timeend")]
        public DateTime? End { get; set; }
        [Newtonsoft.Json.JsonProperty("coefficient")]
        public double? Coefficient { get; set; } = 1;
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
