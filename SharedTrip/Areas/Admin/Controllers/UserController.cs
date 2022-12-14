using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SharedTrip.Areas.Admin.Models.User;
using SharedTrip.Core.Contracts;
using SharedTrip.Core.Models.ServiceModels.User;
using SharedTrip.Core.Models.User;
using SharedTrip.Infrastructure.Data.Entities;

namespace SharedTrip.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService userService;
        private readonly INotyfService notyfService;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<UserController> logger;

        public UserController(
            IUserService userService,
            INotyfService notyfService,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<UserController> logger)
        {
            this.userService = userService;
            this.notyfService = notyfService;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<IActionResult> All([FromQuery] AllUsersQueryModel query)
        {
            try
            {
                var result = await this.userService.GetUsers(query.CurrentPage, AllUsersQueryModel.UsersPerPage);

                query.Users = result.Users;
                query.TotalUsersCount = result.TotalUsersCount;

                return View(query);
            }
            catch (Exception ex)
            {
                this.notyfService.Error("Something went wrong");
                this.logger.LogError($"{ex.GetType()}: {ex.Message}, \n {ex.StackTrace}");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            try
            {
                var user = await this.userService.GetUserForEditAsync(userId);

                if (user == null)
                {
                    this.notyfService.Error("The user does not exist");
                    return RedirectToAction(nameof(All));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                this.notyfService.Error("Something went wrong");
                this.logger.LogError($"{ex.GetType()}: {ex.Message}, \n {ex.StackTrace}");
                return RedirectToAction(nameof(All));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                await this.userService.EditUserAsync(model);

                this.notyfService.Success("Profile is edited successfully");
                return RedirectToAction(nameof(All));
            }
            catch (Exception ex)
            {
                this.notyfService.Error("Something went wrong");
                this.logger.LogError($"{ex.GetType()}: {ex.Message}, \n {ex.StackTrace}");
                return View(model);
            }
        }

        public async Task<IActionResult> Roles(string userId)
        {
            try
            {
                var user = await this.userService.GetUserByIdAsync(userId);
                var model = new UserRolesViewModel()
                {
                    UserId = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}"
                };

                ViewBag.RoleItems = roleManager.Roles
                    .ToList()
                    .Select(r => new SelectListItem()
                    {
                        Text = r.Name,
                        Value = r.Name,
                        Selected = userManager.IsInRoleAsync(user, r.Name).Result
                    }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                this.notyfService.Error("Something went wrong");
                this.logger.LogError($"{ex.GetType()}: {ex.Message}, \n {ex.StackTrace}");
                return RedirectToAction(nameof(All));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Roles(UserRolesViewModel model)
        {
            try
            {
                var user = await this.userService.GetUserByIdAsync(model.UserId);
                var userRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, userRoles);

                if (model.RoleNames?.Length > 0)
                {
                    await userManager.AddToRolesAsync(user, model.RoleNames);
                }

                this.notyfService.Success("You have changed the role successfully");
            }
            catch (Exception ex)
            {
                this.notyfService.Error("Something went wrong");
                this.logger.LogError($"{ex.GetType()}: {ex.Message}, \n {ex.StackTrace}");
            }

            return RedirectToAction(nameof(All));
        }
    }
}