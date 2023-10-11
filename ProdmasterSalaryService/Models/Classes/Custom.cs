using ProdmasterSalaryService.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace ProdmasterSalaryService.Models.Classes
{
    public class Custom : IDisanModel
    {
        [Key]
        public long Id { get; set; }
        [Newtonsoft.Json.JsonProperty("number")]
        public long DisanId { get; set; }
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }
        [Newtonsoft.Json.JsonProperty("salary")]
        public long Salary { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual User? User { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual List<Operation?>? Operations { get; set; }
        public virtual List<Shift?>? Shifts { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
