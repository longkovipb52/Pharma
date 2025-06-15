using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PharmaPlus.Models;
using Newtonsoft.Json;

namespace PharmaPlus.Controllers
{
    public class CartController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: Cart/Index - Hiển thị trang giỏ hàng
        public ActionResult Index()
        {
            // Điều hướng về trang Checkout nếu có sản phẩm trong giỏ
            var cart = Session["Cart"] as List<CartItem>;
            if (cart != null && cart.Any())
            {
                return RedirectToAction("Index", "Checkout");
            }
            
            // Nếu giỏ rỗng, điều hướng về trang Products
            return RedirectToAction("Index", "Products");
        }

        // POST: Cart/Add - Thêm sản phẩm vào giỏ
        [HttpPost]
        public ActionResult Add(int productId, int quantity = 1)
        {
            try
            {
                var product = db.Products.Find(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
        
                var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = product.name,
                        Price = product.price,
                        Quantity = quantity,
                        ImageUrl = product.image_url ?? "/Content/images/no-image.png"
                    });
                }

                // 🔥 FIX: Đảm bảo session được cập nhật
                Session["Cart"] = cart;
        
                // 🔥 DEBUG: Log để kiểm tra
                System.Diagnostics.Debug.WriteLine($"✅ Added to cart: {product.name} x{quantity}");
                System.Diagnostics.Debug.WriteLine($"📦 Total cart items: {cart.Sum(x => x.Quantity)}");

                return Json(new { 
                    success = true, 
                    message = "Đã thêm vào giỏ hàng",
                    totalItems = cart.Sum(x => x.Quantity)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Add to cart error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Cart/Update - Cập nhật số lượng
        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return Remove(productId);
                }

                var product = db.Products.FirstOrDefault(p => p.id == productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                if (quantity > product.stock)
                {
                    return Json(new { success = false, message = "Số lượng vượt quá tồn kho" });
                }

                var cart = GetCartFromSession();
                var item = cart.FirstOrDefault(x => x.ProductId == productId);

                if (item != null)
                {
                    item.Quantity = quantity;
                    SaveCartToSession(cart);

                    var totalItems = cart.Sum(x => x.Quantity);
                    var totalAmount = cart.Sum(x => x.SubTotal);

                    return Json(new 
                    { 
                        success = true, 
                        message = "Đã cập nhật giỏ hàng",
                        totalItems = totalItems,
                        totalAmount = totalAmount.ToString("N0") + " VNĐ",
                        itemSubTotal = item.SubTotal.ToString("N0") + " VNĐ"
                    });
                }

                return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update cart error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật" });
            }
        }

        // POST: Cart/UpdateQuantity - Cập nhật số lượng (AJAX)
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                // Add debug log
                System.Diagnostics.Debug.WriteLine($"UpdateQuantity called: productId={productId}, quantity={quantity}");

                if (!Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                if (quantity < 1)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
                }

                // USE consistent session cart management
                var cart = GetCartFromSession();
                var existingItem = cart.FirstOrDefault(x => x.ProductId == productId);
                
                // FIX: Kiểm tra existingItem trước khi xử lý
                if (existingItem == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng" });
                }
                
                // More debug
                System.Diagnostics.Debug.WriteLine($"Found item: {existingItem.ProductName}, current qty: {existingItem.Quantity}");

                // Check stock availability
                var product = db.Products.Find(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                if (quantity > product.stock)
                {
                    return Json(new { success = false, message = $"Chỉ còn {product.stock} sản phẩm trong kho" });
                }

                // Update quantity in session cart
                existingItem.Quantity = quantity;
                System.Diagnostics.Debug.WriteLine($"Updated quantity to: {quantity}");
                
                // IMPORTANT: Save cart to session
                SaveCartToSession(cart);
                System.Diagnostics.Debug.WriteLine($"Saved cart to session with {cart.Count} items");
                
                // Calculate totals
                var totalItems = cart.Sum(x => x.Quantity);
                var totalAmount = cart.Sum(x => x.SubTotal);
                
                // Debug response
                System.Diagnostics.Debug.WriteLine($"Returning success with totalItems: {totalItems}");
                
                // FIX: Đảm bảo response format nhất quán
                return Json(new
                {
                    success = true,
                    message = "Đã cập nhật số lượng sản phẩm",
                    totalItems = totalItems,
                    totalAmount = totalAmount,
                    formattedTotalAmount = totalAmount.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 UpdateQuantity ERROR: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật số lượng: " + ex.Message });
            }
        }

        // POST: Cart/Remove - Xóa sản phẩm khỏi giỏ
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                var cart = GetCartFromSession();
                var itemToRemove = cart.FirstOrDefault(x => x.ProductId == productId);

                if (itemToRemove == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không có trong giỏ hàng" });
                }

                cart.Remove(itemToRemove);
                SaveCartToSession(cart);

                var totalItems = cart.Sum(x => x.Quantity);
                var totalAmount = cart.Sum(x => x.SubTotal);

                return Json(new
                {
                    success = true,
                    message = "Đã xóa sản phẩm khỏi giỏ hàng",
                    totalItems = totalItems,
                    totalAmount = totalAmount,
                    formattedTotalAmount = totalAmount.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remove item error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa sản phẩm" });
            }
        }

        // POST: Cart/Clear - Xóa toàn bộ giỏ hàng
        [HttpPost]
        public ActionResult Clear()
        {
            try
            {
                if (!Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Invalid request" });
                }

                // Clear cart using consistent method
                Session["Cart"] = new List<CartItem>();

                return Json(new
                {
                    success = true,
                    message = "Đã xóa toàn bộ giỏ hàng",
                    totalItems = 0,
                    totalAmount = 0,
                    formattedTotalAmount = "0 VNĐ"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clear cart error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa giỏ hàng" });
            }
        }

        // GET: Cart/GetData - Lấy dữ liệu giỏ hàng cho AJAX
        [HttpGet]
        public ActionResult GetData()
        {
            try
            {
                var cartItems = GetCartItems();
                
                if (!Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Invalid request" }, JsonRequestBehavior.AllowGet);
                }

                // Thêm xử lý try-catch bên trong để tránh lỗi null
                try {
                    var result = new
                    {
                        success = true,
                        items = cartItems?.Select(item => new
                        {
                            productId = item.ProductId,
                            productName = item.ProductName,
                            price = item.Price,
                            quantity = item.Quantity,
                            subTotal = item.SubTotal,
                            imageUrl = item.ImageUrl ?? "/Content/images/no-image.png",
                            formattedPrice = item.Price.ToString("N0") + " VNĐ",
                            formattedSubTotal = item.SubTotal.ToString("N0") + " VNĐ"
                        })?.ToArray() ?? new object[0],
                        totalItems = cartItems?.Sum(x => x.Quantity) ?? 0,
                        totalAmount = cartItems?.Sum(x => x.SubTotal) ?? 0,
                        formattedTotalAmount = (cartItems?.Sum(x => x.SubTotal) ?? 0).ToString("N0") + " VNĐ"
                    };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner cart data error: {innerEx.Message}");
                    return Json(new {
                        success = true,
                        items = new object[0],
                        totalItems = 0,
                        totalAmount = 0,
                        formattedTotalAmount = "0 VNĐ",
                        error = "Có lỗi khi xử lý dữ liệu"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get cart data error: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải dữ liệu giỏ hàng: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper methods
        private List<CartItem> GetCartFromSession()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // 1. Đảm bảo method này luôn được gọi sau khi thay đổi cart
        private void SaveCartToSession(List<CartItem> cart)
        {
            Session["Cart"] = cart;
        }

        private void SaveCartItems(List<CartItemViewModel> cartItems)
        {
            // Convert CartItemViewModel back to CartItem for session storage
            var sessionCart = cartItems.Select(item => new CartItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            }).ToList();

            Session["Cart"] = sessionCart;
        }

        private List<CartItemViewModel> GetCartItems()
        {
            var cart = GetCartFromSession();
            var productIds = cart.Select(x => x.ProductId).ToList();
            
            if (!productIds.Any())
            {
                return new List<CartItemViewModel>();
            }

            var products = db.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.id))
                .ToList();

            var cartItems = cart.Select(cartItem =>
            {
                var product = products.FirstOrDefault(p => p.id == cartItem.ProductId);
                if (product == null) return null;

                return new CartItemViewModel
                {
                    ProductId = cartItem.ProductId,
                    ProductName = product.name,
                    CategoryName = product.Category?.name ?? "Khác",
                    Price = product.price,
                    Quantity = cartItem.Quantity,
                    ImageUrl = product.DisplayImageUrl,
                    SubTotal = product.price * cartItem.Quantity,
                    Stock = product.stock,
                    PrescriptionRequired = product.prescription_required ?? false
                };
            })
            .Where(x => x != null)
            .ToList();

            return cartItems;
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