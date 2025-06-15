using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminContactsController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: AdminContacts
        public ActionResult Index(string searchTerm = null, string status = null, DateTime? fromDate = null, DateTime? toDate = null, int? page = null)
        {
            var filter = new AdminContactFilterViewModel
            {
                SearchTerm = searchTerm,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate
            };

            ViewBag.Filter = filter;

            // Thực hiện truy vấn cơ sở dữ liệu để lấy danh sách liên hệ
            var contacts = db.Contacts
                .Include(c => c.User)
                .AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(searchTerm))
            {
                contacts = contacts.Where(c =>
                    c.name.Contains(searchTerm) ||
                    c.email.Contains(searchTerm) ||
                    c.subject.Contains(searchTerm) ||
                    c.message.Contains(searchTerm) ||
                    (c.User != null && c.User.full_name.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrEmpty(status))
            {
                contacts = contacts.Where(c => c.status == status);
            }

            if (fromDate.HasValue)
            {
                contacts = contacts.Where(c => c.created_at >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Thêm 1 ngày để bao gồm cả ngày kết thúc
                var endDate = toDate.Value.AddDays(1);
                contacts = contacts.Where(c => c.created_at < endDate);
            }

            // Sắp xếp theo ngày tạo, mới nhất lên đầu
            contacts = contacts.OrderByDescending(c => c.created_at);

            // Chuyển đổi sang ViewModel
            var viewModel = contacts.Select(c => new AdminContactViewModel
            {
                Id = c.id,
                Name = c.name,
                Email = c.email,
                Subject = c.subject,
                Message = c.message,
                Status = c.status ?? "Chưa phản hồi",
                CreatedAt = c.created_at,
                UserId = c.user_id,
                UserFullName = c.User != null ? c.User.full_name : null
            }).ToList();

            return View(viewModel);
        }

        // GET: AdminContacts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var contact = db.Contacts
                .Include(c => c.User)
                .FirstOrDefault(c => c.id == id);

            if (contact == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AdminContactDetailsViewModel
            {
                Id = contact.id,
                Name = contact.name,
                Email = contact.email,
                Subject = contact.subject,
                Message = contact.message,
                Status = contact.status ?? "Chưa phản hồi",
                CreatedAt = contact.created_at,
                UserId = contact.user_id,
                UserFullName = contact.User != null ? contact.User.full_name : null,
                // Lấy thông tin từ database nếu có
                AdminReply = contact.admin_reply,
                RepliedAt = contact.replied_at
            };

            return View(viewModel);
        }

        // GET: AdminContacts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AdminContactEditViewModel
            {
                Id = contact.id,
                Name = contact.name,
                Email = contact.email,
                Subject = contact.subject,
                Message = contact.message,
                Status = contact.status ?? "Chưa phản hồi",
                CreatedAt = contact.created_at,
                AdminReply = contact.admin_reply // Lấy phản hồi cũ nếu có
            };

            return View(viewModel);
        }

        // POST: AdminContacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminContactEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = db.Contacts.Find(model.Id);
                if (contact == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật trạng thái liên hệ
                contact.status = model.Status;
                
                // Lưu phản hồi admin nếu có cột admin_reply trong database
                if (!string.IsNullOrEmpty(model.AdminReply))
                {
                    // Nếu bạn đã thêm cột admin_reply vào database
                    contact.admin_reply = model.AdminReply;
                    contact.replied_at = DateTime.Now;
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái liên hệ thành công.";
                return RedirectToAction("Details", new { id = model.Id });
            }

            return View(model);
        }

        // GET: AdminContacts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var contact = db.Contacts
                .Include(c => c.User)
                .FirstOrDefault(c => c.id == id);

            if (contact == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AdminContactViewModel
            {
                Id = contact.id,
                Name = contact.name,
                Email = contact.email,
                Subject = contact.subject,
                Message = contact.message,
                Status = contact.status ?? "Chưa phản hồi",
                CreatedAt = contact.created_at,
                UserId = contact.user_id,
                UserFullName = contact.User != null ? contact.User.full_name : null
            };

            return View(viewModel);
        }

        // POST: AdminContacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }

            db.Contacts.Remove(contact);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã xóa liên hệ thành công.";
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