using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.DataAccess.Data;
using Microsoft.AspNetCore.Identity;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class UserController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork uniUnitOfWork)
    {
        _unitOfWork = uniUnitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult RoleManagement(string userId)
    {
        var companies = _unitOfWork.Company.GetAll().ToList();
        List<IdentityRole> roles = _roleManager.Roles.ToList();

        RoleManagementVM roleVM = new()
        {
            ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company"),
            RoleList = roles.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Name
            }),
            CompanyList = companies.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            })
        };
        ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
        roleVM.ApplicationUser.Role = _userManager.GetRolesAsync(applicationUser).GetAwaiter().GetResult().FirstOrDefault();

        return View(roleVM);
    }

    [HttpPost]
    public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
    {
        ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);

        string oldRole = _userManager.GetRolesAsync(applicationUser).GetAwaiter().GetResult().FirstOrDefault();
        string newRole = roleManagementVM.ApplicationUser.Role;

        if (newRole != oldRole)
        {
            //a role was updated
            if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
            {
                applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
            }
            if (oldRole == SD.Role_Company)
            {
                applicationUser.CompanyId = null;
            }
            _unitOfWork.ApplicationUser.Update(applicationUser);
            _unitOfWork.Save();

            _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(applicationUser, newRole).GetAwaiter().GetResult();
        }
        else
        {
            if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
            {
                applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();
            }
        }
        return RedirectToAction("Index");
    }

    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        // Normally, I would user _unitOfWork to retrieve the data, but this time, I will do it by directly using ApplicationDbContext.
        
        List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();

        foreach (var user in objUserList)
        {
            user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

            // populate Company everywhere to avoid DataTable error
            if (user.Company == null)
            {
                user.Company = new()
                {
                    Name = ""
                };
            }
        }

        return Json(new { data = objUserList });
    }

    [HttpPost]
    public IActionResult LockUnlock([FromBody]string id)
    {
        var userFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
        if (userFromDb == null)
        {
            return Json(new { success = false, message = "Error while Locking/Unlocking" });
        }

        if (userFromDb.LockoutEnd != null && userFromDb.LockoutEnd > DateTime.Now)
        {
            // lock is in place. Remove it.
            userFromDb.LockoutEnd = DateTime.Now; // From now on, the user will be unlocked.
            
            _unitOfWork.ApplicationUser.Update(userFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "User Unlocked Successfully" });

        }
        else
        {
            // No lock Present. Apply Lock.
            userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            _unitOfWork.ApplicationUser.Update(userFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "User Locked Successfully" });
        }
    }
    #endregion
}
