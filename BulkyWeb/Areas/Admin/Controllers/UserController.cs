using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class UserController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
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

    [HttpDelete]
    public IActionResult Delete(int? id)
    {


        return Json(new { success = true, message = "Delete successful" });

    }

    #endregion


}
