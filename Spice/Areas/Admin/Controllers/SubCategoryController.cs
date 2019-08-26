using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        
        //GET - INDEX
        public async Task<IActionResult> Index()
        {
            //Include() sets what properties we want to load from SubCategory
            var subCategories = await _db.SubCategory.Include(s=>s.Category).ToListAsync();
            return View(subCategories);
        }

        //GET - CREATE
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                SubCategory = new SubCategory(),
                SubCategories = await _db.SubCategory.OrderBy(p=>p.Name).Select(p=>p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s =>
                    s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                //if subCategory already exists
                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error: SubCategory exists under " + doesSubCategoryExists.First().Category.Name +
                                    " category. Please use another name.";

                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryAndCategoryViewModel modelViewModel = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage

            };
            return View(modelViewModel);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories=new List<SubCategory>();
            subCategories = await (from subCategory in _db.SubCategory
                where subCategory.CategoryId == id
                select subCategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s =>
                    s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                //if subCategory already exists
                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error: SubCategory exists under " + doesSubCategoryExists.First().Category.Name +
                                    " category. Please use another name.";

                }
                else
                {
                    StatusMessage = "";
                    var subCategory = await _db.SubCategory.FindAsync(id);
                    subCategory.Name = model.SubCategory.Name;
                    
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryAndCategoryViewModel modelViewModel = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage

            };
            modelViewModel.SubCategory.Id = id;
            return View(modelViewModel);
        }

        //GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);

                if (subCategory == null)
                {
                    return NotFound();
                }

                SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
                {
                    Categories = await _db.Category.ToListAsync(),
                    SubCategory = subCategory,
                    SubCategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
                };

                return View(model);
            }

        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryTokenAttribute]
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return View();
            }

            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}