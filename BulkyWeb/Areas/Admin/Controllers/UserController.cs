using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Bulky.DataAccess.Repository;
using Microsoft.AspNetCore.Identity;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult RoleManagement(string userId)
    {
        string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;

        var companies = _db.Companies.ToList();
        var roles = _db.Roles.ToList();

        RoleManagementVM roleVM = new()
        {
            ApplicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
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

        roleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

        return View(roleVM);
    }

    [HttpPost]
    public IActionResult RoleManagement(RoleManagementVM vm)
    {
        string roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == vm.ApplicationUser.Id).RoleId;
        string oldRole = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
        string newRole = vm.ApplicationUser.Role;

        if (oldRole != newRole)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == vm.ApplicationUser.Id);

            if (newRole == SD.Role_Company)
            {
                userFromDb.CompanyId = vm.ApplicationUser.CompanyId;
            } else
            {
                userFromDb.CompanyId = null;
            }

            _db.SaveChanges();
            _userManager.RemoveFromRoleAsync(userFromDb, oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(userFromDb, newRole).GetAwaiter().GetResult();
        }
        return RedirectToAction("Index");
    }



    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        // Normally, I would user _unitOfWork to retrieve the data, but this time, I will do it by directly using ApplicationDbContext.
        
        List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();

        foreach (var user in objUserList)
        {
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
            user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

            // populupate Com;pany everywheree to avoid DataTable error
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

        var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
        if (objFromDb == null)
        {
            return Json(new { success = false, message = "Error while Locking/Unlocking" });
        }

        if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
        {
            // lock is in place. Remove it.
            objFromDb.LockoutEnd = DateTime.Now; // From now on, the user will be unlocked.
            _db.SaveChanges();
            return Json(new { success = true, message = "User Unlocked Successfully" });

        }
        else
        {
            // No lock Present. Apply Lock.
            objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            _db.SaveChanges();
            return Json(new { success = true, message = "User Locked Successfully" });

        }
    }

    #endregion


}
