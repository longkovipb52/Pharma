document.addEventListener('DOMContentLoaded', function() {
    const checkoutForm = document.querySelector('.checkout-form');
    
    if (checkoutForm) {
        // Xử lý khi submit form
        checkoutForm.addEventListener('submit', function(e) {
            let isValid = true;
            
            // Validate họ tên
            const recipientName = document.querySelector('[name="ShippingInfo.RecipientName"]');
            if (!recipientName.value.trim()) {
                showFieldError(recipientName, 'Vui lòng nhập họ tên người nhận');
                isValid = false;
            }
            
            // Validate số điện thoại
            const recipientPhone = document.querySelector('[name="ShippingInfo.RecipientPhone"]');
            const phoneRegex = /^(0|\+84)[0-9]{9,10}$/;
            if (!phoneRegex.test(recipientPhone.value.trim())) {
                showFieldError(recipientPhone, 'Số điện thoại không hợp lệ');
                isValid = false;
            }
            
            // Validate địa chỉ
            const shippingAddress = document.querySelector('[name="ShippingInfo.ShippingAddress"]');
            if (!shippingAddress.value.trim()) {
                showFieldError(shippingAddress, 'Vui lòng nhập địa chỉ giao hàng');
                isValid = false;
            }
            
            // Validate phương thức thanh toán
            const paymentMethod = document.querySelector('input[name="PaymentInfo.PaymentMethod"]:checked');
            if (!paymentMethod) {
                const paymentMethodsContainer = document.querySelector('.payment-methods');
                showContainerError(paymentMethodsContainer, 'Vui lòng chọn phương thức thanh toán');
                isValid = false;
            }
            
            // Hiển thị thông báo lỗi tổng hợp nếu có
            if (!isValid) {
                e.preventDefault();
                
                // Hiển thị thông báo tổng quan
                showCheckoutToast('Vui lòng điền đầy đủ thông tin thanh toán', 'error');
                
                // Scroll đến trường lỗi đầu tiên
                const firstErrorField = document.querySelector('.field-error');
                if (firstErrorField) {
                    firstErrorField.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            } else {
                // Disable button để tránh submit nhiều lần
                const submitBtn = checkoutForm.querySelector('.checkout-submit-btn');
                if (submitBtn) {
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
                }
            }
        });
        
        // Remove error messages when typing or changing
        checkoutForm.addEventListener('input', function(e) {
            if (e.target.classList.contains('form-control')) {
                clearFieldError(e.target);
            }
        });
        
        // Add changes for payment method selection
        const paymentMethods = document.querySelectorAll('.payment-method-item input');
        paymentMethods.forEach(method => {
            method.addEventListener('change', function() {
                clearContainerError(document.querySelector('.payment-methods'));
                
                paymentMethods.forEach(m => {
                    const item = m.closest('.payment-method-item');
                    item.classList.remove('selected');
                });
                
                if (this.checked) {
                    this.closest('.payment-method-item').classList.add('selected');
                }
            });
        });
    }
    
    // Helper functions
    function showFieldError(field, message) {
        clearFieldError(field);
        
        const errorSpan = document.createElement('span');
        errorSpan.className = 'field-error text-danger';
        errorSpan.textContent = message;
        
        field.classList.add('input-error');
        field.parentNode.appendChild(errorSpan);
    }
    
    function clearFieldError(field) {
        field.classList.remove('input-error');
        
        const existingError = field.parentNode.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }
    }
    
    function showContainerError(container, message) {
        clearContainerError(container);
        
        const errorSpan = document.createElement('span');
        errorSpan.className = 'field-error text-danger';
        errorSpan.textContent = message;
        
        container.classList.add('container-error');
        container.parentNode.appendChild(errorSpan);
    }
    
    function clearContainerError(container) {
        container.classList.remove('container-error');
        
        const existingError = container.parentNode.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }
    }
    
    function showCheckoutToast(message, type = 'info') {
        if (typeof showCartToast === 'function') {
            showCartToast(message, type);
        } else {
            alert(message);
        }
    }
});