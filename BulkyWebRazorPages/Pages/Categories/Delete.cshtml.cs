using BulkyWebRazorPages.Data;
using BulkyWebRazorPages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazorPages.Pages.Categories
{
    [BindProperties]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;


        public Category Category { get; set; }

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet(int? id)
        {
            if (id != null || id != 0)
            {
                Category = _db.Categories.Find(id);
            }
        }

        public IActionResult OnPost()
        {
            Category? cat = _db.Categories.Find(Category.Id);
            if (cat == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(Category);
            _db.SaveChanges();
            TempData["success"] = "Category updated successfully";
            return RedirectToPage("Index");

        }




    }
}
