/**
 * Contact Functionality JavaScript
 * Xử lý tương tác và hiệu ứng cho chức năng liên hệ
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('📞 Contact script initialized');
    
    // Khởi tạo các chức năng
    initContactFormAnimations();
    initFaqAccordion();
    initHistoryPage();
    initFloatingShapes();
    fixBodyScrolling();
    
    // Khởi tạo các hiệu ứng AOS nếu thư viện đã được tải
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 800,
            easing: 'ease-out-cubic',
            once: true,
            offset: 50
        });
    }
});

/**
 * Khởi tạo các hiệu ứng cho form liên hệ
 */
function initContactFormAnimations() {
    const form = document.getElementById('contactForm');
    if (!form) return;
    
    // Hiệu ứng focus cho các trường input
    const inputs = form.querySelectorAll('input, textarea, select');
    inputs.forEach(input => {
        // Kiểm tra nếu đã có giá trị khi tải trang
        if (input.value !== '') {
            input.parentElement.classList.add('focused');
        }
        
        // Xử lý sự kiện focus/blur
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        input.addEventListener('blur', function() {
            if (this.value === '') {
                this.parentElement.classList.remove('focused');
            }
        });
    });
    
    // Hiệu ứng nút gửi khi submit form
    form.addEventListener('submit', function(e) {
        // Kiểm tra validate
        const isValid = validateContactForm();
        if (!isValid) {
            e.preventDefault();
            return;
        }
        
        // Hiệu ứng nút gửi
        const submitBtn = this.querySelector('.btn-send');
        if (submitBtn) {
            submitBtn.classList.add('sending');
            submitBtn.querySelector('.btn-text').textContent = 'Đang gửi...';
            
            // Hiệu ứng chạy vòng tròn
            const iconElem = submitBtn.querySelector('i');
            iconElem.className = 'fas fa-spinner fa-spin';
            
            // Vô hiệu hóa nút để tránh gửi nhiều lần
            submitBtn.disabled = true;
        }
    });
    
    // Thêm hiệu ứng ripple cho buttons
    const buttons = document.querySelectorAll('.btn-send, .btn-history');
    buttons.forEach(button => {
        button.addEventListener('click', createRippleEffect);
    });
}

/**
 * Kiểm tra form trước khi gửi
 */
function validateContactForm() {
    const form = document.getElementById('contactForm');
    if (!form) return true;
    
    let isValid = true;
    const nameInput = form.querySelector('input[name="Name"]');
    const emailInput = form.querySelector('input[name="Email"]');
    const subjectInput = form.querySelector('select[name="Subject"]');
    const messageInput = form.querySelector('textarea[name="Message"]');
    
    // Xóa các thông báo lỗi cũ
    const errorMessages = form.querySelectorAll('.field-error');
    errorMessages.forEach(error => error.remove());
    
    // Kiểm tra tên
    if (!nameInput.value.trim()) {
        showFieldError(nameInput, 'Vui lòng nhập họ tên');
        isValid = false;
    }
    
    // Kiểm tra email
    if (!emailInput.value.trim()) {
        showFieldError(emailInput, 'Vui lòng nhập email');
        isValid = false;
    } else if (!isValidEmail(emailInput.value.trim())) {
        showFieldError(emailInput, 'Email không hợp lệ');
        isValid = false;
    }
    
    // Kiểm tra chủ đề
    if (subjectInput && subjectInput.value === '') {
        showFieldError(subjectInput, 'Vui lòng chọn chủ đề');
        isValid = false;
    }
    
    // Kiểm tra nội dung
    if (!messageInput.value.trim()) {
        showFieldError(messageInput, 'Vui lòng nhập nội dung liên hệ');
        isValid = false;
    } else if (messageInput.value.trim().length < 10) {
        showFieldError(messageInput, 'Nội dung quá ngắn (tối thiểu 10 ký tự)');
        isValid = false;
    }
    
    // Nếu có lỗi, cuộn đến lỗi đầu tiên
    if (!isValid) {
        const firstError = form.querySelector('.field-error');
        if (firstError) {
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
    
    return isValid;
}

/**
 * Hiển thị thông báo lỗi cho trường input
 */
function showFieldError(inputElement, message) {
    const formGroup = inputElement.closest('.form-group');
    const errorElem = document.createElement('div');
    errorElem.className = 'field-error';
    errorElem.innerHTML = `<i class="fas fa-exclamation-circle"></i> ${message}`;
    formGroup.appendChild(errorElem);
    
    // Highlight trường input
    inputElement.classList.add('input-error');
    
    // Xóa lỗi và highlight khi người dùng nhập lại
    inputElement.addEventListener('input', function() {
        const error = formGroup.querySelector('.field-error');
        if (error) error.remove();
        inputElement.classList.remove('input-error');
    }, { once: true });
}

/**
 * Kiểm tra email hợp lệ
 */
function isValidEmail(email) {
    const re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email.toLowerCase());
}

/**
 * Tạo hiệu ứng ripple khi click button
 */
function createRippleEffect(event) {
    const button = event.currentTarget;
    
    // Xóa các ripple cũ
    const ripples = button.querySelectorAll('.ripple');
    ripples.forEach(ripple => {
        ripple.remove();
    });
    
    const ripple = document.createElement('span');
    ripple.classList.add('ripple');
    button.appendChild(ripple);
    
    const buttonRect = button.getBoundingClientRect();
    const diameter = Math.max(buttonRect.width, buttonRect.height);
    const radius = diameter / 2;
    
    ripple.style.width = ripple.style.height = `${diameter}px`;
    ripple.style.left = `${event.clientX - buttonRect.left - radius}px`;
    ripple.style.top = `${event.clientY - buttonRect.top - radius}px`;
    
    ripple.classList.add('ripple-effect');
    
    // Xóa ripple sau khi animation kết thúc
    setTimeout(() => {
        ripple.remove();
    }, 600);
}

/**
 * Khởi tạo FAQ Accordion
 */
function initFaqAccordion() {
    const faqItems = document.querySelectorAll('.faq-item');
    if (!faqItems.length) return;
    
    faqItems.forEach(item => {
        const question = item.querySelector('.faq-question');
        const answer = item.querySelector('.faq-answer');
        
        // Tất cả các câu trả lời ban đầu ẩn (trừ câu đầu tiên)
        if (item !== faqItems[0]) {
            answer.style.display = 'none';
        } else {
            item.classList.add('active');
            const icon = item.querySelector('.faq-icon i');
            if (icon) icon.classList.replace('fa-chevron-down', 'fa-chevron-up');
        }
        
        question.addEventListener('click', function() {
            // Toggle active class
            const isActive = item.classList.contains('active');
            
            // Đóng tất cả các câu hỏi khác
            faqItems.forEach(otherItem => {
                if (otherItem !== item && otherItem.classList.contains('active')) {
                    otherItem.classList.remove('active');
                    const otherAnswer = otherItem.querySelector('.faq-answer');
                    const otherIcon = otherItem.querySelector('.faq-icon i');
                    
                    if (otherAnswer) slideUp(otherAnswer, 200);
                    if (otherIcon) otherIcon.classList.replace('fa-chevron-up', 'fa-chevron-down');
                }
            });
            
            // Toggle câu hỏi hiện tại
            if (!isActive) {
                item.classList.add('active');
                if (answer) slideDown(answer, 200);
                const icon = item.querySelector('.faq-icon i');
                if (icon) icon.classList.replace('fa-chevron-down', 'fa-chevron-up');
            } else {
                item.classList.remove('active');
                if (answer) slideUp(answer, 200);
                const icon = item.querySelector('.faq-icon i');
                if (icon) icon.classList.replace('fa-chevron-up', 'fa-chevron-down');
            }
        });
    });
}

/**
 * SlideUp animation helper
 */
function slideUp(element, duration = 300) {
    element.style.height = element.offsetHeight + 'px';
    element.offsetHeight; // Force reflow
    element.style.transitionProperty = 'height, margin, padding';
    element.style.transitionDuration = duration + 'ms';
    element.style.overflow = 'hidden';
    element.style.height = 0;
    element.style.paddingTop = 0;
    element.style.paddingBottom = 0;
    element.style.marginTop = 0;
    element.style.marginBottom = 0;
    
    setTimeout(() => {
        element.style.display = 'none';
        element.style.removeProperty('height');
        element.style.removeProperty('padding-top');
        element.style.removeProperty('padding-bottom');
        element.style.removeProperty('margin-top');
        element.style.removeProperty('margin-bottom');
        element.style.removeProperty('overflow');
        element.style.removeProperty('transition-duration');
        element.style.removeProperty('transition-property');
    }, duration);
}

/**
 * SlideDown animation helper
 */
function slideDown(element, duration = 300) {
    element.style.removeProperty('display');
    let display = window.getComputedStyle(element).display;
    if (display === 'none') display = 'block';
    element.style.display = display;
    
    const height = element.offsetHeight;
    element.style.overflow = 'hidden';
    element.style.height = 0;
    element.style.paddingTop = 0;
    element.style.paddingBottom = 0;
    element.style.marginTop = 0;
    element.style.marginBottom = 0;
    element.offsetHeight; // Force reflow
    
    element.style.transitionProperty = 'height, margin, padding';
    element.style.transitionDuration = duration + 'ms';
    element.style.height = height + 'px';
    element.style.removeProperty('padding-top');
    element.style.removeProperty('padding-bottom');
    element.style.removeProperty('margin-top');
    element.style.removeProperty('margin-bottom');
    
    setTimeout(() => {
        element.style.removeProperty('height');
        element.style.removeProperty('overflow');
        element.style.removeProperty('transition-duration');
        element.style.removeProperty('transition-property');
    }, duration);
}

/**
 * Khởi tạo trang lịch sử liên hệ
 */
function initHistoryPage() {
    const contactCards = document.querySelectorAll('.contact-card');
    if (!contactCards.length) return;
    
    // Hiệu ứng staggered fade-in cho các card
    contactCards.forEach((card, index) => {
        setTimeout(() => {
            card.classList.add('card-visible');
        }, 100 * index);
    });
    
    // Hiệu ứng toggle nội dung
    const toggleBtns = document.querySelectorAll('.toggle-message');
    toggleBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const messageContent = this.closest('.contact-card').querySelector('.message-content');
            const isExpanded = messageContent.classList.contains('expanded');
            
            if (isExpanded) {
                messageContent.classList.remove('expanded');
                this.innerHTML = 'Xem thêm <i class="fas fa-chevron-down"></i>';
            } else {
                messageContent.classList.add('expanded');
                this.innerHTML = 'Thu gọn <i class="fas fa-chevron-up"></i>';
            }
        });
    });
    
    // Hiệu ứng hover cho các card
    contactCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.classList.add('card-hover');
        });
        
        card.addEventListener('mouseleave', function() {
            this.classList.remove('card-hover');
        });
    });
}

/**
 * Khởi tạo các hình nổi trong background
 */
function initFloatingShapes() {
    // Đảm bảo các shapes được tạo và có hiệu ứng
    const floatingShapes = document.querySelector('.floating-shapes');
    if (!floatingShapes) return;
    
    // Thêm CSS animation cho các shapes nếu chưa có
    const styleExists = document.getElementById('floating-shapes-style');
    if (!styleExists) {
        const style = document.createElement('style');
        style.id = 'floating-shapes-style';
        style.textContent = `
            @keyframes float {
                0% { transform: translate(0, 0) rotate(0deg); }
                25% { transform: translate(10px, -10px) rotate(5deg); }
                50% { transform: translate(0, -20px) rotate(0deg); }
                75% { transform: translate(-10px, -10px) rotate(-5deg); }
                100% { transform: translate(0, 0) rotate(0deg); }
            }
            
            .shape {
                position: absolute;
                background: rgba(59, 130, 246, 0.1);
                border-radius: 50%;
                animation: float 15s ease-in-out infinite;
            }
            
            .shape-1 {
                width: 120px;
                height: 120px;
                top: 10%;
                left: 5%;
                animation-delay: 0s;
            }
            
            .shape-2 {
                width: 80px;
                height: 80px;
                top: 60%;
                right: 10%;
                animation-delay: 2s;
                background: rgba(16, 185, 129, 0.1);
            }
            
            .shape-3 {
                width: 100px;
                height: 100px;
                bottom: 10%;
                left: 15%;
                animation-delay: 4s;
                background: rgba(245, 158, 11, 0.1);
            }
            
            .shape-4 {
                width: 50px;
                height: 50px;
                top: 30%;
                right: 20%;
                animation-delay: 6s;
                background: rgba(239, 68, 68, 0.1);
            }
        `;
        
        document.head.appendChild(style);
    }
}

/**
 * Sửa lỗi cuộn trang
 */
function fixBodyScrolling() {
    // Đảm bảo body có thể cuộn
    document.body.style.overflow = 'auto';
    document.body.style.height = 'auto';
    
    // Xóa bất kỳ inline style nào có thể gây ra vấn đề cuộn
    document.documentElement.style.overflow = 'auto';
    document.documentElement.style.height = 'auto';
    
    // Fix cho iOS Safari
    document.ontouchmove = function(e) {
        return true;
    };
    
    // Loại bỏ các event handler có thể ngăn chặn cuộn trang
    window.onscroll = null;
    document.onmousewheel = null;
    document.ontouchmove = null;
    document.onwheel = null;
}

/**
 * Hiển thị thông báo popup
 */
function showContactNotification(message, type = 'success') {
    const container = document.createElement('div');
    container.className = 'contact-notification ' + type;
    container.innerHTML = `
        <div class="notification-icon">
            <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'}"></i>
        </div>
        <div class="notification-message">${message}</div>
        <div class="notification-close"><i class="fas fa-times"></i></div>
    `;
    
    document.body.appendChild(container);
    
    // Hiệu ứng hiển thị
    setTimeout(() => {
        container.classList.add('show');
    }, 10);
    
    // Tự động ẩn sau 5 giây
    const timeout = setTimeout(() => {
        hideNotification(container);
    }, 5000);
    
    // Xử lý đóng thông báo khi click
    const closeBtn = container.querySelector('.notification-close');
    closeBtn.addEventListener('click', () => {
        clearTimeout(timeout);
        hideNotification(container);
    });
}

/**
 * Ẩn thông báo với animation
 */
function hideNotification(notification) {
    notification.classList.remove('show');
    notification.classList.add('hide');
    
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 300);
}

// Export các hàm cần thiết để sử dụng từ bên ngoài
window.showContactNotification = showContactNotification;
window.validateContactForm = validateContactForm;