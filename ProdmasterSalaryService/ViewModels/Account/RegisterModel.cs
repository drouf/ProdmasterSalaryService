using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ProdmasterSalaryService.ViewModels.Account
{
    public class RegisterModel
    {
        //[Required(ErrorMessage = "Не выбран кастом из дисана")]
        [DisplayName("ФИО в дисан")]
        public string? Name { get; set; } = string.Empty;
        public long DisanId { get; set; }

        [Required(ErrorMessage = "Не введен логин")]
        [DisplayName("Логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        [DisplayName("Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите пароль повторно")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DisplayName("Повторите пароль")]
        public string ConfirmPassword { get; set; }
    }
}
