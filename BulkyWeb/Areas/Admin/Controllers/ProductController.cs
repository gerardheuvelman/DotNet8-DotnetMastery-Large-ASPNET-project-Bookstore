using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    ///////////////////////////////////////////////////////////////////////////////////
    [HttpGet]
    public IActionResult Index()
    {
        List<Product> productList = _unitOfWork.Product.GetAll().ToList();
        return View(productList);
    }
    [HttpGet]
    public IActionResult Create()
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

        return View(vm);
    }

    [HttpPost]
    public IActionResult Create(ProductVM vm)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Product.Add(vm.Product);
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
    ///////////////////////////////////////////////////////////////////////////////////////////
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return BadRequest();
        }
        Product? obj = _unitOfWork.Product.Get(o => o.Id == id);
        if (obj == null) return NotFound();
        return View(obj);
    }

    [HttpPost]
    public IActionResult Edit(Product obj)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Product.Update(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product updated successfully";
            return RedirectToAction("Index");
        }
        return View();
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return BadRequest();
        }
        Product? obj = _unitOfWork.Product.Get(o => o.Id == id);
        if (obj == null) return NotFound();
        return View(obj);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePOST(int? id)
    {
        Product? obj = _unitOfWork.Product.Get(o => o.Id == id);
        if (obj == null)
        {
            return NotFound();
        }
        _unitOfWork.Product.Remove(obj);
        _unitOfWork.Save();
        TempData["success"] = "Product deleted successfully";
        return RedirectToAction("Index");
    }
    /////////////////////////////////////////////////////////////////////////////////////////////
}
