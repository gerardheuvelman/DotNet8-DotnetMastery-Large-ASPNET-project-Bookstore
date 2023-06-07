using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;

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
    public IActionResult Index()
    {
        List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return View(productList);
    }


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
        if (id == null || id == 0)
        {
            // Create action
            return View(vm);
        }
        else
        {
            // Update action
            vm.Product = _unitOfWork.Product.Get(p => p.Id == id, includeProperties: "ProductImages");
            return View(vm);
        }
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM vm, List<IFormFile> files)
    {
        if (ModelState.IsValid)
        {
            if (vm.Product.Id == 0)
            {
                _unitOfWork.Product.Add(vm.Product);
            }
            else
            {
                _unitOfWork.Product.Update(vm.Product);
            }

            _unitOfWork.Save();

            // now, handle images logic.

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (files != null)
            {
                foreach (IFormFile file in files)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"images\product\product-" + vm.Product.Id;
                    string finalPath = Path.Combine(wwwRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                    {
                        Directory.CreateDirectory(finalPath);
                    }

                    using (FileStream fileStream = new FileStream(Path.Combine(finalPath, filename), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    };

                    ProductImage productImage = new()
                    {
                        ImageUrl = @"\" + productPath + @"\" + filename,
                        ProductId = vm.Product.Id,
                    };

                    if (vm.Product.ProductImages == null)
                    {
                        vm.Product.ProductImages = new List<ProductImage>();
                    }
                    vm.Product.ProductImages.Add(productImage);

                    _unitOfWork.ProductImage.Add(productImage);
                }

                _unitOfWork.Product.Update(vm.Product);
                _unitOfWork.Save();


                //if (!string.IsNullOrEmpty(vm.Product.ImageUrl))
                //{
                //    // Delete the old file first
                //    var oldImagePath = Path.Combine(wwwRootPath, vm.Product.ImageUrl.TrimStart('\\'));
                //    if (System.IO.File.Exists(oldImagePath))
                //    {
                //        System.IO.File.Delete(oldImagePath);
                //    }
                //}

                //using (FileStream fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                //{
                //   file.CopyTo(fileStream);
                //    vm.Product.ImageUrl = @"\images\product\" + filename;
                //};
            }


            TempData["success"] = "Product Created/Updated Successfully";
            return RedirectToAction("Index", "Product");
        }
        else
        {
            vm.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            return View(vm);
        }
    }

    public IActionResult DeleteImage(int imageId)
    {
        // first a confirm popup

        ProductImage productImageFromDb = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
        if (productImageFromDb != null)
        {
            if (!string.IsNullOrEmpty(productImageFromDb.ImageUrl))
            {

                // Delete the image on disk
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                var oldImagePath = Path.Combine(wwwRootPath, productImageFromDb.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                // Delete the image from DB
                _unitOfWork.ProductImage.Remove(productImageFromDb);
                _unitOfWork.Save();

                // alert
                TempData["success"] = "Image Deleted Successfully";
            }

        }
        // return to the same page 
        return RedirectToAction(nameof(Upsert), new { id = productImageFromDb.ProductId }); // REMEMBER always pass parameters as a new object
    }


    #region API CALLS

    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return Json(new { data = objProductList });
    }

    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        Product? productToBeDeleted = _unitOfWork.Product.Get(o => o.Id == id);
        if (productToBeDeleted == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        string wwwRootPath = _webHostEnvironment.WebRootPath;
        string productPath = @"images\product\product-" + id;
        string finalPath = Path.Combine(wwwRootPath, productPath);

        if (Directory.Exists(finalPath))
        {
            //Delete all the images in the directory

            string[] paths = Directory.GetFiles(finalPath);
            foreach (string path in paths)
            {
                System.IO.File.Delete(path);
            }
            // Delete the directory
            Directory.Delete(finalPath);
        }
        _unitOfWork.Product.Remove(productToBeDeleted);
        _unitOfWork.Save();

        return Json(new { success = true, message = "Delete successful" });
    }
    #endregion

}
