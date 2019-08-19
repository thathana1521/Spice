using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemViewModel { get; set; }

        public MenuItemController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemViewModel = new MenuItemViewModel()
            {
                Categories = _db.Category,
                MenuItem = new Models.MenuItem()
            };

        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View(MenuItemViewModel);
        }

        //POST - CREATE, We do not need any parameters because we use BindProperty MenuItemViewModel
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (ModelState.IsValid)
            {
                _db.MenuItem.Add(MenuItemViewModel.MenuItem);
                await _db.SaveChangesAsync();

                //Work on the image saving section

                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemViewModel.MenuItem.Id);
                if (files.Count > 0)
                {
                    //files has been uploaded

                    var uploads = Path.Combine(webRootPath, "images");
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var filesStream =
                        new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension),
                            FileMode.Create))
                    {
                        files[0].CopyTo(filesStream);
                    }

                    menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + extension;
                }
                else
                {
                    //no file was uploaded, so use default
                    var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                    System.IO.File.Copy(uploads,webRootPath+@"\images\"+MenuItemViewModel.MenuItem.Id + ".png");
                    menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + ".png";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(MenuItemViewModel);
            }
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemViewModel.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemViewModel.SubCategories = await _db.SubCategory
                .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemViewModel);
        }

        //POST - EDIT, We do not need any parameters because we use BindProperty MenuItemViewModel
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (ModelState.IsValid)
            {
                
                //Work on the image saving section

                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemViewModel.MenuItem.Id);
                if (files.Count > 0)
                {
                    //New image has been uploades

                    var uploads = Path.Combine(webRootPath, "images");
                    var extension_new = Path.GetExtension(files[0].FileName);

                    //delete original file
                    var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    //we will upload the new file
                    using (var filesStream =
                        new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension_new),
                            FileMode.Create))
                    {
                        files[0].CopyTo(filesStream);
                    }

                    menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + extension_new;
                }

                menuItemFromDb.Name = MenuItemViewModel.MenuItem.Name;
                menuItemFromDb.Description = MenuItemViewModel.MenuItem.Description;
                menuItemFromDb.Price = MenuItemViewModel.MenuItem.Price;
                menuItemFromDb.Spicyness = MenuItemViewModel.MenuItem.Spicyness;
                menuItemFromDb.CategoryId = MenuItemViewModel.MenuItem.CategoryId;
                menuItemFromDb.SubCategoryId = MenuItemViewModel.MenuItem.SubCategoryId;


                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                MenuItemViewModel.SubCategories = await _db.SubCategory
                    .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemViewModel);
            }
        }

        //GET - DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemViewModel.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemViewModel.SubCategories = await _db.SubCategory
                .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemViewModel);
        }

        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemViewModel.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);

            MenuItemViewModel.SubCategories = await _db.SubCategory
                .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();

            if (MenuItemViewModel.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemViewModel);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryTokenAttribute]
        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _db.MenuItem.FindAsync(id);
            string webRootPath = _hostingEnvironment.WebRootPath;
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemViewModel.MenuItem.Id);
            if (menuItem == null)
            {
                return View();
            }
            //delete Image from Server
            var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _db.MenuItem.Remove(menuItem);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}