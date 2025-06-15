using System;
using System.Collections.Generic; // Thêm dòng này
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PharmaPlus.Models;
using System.Security.Cryptography;
using System.Text;

namespace PharmaPlus.Controllers
{
    public class AccountController : Controller
    {
        private PharmaContext db = new PharmaContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Force UTF-8 encoding
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.HeaderEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "utf-8";
            
            base.OnActionExecuting(filterContext);
        }

        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u =>
                    u.username == model.Username || u.email == model.Username);

                if (user != null && VerifyPassword(model.Password, user.password))
                {
                    // Xử lý đăng nhập thành công
                    // Tạo ticket với thông tin người dùng
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                        1,                          // version
                        user.username,              // username
                        DateTime.Now,               // issue date
                        DateTime.Now.AddMinutes(model.RememberMe ? 2880 : 30), // expiration
                        model.RememberMe,           // persistent
                        user.role,                  // user data (roles)
                        FormsAuthentication.FormsCookiePath // cookie path
                    );

                    // Mã hóa ticket
                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                    // Tạo cookie
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    if (model.RememberMe)
                        cookie.Expires = ticket.Expiration;
                    cookie.Path = FormsAuthentication.FormsCookiePath;
                    cookie.HttpOnly = true; // Tăng tính bảo mật
                    cookie.Secure = FormsAuthentication.RequireSSL; // Bảo mật

                    // Thêm cookie vào response
                    Response.Cookies.Add(cookie);

                    // Lưu thông tin người dùng vào session
                    Session["UserId"] = user.id;
                    Session["Username"] = user.username;
                    Session["FullName"] = user.full_name;
                    Session["UserRole"] = user.role;

                    // Hợp nhất giỏ hàng
                    MergeCartAfterLogin(user.id);

                    // Chuyển hướng tới trang Admin Dashboard nếu là admin
                    if (user.role == "admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    // 🔥 FIX: Xử lý returnUrl tốt hơn
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validation bổ sung
                if (model.Password.Length < 6)
                {
                    ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự");
                    return View(model);
                }

                // Kiểm tra định dạng email
                if (!IsValidEmail(model.Email))
                {
                    ModelState.AddModelError("Email", "Email không đúng định dạng");
                    return View(model);
                }

                // Kiểm tra username đã tồn tại chưa
                var existingUsername = db.Users.FirstOrDefault(u => u.username == model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã được sử dụng");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = db.Users.FirstOrDefault(u => u.email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng");
                    return View(model);
                }

                try
                {
                    // Tạo người dùng mới
                    var user = new User
                    {
                        username = model.Username,
                        full_name = model.FullName,
                        email = model.Email,
                        password = HashPassword(model.Password),
                        role = "customer",
                        created_at = DateTime.Now
                    };

                    // Lưu vào cơ sở dữ liệu
                    db.Users.Add(user);
                    db.SaveChanges();

                    // Đăng nhập người dùng ngay lập tức sau khi đăng ký
                    FormsAuthentication.SetAuthCookie(user.username, false);

                    // Lưu thông tin người dùng vào session
                    Session["UserId"] = user.id;
                    Session["Username"] = user.username;
                    Session["FullName"] = user.full_name;
                    Session["UserRole"] = user.role;

                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài khoản: " + ex.Message);
                }
            }

            // Nếu đến đây có nghĩa là có lỗi xảy ra, hiển thị form lại
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            // Xóa thông tin phiên
            Session.Clear();
            Session.Abandon();

            // Xóa cookie xác thực
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Home");
        }

        // Cải tiến phương thức HashPassword
        private string HashPassword(string password)
        {
            // Sử dụng BCrypt thay vì SHA256 đơn giản
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Thêm phương thức verify password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            // Kiểm tra xem có phải là hash BCrypt không (chuỗi BCrypt thường bắt đầu bằng $2a$, $2b$ hoặc $2y$)
            if (hashedPassword.StartsWith("$2"))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // Xử lý lỗi salt parsing: so sánh trực tiếp
                    return password == hashedPassword;
                }
            }
            else
            {
                // Đối với mật khẩu đơn giản, so sánh trực tiếp
                return password == hashedPassword;
            }
        }

        // Phương thức kiểm tra email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // THÊM MỚI: Phương thức hợp nhất giỏ hàng sau khi đăng nhập
        private void MergeCartAfterLogin(int userId)
        {
            try
            {
                // Lấy giỏ hàng từ session
                var sessionCart = Session["Cart"] as List<CartItem>;
                if (sessionCart != null && sessionCart.Any())
                {
                    // Lấy giỏ hàng từ database (nếu có)
                    var dbCart = db.Carts.Where(c => c.user_id == userId).ToList();
                    
                    // Hợp nhất giỏ hàng
                    foreach (var item in sessionCart)
                    {
                        var existingItem = dbCart.FirstOrDefault(c => c.product_id == item.ProductId);
                        if (existingItem != null)
                        {
                            // Cập nhật số lượng nếu sản phẩm đã tồn tại
                            existingItem.quantity += item.Quantity;
                            existingItem.updated_at = DateTime.Now;
                        }
                        else
                        {
                            // Thêm mới nếu sản phẩm chưa tồn tại
                            var newCartItem = new Cart
                            {
                                user_id = userId,
                                product_id = item.ProductId,
                                quantity = item.Quantity,
                                updated_at = DateTime.Now
                            };
                            db.Carts.Add(newCartItem);
                        }
                    }
                    
                    // Lưu thay đổi
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Log lỗi, không gây ảnh hưởng đến quá trình đăng nhập
                System.Diagnostics.Debug.WriteLine($"Error merging cart after login: {ex.Message}");
            }
        }

        // Thêm vào AccountController.cs
        [HttpGet]
        public ActionResult CheckLoginStatus()
        {
            var isLoggedIn = Session["UserId"] != null;
            var userData = new
            {
                isLoggedIn = isLoggedIn,
                userId = Session["UserId"],
                username = Session["Username"]?.ToString(),
                fullName = Session["FullName"]?.ToString()
            };
            
            return Json(userData, JsonRequestBehavior.AllowGet);
        }

        // Thêm action sau để kiểm tra tình trạng đăng nhập
        [AllowAnonymous]
        public ActionResult CheckAuth()
        {
            var authStatus = new {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                SessionUserId = Session["UserId"],
                SessionUserName = Session["Username"],
                AuthCookieExists = Request.Cookies[FormsAuthentication.FormsCookieName] != null,
                CurrentUrl = Request.Url.ToString()
            };
            
            return Json(authStatus, JsonRequestBehavior.AllowGet);
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