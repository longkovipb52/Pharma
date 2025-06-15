using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PharmaPlus.Models;
using System.Data.Entity;

namespace PharmaPlus.Controllers
{
    public class ContactController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: Contact
        public ActionResult Index()
        {
            var viewModel = new ContactViewModel();
            
            // Nếu người dùng đã đăng nhập, điền sẵn thông tin
            if (Session["UserId"] != null)
            {
                var userId = (int)Session["UserId"];
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    viewModel.Name = user.full_name;
                    viewModel.Email = user.email;
                }
            }
            
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contact = new Contact
                {
                    name = model.Name,
                    email = model.Email,
                    subject = model.Subject,
                    message = model.Message,
                    status = "Chưa phản hồi",
                    created_at = DateTime.Now
                };
                
                // Nếu người dùng đã đăng nhập, lưu user_id
                if (Session["UserId"] != null)
                {
                    contact.user_id = (int)Session["UserId"];
                }
                
                db.Contacts.Add(contact);
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Thông tin liên hệ của bạn đã được gửi thành công. Chúng tôi sẽ phản hồi sớm nhất có thể.";
                return RedirectToAction("ThankYou");
            }
            
            // Nếu có lỗi validation, hiển thị lại form với thông báo lỗi
            return View("Index", model);
        }
        
        public ActionResult ThankYou()
        {
            if (TempData["SuccessMessage"] == null)
            {
                return RedirectToAction("Index");
            }
            
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }
        
        public ActionResult History()
        {
            // Chỉ cho người dùng đã đăng nhập xem lịch sử liên hệ của họ
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("History", "Contact") });
            }
            
            var userId = (int)Session["UserId"];
            var contacts = db.Contacts
                .Where(c => c.user_id == userId)
                .OrderByDescending(c => c.created_at)
                .ToList();
                
            // Thêm đoạn null check cho các thuộc tính này
            var viewModel = new ContactHistoryViewModel
            {
                Contacts = contacts.Select(c => new ContactHistoryItem
                {
                    Id = c.id,
                    Subject = c.subject,
                    Message = c.message,
                    Status = c.status,
                    CreatedAt = c.created_at ?? DateTime.Now,
                    Response = c.admin_reply,
                    RespondedAt = c.replied_at
                }).ToList(),
                TotalContacts = contacts.Count
            };
            
            return View(viewModel);
        }
    }
}