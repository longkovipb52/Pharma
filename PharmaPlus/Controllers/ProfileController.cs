using System;
using System.Linq;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using PharmaPlus.Models;
using BCrypt.Net;

namespace PharmaPlus.Controllers
{
    public class ProfileController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: Profile
        public ActionResult Index()
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.Find(userId.Value);
            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            // Lấy thống kê đơn hàng
            var orderSummary = GetUserOrderSummary(userId.Value);

            var model = new ProfileViewModel
            {
                UserId = user.id,
                Username = user.username,
                FullName = user.full_name,
                Email = user.email,
                Role = user.role,
                CreatedAt = user.created_at
            };

            ViewBag.OrderSummary = orderSummary;
            return View(model);
        }

        // GET: Profile/EditProfile
        public ActionResult EditProfile()
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.Find(userId.Value);
            if (user == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                UserId = user.id,
                Username = user.username,
                FullName = user.full_name,
                Email = user.email,
                Role = user.role,
                CreatedAt = user.created_at
            };

            return View(model);
        }

        // POST: Profile/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(ProfileViewModel model)
        {
            try
            {
                var userId = Session["UserId"] as int?;
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = db.Users.Find(userId.Value);
                if (user == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin người dùng");
                    return View(model);
                }

                // Kiểm tra username đã tồn tại (trừ user hiện tại)
                var existingUser = db.Users.FirstOrDefault(u => u.username == model.Username && u.id != userId.Value);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã được sử dụng");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại (trừ user hiện tại)
                var existingEmail = db.Users.FirstOrDefault(u => u.email == model.Email && u.id != userId.Value);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Cập nhật thông tin (chỉ các trường có sẵn trong DB)
                user.username = model.Username;
                user.full_name = model.FullName;
                user.email = model.Email;

                db.SaveChanges();

                // Cập nhật session
                Session["Username"] = user.username;
                Session["FullName"] = user.full_name;

                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ EditProfile Error: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin: " + ex.Message);
                return View(model);
            }
        }

        // GET: Profile/ChangePassword
        public ActionResult ChangePassword()
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new ChangePasswordViewModel());
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var userId = Session["UserId"] as int?;
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = db.Users.Find(userId.Value);
                if (user == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin người dùng");
                    return View(model);
                }

                // Kiểm tra mật khẩu hiện tại
                if (!VerifyPassword(model.CurrentPassword, user.password))
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                    return View(model);
                }

                // Mã hóa mật khẩu mới
                user.password = HashPassword(model.NewPassword);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ChangePassword Error: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi đổi mật khẩu: " + ex.Message);
                return View(model);
            }
        }

        // GET: Profile/Orders
        public ActionResult Orders()
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Where(o => o.user_id == userId.Value)
                .OrderByDescending(o => o.created_at)
                .Take(10)
                .ToList();

            return View(orders);
        }

        // Helper methods
        private UserOrderSummary GetUserOrderSummary(int userId)
        {
            var orders = db.Orders.Where(o => o.user_id == userId).ToList();
            
            return new UserOrderSummary
            {
                TotalOrders = orders.Count,
                TotalSpent = orders.Sum(o => o.total_price),
                PendingOrders = orders.Count(o => o.status == "pending" || o.status == "Chờ xử lý" || o.status == "Đang xử lý"),
                CompletedOrders = orders.Count(o => o.status == "completed" || o.status == "Hoàn thành"),
                LastOrderDate = orders.OrderByDescending(o => o.created_at).FirstOrDefault()?.created_at
            };
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Kiểm tra xem mật khẩu có tiền tố của BCrypt không
                if (hashedPassword.StartsWith("$2a$") || hashedPassword.StartsWith("$2b$"))
                {
                    // Sử dụng BCrypt để xác thực
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                else
                {
                    // Trường hợp fallback cho các mật khẩu cũ không dùng BCrypt
                    return HashPassword(password) == hashedPassword;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
        }

        private string HashPassword(string password)
        {
            // Sử dụng BCrypt để băm mật khẩu mới
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(11));
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