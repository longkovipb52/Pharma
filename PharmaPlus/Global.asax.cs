using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Principal;
using PharmaPlus.Models;
using System.Diagnostics;

namespace PharmaPlus
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Kiểm tra kết nối đến cơ sở dữ liệu khi ứng dụng khởi động
            TestDbConnection();
        }
        
        private void TestDbConnection()
        {
            try
            {
                using (var db = new PharmaContext())
                {
                    // Kiểm tra xem cơ sở dữ liệu có tồn tại không
                    bool dbExists = db.Database.Exists();
                    
                    // Log kết quả
                    if (dbExists)
                    {
                        Debug.WriteLine("✅ Kết nối cơ sở dữ liệu thành công!");
                        // Thử đếm số lượng bản ghi trong các bảng
                        int userCount = db.Users.Count();
                        int productCount = db.Products.Count();
                        Debug.WriteLine($"   - Số lượng người dùng: {userCount}");
                        Debug.WriteLine($"   - Số lượng sản phẩm: {productCount}");
                    }
                    else
                    {
                        Debug.WriteLine("❌ Cơ sở dữ liệu không tồn tại hoặc không thể kết nối!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Lỗi kết nối cơ sở dữ liệu:");
                Debug.WriteLine($"   - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"   - Chi tiết: {ex.InnerException.Message}");
                }
            }
        }

        protected void Session_Start()
        {
            // Đảm bảo session timeout đủ lâu
            Session.Timeout = 30; // 30 phút
            
            // Khởi tạo cart nếu chưa có
            if (Session["Cart"] == null)
            {
                Session["Cart"] = new List<CartItem>();
            }
        }

        protected void Application_EndRequest()
        {
            // Chuyển hướng khi xảy ra lỗi 401
            if (Response.StatusCode == 401)
            {
                Response.Clear();
                string loginUrl = FormsAuthentication.LoginUrl + 
                                 "?returnUrl=" + 
                                 HttpUtility.UrlEncode(Request.Url.PathAndQuery);
                
                Response.Redirect(loginUrl);
            }
        }

        // Thêm phương thức này để xử lý vai trò trong Forms Authentication
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            // Kiểm tra xem người dùng đã xác thực chưa
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                if (HttpContext.Current.User.Identity is FormsIdentity)
                {
                    // Lấy identity từ Forms Authentication
                    FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
                    FormsAuthenticationTicket ticket = id.Ticket;
                    
                    // UserData chứa vai trò người dùng
                    string userData = ticket.UserData;
                    
                    // Tách chuỗi thành mảng các vai trò (nếu chỉ có 1 vai trò thì sẽ là mảng 1 phần tử)
                    string[] roles = userData.Split(',');
                    
                    // Tạo một principal mới với identity và vai trò
                    HttpContext.Current.User = new GenericPrincipal(id, roles);
                }
            }
        }
    }
}
