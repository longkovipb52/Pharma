document.addEventListener('DOMContentLoaded', function() {
    // Password visibility toggle
    window.togglePassword = function(inputId) {
        const input = document.getElementById(inputId);
        const icon = document.getElementById(inputId + 'Icon');
        
        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.remove('fa-eye');
            icon.classList.add('fa-eye-slash');
        } else {
            input.type = 'password';
            icon.classList.remove('fa-eye-slash');
            icon.classList.add('fa-eye');
        }
    };

    // Password strength checker
    const newPasswordInput = document.getElementById('newPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const submitBtn = document.getElementById('submitBtn');
    
    if (newPasswordInput) {
        newPasswordInput.addEventListener('input', function() {
            checkPasswordStrength(this.value);
            checkPasswordRequirements(this.value);
            validatePasswords();
        });
    }
    
    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', function() {
            validatePasswords();
        });
    }

    function checkPasswordStrength(password) {
        const strengthIndicator = document.getElementById('passwordStrength');
        if (!strengthIndicator) return;
        
        let strength = 0;
        let feedback = '';
        
        if (password.length >= 6) strength++;
        if (password.match(/[a-z]/)) strength++;
        if (password.match(/[A-Z]/)) strength++;
        if (password.match(/[0-9]/)) strength++;
        if (password.match(/[^a-zA-Z0-9]/)) strength++;
        
        switch (strength) {
            case 0:
            case 1:
                strengthIndicator.className = 'password-strength weak';
                feedback = 'Mật khẩu rất yếu';
                break;
            case 2:
            case 3:
                strengthIndicator.className = 'password-strength medium';
                feedback = 'Mật khẩu trung bình';
                break;
            case 4:
            case 5:
                strengthIndicator.className = 'password-strength strong';
                feedback = 'Mật khẩu mạnh';
                break;
        }
        
        strengthIndicator.textContent = feedback;
    }
    
    function checkPasswordRequirements(password) {
        const requirements = [
            { id: 'length-requirement', test: password.length >= 6 },
            { id: 'uppercase-requirement', test: /[A-Z]/.test(password) },
            { id: 'lowercase-requirement', test: /[a-z]/.test(password) },
            { id: 'number-requirement', test: /[0-9]/.test(password) }
        ];
        
        requirements.forEach(req => {
            const element = document.getElementById(req.id);
            if (element) {
                const icon = element.querySelector('i');
                if (req.test) {
                    element.classList.add('valid');
                    icon.className = 'fas fa-check';
                } else {
                    element.classList.remove('valid');
                    icon.className = 'fas fa-times';
                }
            }
        });
    }
    
    function validatePasswords() {
        if (!newPasswordInput || !confirmPasswordInput || !submitBtn) return;
        
        const newPassword = newPasswordInput.value;
        const confirmPassword = confirmPasswordInput.value;
        
        const isNewPasswordValid = newPassword.length >= 6 && 
                                  /[A-Z]/.test(newPassword) && 
                                  /[a-z]/.test(newPassword) && 
                                  /[0-9]/.test(newPassword);
        
        const doPasswordsMatch = newPassword === confirmPassword && newPassword.length > 0;
        
        submitBtn.disabled = !(isNewPasswordValid && doPasswordsMatch);
        
        // Update confirm password field styling
        if (confirmPassword.length > 0) {
            if (doPasswordsMatch) {
                confirmPasswordInput.style.borderColor = '#16a34a';
            } else {
                confirmPasswordInput.style.borderColor = '#dc2626';
            }
        } else {
            confirmPasswordInput.style.borderColor = '#e5e7eb';
        }
    }

    // Form validation for edit profile
    const editForm = document.querySelector('.edit-profile-form');
    if (editForm) {
        editForm.addEventListener('submit', function(e) {
            const username = document.querySelector('input[name="Username"]').value.trim();
            const fullName = document.querySelector('input[name="FullName"]').value.trim();
            const email = document.querySelector('input[name="Email"]').value.trim();
            
            if (!username || !fullName || !email) {
                e.preventDefault();
                alert('Vui lòng điền đầy đủ thông tin bắt buộc');
                return false;
            }
            
            if (!isValidEmail(email)) {
                e.preventDefault();
                alert('Email không hợp lệ');
                return false;
            }
        });
    }
    
    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Auto-save draft functionality (optional)
    const formInputs = document.querySelectorAll('.edit-profile-form input, .edit-profile-form textarea, .edit-profile-form select');
    formInputs.forEach(input => {
        input.addEventListener('input', function() {
            // Save to localStorage as draft
            const formData = new FormData(editForm);
            const draftData = {};
            for (let [key, value] of formData.entries()) {
                draftData[key] = value;
            }
            localStorage.setItem('profileDraft', JSON.stringify(draftData));
        });
    });

    // Load draft on page load
    if (editForm) {
        const savedDraft = localStorage.getItem('profileDraft');
        if (savedDraft) {
            try {
                const draftData = JSON.parse(savedDraft);
                Object.keys(draftData).forEach(key => {
                    const input = editForm.querySelector(`[name="${key}"]`);
                    if (input && input.value === '') {
                        input.value = draftData[key];
                    }
                });
            } catch (e) {
                console.log('Could not load draft data');
            }
        }
    }

    // Clear draft on successful submit
    if (editForm) {
        editForm.addEventListener('submit', function() {
            localStorage.removeItem('profileDraft');
        });
    }

    // Smooth scroll to error fields
    const errorFields = document.querySelectorAll('.field-validation-error');
    if (errorFields.length > 0) {
        errorFields[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
});