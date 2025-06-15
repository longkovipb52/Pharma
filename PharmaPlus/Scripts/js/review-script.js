document.addEventListener('DOMContentLoaded', function() {
    console.log('🌟 Review script initialized');
    
    // Hiệu ứng cho rating stars
    initRatingStars();
    
    // Hiệu ứng cho các card đánh giá
    initReviewCards();
    
    // Hiệu ứng cho các nút
    initButtons();
    
    // Hiệu ứng fade-in khi trang được tải
    document.body.classList.add('page-loaded');
});

// Hiệu ứng chọn sao đánh giá
function initRatingStars() {
    const stars = document.querySelectorAll('.rating-stars input');
    const starLabels = document.querySelectorAll('.rating-stars label');
    
    // Hiệu ứng glow khi hover
    starLabels.forEach(label => {
        label.addEventListener('mouseenter', function() {
            this.classList.add('star-hover');
        });
        
        label.addEventListener('mouseleave', function() {
            this.classList.remove('star-hover');
        });
    });
    
    // Hiệu ứng khi chọn sao
    stars.forEach(star => {
        star.addEventListener('change', function() {
            const ratingValue = this.value;
            console.log('Rating selected: ' + ratingValue);
            
            // Hiệu ứng pulse khi chọn
            const label = document.querySelector(`label[for="star${ratingValue}"]`);
            label.classList.add('star-selected');
            
            // Hiển thị text tương ứng với số sao
            const ratingText = getRatingText(ratingValue);
            const ratingContainer = document.querySelector('.rating-input');
            
            // Xóa text cũ nếu có
            const oldText = ratingContainer.querySelector('.rating-text');
            if (oldText) {
                oldText.remove();
            }
            
            // Thêm text mới
            const ratingTextElement = document.createElement('div');
            ratingTextElement.className = 'rating-text';
            ratingTextElement.textContent = ratingText;
            ratingContainer.appendChild(ratingTextElement);
            
            // Hiệu ứng fade-in cho text
            setTimeout(() => {
                ratingTextElement.classList.add('show');
            }, 10);
            
            // Reset hiệu ứng pulse sau 300ms
            setTimeout(() => {
                label.classList.remove('star-selected');
            }, 300);
        });
    });
}

// Hiệu ứng cho các card đánh giá
function initReviewCards() {
    const cards = document.querySelectorAll('.review-card');
    
    // Áp dụng hiệu ứng staggered fade-in
    cards.forEach((card, index) => {
        // Thêm delay tăng dần cho mỗi card
        setTimeout(() => {
            card.classList.add('card-visible');
        }, 100 * index);
        
        // Thêm hiệu ứng hover
        card.addEventListener('mouseenter', function() {
            this.classList.add('card-hover');
        });
        
        card.addEventListener('mouseleave', function() {
            this.classList.remove('card-hover');
        });
    });
}

// Hiệu ứng ripple cho các nút
function initButtons() {
    const buttons = document.querySelectorAll('.btn');
    
    buttons.forEach(button => {
        button.addEventListener('click', createRipple);
        
        // Thêm hiệu ứng hover
        button.addEventListener('mouseenter', function() {
            this.classList.add('btn-hover');
        });
        
        button.addEventListener('mouseleave', function() {
            this.classList.remove('btn-hover');
        });
    });
}

// Tạo hiệu ứng ripple (gợn sóng) khi click nút
function createRipple(event) {
    const button = event.currentTarget;
    
    const circle = document.createElement('span');
    const diameter = Math.max(button.clientWidth, button.clientHeight);
    const radius = diameter / 2;
    
    circle.style.width = circle.style.height = `${diameter}px`;
    circle.style.left = `${event.clientX - button.offsetLeft - radius}px`;
    circle.style.top = `${event.clientY - button.offsetTop - radius}px`;
    circle.classList.add('ripple');
    
    // Xóa ripple cũ nếu có
    const ripple = button.querySelector('.ripple');
    if (ripple) {
        ripple.remove();
    }
    
    button.appendChild(circle);
}

// Lấy text mô tả cho đánh giá
function getRatingText(rating) {
    switch(parseInt(rating)) {
        case 1: return 'Rất không hài lòng';
        case 2: return 'Không hài lòng';
        case 3: return 'Bình thường';
        case 4: return 'Hài lòng';
        case 5: return 'Rất hài lòng';
        default: return '';
    }
}

// Hiệu ứng cho textarea comment
document.addEventListener('DOMContentLoaded', function() {
    const textarea = document.getElementById('comment');
    if (textarea) {
        textarea.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        textarea.addEventListener('blur', function() {
            this.parentElement.classList.remove('focused');
        });
        
        // Auto resize textarea khi nhập
        textarea.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });
    }
});

// Hiệu ứng animation cho form đánh giá
document.addEventListener('DOMContentLoaded', function() {
    const formElements = document.querySelectorAll('.review-form .form-group');
    
    formElements.forEach((element, index) => {
        // Thêm delay tăng dần cho mỗi element
        setTimeout(() => {
            element.classList.add('form-group-visible');
        }, 200 * index);
    });
});