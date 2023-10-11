using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ProdmasterSalaryService.ViewModels.Account
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Не указан старый пароль")]
        [DataType(DataType.Password)]
        [DisplayName("Старый пароль")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Не указан новый пароль")]
        [DataType(DataType.Password)]
        [DisplayName("Новый пароль")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Введите пароль повторно")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [DisplayName("Повторите пароль")]
        public string ConfirmNewPassword { get; set; }
    }
}
