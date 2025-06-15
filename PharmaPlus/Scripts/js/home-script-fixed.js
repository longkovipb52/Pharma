// Home Page JavaScript - Fixed for Cart Integration and Conflict Resolution

// ===== GLOBAL VARIABLES =====
let isPageLoaded = false;
let activeTab = 'featured';
let favoriteProducts = [];
let cartItems = [];
let lastScrollY = 0;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('🏠 Home page script loaded');
    
    // Initialize header scroll effect
    initHeaderScrollEffect();
    
    // Initialize any other components
    initializeAllComponents();
    
    // Animation cho nút View More
    const viewMoreBtn = document.querySelector('.btn-view-more');
    if (viewMoreBtn) {
        viewMoreBtn.addEventListener('mouseenter', function() {
            const icon = this.querySelector('i');
            if (icon) {
                icon.style.transform = 'translateX(8px)';
            }
        });
        
        viewMoreBtn.addEventListener('mouseleave', function() {
            const icon = this.querySelector('i');
            if (icon) {
                icon.style.transform = 'translateX(0)';
            }
        });
    }
});

// Header scroll effect
function initHeaderScrollEffect() {
    const header = document.querySelector('.header');
    if (!header) return;
    
    window.addEventListener('scroll', function() {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });
    
    // Activate immediately if page is already scrolled
    if (window.scrollY > 50) {
        header.classList.add('scrolled');
    }
}

// Initialize all components
function initializeAllComponents() {
    // Initialize animations
    initializeAnimations();
    
    // Enable mobile menu
    enableMobileMenu();
    
    // Enable category hover effects
    enableCategoryHover();
    
    // Update cart badge
    updateCartBadge();
}

// Animation initialization
function initializeAnimations() {
    // Add fade-in animation to elements
    const animatedElements = document.querySelectorAll(
        '.hero-text, .hero-visual, .section-header, .category-item, .product-card, .benefit-item, .testimonial-card'
    );
    
    animatedElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            element.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        }, 100 + (index * 50));
    });
}

// Mobile menu functionality
function enableMobileMenu() {
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const navMenu = document.getElementById('navMenu');
    
    if (mobileMenuBtn && navMenu) {
        mobileMenuBtn.addEventListener('click', function() {
            navMenu.classList.toggle('active');
            
            const icon = this.querySelector('i');
            if (icon) {
                if (navMenu.classList.contains('active')) {
                    icon.classList.remove('fa-bars');
                    icon.classList.add('fa-times');
                } else {
                    icon.classList.remove('fa-times');
                    icon.classList.add('fa-bars');
                }
            }
        });
    }
}

// Category hover effects
function enableCategoryHover() {
    const categories = document.querySelectorAll('.category-item');
    
    categories.forEach(category => {
        category.addEventListener('mouseenter', function() {
            const icon = this.querySelector('.category-icon');
            if (icon) {
                icon.style.transform = 'scale(1.1)';
            }
        });
        
        category.addEventListener('mouseleave', function() {
            const icon = this.querySelector('.category-icon');
            if (icon) {
                icon.style.transform = 'scale(1)';
            }
        });
    });
}

// Update cart badge
function updateCartBadge(count) {
    const badge = document.querySelector('.badge');
    if (!badge) return;
    
    if (count === undefined) {
        // Try to get count from localStorage if count parameter is not provided
        try {
            const cartItems = JSON.parse(localStorage.getItem('pharma_cart')) || [];
            count = cartItems.reduce((total, item) => total + item.quantity, 0);
        } catch (e) {
            count = 0;
        }
    }
    
    if (count && count > 0) {
        badge.textContent = count;
        badge.style.display = 'flex';
    } else {
        badge.style.display = 'none';
    }
}

// Add to cart function
function addToCart(productId, quantity = 1) {
    try {
        // Add loading effect to button
        const button = event ? event.target.closest('.btn-add-cart') : null;
        if (button) {
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Adding...';
            button.disabled = true;
        }
        
        // Make AJAX call to add to cart
        fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ productId: productId, quantity: quantity })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Show success notification
                showNotification('Item added to cart successfully', 'success');
                
                // Update cart badge
                updateCartBadge(data.totalItems);
                
                // Reset button
                if (button) {
                    button.innerHTML = '<i class="fas fa-check"></i> Added';
                    setTimeout(() => {
                        button.innerHTML = '<i class="fas fa-cart-plus"></i> Add to Cart';
                        button.disabled = false;
                    }, 2000);
                }
            } else {
                // Show error notification
                showNotification(data.message || 'Failed to add item to cart', 'error');
                
                // Reset button
                if (button) {
                    button.innerHTML = '<i class="fas fa-cart-plus"></i> Add to Cart';
                    button.disabled = false;
                }
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('An error occurred. Please try again.', 'error');
            
            // Reset button
            if (button) {
                button.innerHTML = '<i class="fas fa-cart-plus"></i> Add to Cart';
                button.disabled = false;
            }
        });
    } catch (error) {
        console.error('Error adding to cart:', error);
        showNotification('An error occurred. Please try again.', 'error');
        
        // Reset button
        if (button) {
            button.innerHTML = '<i class="fas fa-cart-plus"></i> Add to Cart';
            button.disabled = false;
        }
    }
}

// Show notification
function showNotification(message, type = 'info') {
    const notification = document.getElementById('notification');
    const notificationText = document.getElementById('notificationText');
    
    if (notification && notificationText) {
        // Set message
        notificationText.textContent = message;
        
        // Set type
        notification.className = 'notification';
        notification.classList.add(`notification-${type}`);
        
        // Set icon
        const icon = notification.querySelector('i');
        if (icon) {
            icon.className = 'fas';
            
            switch (type) {
                case 'success':
                    icon.classList.add('fa-check-circle');
                    break;
                case 'error':
                    icon.classList.add('fa-exclamation-circle');
                    break;
                case 'warning':
                    icon.classList.add('fa-exclamation-triangle');
                    break;
                default:
                    icon.classList.add('fa-info-circle');
            }
        }
        
        // Show notification
        notification.classList.add('show');
        
        // Hide notification after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
        }, 3000);
    }
}

// Make functions available globally
window.addToCart = addToCart;
window.updateCartBadge = updateCartBadge;
window.showNotification = showNotification;