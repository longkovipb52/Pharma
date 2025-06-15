// Home Page JavaScript - Fixed for Cart Integration

// ===== GLOBAL VARIABLES =====
let isPageLoaded = false;
let activeTab = 'featured';
let favoriteProducts = [];
let cartItems = [];
let lastScrollY = 0;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('🏠 Home page script loaded');
    
    // Khởi tạo floating shapes cho hero section
    initFloatingShapes();
    
    // Khởi tạo header scroll effect
    initHeaderScrollEffect();
    
    // Các khởi tạo khác
    initializeHomePage();
});

// ===== MAIN INITIALIZATION =====
function initializeHomePage() {
    try {
        // Load saved preferences
        loadSavedPreferences();
        
        // Initialize components
        initializeAnimations();
        initializeSmoothScroll();
        initializeIntersectionObserver();
        initializeProductTabs();
        initializeFavorites();
        initializeTooltips();
        initializeScrollEffects();
        
        // Load initial data
        loadInitialData();
        
        // Mark as loaded
        isPageLoaded = true;
        
        console.log('✅ Home page initialized successfully');
        showNotification('Trang chủ đã tải xong!', 'success');
        
    } catch (error) {
        console.error('❌ Error initializing home page:', error);
        showNotification('Có lỗi khi khởi tạo trang', 'error');
    }
}

// ===== LOAD SAVED PREFERENCES =====
function loadSavedPreferences() {
    try {
        // Load favorites
        const savedFavorites = localStorage.getItem('pharma_favorites');
        if (savedFavorites) {
            favoriteProducts = JSON.parse(savedFavorites);
            console.log('💾 Loaded favorites:', favoriteProducts);
        }
        
        // Load active tab
        const savedTab = sessionStorage.getItem('pharma_active_tab');
        if (savedTab) {
            activeTab = savedTab;
        }
        
        // Apply dark mode if saved
        const darkMode = localStorage.getItem('pharma_dark_mode');
        if (darkMode === 'true') {
            document.body.classList.add('dark-mode');
        }
        
    } catch (error) {
        console.error('❌ Error loading preferences:', error);
    }
}

// ===== ANIMATIONS =====
function initializeAnimations() {
    // Add entrance animations to elements
    const animatedElements = document.querySelectorAll(
        '.hero-text, .hero-visual, .section-header, .category-card, .product-card, .review-card'
    );
    
    animatedElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(30px)';
        
        setTimeout(() => {
            element.style.transition = 'all 0.6s ease';
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        }, index * 100);
    });
    
    // Animate stats
    animateStatsOnScroll();
}

// Thêm hàm animate stats khi scroll
function animateStatsOnScroll() {
    const statElements = document.querySelectorAll('.stat-card');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const statNumber = entry.target.querySelector('.stat-number');
                if (statNumber && !statNumber.classList.contains('animated')) {
                    animateNumber(statNumber);
                    statNumber.classList.add('animated');
                }
                
                entry.target.classList.add('animate-in');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.2 });
    
    statElements.forEach(stat => {
        observer.observe(stat);
    });
}

// Thêm hàm animate number
function animateNumber(element) {
    const targetNumber = parseInt(element.getAttribute('data-count') || '0');
    const duration = 2000; // ms
    const framesPerSecond = 60;
    const frameDuration = 1000 / framesPerSecond;
    const totalFrames = duration / frameDuration;
    let frame = 0;
    let currentNumber = 0;
    
    const animate = () => {
        frame++;
        const progress = frame / totalFrames;
        currentNumber = Math.floor(progress * targetNumber);
        
        if (currentNumber > targetNumber) {
            currentNumber = targetNumber;
        }
        
        element.textContent = currentNumber.toLocaleString();
        
        if (frame < totalFrames) {
            requestAnimationFrame(animate);
        }
    };
    
    requestAnimationFrame(animate);
}

// ===== SMOOTH SCROLL =====
function initializeSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                const headerOffset = 80;
                const elementPosition = target.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// ===== INTERSECTION OBSERVER =====
function initializeIntersectionObserver() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
                
                // Trigger specific animations
                if (entry.target.classList.contains('stat-card')) {
                    animateNumbers(entry.target);
                }
                
                if (entry.target.classList.contains('product-card')) {
                    animateProductCard(entry.target);
                }
            }
        });
    }, observerOptions);
    
    // Observe elements
    document.querySelectorAll(
        '.section-header, .category-card, .product-card, .review-card, .stat-card'
    ).forEach(el => {
        observer.observe(el);
    });
}

// ===== ANIMATE NUMBERS =====
function animateNumbers(element) {
    const numberElement = element.querySelector('.stat-number');
    if (!numberElement) return;
    
    const finalNumber = parseInt(numberElement.textContent.replace(/\D/g, ''));
    if (isNaN(finalNumber)) return;
    
    let currentNumber = 0;
    const increment = finalNumber / 50;
    const timer = setInterval(() => {
        currentNumber += increment;
        if (currentNumber >= finalNumber) {
            clearInterval(timer);
            numberElement.textContent = finalNumber.toLocaleString();
        } else {
            numberElement.textContent = Math.floor(currentNumber).toLocaleString();
        }
    }, 30);
}

// ===== ANIMATE PRODUCT CARD =====
function animateProductCard(card) {
    const image = card.querySelector('.product-image img');
    const info = card.querySelector('.product-info');
    
    if (image) {
        image.style.transform = 'scale(1.02)';
        setTimeout(() => {
            image.style.transform = 'scale(1)';
        }, 300);
    }
    
    if (info) {
        info.style.opacity = '0.8';
        setTimeout(() => {
            info.style.opacity = '1';
        }, 200);
    }
}

// ===== PRODUCT TABS =====
function initializeProductTabs() {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const productGrids = document.querySelectorAll('.products-grid');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            const tabName = button.getAttribute('data-tab');
            switchProductTab(tabName, button);
        });
    });
    
    // Set initial active tab
    const initialTab = document.querySelector(`[data-tab="${activeTab}"]`);
    if (initialTab) {
        switchProductTab(activeTab, initialTab);
    }
}

// ===== SWITCH PRODUCT TAB (ENHANCED) =====
function switchProductTab(tabName, button) {
    try {
        console.log('🔄 Switching to tab:', tabName);
        
        // Update active tab
        activeTab = tabName;
        sessionStorage.setItem('pharma_active_tab', tabName);
        
        // Update tab buttons
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.classList.remove('active');
        });
        if (button) {
            button.classList.add('active');
        }
        
        // Hide all grids with fade effect
        document.querySelectorAll('.products-grid').forEach(grid => {
            grid.style.opacity = '0';
            grid.style.pointerEvents = 'none';
            setTimeout(() => {
                grid.classList.remove('active');
            }, 200);
        });
        
        // Show selected grid with fade effect
        setTimeout(() => {
            const targetGrid = document.getElementById(tabName + '-grid');
            if (targetGrid) {
                targetGrid.classList.add('active');
                targetGrid.style.opacity = '1';
                targetGrid.style.pointerEvents = 'auto';
                
                // Animate product cards
                const cards = targetGrid.querySelectorAll('.product-card');
                cards.forEach((card, index) => {
                    card.style.opacity = '0';
                    card.style.transform = 'translateY(20px)';
                    setTimeout(() => {
                        card.style.transition = 'all 0.4s ease';
                        card.style.opacity = '1';
                        card.style.transform = 'translateY(0)';
                    }, index * 80);
                });
            }
        }, 200);
        
    } catch (error) {
        console.error('❌ Error switching tabs:', error);
    }
}

// ===== FAVORITES =====
function initializeFavorites() {
    // Apply saved favorites
    favoriteProducts.forEach(productId => {
        const favoriteBtn = document.querySelector(`[data-product-id="${productId}"] .favorite`);
        if (favoriteBtn) {
            favoriteBtn.classList.add('active');
            favoriteBtn.innerHTML = '<i class="fas fa-heart"></i>';
        }
    });
}

function toggleFavorite(productId, event) {
    try {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }
        
        console.log('❤️ Toggling favorite for product:', productId);
        
        const favoriteBtn = document.querySelector(`[data-product-id="${productId}"] .favorite`);
        if (!favoriteBtn) {
            console.error('❌ Favorite button not found');
            return;
        }
        
        const isActive = favoriteBtn.classList.contains('active');
        
        if (isActive) {
            // Remove from favorites
            favoriteBtn.classList.remove('active');
            favoriteBtn.innerHTML = '<i class="far fa-heart"></i>';
            favoriteProducts = favoriteProducts.filter(id => id != productId);
            showNotification('Đã xóa khỏi danh sách yêu thích', 'info');
        } else {
            // Add to favorites
            favoriteBtn.classList.add('active');
            favoriteBtn.innerHTML = '<i class="fas fa-heart"></i>';
            favoriteProducts.push(parseInt(productId));
            showNotification('Đã thêm vào danh sách yêu thích', 'success');
        }
        
        // Save to localStorage
        localStorage.setItem('pharma_favorites', JSON.stringify(favoriteProducts));
        
        // Add animation
        favoriteBtn.style.transform = 'scale(1.2)';
        setTimeout(() => {
            favoriteBtn.style.transform = 'scale(1)';
        }, 200);
        
    } catch (error) {
        console.error('❌ Error toggling favorite:', error);
        showNotification('Có lỗi xảy ra', 'error');
    }
}

// ===== ADD TO CART (ENHANCED) =====
function addToCart(productId, quantity = 1) {
    try {
        console.log('🛒 Adding to cart:', { productId, quantity });
        
        const button = event?.target?.closest('.btn-add-cart');
        
        // Add loading state
        if (button) {
            button.classList.add('loading');
            button.disabled = true;
            const originalText = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>Đang thêm...</span>';
            
            // Simulate API call
            setTimeout(() => {
                button.classList.remove('loading');
                button.classList.add('success');
                button.innerHTML = '<i class="fas fa-check"></i><span>Đã thêm!</span>';
                
                setTimeout(() => {
                    button.classList.remove('success');
                    button.disabled = false;
                    button.innerHTML = originalText;
                }, 1500);
            }, 800);
        }
        
        // Check if cart system is available
        if (typeof window.cartAddToCart === 'function') {
            window.cartAddToCart(parseInt(productId), quantity);
        } else {
            // Fallback: local storage
            addToLocalCart(productId, quantity);
        }
        
        showNotification('Sản phẩm đã được thêm vào giỏ hàng', 'success');
        
        // Update cart badge if available
        if (typeof window.updateCartBadge === 'function') {
            window.updateCartBadge();
        }
        
    } catch (error) {
        console.error('❌ Error adding to cart:', error);
        showNotification('Có lỗi khi thêm vào giỏ hàng', 'error');
        
        // Reset button state
        if (button) {
            button.classList.remove('loading');
            button.disabled = false;
        }
    }
}

// ===== LOCAL CART FALLBACK =====
function addToLocalCart(productId, quantity) {
    let cart = JSON.parse(localStorage.getItem('pharma_cart') || '[]');
    
    const existingItem = cart.find(item => item.productId == productId);
    if (existingItem) {
        existingItem.quantity += quantity;
    } else {
        cart.push({ productId: parseInt(productId), quantity });
    }
    
    localStorage.setItem('pharma_cart', JSON.stringify(cart));
    cartItems = cart;
    
    console.log('💾 Cart saved to localStorage:', cart);
}

// ===== CATEGORY FILTERING =====
function viewCategory(categoryId) {
    try {
        console.log('🏷️ Viewing category:', categoryId);
        
        // Add loading effect to category card
        const categoryCard = event?.target?.closest('.category-card');
        if (categoryCard) {
            categoryCard.style.transform = 'scale(0.98)';
            categoryCard.style.opacity = '0.8';
        }
        
        showNotification('Đang chuyển đến danh mục...', 'info');
        
        setTimeout(() => {
            const url = `/Products?category=${encodeURIComponent(categoryId)}`;
            window.location.href = url;
        }, 300);
        
    } catch (error) {
        console.error('❌ Error viewing category:', error);
        showNotification('Có lỗi khi chuyển trang', 'error');
    }
}

// ===== QUICK VIEW =====
function quickView(productId) {
    try {
        console.log('👁️ Quick view product:', productId);
        
        // Create modal
        const modal = createQuickViewModal(productId);
        document.body.appendChild(modal);
        
        // Show modal
        setTimeout(() => {
            modal.classList.add('show');
        }, 10);
        
        // Load product data
        loadProductData(productId, modal);
        
    } catch (error) {
        console.error('❌ Error in quick view:', error);
        showNotification('Có lỗi khi xem nhanh sản phẩm', 'error');
    }
}

function createQuickViewModal(productId) {
    const modal = document.createElement('div');
    modal.className = 'quick-view-modal';
    modal.innerHTML = `
        <div class="modal-backdrop" onclick="closeQuickView()"></div>
        <div class="modal-content">
            <button class="modal-close" onclick="closeQuickView()">
                <i class="fas fa-times"></i>
            </button>
            <div class="modal-loading">
                <i class="fas fa-spinner fa-spin"></i>
                <p>Đang tải thông tin sản phẩm...</p>
            </div>
            <div class="modal-body" style="display: none;">
                <!-- Product content will be loaded here -->
            </div>
        </div>
    `;
    return modal;
}

function loadProductData(productId, modal) {
    // Simulate API call
    setTimeout(() => {
        const modalBody = modal.querySelector('.modal-body');
        const modalLoading = modal.querySelector('.modal-loading');
        
        modalLoading.style.display = 'none';
        modalBody.style.display = 'block';
        modalBody.innerHTML = `
            <div class="quick-view-content">
                <div class="product-image">
                    <img src="https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=300&h=250&fit=crop" 
                         alt="Product ${productId}">
                </div>
                <div class="product-details">
                    <h3>Sản phẩm #${productId}</h3>
                    <p class="product-price">Đang tải giá...</p>
                    <p class="product-description">Đang tải mô tả sản phẩm...</p>
                    <div class="quick-actions">
                        <button class="btn btn-primary" onclick="addToCart(${productId}); closeQuickView();">
                            <i class="fas fa-shopping-cart"></i>
                            Thêm vào giỏ
                        </button>
                        <button class="btn btn-secondary" onclick="viewProduct(${productId})">
                            Xem chi tiết
                        </button>
                    </div>
                </div>
            </div>
        `;
    }, 1000);
}

function closeQuickView() {
    const modal = document.querySelector('.quick-view-modal');
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => {
            modal.remove();
        }, 300);
    }
}

// ===== TOOLTIPS =====
function initializeTooltips() {
    const tooltipElements = document.querySelectorAll('[data-tooltip]');
    
    tooltipElements.forEach(element => {
        element.addEventListener('mouseenter', showTooltip);
        element.addEventListener('mouseleave', hideTooltip);
    });
}

function showTooltip(event) {
    const element = event.target;
    const text = element.getAttribute('data-tooltip');
    
    const tooltip = document.createElement('div');
    tooltip.className = 'tooltip';
    tooltip.textContent = text;
    document.body.appendChild(tooltip);
    
    const rect = element.getBoundingClientRect();
    tooltip.style.top = (rect.top - tooltip.offsetHeight - 10) + 'px';
    tooltip.style.left = (rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2)) + 'px';
    
    element._tooltip = tooltip;
}

function hideTooltip(event) {
    const element = event.target;
    if (element._tooltip) {
        element._tooltip.remove();
        delete element._tooltip;
    }
}

// ===== SCROLL EFFECTS =====
function initializeScrollEffects() {
    window.addEventListener('scroll', handleScroll, { passive: true });
}

function handleScroll() {
    const currentScrollY = window.scrollY;
    
    // Parallax effect for hero
    const hero = document.querySelector('.hero-banner');
    if (hero) {
        const scrolled = currentScrollY * 0.5;
        hero.style.transform = `translateY(${scrolled}px)`;
    }
    
    // Show/hide back to top button
    const backToTop = document.querySelector('.back-to-top');
    if (backToTop) {
        if (currentScrollY > 500) {
            backToTop.classList.add('show');
        } else {
            backToTop.classList.remove('show');
        }
    }
    
    lastScrollY = currentScrollY;
}

// ===== NOTIFICATION SYSTEM (ENHANCED) =====
function showNotification(message, type = 'info', duration = 4000) {
    try {
        console.log(`📢 Notification: ${message} (${type})`);
        
        // Try existing toast systems first
        if (typeof window.showCartToast === 'function') {
            return window.showCartToast(message, type);
        }
        
        if (typeof window.showToast === 'function') {
            return window.showToast(message, type);
        }
        
        // Create our own notification
        createAdvancedNotification(message, type, duration);
        
    } catch (error) {
        console.error('❌ Error showing notification:', error);
        alert(message); // Fallback
    }
}

function createAdvancedNotification(message, type, duration) {
    // Remove existing notifications
    const existing = document.querySelectorAll('.advanced-notification');
    existing.forEach(notif => notif.remove());
    
    const notification = document.createElement('div');
    notification.className = `advanced-notification notification-${type}`;
    
    const icons = {
        success: 'fas fa-check-circle',
        error: 'fas fa-exclamation-circle',
        warning: 'fas fa-exclamation-triangle',
        info: 'fas fa-info-circle'
    };
    
    notification.innerHTML = `
        <div class="notification-content">
            <div class="notification-icon">
                <i class="${icons[type] || icons.info}"></i>
            </div>
            <div class="notification-text">
                <div class="notification-message">${message}</div>
            </div>
            <button class="notification-close" onclick="this.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
        <div class="notification-progress">
            <div class="progress-bar"></div>
        </div>
    `;
    
    // Styles
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        background: 'white',
        borderRadius: '12px',
        boxShadow: '0 10px 30px rgba(0,0,0,0.15)',
        zIndex: '11000',
        minWidth: '320px',
        maxWidth: '400px',
        transform: 'translateX(120%)',
        transition: 'transform 0.4s cubic-bezier(0.4, 0, 0.2, 1)',
        border: `3px solid ${getNotificationColor(type)}`,
        overflow: 'hidden'
    });
    
    document.body.appendChild(notification);
    
    // Show animation
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 10);
    
    // Progress bar animation
    const progressBar = notification.querySelector('.progress-bar');
    progressBar.style.width = '100%';
    progressBar.style.transition = `width ${duration}ms linear`;
    progressBar.style.background = getNotificationColor(type);
    progressBar.style.height = '3px';
    
    setTimeout(() => {
        progressBar.style.width = '0%';
    }, 100);
    
    // Auto remove
    setTimeout(() => {
        notification.style.transform = 'translateX(120%)';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 400);
    }, duration);
}

function getNotificationColor(type) {
    const colors = {
        success: '#22c55e',
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    };
    return colors[type] || colors.info;
}

// ===== LOAD INITIAL DATA =====
function loadInitialData() {
    // This would typically make API calls to refresh data
    console.log('📊 Loading initial data...');
    
    // Simulate loading delay
    setTimeout(() => {
        updateDataCounters();
        refreshProductData();
    }, 1000);
}

function updateDataCounters() {
    // Animate counters if visible
    const statNumbers = document.querySelectorAll('.stat-number');
    statNumbers.forEach(element => {
        if (isElementInViewport(element)) {
            animateNumbers(element.closest('.stat-card'));
        }
    });
}

function refreshProductData() {
    // This would refresh product data from server
    console.log('🔄 Refreshing product data...');
    
    // Example: Update product availability
    const productCards = document.querySelectorAll('.product-card');
    productCards.forEach(card => {
        const stockElement = card.querySelector('.product-stock');
        if (stockElement && Math.random() > 0.8) {
            // Simulate stock update
            const isInStock = Math.random() > 0.3;
            updateProductStock(card, isInStock);
        }
    });
}

function updateProductStock(productCard, inStock) {
    const stockElement = productCard.querySelector('.product-stock');
    const addButton = productCard.querySelector('.btn-add-cart');
    
    if (stockElement) {
        if (inStock) {
            stockElement.innerHTML = `
                <span class="in-stock">
                    <i class="fas fa-check-circle"></i>
                    Còn hàng
                </span>
            `;
        } else {
            stockElement.innerHTML = `
                <span class="out-of-stock">
                    <i class="fas fa-times-circle"></i>
                    Hết hàng
                </span>
            `;
        }
    }
    
    if (addButton) {
        if (inStock) {
            addButton.disabled = false;
            addButton.innerHTML = '<i class="fas fa-shopping-cart"></i><span>Thêm vào giỏ</span>';
        } else {
            addButton.disabled = true;
            addButton.innerHTML = '<i class="fas fa-times"></i><span>Hết hàng</span>';
        }
    }
}

// ===== UTILITY FUNCTIONS =====
function isElementInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function throttle(func, limit) {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// ===== EXPORT FUNCTIONS TO GLOBAL SCOPE =====
window.switchProductTab = switchProductTab;
window.viewCategory = viewCategory;
window.addToCart = addToCart;
window.toggleFavorite = toggleFavorite;
window.quickView = quickView;
window.closeQuickView = closeQuickView;
window.showNotification = showNotification;

// ===== ERROR HANDLING =====
window.addEventListener('error', function(error) {
    console.error('🚨 Global error caught:', error);
    showNotification('Đã xảy ra lỗi không mong muốn', 'error');
});

window.addEventListener('unhandledrejection', function(event) {
    console.error('🚨 Unhandled promise rejection:', event.reason);
    showNotification('Đã xảy ra lỗi kết nối', 'error');
});

console.log('🚀 Home script loaded successfully');

// Add CSS for quick view modal
const modalStyles = `
<style>
.quick-view-modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 9999;
    opacity: 0;
    transition: opacity 0.3s ease;
}

.quick-view-modal.show {
    opacity: 1;
}

.modal-backdrop {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(5px);
}

.modal-content {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background: white;
    border-radius: 16px;
    padding: 2rem;
    max-width: 500px;
    width: 90%;
    box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
}

.modal-close {
    position: absolute;
    top: 1rem;
    right: 1rem;
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
    color: #718096;
}

.modal-close:hover {
    color: #2d3748;
}
</style>
`;

document.head.insertAdjacentHTML('beforeend', modalStyles);

// Thêm biến và hàm mới vào đầu file
document.addEventListener('DOMContentLoaded', function() {
    console.log('🏠 Home page script loaded');
    
    // Khởi tạo floating shapes cho hero section
    initFloatingShapes();
    
    // Khởi tạo header scroll effect
    initHeaderScrollEffect();
    
    // Các khởi tạo khác
    initializeHomePage();
});

// Thêm hàm khởi tạo floating shapes
function initFloatingShapes() {
    // Kiểm tra xem section hero có tồn tại không
    const heroSection = document.querySelector('.hero-banner');
    if (!heroSection) return;
    
    // Tạo container cho floating shapes nếu chưa có
    if (!heroSection.querySelector('.floating-shapes')) {
        const shapesContainer = document.createElement('div');
        shapesContainer.className = 'floating-shapes';
        
        // Tạo các shapes
        for (let i = 1; i <= 5; i++) {
            const shape = document.createElement('div');
            shape.className = `shape shape-${i}`;
            shapesContainer.appendChild(shape);
        }
        
        heroSection.appendChild(shapesContainer);
    }
}

// Thêm hàm xử lý hiệu ứng header
function initHeaderScrollEffect() {
    const header = document.querySelector('.header');
    if (!header) return;
    
    // Xử lý cuộn trang
    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });
    
    // Kích hoạt ngay lập tức nếu đã cuộn
    if (window.scrollY > 50) {
        header.classList.add('scrolled');
    }
}