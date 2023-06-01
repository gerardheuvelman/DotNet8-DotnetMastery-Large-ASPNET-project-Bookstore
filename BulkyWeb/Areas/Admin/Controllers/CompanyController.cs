using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class CompanyController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public CompanyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    ///////////////////////////////////////////////////////////////////////////////////
    public IActionResult Index()
    {
        List<Company> companyList = _unitOfWork.Company.GetAll().ToList();
        return View(companyList);
    }

    ///////////////////////////////////////////////////////////////////////////////////

    public IActionResult Upsert(int? id)
    {
        if (id == null ||  id == 0)
        {
            // Create action
            return View(new Company());
        } else
        {
            // Update action
            Company company = _unitOfWork.Company.Get(c => c.Id == id);
            return View(company);
        }
    }

    [HttpPost]
    public IActionResult Upsert(Company company)
    {
        if (ModelState.IsValid)
        {
            if (company.Id == 0)
            {
                _unitOfWork.Company.Add(company);
            } 
            else
            {
                _unitOfWork.Company.Update(company);
            }

            _unitOfWork.Save();
            TempData["success"] = "Company created successfully";
            return RedirectToAction("Index", "Company");
        } else
        {
            return View(company);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    
    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
        return Json( new { data = objCompanyList } );
    }
    ////////////////////////////////////////////////////////////////////////////////////////////

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        Company? companyToBeDeleted = _unitOfWork.Company.Get(o => o.Id == id);
        if (companyToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        _unitOfWork.Company.Remove(companyToBeDeleted);
        _unitOfWork.Save();

        return Json(new { success = true, message = "Delete successful" });

    }

    #endregion


}
