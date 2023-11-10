using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProdmasterSalaryService.Models.Interfaces;

namespace ProdmasterSalaryService.Models.Classes
{
    public class User : IDisanModel
    {
        [Key]
        public long Id { get; set; }
        [Newtonsoft.Json.JsonProperty("number")]
        public long DisanId { get; set; }
        [Newtonsoft.Json.JsonProperty("name")]
        public string Login { get; set; }
        public string Password { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [ForeignKey("CustomId")]
        public long CustomId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Custom? Custom { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
