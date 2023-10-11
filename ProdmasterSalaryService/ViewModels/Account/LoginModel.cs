using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ProdmasterSalaryService.ViewModels.Account
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан Email или ИНН")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        [DisplayName("Пароль")]
        public string Password { get; set; }
    }
}
