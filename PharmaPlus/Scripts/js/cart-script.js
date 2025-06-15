// Cart functionality for PharmaPlus
let cartModal = null;
let cartData = null;

// Initialize cart functionality
document.addEventListener('DOMContentLoaded', function() {
    initializeCart();
    setupCartEventListeners();
    updateCartBadgeFromServer();
});

// Thêm vào phần initializeCart() - đảm bảo DOM được tạo đúng cách
function initializeCart() {
    cartModal = document.getElementById('cartModal');
    
    // Nếu không tìm thấy modal, tạo mới
    if (!cartModal) {
        createCartModal();
        cartModal = document.getElementById('cartModal');
    }
    
    if (cartModal) {
        cartModal.addEventListener('click', function(e) {
            if (e.target === cartModal) {
                closeCartModal();
            }
        });
    }
    
    // Đảm bảo có cartModalBody và cartModalFooter
    ensureCartModalElements();
    
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && cartModal && cartModal.classList.contains('show')) {
            closeCartModal();
        }
    });
    
    console.log('Cart system initialized');
}

// Thêm function mới để đảm bảo các thành phần DOM cần thiết
function ensureCartModalElements() {
    if (!cartModal) return;
    
    let cartModalBody = document.getElementById('cartModalBody');
    if (!cartModalBody) {
        const modalContent = cartModal.querySelector('.cart-modal-content');
        if (modalContent) {
            cartModalBody = document.createElement('div');
            cartModalBody.id = 'cartModalBody';
            cartModalBody.className = 'cart-modal-body';
            modalContent.appendChild(cartModalBody);
        }
    }
    
    let cartModalFooter = document.getElementById('cartModalFooter');
    if (!cartModalFooter) {
        const modalContent = cartModal.querySelector('.cart-modal-content');
        if (modalContent) {
            cartModalFooter = document.createElement('div');
            cartModalFooter.id = 'cartModalFooter';
            cartModalFooter.className = 'cart-modal-footer';
            cartModalFooter.innerHTML = `
                <div class="cart-modal-total">
                    <span>Tổng cộng:</span>
                    <span id="modalTotalAmount">0 VNĐ</span>
                </div>
                <div class="cart-modal-actions">
                    <button class="btn btn-outline" onclick="clearAllCart()">
                        <i class="fas fa-trash"></i> Xóa giỏ hàng
                    </button>
                    <button class="btn btn-primary" onclick="proceedToCheckout()">
                        <i class="fas fa-check"></i> Thanh toán
                    </button>
                </div>
            `;
            modalContent.appendChild(cartModalFooter);
        }
    }
}

// Thêm function để tạo modal nếu không tồn tại
function createCartModal() {
    const modalHTML = `
        <div id="cartModal" class="cart-modal">
            <div class="cart-modal-content">
                <div class="cart-modal-header">
                    <h3><i class="fas fa-shopping-cart"></i> Giỏ hàng</h3>
                    <button class="cart-modal-close" onclick="closeCartModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="cart-modal-body" id="cartModalBody">
                    <!-- Cart items will be loaded here -->
                </div>
                <div class="cart-modal-footer" id="cartModalFooter">
                    <div class="cart-modal-total">
                        <span>Tổng cộng:</span>
                        <span id="modalTotalAmount">0 VNĐ</span>
                    </div>
                    <div class="cart-modal-actions">
                        <button class="btn btn-outline" onclick="clearAllCart()">
                            <i class="fas fa-trash"></i> Xóa giỏ hàng
                        </button>
                        <button class="btn btn-primary" onclick="proceedToCheckout()">
                            <i class="fas fa-check"></i> Thanh toán
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHTML);
}

function openCartModal() {
    console.log('Opening CART modal');
    
    // Close quickview if open
    const quickviewModal = document.getElementById('quickviewModal');
    if (quickviewModal && quickviewModal.classList.contains('show')) {
        if (typeof closeQuickView === 'function') {
            closeQuickView();
        }
    }
    
    if (!cartModal) {
        cartModal = document.getElementById('cartModal');
    }
    
    if (!cartModal) {
        console.error('❌ Cart modal not found');
        alert('Không tìm thấy modal giỏ hàng!');
        return;
    }
    
    cartModal.classList.add('show');
    document.body.style.overflow = 'hidden';
    
    loadCartModal();
}

function closeCartModal() {
    console.log('Closing CART modal');
    
    if (cartModal) {
        cartModal.classList.remove('show');
        document.body.style.overflow = '';
        
        // FIX: Reset mọi style có thể ảnh hưởng đến các phần tử ngoài modal
        resetAllProductStyles();
    }
}

// THÊM: Hàm reset style cho tất cả sản phẩm
function resetAllProductStyles() {
    // Reset style cho các sản phẩm trên trang chính
    const productCards = document.querySelectorAll('.products-card');
    productCards.forEach(card => {
        card.style.pointerEvents = '';
        card.style.opacity = '';
        
        // Reset các button trong card
        const buttons = card.querySelectorAll('button');
        buttons.forEach(btn => {
            btn.style.pointerEvents = '';
            btn.style.opacity = '';
            btn.disabled = false;
        });
    });
    
    // Reset bất kỳ style nào đã được áp dụng cho add-to-cart buttons
    const addToCartButtons = document.querySelectorAll('.products-btn-add-cart, .btn-add-cart');
    addToCartButtons.forEach(btn => {
        btn.style.pointerEvents = '';
        btn.style.opacity = '';
        btn.disabled = false;
    });
    
    console.log('✅ Reset all product styles on modal close');
}

function updateCartBadge(totalItems) {
    console.log('🔄 Updating cart badge with total items:', totalItems);
    
    const badge = document.querySelector('#cartBtn .badge');
    if (badge) {
        badge.textContent = totalItems || 0;
        badge.style.display = totalItems > 0 ? 'flex' : 'none';
        console.log('✅ Cart badge updated to:', totalItems);
    } else {
        console.error('❌ Cart badge element not found');
    }
}

function updateCartBadgeFromServer() {
    console.log('🔄 Updating cart badge from server...');
    
    fetch('/Cart/GetData', {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        console.log('📦 Cart badge data from server:', data);
        
        if (data.success) {
            updateCartBadge(data.totalItems);
        } else {
            console.error('❌ Failed to get cart badge data:', data.message);
        }
    })
    .catch(error => {
        console.error('❌ Update cart badge error:', error);
    });
}

function setupCartEventListeners() {
    console.log('🎯 Setting up cart event listeners...');
    
    // Priority 1: Direct cart button binding
    const cartBtn = document.getElementById('cartBtn');
    if (cartBtn) {
        console.log('✅ Found cart button directly');
        
        // Remove ALL existing listeners
        const newCartBtn = cartBtn.cloneNode(true);
        cartBtn.parentNode.replaceChild(newCartBtn, cartBtn);
        
        // Add fresh listener
        newCartBtn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🛒 Cart button clicked directly');
            openCartModal();
        });
    }
    
    // Priority 2: Event delegation backup
    document.addEventListener('click', function(e) {
        // Handle cart button clicks via delegation
        if (e.target.id === 'cartBtn' || e.target.closest('#cartBtn')) {
            e.preventDefault();
            e.stopPropagation();
            console.log('🛒 Cart button clicked via delegation');
            openCartModal();
            return;
        }
        
        // Handle add to cart buttons - CHỈ SỬA Ở ĐÂY
        if (e.target.matches('.add-to-cart-btn, .products-btn-add-cart, .btn-add-cart:not(.quickview-btn)') ||
            e.target.closest('.add-to-cart-btn, .products-btn-add-cart, .btn-add-cart:not(.quickview-btn)')) {
            
            // Bỏ qua nếu là nút từ QuickView modal
            if (e.target.closest('.quickview-modal') || 
                (e.target.matches('.btn-add-cart.quickview-btn') || 
                e.target.closest('.btn-add-cart.quickview-btn'))) {
                console.log('👁️ QuickView button - SKIPPING delegation');
                return;
            }
            
            e.preventDefault();
            e.stopPropagation();
            
            const button = e.target.matches('button') ? e.target : e.target.closest('button');
            const productId = extractProductId(button);
            
            if (productId) {
                console.log('🛍️ Add to cart clicked for product:', productId);
                cartAddToCart(parseInt(productId), 1); // Luôn là số lượng 1 cho các nút thông thường
            }
            return;
        }
    }, true); // Sử dụng capture phase để có thể ngăn chặn trước khi xử lý
}

function extractProductId(button) {
    if (!button) return null;
    
    // Try data attributes
    if (button.dataset.productId) {
        return button.dataset.productId;
    }
    
    // Try onclick attribute
    const onclick = button.getAttribute('onclick');
    if (onclick) {
        const match = onclick.match(/\d+/);
        if (match) return match[0];
    }
    
    // Try parent card
    const card = button.closest('[data-product-id]');
    if (card) {
        return card.dataset.productId;
    }
    
    return null;
}

// Add to cart function - FIX: Đảm bảo quantity được sử dụng đúng
function cartAddToCart(productId, quantity = 1) {
    // Đảm bảo quantity là số
    quantity = parseInt(quantity || 1);
    
    // Log tham số cho debugging
    console.log(`🛒 Adding to cart: Product ID=${productId}, Quantity=${quantity}`);
    
    // Add animation to cart button
    const cartBtn = document.getElementById('cartBtn');
    if (cartBtn) {
        cartBtn.classList.add('cart-btn-pulse');
        setTimeout(() => cartBtn.classList.remove('cart-btn-pulse'), 700);
    }
    
    // FIX: Sử dụng URL encoded thay vì FormData
    const formData = `productId=${encodeURIComponent(productId)}&quantity=${encodeURIComponent(quantity)}`;
    
    // Send AJAX request với đúng Content-Type
    fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: formData // Sử dụng chuỗi URL encoded
    })
    .then(response => response.json())
    .then(data => {
        console.log('✅ Add to cart response:', data);
        
        if (data.success) {
            // Update cart badge
            updateCartBadge(data.totalItems);
            
            // Hiển thị số lượng trong thông báo
            showCartToast(`Đã thêm ${quantity} sản phẩm vào giỏ hàng`, 'success');
            
            // Force reload cart modal if open
            if (document.getElementById('cartModal')?.classList.contains('show')) {
                loadCartModal();
            }
        } else {
            showCartToast(data.message || 'Không thể thêm vào giỏ hàng', 'error');
        }
    })
    .catch(error => {
        console.error('❌ Add to cart error:', error);
        showCartToast('Có lỗi xảy ra khi thêm vào giỏ hàng', 'error');
    });
}

function animateCartButton() {
    const cartBtn = document.getElementById('cartBtn');
    if (cartBtn) {
        cartBtn.classList.add('cart-btn-pulse');
        
        setTimeout(() => {
            cartBtn.classList.remove('cart-btn-pulse');
        }, 600);
    }
}

// Đảm bảo làm mới dữ liệu từ server mỗi lần
function loadCartModal() {
    const cartModalBody = document.getElementById('cartModalBody');
    
    if (!cartModalBody) {
        console.error('Cart modal body not found');
        return;
    }
    
    // Show loading indicator
    cartModalBody.innerHTML = '<div class="loading-spinner"><i class="fas fa-spinner fa-spin"></i><p>Đang tải giỏ hàng...</p></div>';
    
    fetch('/Cart/GetData', {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('📦 Cart data received:', data);
        
        if (data.success) {
            const cartItems = data.items || [];
            cartData = data;
            
            if (cartItems.length === 0) {
                cartModalBody.innerHTML = `
                    <div class="empty-cart">
                        <i class="fas fa-shopping-cart"></i>
                        <p>Giỏ hàng của bạn đang trống</p>
                        <a href="/Products" class="btn btn-primary">Mua sắm ngay</a>
                    </div>
                `;
                
                // Update footer for empty cart
                updateCartModalFooter(0, "0 VNĐ", true);
            } else {
                // 🔥 FIX: Tạo HTML chi tiết cho từng sản phẩm
                const itemsHtml = cartItems.map(item => `
                    <div class="cart-item" data-product-id="${item.productId}">
                        <div class="cart-item-image">
                            <img src="${item.imageUrl}" alt="${item.productName}" />
                        </div>
                        <div class="cart-item-details">
                            <h4 class="cart-item-name">${item.productName}</h4>
                            <div class="cart-item-price">${item.formattedPrice}</div>
                        </div>
                        <div class="cart-item-quantity">
                            <button class="cart-quantity-btn cart-quantity-decrease" data-product-id="${item.productId}">
                                <i class="fas fa-minus"></i>
                            </button>
                            <input type="number" class="cart-quantity-input" value="${item.quantity}" min="1" data-product-id="${item.productId}" />
                            <button class="cart-quantity-btn cart-quantity-increase" data-product-id="${item.productId}">
                                <i class="fas fa-plus"></i>
                            </button>
                        </div>
                        <div class="cart-item-subtotal">${item.formattedSubTotal}</div>
                        <button class="cart-item-remove" data-product-id="${item.productId}" title="Xóa sản phẩm">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                `).join('');
                
                cartModalBody.innerHTML = `
                    <div class="cart-items-container">
                        ${itemsHtml}
                    </div>
                `;
                
                // Update footer with totals
                updateCartModalFooter(data.totalItems, data.formattedTotalAmount, false);
                
                // 🔥 FIX: Thiết lập lại event listeners cho các nút trong modal
                setupCartItemControls();
            }
            
            // Update badge
            updateCartBadge(data.totalItems);
        } else {
            cartModalBody.innerHTML = `
                <div class="cart-error">
                    <i class="fas fa-exclamation-triangle"></i>
                    <p>Có lỗi xảy ra: ${data.message || 'Không thể tải giỏ hàng'}</p>
                    <button class="btn btn-outline" onclick="loadCartModal()">Thử lại</button>
                </div>
            `;
        }
    })
    .catch(error => {
        console.error('❌ Error loading cart:', error);
        cartModalBody.innerHTML = `
            <div class="cart-error">
                <i class="fas fa-exclamation-triangle"></i>
                <p>Có lỗi xảy ra khi tải giỏ hàng: ${error.message}</p>
                <button class="btn btn-outline" onclick="loadCartModal()">Thử lại</button>
            </div>
        `;
    });
}

// Thêm helper function
function updateCartModalFooter(totalItems, formattedAmount, disableCheckout) {
    const cartModalFooter = document.getElementById('cartModalFooter');
    if (cartModalFooter) {
        const totalItemsElement = document.getElementById('modalTotalItems');
        const totalAmountElement = document.getElementById('modalTotalAmount');
        
        if (totalItemsElement) totalItemsElement.textContent = totalItems;
        if (totalAmountElement) totalAmountElement.textContent = formattedAmount;
        
        const checkoutButton = cartModalFooter.querySelector('.btn-primary');
        if (checkoutButton) {
            checkoutButton.disabled = disableCheckout;
        }
    }
}

function proceedToCheckout() {
    // 🔥 THÊM: Kiểm tra đăng nhập trước khi thanh toán
    fetch('/Account/CheckLoginStatus', {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (!data.isLoggedIn) {
            // Hiển thị modal yêu cầu đăng nhập
            showLoginRequiredModal();
        } else {
            // Đã đăng nhập, chuyển đến trang thanh toán
            window.location.href = '/Checkout';
        }
    })
    .catch(error => {
        console.error('Error checking login status:', error);
        // Fallback: chuyển đến checkout và để server xử lý
        window.location.href = '/Checkout';
    });
}

// 🔥 THÊM: Hàm hiển thị modal yêu cầu đăng nhập
function showLoginRequiredModal() {
    // Tạo modal yêu cầu đăng nhập
    const modalHtml = `
        <div id="loginRequiredModal" class="auth-required-modal">
            <div class="auth-required-content">
                <div class="auth-required-header">
                    <h3><i class="fas fa-sign-in-alt"></i> Yêu cầu đăng nhập</h3>
                    <button class="auth-required-close" onclick="closeLoginRequiredModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="auth-required-body">
                    <div class="auth-required-icon">
                        <i class="fas fa-user-lock"></i>
                    </div>
                    <p>Bạn cần đăng nhập để thực hiện thanh toán.</p>
                    <p>Đăng nhập ngay để tiếp tục mua sắm!</p>
                </div>
                <div class="auth-required-footer">
                    <button class="btn btn-outline" onclick="closeLoginRequiredModal()">
                        <i class="fas fa-times"></i> Tiếp tục mua sắm
                    </button>
                    <button class="btn btn-primary" onclick="redirectToLogin()">
                        <i class="fas fa-sign-in-alt"></i> Đăng nhập ngay
                    </button>
                </div>
            </div>
        </div>
    `;
    
    // Thêm modal vào body
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Hiển thị modal
    document.getElementById('loginRequiredModal').style.display = 'flex';
}

// 🔥 THÊM: Đóng modal yêu cầu đăng nhập
function closeLoginRequiredModal() {
    const modal = document.getElementById('loginRequiredModal');
    if (modal) {
        modal.remove();
    }
}

// 🔥 THÊM: Chuyển hướng đến trang đăng nhập
function redirectToLogin() {
    const returnUrl = encodeURIComponent('/Checkout');
    window.location.href = `/Account/Login?returnUrl=${returnUrl}`;
}

// Chỉ giữ lại một phần export để tránh xung đột
(function() {
    // Đảm bảo tất cả functions cần thiết được export
    window.cartAddToCart = cartAddToCart;
    window.openCartModal = openCartModal;
    window.closeCartModal = closeCartModal;
    window.loadCartModal = loadCartModal;
    window.updateCartBadge = updateCartBadge;
    window.updateCartBadgeFromServer = updateCartBadgeFromServer;
    window.showCartToast = showCartToast;
    window.proceedToCheckout = proceedToCheckout;
    window.clearAllCart = clearAllCart;
    
    // Hai hàm quan trọng cho tăng/giảm số lượng
    window.updateCartQuantity = updateCartQuantity;
    window.removeFromCart = removeFromCart;
    
    console.log('🛒 Cart script functions exported to global scope');
    console.log('Available functions:', Object.keys(window).filter(key => 
        key.startsWith('cart') || 
        key.includes('Cart') || 
        key === 'updateCartQuantity' || 
        key === 'removeFromCart'
    ));
})();

// Make sure cart modal is properly styled and positioned
document.addEventListener('DOMContentLoaded', function() {
    // Fix cart modal styling
    const cartModal = document.getElementById('cartModal');
    if (cartModal) {
        cartModal.style.display = 'none';
        cartModal.style.position = 'fixed';
        cartModal.style.top = '0';
        cartModal.style.left = '0';
        cartModal.style.width = '100vw';
        cartModal.style.height = '100vh';
        cartModal.style.zIndex = '10000';
        cartModal.style.backgroundColor = 'rgba(0,0,0,0.75)';
        
        // Set flex properties for centering
        cartModal.addEventListener('transitionend', function() {
            if (cartModal.classList.contains('show')) {
                cartModal.style.display = 'flex';
                cartModal.style.alignItems = 'center';
                cartModal.style.justifyContent = 'center';
            }
        });
    }
});

// THÊM vào cuối file cart-script.js:
// Fix cho modal layout
document.addEventListener('DOMContentLoaded', function() {
    // Fix cart modal layout styling
    const fixCartModalLayout = function() {
        const cartModal = document.getElementById('cartModal');
        if (!cartModal) return;
        
        const modalContent = cartModal.querySelector('.cart-modal-content');
        if (modalContent) {
            // Fix content layout
            modalContent.style.display = 'flex';
            modalContent.style.flexDirection = 'column';
            modalContent.style.overflow = 'hidden';
            
            // Fix body, header và footer
            const modalHeader = cartModal.querySelector('.cart-modal-header');
            const modalBody = cartModal.querySelector('.cart-modal-body');
            const modalFooter = cartModal.querySelector('.cart-modal-footer');
            
            if (modalHeader) modalHeader.style.flex = '0 0 auto';
            
            if (modalBody) {
                modalBody.style.flex = '1 1 auto';
                modalBody.style.overflowY = 'auto';
                modalBody.style.maxHeight = 'none';
                modalBody.style.minHeight = '100px';
            }
            
            if (modalFooter) modalFooter.style.flex = '0 0 auto';
        }
    };
    
    // Apply fix immediately
    fixCartModalLayout();
    
    // Also ensure fix is applied when opening modal
    const originalOpenCartModal = window.openCartModal;
    if (typeof originalOpenCartModal === 'function') {
        window.openCartModal = function() {
            originalOpenCartModal();
            setTimeout(fixCartModalLayout, 10);
        };
    }
    
    console.log('✅ Applied cart modal layout fix');
});

// Thêm các hàm xử lý các action trong cart modal
function updateCartQuantity(productId, quantity) {
    console.log(`🔢 Updating cart quantity for product ${productId} to ${quantity}`);
    
    // Đảm bảo quantity là số
    quantity = parseInt(quantity);
    if (isNaN(quantity)) {
        showCartToast('Số lượng không hợp lệ', 'error');
        loadCartModal(); // Tải lại để hiển thị số lượng đúng
        return;
    }
    
    if (quantity < 1) {
        // Nếu số lượng < 1, xóa sản phẩm
        removeFromCart(productId);
        return;
    }
    
    // Disable inputs during update
    const cartItem = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
    if (cartItem) {
        cartItem.style.opacity = '0.5';
        cartItem.style.pointerEvents = 'none';
    }
    
    // Thêm hiệu ứng loading
    const spinner = document.createElement('div');
    spinner.className = 'quantity-spinner';
    spinner.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    
    if (cartItem) {
        cartItem.appendChild(spinner);
    }
    
    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: `productId=${encodeURIComponent(productId)}&quantity=${encodeURIComponent(quantity)}`
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('✅ Update quantity response:', data);
        
        // Xóa spinner
        if (cartItem) {
            const spinners = cartItem.querySelectorAll('.quantity-spinner');
            spinners.forEach(s => s.remove());
        }
        
        // Re-enable cart item
        if (cartItem) {
            cartItem.style.opacity = '';
            cartItem.style.pointerEvents = '';
        }
        
        if (data.success) {
            // Update totals in modal
            updateCartBadge(data.totalItems);
            
            // Cập nhật tổng số lượng và tổng tiền
            const totalItemsElement = document.getElementById('modalTotalItems');
            const totalAmountElement = document.getElementById('modalTotalAmount');
            
            if (totalItemsElement) totalItemsElement.textContent = data.totalItems;
            if (totalAmountElement) totalAmountElement.textContent = data.formattedTotalAmount;
            
            // Update cart item quantity
            const quantityInput = cartItem?.querySelector('.cart-quantity-input');
            if (quantityInput) {
                quantityInput.value = quantity;
            }
        } else {
            // Show error and reload cart modal
            showCartToast(data.message || 'Không thể cập nhật số lượng', 'error');
            setTimeout(loadCartModal, 500);
        }
    })
    .catch(error => {
        console.error('❌ Update cart quantity error:', error);
        
        // Xóa spinner
        if (cartItem) {
            const spinners = cartItem.querySelectorAll('.quantity-spinner');
            spinners.forEach(s => s.remove());
        }
        
        // Re-enable cart item
        if (cartItem) {
            cartItem.style.opacity = '';
            cartItem.style.pointerEvents = '';
        }
        
        showCartToast('Có lỗi xảy ra khi cập nhật số lượng', 'error');
        setTimeout(loadCartModal, 500);
    });
}

function removeFromCart(productId) {
    console.log(`Removing product ${productId} from cart`);
    
    if (!confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        return;
    }
    
    // Disable item during removal
    const cartItem = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
    if (cartItem) {
        cartItem.style.opacity = '0.5';
        cartItem.style.pointerEvents = 'none';
    }
    
    fetch('/Cart/Remove', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: `productId=${productId}`
    })
    .then(response => response.json())
    .then(data => {
        console.log('Remove from cart response:', data);
        
        if (data.success) {
            // Update badge and totals
            updateCartBadge(data.totalItems);
            document.getElementById('modalTotalItems').textContent = data.totalItems;
            document.getElementById('modalTotalAmount').textContent = data.formattedTotalAmount;
            
            // Remove item with animation
            if (cartItem) {
                cartItem.style.height = `${cartItem.offsetHeight}px`;
                setTimeout(() => {
                    cartItem.style.height = '0';
                    cartItem.style.opacity = '0';
                    cartItem.style.padding = '0';
                    cartItem.style.margin = '0';
                    cartItem.style.overflow = 'hidden';
                    cartItem.style.transition = 'all 0.3s ease';
                    
                    setTimeout(() => {
                        cartItem.remove();
                        
                        // If no items left, reload modal to show empty state
                        const cartItems = document.querySelectorAll('.cart-item');
                        if (!cartItems.length) {
                            loadCartModal();
                        }
                    }, 300);
                }, 10);
            }
            
            showCartToast('Đã xóa sản phẩm khỏi giỏ hàng', 'success');
        } else {
            // Re-enable cart item
            if (cartItem) {
                cartItem.style.opacity = '';
                cartItem.style.pointerEvents = '';
            }
            
            showCartToast(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Remove from cart error:', error);
        
        // Re-enable cart item
        if (cartItem) {
            cartItem.style.opacity = '';
            cartItem.style.pointerEvents = '';
        }
        
        showCartToast('Có lỗi xảy ra khi xóa sản phẩm', 'error');
    });
}

// Thêm hàm mới để gắn event listeners
function setupCartItemControls() {
    // Xử lý nút giảm số lượng
    document.querySelectorAll('.cart-quantity-decrease').forEach(button => {
        button.addEventListener('click', function() {
            const productId = this.dataset.productId;
            const input = this.parentElement.querySelector('.cart-quantity-input');
            const currentQuantity = parseInt(input.value) || 1;
            if (currentQuantity > 1) {
                window.updateCartQuantity(productId, currentQuantity - 1);
            } else {
                // Nếu số lượng là 1, xác nhận xóa
                if (confirm('Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?')) {
                    window.removeFromCart(productId);
                }
            }
        });
    });
    
    // Xử lý nút tăng số lượng
    document.querySelectorAll('.cart-quantity-increase').forEach(button => {
        button.addEventListener('click', function() {
            const productId = this.dataset.productId;
            const input = this.parentElement.querySelector('.cart-quantity-input');
            const currentQuantity = parseInt(input.value) || 1;
            window.updateCartQuantity(productId, currentQuantity + 1);
        });
    });
    
    // Xử lý khi thay đổi trực tiếp ô input
    document.querySelectorAll('.cart-quantity-input').forEach(input => {
        input.addEventListener('change', function() {
            const productId = this.dataset.productId;
            const quantity = parseInt(this.value) || 1;
            if (quantity > 0) {
                window.updateCartQuantity(productId, quantity);
            } else {
                this.value = 1; // Reset về 1 nếu giá trị không hợp lệ
            }
        });
    });
    
    // Xử lý nút xóa
    document.querySelectorAll('.cart-item-remove').forEach(button => {
        button.addEventListener('click', function() {
            const productId = this.dataset.productId;
            window.removeFromCart(productId);
        });
    });
}

// Sửa lỗi tăng giảm số lượng không hoạt động
function clearAllCart() {
    console.log('Clearing cart...');
    
    if (!confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng không?')) {
        return;
    }
    
    fetch('/Cart/Clear', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            updateCartBadge(0);
            loadCartModal(); // Tải lại modal để hiển thị trạng thái giỏ hàng rỗng
            showCartToast('Đã xóa toàn bộ giỏ hàng', 'success');
        } else {
            showCartToast('Không thể xóa giỏ hàng: ' + data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Clear cart error:', error);
        showCartToast('Có lỗi xảy ra khi xóa giỏ hàng', 'error');
    });
}

function showCartToast(message, type = 'success') {
    const notification = document.getElementById('notification');
    const notificationText = document.getElementById('notificationText');
    
    if (notification && notificationText) {
        notificationText.textContent = message;
        notification.className = `notification ${type}`;
        notification.classList.add('show');
        
        setTimeout(() => {
            notification.classList.remove('show');
        }, 3000);
    } else {
        // Fallback to alert if notification elements don't exist
        alert(message);
    }
}

// Fix lỗi: Setup lại event delegation cho các nút tăng giảm số lượng
document.addEventListener('click', function(e) {
    // Xử lý nút giảm số lượng
    if (e.target.classList.contains('fa-minus') || e.target.closest('.cart-quantity-decrease')) {
        const button = e.target.classList.contains('cart-quantity-decrease') ? 
            e.target : e.target.closest('.cart-quantity-decrease');
        
        if (button) {
            e.preventDefault();
            e.stopPropagation();
            const productId = button.dataset.productId;
            const input = button.parentElement.querySelector('.cart-quantity-input');
            const currentQuantity = parseInt(input.value) || 1;
            
            console.log('Decreasing quantity for product:', productId, 'current:', currentQuantity);
            
            if (currentQuantity > 1) {
                updateCartQuantity(productId, currentQuantity - 1);
            } else {
                if (confirm('Bạn có muốn xóa sản phẩm này khỏi giỏ hàng?')) {
                    removeFromCart(productId);
                }
            }
        }
    }
    
    // Xử lý nút tăng số lượng
    if (e.target.classList.contains('fa-plus') || e.target.closest('.cart-quantity-increase')) {
        const button = e.target.classList.contains('cart-quantity-increase') ? 
            e.target : e.target.closest('.cart-quantity-increase');
        
        if (button) {
            e.preventDefault();
            e.stopPropagation();
            const productId = button.dataset.productId;
            const input = button.parentElement.querySelector('.cart-quantity-input');
            const currentQuantity = parseInt(input.value) || 1;
            
            console.log('Increasing quantity for product:', productId, 'current:', currentQuantity);
            updateCartQuantity(productId, currentQuantity + 1);
        }
    }
    
    // Xử lý nút xóa sản phẩm
    if (e.target.classList.contains('fa-trash') || e.target.closest('.cart-item-remove')) {
        const button = e.target.classList.contains('cart-item-remove') ? 
            e.target : e.target.closest('.cart-item-remove');
        
        if (button) {
            e.preventDefault();
            e.stopPropagation();
            const productId = button.dataset.productId;
            console.log('Removing product:', productId);
            removeFromCart(productId);
        }
    }
});

// Fix lỗi input số lượng
document.addEventListener('change', function(e) {
    if (e.target.classList.contains('cart-quantity-input')) {
        const productId = e.target.dataset.productId;
        const quantity = parseInt(e.target.value) || 1;
        
        console.log('Manual quantity change for product:', productId, 'new quantity:', quantity);
        
        if (quantity > 0) {
            updateCartQuantity(productId, quantity);
        } else {
            e.target.value = 1; // Reset về 1 nếu giá trị không hợp lệ
        }
    }
});

// Khi DOM đã sẵn sàng
document.addEventListener('DOMContentLoaded', function() {
    console.log('🔄 Adding additional cart event handlers');
    
    // Gắn sự kiện cho nút giỏ hàng trong header
    const cartBtn = document.getElementById('cartBtn');
    if (cartBtn) {
        cartBtn.addEventListener('click', function(e) {
            e.preventDefault();
            openCartModal();
        });
    }
});

console.log('✅ Additional cart event handlers loaded');