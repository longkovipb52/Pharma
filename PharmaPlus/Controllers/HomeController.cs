using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PharmaPlus.Models;
using System.Data.Entity;

namespace PharmaPlus.Controllers
{
    public class HomeController : Controller
    {
        private PharmaContext db = new PharmaContext();

        public ActionResult Index()
        {
            var viewModel = new HomeViewModel();
            
            try
            {
                // Thống kê cơ bản - ít khả năng gây lỗi
                viewModel.TotalProducts = db.Products.Count();
                viewModel.TotalCategories = db.Categories.Count();
                viewModel.TotalCustomers = db.Users.Count(u => u.role == "customer");
                viewModel.TotalOrders = db.Orders.Count();

                // Danh mục sản phẩm
                try
                {
                    viewModel.Categories = db.Categories
                        .OrderBy(c => c.name)
                        .Take(8)
                        .ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading categories: " + ex.Message);
                    viewModel.Categories = new List<Category>();
                }

                // Sản phẩm nổi bật (được đơn giản hóa)
                try
                {
                    viewModel.FeaturedProducts = db.Products
                        .Where(p => p.stock > 0)
                        .OrderByDescending(p => p.created_at)
                        .Take(8)
                        .ToList();

                    // Load Category và Reviews riêng để tránh lỗi
                    foreach (var product in viewModel.FeaturedProducts)
                    {
                        if (product.category_id.HasValue)
                        {
                            product.Category = db.Categories.Find(product.category_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading featured products: " + ex.Message);
                    viewModel.FeaturedProducts = new List<Product>();
                }

                // Sản phẩm bán chạy (đơn giản hóa)
                try
                {
                    // Thay thế bằng truy vấn đơn giản hơn
                    viewModel.BestSellingProducts = db.Products
                        .Where(p => p.stock > 0)
                        .OrderByDescending(p => p.created_at) // Tạm thời dùng ngày tạo thay cho bestselling
                        .Take(8)
                        .ToList();
                    
                    // Load Category riêng để tránh lỗi
                    foreach (var product in viewModel.BestSellingProducts)
                    {
                        if (product.category_id.HasValue)
                        {
                            product.Category = db.Categories.Find(product.category_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading bestselling products: " + ex.Message);
                    viewModel.BestSellingProducts = new List<Product>();
                }

                // Sản phẩm mới
                try
                {
                    viewModel.NewProducts = db.Products
                        .Where(p => p.stock > 0 && p.created_at != null)
                        .OrderByDescending(p => p.created_at)
                        .Take(8)
                        .ToList();
                    
                    // Load Category riêng để tránh lỗi
                    foreach (var product in viewModel.NewProducts)
                    {
                        if (product.category_id.HasValue)
                        {
                            product.Category = db.Categories.Find(product.category_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading new products: " + ex.Message);
                    viewModel.NewProducts = new List<Product>();
                }

                // Sản phẩm đánh giá cao (được đơn giản hóa đáng kể)
                try
                {
                    // Thay thế bằng truy vấn đơn giản hơn
                    viewModel.TopRatedProducts = db.Products
                        .Where(p => p.stock > 0)
                        .OrderByDescending(p => p.created_at) // Tạm thời dùng ngày tạo thay cho đánh giá cao
                        .Take(8)
                        .ToList();
                    
                    // Load Category riêng để tránh lỗi
                    foreach (var product in viewModel.TopRatedProducts)
                    {
                        if (product.category_id.HasValue)
                        {
                            product.Category = db.Categories.Find(product.category_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading top rated products: " + ex.Message);
                    viewModel.TopRatedProducts = new List<Product>();
                }

                // Đánh giá mới nhất
                try
                {
                    viewModel.LatestReviews = db.Reviews
                        .Where(r => r.comment != null && r.comment != "")
                        .OrderByDescending(r => r.created_at)
                        .Take(6)
                        .ToList();
                    
                    // Load User và Product riêng để tránh lỗi
                    foreach (var review in viewModel.LatestReviews)
                    {
                        if (review.user_id.HasValue)
                        {
                            review.User = db.Users.Find(review.user_id);
                        }
                        if (review.product_id.HasValue)
                        {
                            review.Product = db.Products.Find(review.product_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading latest reviews: " + ex.Message);
                    viewModel.LatestReviews = new List<Review>();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi và inner exception
                System.Diagnostics.Debug.WriteLine("HomeController Error: " + ex.Message);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                
                return View(viewModel); // Trả về model với dữ liệu đã được khởi tạo
            }
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

    // ViewModel cho trang chủ (không thay đổi)
    public class HomeViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        
        public List<Category> Categories { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Product> BestSellingProducts { get; set; }
        public List<Product> NewProducts { get; set; }
        public List<Product> TopRatedProducts { get; set; }
        public List<Review> LatestReviews { get; set; }

        public HomeViewModel()
        {
            Categories = new List<Category>();
            FeaturedProducts = new List<Product>();
            BestSellingProducts = new List<Product>();
            NewProducts = new List<Product>();
            TopRatedProducts = new List<Product>();
            LatestReviews = new List<Review>();
        }
    }
}