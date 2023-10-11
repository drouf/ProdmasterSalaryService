using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Services.Interfaces;
using ProdmasterSalaryService.ViewModels.Operation;

namespace ProdmasterSalaryService.Controllers
{
    [Route("operation")]
    [Authorize]
    public class OperationController : Controller
    {
        private readonly ILogger<OperationController> _logger;
        private readonly IUserService _userService;
        private readonly  IOperationService _operationService;

        public OperationController(ILogger<OperationController> logger, IOperationService operationService, IUserService userService)
        {
            _logger = logger;
            _operationService = operationService;
            _userService = userService;
        }

        [HttpGet]

        public async Task<IActionResult> Index([FromQuery] long? id)
        {
            var user = await GetUser();
            //var operations = await _operationService.GetOperations().Result.OrderByDescending;

            var custom = user.Custom;

            if (user == null)
            {
                _logger.LogWarning($"Failed to get user, userName: {User.Identity?.Name};");
                return BadRequest("Failed to get user");
            }
            var operations = await _operationService.GetOperationsByCustom(custom);
            if (operations != null && operations.Any())
            {
                if (id != null)
                {
                    //return View("ViewSpecification", await _specificationService.GetSpecificationModel(user, id.Value));
                }

                return View(new OperationModel { Operations = operations.OrderByDescending(o => o.Paid).ToList() });
            }
            else
            {
                return View(new OperationModel { Operations = new List<Operation>() });
            }
        }

        [NonAction]
        private Task<User> GetUser()
        {
            var userLogin = User.Identity?.Name;
            return _userService.GetByLogin(userLogin);
        }
    }
}
