using System;
using System.Linq;
using System.Web.Mvc;
using PharmaPlus.Models;
using System.Data.Entity;

namespace PharmaPlus.Controllers
{
    public class ReviewController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: Review/History
        [Authorize]
        public ActionResult History()
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var userReviews = db.Reviews
                .Include(r => r.Product)
                .Where(r => r.user_id == userId.Value)
                .OrderByDescending(r => r.created_at)
                .ToList();

            return View(userReviews);
        }

        // GET: Review/Create/5 (5 là productId)
        [Authorize]
        public ActionResult Create(int? productId)
        {
            // Ghi log chi tiết
            System.Diagnostics.Debug.WriteLine($"Create action called with productId: {productId}");
            System.Diagnostics.Debug.WriteLine($"Raw URL: {Request.RawUrl}");
            
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Thêm code để lấy productId từ form data nếu có
            if (!productId.HasValue)
            {
                if (Request.QueryString["productId"] != null)
                {
                    int id;
                    if (int.TryParse(Request.QueryString["productId"], out id))
                    {
                        productId = id;
                        System.Diagnostics.Debug.WriteLine($"Found productId in query string: {productId}");
                    }
                }
            }

            // Nếu vẫn không tìm thấy productId
            if (!productId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sản phẩm để đánh giá";
                return RedirectToAction("Index", "OrderHistory");
            }

            // Kiểm tra sản phẩm
            var product = db.Products.Find(productId.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sản phẩm để đánh giá";
                return RedirectToAction("Index", "OrderHistory");
            }

            // Kiểm tra đánh giá đã tồn tại chưa
            var existingReview = db.Reviews
                .FirstOrDefault(r => r.user_id == userId.Value && r.product_id == productId.Value);

            if (existingReview != null)
            {
                return RedirectToAction("Edit", new { id = existingReview.id });
            }

            ViewBag.Product = product;
            return View();
        }

        // GET: Review/Edit/5 (5 là reviewId)
        [Authorize]
        public ActionResult Edit(int id)
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = db.Reviews
                .Include(r => r.Product)
                .FirstOrDefault(r => r.id == id && r.user_id == userId.Value);

            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
        }

        // POST: Review/SubmitReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReview(int productId, int rating, string comment, int? orderId = null)
        {
            try
            {
                // Ghi log chi tiết để debug
                System.Diagnostics.Debug.WriteLine($"SubmitReview called with productId={productId}, rating={rating}, comment={comment}");
                
                var userId = Session["UserId"] as int?;
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Sử dụng ADO.NET trực tiếp để có thông tin lỗi chi tiết hơn
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PharmaContext"].ConnectionString;
                
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");
                    
                    // Kiểm tra xem đánh giá đã tồn tại chưa
                    using (var checkCommand = new System.Data.SqlClient.SqlCommand(
                        "SELECT COUNT(*) FROM Review WHERE user_id = @userId AND product_id = @productId", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@userId", userId.Value);
                        checkCommand.Parameters.AddWithValue("@productId", productId);
                        
                        int count = (int)checkCommand.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Found {count} existing reviews");
                        
                        if (count > 0)
                        {
                            // Cập nhật đánh giá đã tồn tại - đặt lại trạng thái thành pending khi cập nhật
                            using (var updateCommand = new System.Data.SqlClient.SqlCommand(
                                "UPDATE Review SET rating = @rating, comment = @comment, created_at = @createdAt, status = @status " +
                                "WHERE user_id = @userId AND product_id = @productId", connection))
                            {
                                updateCommand.Parameters.AddWithValue("@rating", rating);
                                updateCommand.Parameters.AddWithValue("@comment", comment);
                                updateCommand.Parameters.AddWithValue("@createdAt", DateTime.Now);
                                updateCommand.Parameters.AddWithValue("@status", "pending"); // Đặt lại trạng thái pending khi cập nhật
                                updateCommand.Parameters.AddWithValue("@userId", userId.Value);
                                updateCommand.Parameters.AddWithValue("@productId", productId);
                                
                                int rowsAffected = updateCommand.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Updated {rowsAffected} rows");
                            }
                        }
                        else
                        {
                            // Thêm đánh giá mới với trạng thái pending
                            using (var insertCommand = new System.Data.SqlClient.SqlCommand(
                                "INSERT INTO Review (user_id, product_id, rating, comment, created_at, status) " +
                                "VALUES (@userId, @productId, @rating, @comment, @createdAt, @status)", connection))
                            {
                                insertCommand.Parameters.AddWithValue("@userId", userId.Value);
                                insertCommand.Parameters.AddWithValue("@productId", productId);
                                insertCommand.Parameters.AddWithValue("@rating", rating);
                                insertCommand.Parameters.AddWithValue("@comment", comment);
                                insertCommand.Parameters.AddWithValue("@createdAt", DateTime.Now);
                                insertCommand.Parameters.AddWithValue("@status", "pending"); // Thiết lập trạng thái pending
                                
                                int rowsAffected = insertCommand.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Inserted {rowsAffected} rows");
                            }
                        }
                    }
                }
                
                TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi thành công và đang chờ phê duyệt!";
                
                // Nếu có orderId, quay lại trang chi tiết đơn hàng
                if (orderId.HasValue)
                {
                    return RedirectToAction("Detail", "OrderHistory", new { id = orderId.Value });
                }
                
                // Nếu không có orderId, đến trang lịch sử đánh giá
                return RedirectToAction("History", "Review");
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                // Bắt lỗi SQL cụ thể
                System.Diagnostics.Debug.WriteLine($"SQL Error: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                
                TempData["ErrorMessage"] = "Lỗi cơ sở dữ liệu: " + sqlEx.Message;
                return RedirectToAction("Create", new { productId = productId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = "Lỗi khi gửi đánh giá: " + ex.Message;
                return RedirectToAction("Create", new { productId = productId });
            }
        }

        // GET: Review/Delete/5
        [Authorize]
        public ActionResult Delete(int id)
        {
            try
            {
                var userId = Session["UserId"] as int?;
                // Ghi log để debug
                System.Diagnostics.Debug.WriteLine($"Delete action called for review ID: {id}, User ID in session: {userId}");
                
                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện thao tác này.";
                    return RedirectToAction("Login", "Account");
                }

                // Tìm đánh giá bất kể user_id để xác định xem đánh giá có tồn tại không
                var reviewExists = db.Reviews.Find(id);
                if (reviewExists == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Review with ID: {id} does not exist");
                    TempData["ErrorMessage"] = "Không tìm thấy đánh giá này.";
                    return RedirectToAction("History");
                }
                
                // Kiểm tra xem đánh giá có thuộc về người dùng hiện tại không
                var review = db.Reviews
                    .Include(r => r.Product)
                    .FirstOrDefault(r => r.id == id && r.user_id == userId.Value);

                if (review == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Review with ID: {id} exists but does not belong to user ID: {userId}");
                    TempData["ErrorMessage"] = "Bạn không có quyền xóa đánh giá này.";
                    return RedirectToAction("History");
                }

                // Kiểm tra xem Product có null không
                if (review.Product == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Product is null for review ID: {id}");
                    // Cần lấy thông tin Product riêng để tránh lỗi null
                    review.Product = db.Products.Find(review.product_id) ?? new Product { name = "Sản phẩm không tồn tại" };
                }

                return View(review);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in Delete action: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("History");
            }
        }

        // POST: Review/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                // Ghi log ngay khi phương thức được gọi
                System.Diagnostics.Debug.WriteLine($"DeleteConfirmed called with id={id}");
                
                var userId = Session["UserId"] as int?;
                System.Diagnostics.Debug.WriteLine($"User ID from session: {userId}");
                
                if (!userId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("User not logged in");
                    return RedirectToAction("Login", "Account");
                }

                var review = db.Reviews.FirstOrDefault(r => r.id == id && r.user_id == userId.Value);
                if (review == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Review not found or doesn't belong to user {userId}");
                    TempData["ErrorMessage"] = "Không tìm thấy đánh giá hoặc bạn không có quyền xóa đánh giá này";
                    return RedirectToAction("History");
                }

                // Ghi log thông tin đánh giá sẽ bị xóa
                System.Diagnostics.Debug.WriteLine($"Found review to delete - ID: {review.id}, Product ID: {review.product_id}, User ID: {review.user_id}");
                
                db.Reviews.Remove(review);
                int result = db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SaveChanges result: {result} rows affected");
                
                TempData["SuccessMessage"] = "Đánh giá đã được xóa thành công!";
                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine($"Error in DeleteConfirmed: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa đánh giá: " + ex.Message;
                return RedirectToAction("History");
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
}