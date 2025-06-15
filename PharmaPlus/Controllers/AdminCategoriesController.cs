using System;
using System.Collections.Generic;  // Thêm dòng này
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminCategoriesController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: AdminCategories
        public ActionResult Index()
        {
            var categories = db.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.id,
                    Name = c.name,
                    Description = c.description,
                    ProductCount = c.Products.Count
                })
                .ToList();

            return View(categories);
        }

        // GET: AdminCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Thay đổi cách load dữ liệu để bao gồm Products
            var category = db.Categories
                .Include(c => c.Products)  // Load các sản phẩm cùng với danh mục
                .FirstOrDefault(c => c.id == id);
                
            if (category == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new CategoryDetailsViewModel
            {
                Id = category.id,
                Name = category.name,
                Description = category.description,
                // Thêm kiểm tra null để tránh NullReferenceException
                ProductCount = category.Products != null ? category.Products.Count : 0,
                Products = category.Products != null ? category.Products.Select(p => new ProductSummaryViewModel
                {
                    Id = p.id,
                    Name = p.name,
                    Price = p.price,
                    Stock = p.stock,
                    ImageUrl = p.image_url
                }).ToList() : new List<ProductSummaryViewModel>()
            };
            
            return View(viewModel);
        }

        // GET: AdminCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên danh mục đã tồn tại chưa
                if (db.Categories.Any(c => c.name == viewModel.Name))
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại");
                    return View(viewModel);
                }

                var category = new Category
                {
                    name = viewModel.Name,
                    description = viewModel.Description
                };

                db.Categories.Add(category);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        // GET: AdminCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new CategoryViewModel
            {
                Id = category.id,
                Name = category.name,
                Description = category.description
            };
            
            return View(viewModel);
        }

        // POST: AdminCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CategoryViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên danh mục đã tồn tại chưa (trừ chính nó)
                if (db.Categories.Any(c => c.name == viewModel.Name && c.id != viewModel.Id))
                {
                    ModelState.AddModelError("Name", "Tên danh mục đã tồn tại");
                    return View(viewModel);
                }
                
                var category = db.Categories.Find(viewModel.Id);
                if (category == null)
                {
                    return HttpNotFound();
                }
                
                category.name = viewModel.Name;
                category.description = viewModel.Description;
                
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
            }
            
            return View(viewModel);
        }

        // GET: AdminCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            // Thay vì Find, sử dụng Include để load Products cùng lúc
            var category = db.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.id == id);
                
            if (category == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new CategoryViewModel
            {
                Id = category.id,
                Name = category.name,
                Description = category.description,
                ProductCount = category.Products != null ? category.Products.Count : 0
            };
            
            return View(viewModel);
        }

        // POST: AdminCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Cũng cần Include Products để tránh NullReferenceException
            var category = db.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.id == id);
                
            if (category == null)
            {
                return HttpNotFound();
            }
            
            // Kiểm tra xem danh mục có đang chứa sản phẩm không
            if (category.Products != null && category.Products.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục này vì đang chứa sản phẩm!";
                return RedirectToAction("Delete", new { id = id });
            }
            
            db.Categories.Remove(category);
            db.SaveChanges();
            
            TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}