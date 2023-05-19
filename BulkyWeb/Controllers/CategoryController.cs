using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryRepository _repo;
    public CategoryController(ICategoryRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        List<Category> categoryList = _repo.GetAll().ToList();
        return View(categoryList);
    }
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category newCategory)
    {
        if (newCategory.Name == newCategory.DisplayOrder.ToString())
        {
            ModelState.AddModelError("Name", "The Display Order cannot exactly match the name");
        }
        if (ModelState.IsValid)
        {
            _repo.Add(newCategory);
            _repo.Save();
            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index", "Category");
        }
        return View();
    }
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0)
        {
            return BadRequest();
        }
        //Category cat = _db.Categories.FirstOrDefault(c => c.Id == id);
        //Category cat = _db.Categories.Where(c => c.Id == id).FirstOrDefault();
        Category? cat = _repo.Get(c => c.Id == id);
        if (cat == null) return NotFound();
        return View(cat);
    }
    
    [HttpPost]
    public IActionResult Edit(Category cat)
    {
        if (ModelState.IsValid)
        {
            _repo.Update(cat);
            _repo.Save();
            TempData["success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    [HttpGet]
    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return BadRequest();
        }
        Category? cat = _repo.Get(c => c.Id == id);
        if (cat == null) return NotFound();
        return View(cat);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePOST(int? id)
    {
        Category? cat = _repo.Get(c => c.Id == id);
        if (cat == null)
        {
            return NotFound();
        }
        _repo.Remove(cat);
        _repo.Save();
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index");

    }
}
