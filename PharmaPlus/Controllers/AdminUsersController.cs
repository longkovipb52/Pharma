using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminUsersController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: AdminUsers
        public ActionResult Index(string searchTerm = null, string role = null)
        {
            ViewBag.CurrentSearchTerm = searchTerm;
            ViewBag.CurrentRole = role;

            var users = db.Users.AsQueryable();

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => 
                    u.username.Contains(searchTerm) || 
                    u.email.Contains(searchTerm) || 
                    u.full_name.Contains(searchTerm)
                );
            }

            // Filter by role
            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.role == role);
            }

            // Get order counts for each user
            var userOrderCounts = db.Orders
                .GroupBy(o => o.user_id)
                .Select(g => new { UserId = g.Key, OrderCount = g.Count() })
                .ToList();

            // Convert to view models
            var viewModels = users.ToList().Select(u => new AdminUserViewModel
            {
                Id = u.id,
                Username = u.username,
                FullName = u.full_name,
                Email = u.email,
                Role = u.role,
                CreatedAt = u.created_at,
                OrderCount = userOrderCounts
                    .Where(c => c.UserId == u.id)
                    .Select(c => c.OrderCount)
                    .FirstOrDefault()
            }).ToList();

            return View(viewModels);
        }

        // GET: AdminUsers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get order count
            int orderCount = db.Orders.Count(o => o.user_id == id);

            // Get review count
            int reviewCount = db.Reviews.Count(r => r.user_id == id);

            // Get recent orders
            var recentOrders = db.Orders
                .Where(o => o.user_id == id)
                .OrderByDescending(o => o.created_at)
                .Take(5)
                .ToList()
                .Select(o => new AdminOrderSummaryViewModel
                {
                    Id = o.id,
                    TotalPrice = o.total_price,
                    Status = o.status,
                    CreatedAt = o.created_at
                }).ToList();

            // Get recent reviews with product names
            var recentReviews = db.Reviews
                .Where(r => r.user_id == id)
                .OrderByDescending(r => r.created_at)
                .Take(5)
                .ToList();

            var reviewViewModels = new List<AdminReviewSummaryViewModel>();
            foreach (var review in recentReviews)
            {
                var product = db.Products.Find(review.product_id);
                reviewViewModels.Add(new AdminReviewSummaryViewModel
                {
                    Id = review.id,
                    ProductId = review.product_id ?? 0,
                    ProductName = product != null ? product.name : "Sản phẩm không tồn tại",
                    Rating = review.rating,
                    Comment = review.comment,
                    CreatedAt = review.created_at
                });
            }

            var viewModel = new AdminUserDetailsViewModel
            {
                Id = user.id,
                Username = user.username,
                FullName = user.full_name,
                Email = user.email,
                Role = user.role,
                CreatedAt = user.created_at,
                OrderCount = orderCount,
                ReviewCount = reviewCount,
                RecentOrders = recentOrders,
                RecentReviews = reviewViewModels
            };

            return View(viewModel);
        }

        // GET: AdminUsers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AdminUserEditViewModel
            {
                Id = user.id,
                Username = user.username,
                FullName = user.full_name,
                Email = user.email,
                Role = user.role,
                ResetPassword = false
            };

            return View(viewModel);
        }

        // POST: AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminUserEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(viewModel.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Check if username is already taken by another user
                if (user.username != viewModel.Username && 
                    db.Users.Any(u => u.username == viewModel.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(viewModel);
                }

                // Check if email is already taken by another user
                if (user.email != viewModel.Email && 
                    db.Users.Any(u => u.email == viewModel.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(viewModel);
                }

                // Update user information
                user.username = viewModel.Username;
                user.full_name = viewModel.FullName;
                user.email = viewModel.Email;
                user.role = viewModel.Role;

                // Reset password if requested
                if (viewModel.ResetPassword && !string.IsNullOrEmpty(viewModel.NewPassword))
                {
                    // In a real application, you should hash the password
                    // This is a simplified example - in production use proper password hashing
                    user.password = BCrypt.Net.BCrypt.HashPassword(viewModel.NewPassword);
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin người dùng thành công!";
                return RedirectToAction("Details", new { id = viewModel.Id });
            }

            return View(viewModel);
        }

        // GET: AdminUsers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Get order count
            int orderCount = db.Orders.Count(o => o.user_id == id);

            // Get review count
            int reviewCount = db.Reviews.Count(r => r.user_id == id);

            var viewModel = new AdminUserViewModel
            {
                Id = user.id,
                Username = user.username,
                FullName = user.full_name,
                Email = user.email,
                Role = user.role,
                CreatedAt = user.created_at,
                OrderCount = orderCount
            };

            return View(viewModel);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Không xóa người dùng admin đầu tiên
            if (user.role == "admin" && user.id == 1)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản admin mặc định!";
                return RedirectToAction("Delete", new { id = id });
            }

            // Kiểm tra nếu người dùng có đơn hàng
            bool hasOrders = db.Orders.Any(o => o.user_id == id);
            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng này vì đã có đơn hàng!";
                return RedirectToAction("Delete", new { id = id });
            }

            // Xóa tất cả đánh giá của người dùng
            var reviews = db.Reviews.Where(r => r.user_id == id);
            foreach (var review in reviews)
            {
                db.Reviews.Remove(review);
            }

            // Xóa tất cả liên hệ của người dùng
            var contacts = db.Contacts.Where(c => c.user_id == id);
            foreach (var contact in contacts)
            {
                db.Contacts.Remove(contact);
            }

            // Xóa người dùng
            db.Users.Remove(user);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            return RedirectToAction("Index");
        }

        // GET: AdminUsers/Create
        public ActionResult Create()
        {
            var viewModel = new AdminUserEditViewModel
            {
                Role = "customer",
                ResetPassword = true
            };

            return View(viewModel);
        }

        // POST: AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminUserEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (db.Users.Any(u => u.username == viewModel.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(viewModel);
                }

                // Check if email already exists
                if (db.Users.Any(u => u.email == viewModel.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(viewModel);
                }

                // Require password for new user
                if (string.IsNullOrEmpty(viewModel.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu cho người dùng mới");
                    return View(viewModel);
                }

                // Create new user
                var user = new User
                {
                    username = viewModel.Username,
                    full_name = viewModel.FullName,
                    email = viewModel.Email,
                    role = viewModel.Role,
                    created_at = DateTime.Now,
                    // Hash password
                    password = BCrypt.Net.BCrypt.HashPassword(viewModel.NewPassword)
                };

                db.Users.Add(user);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Tạo người dùng mới thành công!";
                return RedirectToAction("Index");
            }

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