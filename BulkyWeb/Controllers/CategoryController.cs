using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;
    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        List<Category> categoryList = _db.Categories.ToList();
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
            _db.Categories.Add(newCategory);
            _db.SaveChanges();
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
        Category? cat = _db.Categories.Find(id);
        if (cat == null) return NotFound();
        return View(cat);
    }
    
    [HttpPost]
    public IActionResult Edit(Category cat)
    {
        if (ModelState.IsValid)
        {
            _db.Categories.Update(cat);
            _db.SaveChanges();
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
        Category? cat = _db.Categories.Find(id);
        if (cat == null) return NotFound();
        return View(cat);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeletePOST(int? id)
    {
        Category? cat = _db.Categories.Find(id);
        if (cat == null)
        {
            return NotFound();
        }
        _db.Categories.Remove(cat);
        _db.SaveChanges();
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index");

    }
}
