using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminProductsController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: AdminProducts
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? categoryId, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "price" ? "price_desc" : "price";
            ViewBag.StockSortParm = sortOrder == "stock" ? "stock_desc" : "stock";
            ViewBag.DateSortParm = sortOrder == "date" ? "date_desc" : "date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = db.Categories.ToList();

            var products = db.Products.Include(p => p.Category);

            // Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.category_id == categoryId.Value);
                System.Diagnostics.Debug.WriteLine("Filtering by category: " + categoryId.Value);
            }

            // Tìm kiếm theo tên hoặc mô tả
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.name.Contains(searchString) || 
                                              p.description.Contains(searchString));
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.name);
                    break;
                case "price":
                    products = products.OrderBy(p => p.price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.price);
                    break;
                case "stock":
                    products = products.OrderBy(p => p.stock);
                    break;
                case "stock_desc":
                    products = products.OrderByDescending(p => p.stock);
                    break;
                case "date":
                    products = products.OrderBy(p => p.created_at);
                    break;
                case "date_desc":
                    products = products.OrderByDescending(p => p.created_at);
                    break;
                default:
                    products = products.OrderBy(p => p.name);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            
            return View(products.ToList());
        }

        // GET: AdminProducts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.id == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Lấy thêm thông tin đánh giá
            var reviews = db.Reviews.Where(r => r.product_id == id).Include(r => r.User).ToList();
            
            // Lấy thông tin doanh số
            var orderItems = db.OrderItems.Where(oi => oi.product_id == id).ToList();
            var totalSold = orderItems.Sum(oi => oi.quantity);
            var totalRevenue = orderItems.Sum(oi => oi.quantity * oi.unit_price);

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Reviews = reviews,
                TotalSold = totalSold,
                TotalRevenue = totalRevenue,
                AverageRating = reviews.Any() ? reviews.Average(r => r.rating) : 0
            };

            return View(viewModel);
        }

        // GET: AdminProducts/Create
        public ActionResult Create()
        {
            ViewBag.category_id = new SelectList(db.Categories, "id", "name");
            var model = new ProductFormViewModel();
            return View(model);
        }

        // POST: AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductFormViewModel productVM, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý tệp hình ảnh nếu được tải lên
                    string newFileName = null;
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(imageFile.FileName);
                        newFileName = Guid.NewGuid().ToString() + extension;
                        string folderPath = Server.MapPath("~/images/products");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        string path = Path.Combine(folderPath, newFileName);
                        imageFile.SaveAs(path);
                    }
                    
                    // Sử dụng ADO.NET trực tiếp để chèn sản phẩm - tránh xung đột với Entity Framework
                    using (var connection = new System.Data.SqlClient.SqlConnection(db.Database.Connection.ConnectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO Product (name, description, price, stock, prescription_required, category_id, created_at, image_url)
                                VALUES (@Name, @Description, @Price, @Stock, @PrescriptionRequired, @CategoryId, @CreatedAt, @ImageUrl);
                                SELECT SCOPE_IDENTITY();";
                        
                            command.Parameters.AddWithValue("@Name", productVM.Name);
                            command.Parameters.AddWithValue("@Description", productVM.Description ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@Price", productVM.Price);
                            command.Parameters.AddWithValue("@Stock", productVM.Stock);
                            command.Parameters.AddWithValue("@PrescriptionRequired", productVM.PrescriptionRequired);
                            command.Parameters.AddWithValue("@CategoryId", productVM.CategoryId);
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            command.Parameters.AddWithValue("@ImageUrl", newFileName ?? (object)DBNull.Value);
                            
                            // Thực thi câu lệnh và lấy ID sản phẩm mới
                            var productId = Convert.ToInt32(command.ExecuteScalar());
                        }
                    }
                    
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi khi thêm sản phẩm: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    }
                    
                    ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
                }
            }
            
            ViewBag.category_id = new SelectList(db.Categories, "id", "name", productVM.CategoryId);
            return View(productVM);
        }

        // GET: AdminProducts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            var productVM = new ProductFormViewModel
            {
                Id = product.id,
                Name = product.name,
                Description = product.description,
                Price = product.price,
                Stock = product.stock,
                PrescriptionRequired = product.prescription_required ?? false,
                CategoryId = product.category_id ?? 0,
                ImageUrl = product.image_url
            };
            
            ViewBag.category_id = new SelectList(db.Categories, "id", "name", product.category_id);
    
            return View(productVM);
        }

        // POST: AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductFormViewModel productVM, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy sản phẩm từ database
                    var product = db.Products.Find(productVM.Id);
                    if (product == null)
                    {
                        return HttpNotFound();
                    }

                    // Lưu ID danh mục trước khi cập nhật
                    int? originalCategoryId = product.category_id;
                    
                    // Cập nhật các thuộc tính
                    product.name = productVM.Name;
                    product.description = productVM.Description;
                    product.price = productVM.Price;
                    product.stock = productVM.Stock;
                    product.prescription_required = productVM.PrescriptionRequired;
                    
                    // Chỉ cập nhật category_id nếu nó thay đổi
                    if (originalCategoryId != productVM.CategoryId)
                    {
                        // Tách khỏi ngữ cảnh theo dõi hiện tại
                        db.Entry(product).State = EntityState.Detached;
                        
                        // Cập nhật thuộc tính
                        product.category_id = productVM.CategoryId;
                        
                        // Gắn lại và đánh dấu là đã sửa đổi
                        db.Products.Attach(product);
                        db.Entry(product).State = EntityState.Modified;
                    }

                    // Xử lý upload hình ảnh mới
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        // Xóa hình ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(product.image_url))
                        {
                            string oldImagePath = Path.Combine(Server.MapPath("~/images/products"), product.image_url);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string fileName = Path.GetFileName(imageFile.FileName);
                        string extension = Path.GetExtension(fileName);
                        string newFileName = Guid.NewGuid().ToString() + extension;
                        string path = Path.Combine(Server.MapPath("~/images/products"), newFileName);
                        
                        imageFile.SaveAs(path);
                        product.image_url = newFileName;
                    }

                    // Chỉ cập nhật các thuộc tính cụ thể
                    var entry = db.Entry(product);
                    entry.Property(p => p.name).IsModified = true;
                    entry.Property(p => p.description).IsModified = true;
                    entry.Property(p => p.price).IsModified = true;
                    entry.Property(p => p.stock).IsModified = true;
                    entry.Property(p => p.prescription_required).IsModified = true;
                    entry.Property(p => p.category_id).IsModified = true;
                    entry.Property(p => p.image_url).IsModified = imageFile != null && imageFile.ContentLength > 0;
                    
                    // Lưu thay đổi vào database
                    db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Bắt lỗi và ghi log
                    ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm: " + ex.Message);
                }
            }
            
            ViewBag.category_id = new SelectList(db.Categories, "id", "name", productVM.CategoryId);
            return View(productVM);
        }

        // GET: AdminProducts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.id == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }

        // POST: AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Product product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra xem sản phẩm đã được đặt hàng chưa
                var hasOrders = db.OrderItems.Any(oi => oi.product_id == id);
                if (hasOrders)
                {
                    TempData["ErrorMessage"] = "Không thể xóa sản phẩm vì đã có đơn hàng liên quan.";
                    return RedirectToAction("Delete", new { id = id });
                }

                // Xóa hình ảnh nếu có
                if (!string.IsNullOrEmpty(product.image_url))
                {
                    string imagePath = Path.Combine(Server.MapPath("~/images/products"), product.image_url);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa đánh giá trước
                var reviews = db.Reviews.Where(r => r.product_id == id).ToList();
                foreach (var review in reviews)
                {
                    db.Reviews.Remove(review);
                }

                db.Products.Remove(product);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
                return RedirectToAction("Delete", new { id = id });
            }
        }

        // GET: AdminProducts/Inventory
        public ActionResult Inventory()
        {
            var products = db.Products.Include(p => p.Category)
                            .OrderBy(p => p.stock)
                            .ToList();
            
            var viewModel = new InventoryViewModel
            {
                Products = products,
                LowStockCount = products.Count(p => p.stock <= 10 && p.stock > 0),
                OutOfStockCount = products.Count(p => p.stock == 0),
                TotalProducts = products.Count
            };
            
            return View(viewModel);
        }

        // POST: AdminProducts/UpdateStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock(int id, int newStock)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            product.stock = newStock;
            db.SaveChanges();
            
            return Json(new { success = true, message = "Cập nhật tồn kho thành công!" });
        }

        // GET: AdminProducts/Report
        public ActionResult Report()
        {
            var products = db.Products.Include(p => p.Category).ToList();
            var orderItems = db.OrderItems.Include(oi => oi.Order).ToList();
            
            // Nhóm theo danh mục
            var categorySalesData = products
                .GroupBy(p => p.Category.name)
                .Select(g => new {
                    Category = g.Key,
                    Count = g.Count(),
                    Revenue = orderItems.Where(oi => g.Select(p => p.id).Contains(oi.product_id ?? 0)).Sum(oi => oi.quantity * oi.unit_price)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Top sản phẩm bán chạy
            var topProducts = orderItems
                .GroupBy(oi => oi.product_id)
                .Select(g => new {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.quantity),
                    Revenue = g.Sum(oi => oi.quantity * oi.unit_price)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToList();

            var topProductsDetails = topProducts.Select(tp => {
                var product = products.FirstOrDefault(p => p.id == tp.ProductId);
                return new TopProductViewModel {
                    ProductId = tp.ProductId ?? 0,
                    ProductName = product?.name ?? "Unknown Product",
                    CategoryName = product?.Category?.name ?? "Unknown Category",
                    TotalSold = tp.TotalSold,
                    Revenue = tp.Revenue,
                    ImageUrl = product?.image_url
                };
            }).ToList();

            var viewModel = new ProductReportViewModel
            {
                TotalProducts = products.Count,
                TotalCategories = products.Select(p => p.category_id).Distinct().Count(),
                TotalRevenue = orderItems.Sum(oi => oi.quantity * oi.unit_price),
                TopSellingProducts = topProductsDetails,
                CategoryData = categorySalesData.Select(c => new CategorySalesViewModel {
                    CategoryName = c.Category ?? "Không phân loại",
                    ProductCount = c.Count,
                    Revenue = c.Revenue
                }).ToList()
            };
            
            return View(viewModel);
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