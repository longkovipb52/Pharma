using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    public class ProductsController : Controller
    {
        private PharmaContext db = new PharmaContext();

        public ActionResult Index(string category = "", string search = "", int page = 1, string sortBy = "name")
        {
            var products = db.Products.Include(p => p.Category).AsQueryable();

            // ✅ FIX: Filter by category - SỬA LOGIC CHÍNH XÁC
            if (!string.IsNullOrEmpty(category))
            {
                // Thử cả tên category và ID
                int categoryId;
                if (int.TryParse(category, out categoryId))
                {
                    // Nếu category là số (ID)
                    products = products.Where(p => p.category_id == categoryId);
                    System.Diagnostics.Debug.WriteLine($"Filtering by category ID: {categoryId}");
                }
                else
                {
                    // Nếu category là tên
                    products = products.Where(p => p.Category != null && p.Category.name.Equals(category, StringComparison.OrdinalIgnoreCase));
                    System.Diagnostics.Debug.WriteLine($"Filtering by category name: {category}");
                }
            }

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.name.Contains(search) || 
                                              (p.description != null && p.description.Contains(search)) ||
                                              (p.Category != null && p.Category.name.Contains(search)));
            }

            // Debug sau khi filter
            var filteredCount = products.Count();
            System.Diagnostics.Debug.WriteLine($"Products count after filtering: {filteredCount}");
            
            // Nếu không có sản phẩm với filter, log để debug
            if (filteredCount == 0 && !string.IsNullOrEmpty(category))
            {
                var allCategories = db.Categories.Select(c => new { c.id, c.name }).ToList();
                System.Diagnostics.Debug.WriteLine("Available categories:");
                foreach (var cat in allCategories)
                {
                    System.Diagnostics.Debug.WriteLine($"  ID: {cat.id}, Name: '{cat.name}'");
                }
                
                var allProductCategories = db.Products.Where(p => p.Category != null)
                    .Select(p => new { p.category_id, CategoryName = p.Category.name })
                    .Distinct().ToList();
                System.Diagnostics.Debug.WriteLine("Product categories:");
                foreach (var pc in allProductCategories)
                {
                    System.Diagnostics.Debug.WriteLine($"  Category ID: {pc.category_id}, Name: '{pc.CategoryName}'");
                }
            }

            // Sorting
            switch (sortBy.ToLower())
            {
                case "price_asc":
                    products = products.OrderBy(p => p.price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.price);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.created_at ?? DateTime.MinValue);
                    break;
                case "popular":
                    products = products.OrderBy(p => p.name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.name);
                    break;
                case "name":
                case "name_asc":
                default:
                    products = products.OrderBy(p => p.name);
                    break;
            }

            // Pagination
            int pageSize = 12;
            var totalProducts = products.Count();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new ProductListViewModel
            {
                Products = pagedProducts,
                Categories = db.Categories.OrderBy(c => c.name).ToList(),
                CurrentCategory = category,
                CurrentSearch = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalProducts = totalProducts,
                SortBy = sortBy,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        public ActionResult Details(int id)
        {
            var product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.id == id);
                
            if (product == null)
            {
                return HttpNotFound();
            }
            
            // Chỉ lấy đánh giá đã được duyệt
            var reviews = db.Reviews
                .Include(r => r.User)
                .Where(r => r.product_id == id && r.status == "approved")
                .OrderByDescending(r => r.created_at)
                .ToList();
                
            // Tính rating trung bình
            if (reviews != null && reviews.Count > 0)
            {
                product.AverageRating = reviews.Average(r => r.rating);
            }
            else
            {
                product.AverageRating = 0;
            }

            // Kiểm tra người dùng hiện tại đã đánh giá chưa
            var userId = Session["UserId"] as int?;
            if (userId.HasValue)
            {
                product.UserReview = reviews.FirstOrDefault(r => r.user_id == userId.Value);
            }

            return View(product);
        }

        // API endpoint for AJAX quick view - SỬA LẠI ACTION NÀY
        [HttpGet]
        public ActionResult QuickView(int id)
        {
            try
            {
                var product = db.Products
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.id == id);

                if (product == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return new HttpStatusCodeResult(404, "Không tìm thấy sản phẩm");
                    }
                    return HttpNotFound();
                }

                // Nếu là AJAX request thì trả về PartialView
                if (Request.IsAjaxRequest())
                {
                    return PartialView("QuickView", product);
                }
                
                // Nếu không phải AJAX thì redirect về Details
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QuickView Error: {ex.Message}");
                
                if (Request.IsAjaxRequest())
                {
                    return new HttpStatusCodeResult(500, "Lỗi server");
                }
                return RedirectToAction("Index");
            }
        }

        // API endpoint for product search autocomplete
        [HttpGet]
        public JsonResult SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var suggestions = db.Products
                .Where(p => p.name.Contains(term))
                .Take(10)
                .Select(p => new { 
                    id = p.id,
                    name = p.name,
                    price = p.price,
                    image = p.image_url ?? "/images/products/default-product.jpg" // CẬP NHẬT đường dẫn
                })
                .ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        // Thêm action để test ảnh
        [HttpGet]
        public ActionResult TestImages()
        {
            var products = db.Products.Take(10).ToList();
            return View(products);
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