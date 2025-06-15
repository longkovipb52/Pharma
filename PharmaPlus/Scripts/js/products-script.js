// Global variables
let searchInput;
let categoryFilter;
let sortFilter;

// Initialize Products Page
function initializeProductsPage() {
    try {
        console.log('Initializing Products Page...');
        
        // CẬP NHẬT: Sử dụng đúng ID từ HTML
        searchInput = document.getElementById('productsSearchInput');
        categoryFilter = document.getElementById('productsCategoryFilter');
        sortFilter = document.getElementById('productsSortFilter');
        
        // Setup event listeners
        setupSearchEventListeners();
        setupViewToggle();
        setupLazyLoading();
        animateProductCards();
        
        console.log('Products page initialized successfully');
        console.log('Search input:', searchInput);
        console.log('Category filter:', categoryFilter);
        console.log('Sort filter:', sortFilter);
    } catch (error) {
        console.error('Error initializing products page:', error);
    }
}

// QuickView functionality
function quickView(productId) {
    console.log('Opening QuickView for product:', productId);
    
    let modal = document.getElementById('quickviewModal');
    if (!modal) {
        modal = createQuickViewModal();
        document.body.appendChild(modal);
    }
    
    modal.classList.add('show');
    document.body.style.overflow = 'hidden';
    
    loadQuickViewData(productId);
}

function createQuickViewModal() {
    const modal = document.createElement('div');
    modal.id = 'quickviewModal';
    modal.className = 'quickview-modal';
    
    modal.innerHTML = `
        <div class="quickview-modal-content">
            <div class="quickview-header-bar">
                <h3>Chi tiết sản phẩm</h3>
                <button type="button" class="quickview-close-btn" onclick="closeQuickView()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            <div class="quickview-body">
                <div class="quickview-loading">
                    <i class="fas fa-spinner fa-spin"></i>
                    <span>Đang tải thông tin sản phẩm...</span>
                </div>
            </div>
        </div>
    `;
    
    modal.addEventListener('click', function(e) {
        if (e.target === modal) {
            closeQuickView();
        }
    });
    
    return modal;
}

function loadQuickViewData(productId) {
    const modalBody = document.querySelector('#quickviewModal .quickview-body');
    
    modalBody.innerHTML = `
        <div class="quickview-loading">
            <i class="fas fa-spinner fa-spin"></i>
            <span>Đang tải thông tin sản phẩm...</span>
        </div>
    `;
    
    const url = `/Products/QuickView/${productId}`;
    console.log('Loading QuickView from:', url);
    
    fetch(url, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest',
            'Accept': 'text/html'
        },
        credentials: 'same-origin'
    })
    .then(response => {
        console.log('QuickView response:', response.status, response.statusText);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.text();
    })
    .then(html => {
        console.log('QuickView HTML loaded, length:', html.length);
        
        if (html.trim().length === 0) {
            throw new Error('Received empty response');
        }
        
        modalBody.innerHTML = html;
        setupQuickViewControls();
        console.log('QuickView data loaded successfully');
    })
    .catch(error => {
        console.error('Error loading QuickView:', error);
        modalBody.innerHTML = `
            <div class="quickview-error">
                <div class="quickview-error-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
                <h3>Không thể tải thông tin sản phẩm</h3>
                <p><strong>URL:</strong> ${url}</p>
                <p><strong>Lỗi:</strong> ${error.message}</p>
                <div class="quickview-error-actions">
                    <button class="error-btn error-btn-retry" onclick="loadQuickViewData(${productId})">
                        <i class="fas fa-redo"></i>
                        Thử lại
                    </button>
                    <button class="error-btn error-btn-close" onclick="closeQuickView()">
                        <i class="fas fa-times"></i>
                        Đóng
                    </button>
                </div>
            </div>
        `;
    });
}

function closeQuickView() {
    console.log('Closing QuickView...');
    const modal = document.getElementById('quickviewModal');
    if (modal) {
        modal.classList.remove('show');
        document.body.style.overflow = '';
        
        setTimeout(() => {
            const modalBody = modal.querySelector('.quickview-body');
            if (modalBody) {
                modalBody.innerHTML = '';
            }
        }, 300);
    }
}

function setupQuickViewControls() {
    console.log('Setting up QuickView controls...');
    
    window.decreaseQuantity = function() {
        const input = document.getElementById('quickviewQuantity');
        if (input) {
            const current = parseInt(input.value) || 1;
            if (current > 1) {
                input.value = current - 1;
            }
        }
    };
    
    window.increaseQuantity = function(maxStock) {
        const input = document.getElementById('quickviewQuantity');
        if (input) {
            const current = parseInt(input.value) || 1;
            if (current < maxStock) {
                input.value = current + 1;
            }
        }
    };
}

// SIMPLIFIED: Remove conflicting cart functions, defer to cart-script.js
function addToCartFromProducts(productId) {
    console.log('🏪 Products: Delegating to cart system:', productId);
    
    // SIMPLE delegation
    if (typeof window.cartAddToCart === 'function') {
        window.cartAddToCart(productId, 1);
    } else {
        console.error('❌ Cart system not available');
        
        // Fallback toast
        if (typeof showToast === 'function') {
            showToast('Cart system not ready, please try again', 'warning');
        } else {
            alert('Cart system not ready!');
        }
    }
}

function addToCartFromQuickView(productId, event) {
    // Thêm event và ngăn chặn bubbling
    if (event) {
        event.preventDefault();
        event.stopPropagation();
        console.log('QuickView add to cart button clicked directly');
    }
    
    const quantityInput = document.getElementById('quickviewQuantity');
    const quantity = parseInt(quantityInput?.value || 1);
    
    console.log(`🪟 QuickView: Thêm sản phẩm ID=${productId} với số lượng=${quantity}`);
    
    if (typeof window.cartAddToCart === 'function') {
        window.cartAddToCart(parseInt(productId), quantity);
        
        // Update button feedback
        const addButton = event?.target || document.querySelector('.btn-add-cart');
        if (addButton) {
            const originalText = addButton.innerHTML;
            addButton.disabled = true;
            addButton.innerHTML = `<i class="fas fa-check"></i> Đã thêm ${quantity} sản phẩm`;
            
            setTimeout(() => {
                addButton.disabled = false;
                addButton.innerHTML = originalText;
            }, 2000);
        }
        
        // Đóng modal sau khi thêm thành công
        setTimeout(() => {
            if (typeof closeQuickView === 'function') {
                closeQuickView();
            }
        }, 1500);
    } else {
        console.error('❌ Cart system not available for QuickView');
        alert('Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại sau.');
    }
}

// Simple wrapper for backward compatibility
function addToCart(productId) {
    addToCartFromProducts(productId);
}

function addToWishlist(productId) {
    // Placeholder function để tránh lỗi
    showToast('Chức năng yêu thích sẽ có sớm!', 'info');
}

// Toast notification function with longer duration and progress bar
function showToast(message, type = 'info', duration = 8000) { // Tăng thời gian mặc định lên 8000ms
    const existingToast = document.querySelector('.toast-notification');
    if (existingToast) {
        existingToast.remove();
    }
    
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : 'info-circle'}"></i>
            <span>${message}</span>
            <button class="toast-close-btn" aria-label="Đóng thông báo">
                <i class="fas fa-times"></i>
            </button>
        </div>
        <div class="toast-progress-bar"></div>
    `;
    
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'success' ? '#10b981' : type === 'warning' ? '#f59e0b' : '#3b82f6'};
        color: white;
        padding: 15px 20px;
        border-radius: 12px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        z-index: 11000;
        animation: slideInRight 0.5s ease;
        font-weight: 600;
        min-width: 320px;
        max-width: 420px;
        word-wrap: break-word;
        position: relative;
        overflow: hidden;
    `;
    
    document.body.appendChild(toast);
    
    // Add progress bar that indicates time remaining
    const progressBar = toast.querySelector('.toast-progress-bar');
    progressBar.style.cssText = `
        position: absolute;
        bottom: 0;
        left: 0;
        height: 4px;
        background: rgba(255, 255, 255, 0.5);
        width: 100%;
        animation: progressShrink ${duration}ms linear forwards;
    `;
    
    // Add close button functionality
    const closeBtn = toast.querySelector('.toast-close-btn');
    closeBtn.addEventListener('click', () => {
        toast.style.animation = 'slideOutRight 0.5s ease';
        setTimeout(() => toast.remove(), 500);
    });
    
    // Auto-remove after duration
    setTimeout(() => {
        if (toast.parentNode) {
            toast.style.animation = 'slideOutRight 0.5s ease';
            setTimeout(() => toast.remove(), 500);
        }
    }, duration);
    
    // Pause timer when hovering
    toast.addEventListener('mouseenter', () => {
        progressBar.style.animationPlayState = 'paused';
    });
    
    toast.addEventListener('mouseleave', () => {
        progressBar.style.animationPlayState = 'running';
    });
}

// Search and filter functions
function performSearch() {
    const searchValue = searchInput ? searchInput.value.trim() : '';
    console.log('Performing search for:', searchValue);
    
    if (searchValue) {
        window.location.href = `/Products?search=${encodeURIComponent(searchValue)}`;
    } else {
        window.location.href = '/Products';
    }
}

function filterProducts() {
    console.log('Filtering products...');
    
    const category = categoryFilter ? categoryFilter.value : '';
    const sort = sortFilter ? sortFilter.value : '';
    const search = searchInput ? searchInput.value.trim() : '';
    
    console.log('Filter values:', { category, sort, search });
    
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    if (search) params.append('search', search);
    if (sort) params.append('sortBy', sort);
    
    const newUrl = `/Products?${params.toString()}`;
    console.log('Navigating to:', newUrl);
    
    window.location.href = newUrl;
}

function clearAllFilters() {
    console.log('Clearing all filters...');
    
    if (searchInput) searchInput.value = '';
    if (categoryFilter) categoryFilter.value = '';
    if (sortFilter) sortFilter.value = 'name';
    
    window.location.href = '/Products';
}

function handleImageError(img) {
    console.log('Image error for:', img.src);
    
    const fallbackImages = [
        '/images/products/default-product.jpg',
        '/Content/images/no-image.png',
        '/images/no-image.jpg'
    ];
    
    if (img.dataset.fallbackIndex === undefined) {
        img.dataset.fallbackIndex = 0;
    }
    
    const index = parseInt(img.dataset.fallbackIndex);
    
    if (index < fallbackImages.length) {
        img.src = fallbackImages[index];
        img.dataset.fallbackIndex = index + 1;
    } else {
        img.style.display = 'none';
        
        const placeholder = document.createElement('div');
        placeholder.className = 'image-placeholder';
        placeholder.style.cssText = `
            width: 100%; 
            height: 200px; 
            background: #f3f4f6; 
            display: flex; 
            align-items: center; 
            justify-content: center; 
            color: #9ca3af;
            border-radius: 8px;
        `;
        placeholder.innerHTML = `
            <div style="text-align: center;">
                <i class="fas fa-image" style="font-size: 2rem; margin-bottom: 8px; display: block;"></i>
                <span style="font-size: 0.75rem;">Không có hình</span>
            </div>
        `;
        
        img.parentNode.insertBefore(placeholder, img);
    }
}

function setupSearchEventListeners() {
    if (searchInput) {
        searchInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                performSearch();
            }
        });
    }
}

function setupViewToggle() {
    const viewButtons = document.querySelectorAll('.products-view-btn');
    const productsGrid = document.getElementById('productsGrid');
    
    viewButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            viewButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            
            const viewType = this.dataset.view;
            if (productsGrid) {
                productsGrid.className = `products-grid products-${viewType}-view`;
            }
        });
    });
}

function setupLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src || img.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img.lazy').forEach(img => {
            imageObserver.observe(img);
        });
    }
}

function animateProductCards() {
    const cards = document.querySelectorAll('.products-card');
    
    if ('IntersectionObserver' in window) {
        const cardObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        });

        cards.forEach(card => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            cardObserver.observe(card);
        });
    }
}

// Close modal on ESC key
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        const modal = document.getElementById('quickviewModal');
        if (modal && modal.classList.contains('show')) {
            closeQuickView();
        }
    }
});

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeProductsPage();
});

// CẬP NHẬT global exports
window.quickView = quickView;
window.closeQuickView = closeQuickView;
window.addToCart = addToCart; // ← THÊM WRAPPER
window.addToCartFromProducts = addToCartFromProducts;
window.addToCartFromQuickView = addToCartFromQuickView;
window.addToWishlist = addToWishlist; // ← THÊM PLACEHOLDER
window.performSearch = performSearch;
window.filterProducts = filterProducts;
window.clearAllFilters = clearAllFilters;
window.handleImageError = handleImageError;
window.quickCheckout = quickCheckout; // ← THÊM quickCheckout