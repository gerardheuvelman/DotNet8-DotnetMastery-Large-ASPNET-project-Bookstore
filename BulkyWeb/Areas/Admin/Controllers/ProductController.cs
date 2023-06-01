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
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;
    }
    ///////////////////////////////////////////////////////////////////////////////////
    public IActionResult Index()
    {
        List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return View(productList);
    }

    ///////////////////////////////////////////////////////////////////////////////////

    public IActionResult Upsert(int? id)
    {
        ProductVM vm = new()
        {
            Product = new Product(),
            CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            })
        };
        if (id == null ||  id == 0)
        {
            // Create action
            return View(vm);
        } else
        {
            // Update action
            vm.Product = _unitOfWork.Product.Get(p => p.Id == id);
            return View(vm);
        }
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM vm, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file !=null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine( wwwRootPath, @"images\product");

                if (!string.IsNullOrEmpty(vm.Product.ImageUrl))
                {
                    // Delete the old file first
                    var oldImagePath = Path.Combine(wwwRootPath, vm.Product.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                using (FileStream fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                {
                   file.CopyTo(fileStream);
                    vm.Product.ImageUrl = @"\images\product\" + filename;
                };
            }

            if (vm.Product.Id == 0)
            {
                _unitOfWork.Product.Add(vm.Product);
            } 
            else
            {
                _unitOfWork.Product.Update(vm.Product);
            }

            _unitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index", "Product");
        } else
        {
            vm.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            return View(vm);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    
    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return Json( new { data = objProductList } );
    }
    ////////////////////////////////////////////////////////////////////////////////////////////

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        Product? productToBeDeleted = _unitOfWork.Product.Get(o => o.Id == id);
        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        // Delete the image on disk
        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));
        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }

        _unitOfWork.Product.Remove(productToBeDeleted);
        _unitOfWork.Save();

        return Json(new { success = true, message = "Delete successful" });

    }

    #endregion


}
