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
    public class AdminReviewsController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: AdminReviews
        public ActionResult Index(string searchTerm = null, int? productId = null, 
            int? minRating = null, int? maxRating = null, 
            DateTime? fromDate = null, DateTime? toDate = null, 
            string status = null, int? page = null)
        {
            // Lưu các tham số lọc vào ViewBag để hiển thị trong view
            var filter = new AdminReviewFilterViewModel
            {
                SearchTerm = searchTerm,
                ProductId = productId,
                MinRating = minRating,
                MaxRating = maxRating,
                FromDate = fromDate,
                ToDate = toDate,
                Status = status
            };
            
            ViewBag.Filter = filter;
            
            // Lấy danh sách sản phẩm cho dropdown lọc
            ViewBag.Products = db.Products
                .OrderBy(p => p.name)
                .Select(p => new SelectListItem
                {
                    Value = p.id.ToString(),
                    Text = p.name
                })
                .ToList();
            
            // Thêm mục "Tất cả sản phẩm" vào đầu danh sách
            ViewBag.Products.Insert(0, new SelectListItem { Value = "", Text = "-- Tất cả sản phẩm --" });
            
            // Thực hiện query
            var reviews = db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();
            
            // Áp dụng các điều kiện lọc
            if (!string.IsNullOrEmpty(searchTerm))
            {
                reviews = reviews.Where(r => 
                    r.User.full_name.Contains(searchTerm) || 
                    r.User.email.Contains(searchTerm) || 
                    r.Product.name.Contains(searchTerm) || 
                    r.comment.Contains(searchTerm)
                );
            }
            
            if (productId.HasValue)
            {
                reviews = reviews.Where(r => r.product_id == productId.Value);
            }
            
            if (minRating.HasValue)
            {
                reviews = reviews.Where(r => r.rating >= minRating.Value);
            }
            
            if (maxRating.HasValue)
            {
                reviews = reviews.Where(r => r.rating <= maxRating.Value);
            }
            
            if (fromDate.HasValue)
            {
                reviews = reviews.Where(r => r.created_at >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                reviews = reviews.Where(r => r.created_at < endDate);
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                reviews = reviews.Where(r => r.status == status);
            }
            
            // Sắp xếp theo ngày tạo, mới nhất lên đầu
            reviews = reviews.OrderByDescending(r => r.created_at);
            
            // Chuyển đổi sang view model
            var viewModels = reviews.ToList().Select(r => new AdminReviewViewModel
            {
                Id = r.id,
                UserId = r.user_id ?? 0,
                UserName = r.User != null ? r.User.full_name : "Người dùng không tồn tại",
                UserEmail = r.User != null ? r.User.email : "",
                ProductId = r.product_id ?? 0,
                ProductName = r.Product != null ? r.Product.name : "Sản phẩm không tồn tại",
                ProductImage = r.Product != null ? r.Product.image_url : null,
                Rating = r.rating,
                Comment = r.comment,
                CreatedAt = r.created_at,
                Status = r.status ?? "pending"
            }).ToList();
            
            return View(viewModels);
        }

        // GET: AdminReviews/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var review = db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefault(r => r.id == id);
                
            if (review == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new AdminReviewDetailsViewModel
            {
                Id = review.id,
                UserId = review.user_id ?? 0,
                UserName = review.User != null ? review.User.full_name : "Người dùng không tồn tại",
                UserEmail = review.User != null ? review.User.email : "",
                ProductId = review.product_id ?? 0,
                ProductName = review.Product != null ? review.Product.name : "Sản phẩm không tồn tại",
                ProductImage = review.Product != null ? review.Product.image_url : null,
                ProductPrice = review.Product != null ? review.Product.price : 0,
                Rating = review.rating,
                Comment = review.comment,
                CreatedAt = review.created_at,
                Status = review.status ?? "pending"
            };
            
            return View(viewModel);
        }

        // GET: AdminReviews/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var review = db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefault(r => r.id == id);
                
            if (review == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new AdminReviewEditViewModel
            {
                Id = review.id,
                UserId = review.user_id ?? 0,
                UserName = review.User != null ? review.User.full_name : "Người dùng không tồn tại",
                ProductId = review.product_id ?? 0,
                ProductName = review.Product != null ? review.Product.name : "Sản phẩm không tồn tại",
                Rating = review.rating,
                Comment = review.comment,
                CreatedAt = review.created_at,
                Status = review.status ?? "pending",
                SendNotification = false
            };
            
            return View(viewModel);
        }

        // POST: AdminReviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminReviewEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var review = db.Reviews.Find(viewModel.Id);
                if (review == null)
                {
                    return HttpNotFound();
                }
                
                // Cập nhật trạng thái
                review.status = viewModel.Status;
                
                // Lưu ghi chú của admin nếu cần
                if (!string.IsNullOrEmpty(viewModel.AdminNote))
                {
                    // Trong thực tế, bạn có thể cần thêm trường admin_note vào bảng Review
                    // Hoặc lưu vào bảng riêng
                }
                
                db.SaveChanges();
                
                // Gửi email thông báo nếu được chọn
                if (viewModel.SendNotification && review.User != null)
                {
                    // Trong thực tế, bạn sẽ cần thêm code gửi email ở đây
                    // SendEmail(review.User.email, "Cập nhật trạng thái đánh giá", "Đánh giá của bạn đã được cập nhật");
                }
                
                TempData["SuccessMessage"] = "Cập nhật trạng thái đánh giá thành công!";
                return RedirectToAction("Details", new { id = viewModel.Id });
            }
            
            return View(viewModel);
        }

        // GET: AdminReviews/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var review = db.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefault(r => r.id == id);
                
            if (review == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new AdminReviewDetailsViewModel
            {
                Id = review.id,
                UserId = review.user_id ?? 0,
                UserName = review.User != null ? review.User.full_name : "Người dùng không tồn tại",
                UserEmail = review.User != null ? review.User.email : "",
                ProductId = review.product_id ?? 0,
                ProductName = review.Product != null ? review.Product.name : "Sản phẩm không tồn tại",
                ProductImage = review.Product != null ? review.Product.image_url : null,
                Rating = review.rating,
                Comment = review.comment,
                CreatedAt = review.created_at,
                Status = review.status ?? "pending"
            };
            
            return View(viewModel);
        }

        // POST: AdminReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            
            db.Reviews.Remove(review);
            db.SaveChanges();
            
            TempData["SuccessMessage"] = "Xóa đánh giá thành công!";
            return RedirectToAction("Index");
        }
        
        // POST: AdminReviews/ApproveReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveReview(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            
            review.status = "approved";
            db.SaveChanges();
            
            TempData["SuccessMessage"] = "Đánh giá đã được duyệt!";
            return RedirectToAction("Details", new { id = id });
        }
        
        // POST: AdminReviews/RejectReview/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectReview(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            
            review.status = "rejected";
            db.SaveChanges();
            
            TempData["SuccessMessage"] = "Đánh giá đã bị từ chối!";
            return RedirectToAction("Details", new { id = id });
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