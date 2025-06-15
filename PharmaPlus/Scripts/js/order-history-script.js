/**
 * Order History JavaScript
 * Xử lý tương tác và hiệu ứng cho chức năng lịch sử đơn hàng
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('🚀 Order History JS initialized');
    
    // Khởi tạo tất cả các chức năng
    initCancelOrderModal();
    initDateFilters();
    initEntryAnimations();
    initTimelineAnimations();
    
    // Hiển thị alert messages tạm thời
    initAlertMessages();
    
    // Hiệu ứng tiến trình đơn hàng
    initOrderProgressBar();
    
    // Thêm hàm mới để giải quyết vấn đề không hiển thị trạng thái đúng
    updateOrderStatusDisplay();
    
    // Thêm nút xuất đơn hàng ra PDF
    addExportToPdfButton();
    
    // Các hàm khởi tạo khác...
    initLoadingEffect();
    
    // Hiển thị loading ban đầu
    window.showPageLoading();
});

/**
 * Xử lý modal hủy đơn hàng
 */
function initCancelOrderModal() {
    // Tìm modal và form
    const cancelOrderModal = document.getElementById('cancelOrderModal');
    const cancelOrderForm = document.getElementById('cancelOrderForm');
    const cancelOrderIdInput = document.getElementById('cancelOrderId');
    
    // Lấy tất cả nút hủy đơn hàng
    const cancelButtons = document.querySelectorAll('.btn-cancel-order');
    
    // Thêm sự kiện cho từng nút
    cancelButtons.forEach(button => {
        button.addEventListener('click', function() {
            const orderId = this.getAttribute('data-order-id');
            if (cancelOrderIdInput) {
                cancelOrderIdInput.value = orderId;
            }
            
            // Hiện modal (nếu không dùng Bootstrap's data attributes)
            try {
                $(cancelOrderModal).modal('show');
            } catch (e) {
                console.warn('Bootstrap modal not available, using native dialog');
                // Fallback nếu không có Bootstrap
                if (confirm('Bạn có chắc chắn muốn hủy đơn hàng này không?')) {
                    if (cancelOrderForm) cancelOrderForm.submit();
                }
            }
        });
    });
    
    // Xử lý submit form từ modal (để tăng UX)
    if (cancelOrderForm) {
        cancelOrderForm.addEventListener('submit', function() {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
            }
        });
    }
}

/**
 * Xử lý bộ lọc ngày tháng
 */
function initDateFilters() {
    const fromDateInput = document.getElementById('Filter_FromDate');
    const toDateInput = document.getElementById('Filter_ToDate');
    
    if (fromDateInput && toDateInput) {
        // Đặt giá trị tối đa cho fromDate là ngày hiện tại
        const today = new Date().toISOString().split('T')[0];
        fromDateInput.setAttribute('max', today);
        
        // Cập nhật giá trị min của toDate dựa trên fromDate
        fromDateInput.addEventListener('change', function() {
            if (this.value) {
                toDateInput.setAttribute('min', this.value);
            } else {
                toDateInput.removeAttribute('min');
            }
        });
        
        // Cập nhật giá trị max của fromDate dựa trên toDate
        toDateInput.addEventListener('change', function() {
            if (this.value) {
                fromDateInput.setAttribute('max', this.value);
            } else {
                fromDateInput.setAttribute('max', today);
            }
        });
        
        // Khởi tạo giá trị min nếu fromDate đã có giá trị
        if (fromDateInput.value) {
            toDateInput.setAttribute('min', fromDateInput.value);
        }
        
        // Khởi tạo giá trị max nếu toDate đã có giá trị
        if (toDateInput.value) {
            fromDateInput.setAttribute('max', toDateInput.value);
        }
    }
}

/**
 * Thêm hiệu ứng cho các phần tử khi tải trang
 */
function initEntryAnimations() {
    // Animation cho danh sách đơn hàng
    const orderCards = document.querySelectorAll('.order-card');
    if (orderCards.length) {
        orderCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            
            setTimeout(() => {
                card.style.transition = 'all 0.4s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 100 + (index * 50)); // Staggered animation
        });
    }
    
    // Animation cho thông tin đơn hàng chi tiết
    const infoCards = document.querySelectorAll('.info-card');
    if (infoCards.length) {
        infoCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            
            setTimeout(() => {
                card.style.transition = 'all 0.4s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 200 + (index * 100)); // Staggered animation
        });
    }
    
    // Animation cho bảng sản phẩm
    const productRows = document.querySelectorAll('tbody tr');
    if (productRows.length) {
        productRows.forEach((row, index) => {
            row.style.opacity = '0';
            
            setTimeout(() => {
                row.style.transition = 'opacity 0.3s ease';
                row.style.opacity = '1';
            }, 300 + (index * 50)); // Staggered animation
        });
    }
    
    // Animation cho từng hàng sản phẩm
    const productCells = document.querySelectorAll('.product-cell');
    if (productCells.length) {
        productCells.forEach((cell, index) => {
            cell.style.opacity = '0';
            cell.style.transform = 'translateX(-15px)';
            
            setTimeout(() => {
                cell.style.transition = 'all 0.4s ease';
                cell.style.opacity = '1';
                cell.style.transform = 'translateX(0)';
            }, 400 + (index * 100)); // Delayed staggered animation
        });
    }
    
    // Animation cho footer bảng (tổng tiền)
    const footerRows = document.querySelectorAll('tfoot tr');
    if (footerRows.length) {
        footerRows.forEach((row, index) => {
            row.style.opacity = '0';
            
            setTimeout(() => {
                row.style.transition = 'all 0.3s ease';
                row.style.opacity = '1';
            }, 800 + (index * 100)); // Delayed after product rows
        });
    }
}

/**
 * Hiệu ứng cho timeline
 */
function initTimelineAnimations() {
    const timelineItems = document.querySelectorAll('.timeline-item');
    
    if (timelineItems.length) {
        // Thêm class để xác định phần tử có animation
        timelineItems.forEach(item => {
            item.classList.add('with-animation');
        });
        
        // Sử dụng Intersection Observer để kích hoạt animation khi scroll đến
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.2
        });
        
        // Thiết lập delay và observe
        timelineItems.forEach((item, index) => {
            item.style.transitionDelay = `${index * 150}ms`;
            observer.observe(item);
        });
        
        // Fallback: Hiển thị tất cả items sau 1 giây nếu IntersectionObserver không hoạt động
        setTimeout(() => {
            timelineItems.forEach(item => {
                if (!item.classList.contains('fade-in')) {
                    item.classList.add('fade-in');
                }
            });
        }, 1000);
    }
}

/**
 * Xử lý thông báo tạm thời
 */
function initAlertMessages() {
    const alerts = document.querySelectorAll('.alert');
    
    if (alerts.length) {
        alerts.forEach(alert => {
            // Hiệu ứng fade in
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.style.transition = 'opacity 0.5s ease';
                alert.style.opacity = '1';
            }, 100);
            
            // Tự động ẩn sau 5 giây
            setTimeout(() => {
                alert.style.opacity = '0';
                setTimeout(() => {
                    alert.style.display = 'none';
                }, 500);
            }, 5000);
            
            // Thêm nút đóng
            const closeBtn = document.createElement('button');
            closeBtn.type = 'button';
            closeBtn.className = 'close';
            closeBtn.innerHTML = '&times;';
            closeBtn.style.marginLeft = 'auto';
            closeBtn.style.background = 'none';
            closeBtn.style.border = 'none';
            closeBtn.style.fontSize = '20px';
            closeBtn.style.cursor = 'pointer';
            closeBtn.style.padding = '0';
            closeBtn.style.lineHeight = '1';
            
            closeBtn.addEventListener('click', () => {
                alert.style.opacity = '0';
                setTimeout(() => {
                    alert.style.display = 'none';
                }, 300);
            });
            
            alert.appendChild(closeBtn);
        });
    }
}

/**
 * Hiển thị một thông báo popup
 * @param {string} message Nội dung thông báo
 * @param {string} type Loại thông báo (success, error, info)
 */
function showNotification(message, type = 'info') {
    // Tạo container nếu chưa có
    let container = document.getElementById('notifications-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'notifications-container';
        container.style.position = 'fixed';
        container.style.top = '20px';
        container.style.right = '20px';
        container.style.zIndex = '9999';
        container.style.display = 'flex';
        container.style.flexDirection = 'column';
        container.style.gap = '10px';
        document.body.appendChild(container);
    }
    
    // Tạo notification
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    
    // Thiết lập style
    notification.style.backgroundColor = type === 'success' ? '#34a853' : 
                                         type === 'error' ? '#ea4335' : '#4285f4';
    notification.style.color = 'white';
    notification.style.padding = '15px 20px';
    notification.style.borderRadius = '8px';
    notification.style.boxShadow = '0 3px 8px rgba(0,0,0,0.2)';
    notification.style.display = 'flex';
    notification.style.alignItems = 'center';
    notification.style.gap = '10px';
    notification.style.minWidth = '300px';
    notification.style.opacity = '0';
    notification.style.transform = 'translateX(100px)';
    notification.style.transition = 'all 0.3s ease';
    
    // Thêm icon
    const icon = document.createElement('i');
    icon.className = type === 'success' ? 'fas fa-check-circle' : 
                     type === 'error' ? 'fas fa-exclamation-circle' : 'fas fa-info-circle';
    
    // Thêm nội dung
    const content = document.createElement('span');
    content.textContent = message;
    content.style.flex = '1';
    
    // Thêm nút đóng
    const closeBtn = document.createElement('i');
    closeBtn.className = 'fas fa-times';
    closeBtn.style.cursor = 'pointer';
    
    // Thêm các phần tử vào notification
    notification.appendChild(icon);
    notification.appendChild(content);
    notification.appendChild(closeBtn);
    
    // Thêm notification vào container
    container.appendChild(notification);
    
    // Hiển thị notification với animation
    setTimeout(() => {
        notification.style.opacity = '1';
        notification.style.transform = 'translateX(0)';
    }, 10);
    
    // Tự động đóng sau 5 giây
    const timeout = setTimeout(() => {
        closeNotification(notification);
    }, 5000);
    
    // Xử lý đóng notification
    closeBtn.addEventListener('click', () => {
        clearTimeout(timeout);
        closeNotification(notification);
    });
    
    // Hàm đóng notification
    function closeNotification(notif) {
        notif.style.opacity = '0';
        notif.style.transform = 'translateX(100px)';
        
        setTimeout(() => {
            container.removeChild(notif);
            if (container.children.length === 0) {
                document.body.removeChild(container);
            }
        }, 300);
    }
}

/**
 * Hiệu ứng tiến trình đơn hàng (thanh progress bar)
 * Hiển thị dưới dạng thanh tiến trình trực quan
 */
function initOrderProgressBar() {
    // Chỉ chạy trên trang chi tiết đơn hàng
    const orderDetailPage = document.querySelector('.order-detail-page');
    if (!orderDetailPage) return;
    
    // Tạo phần tử hiển thị tiến trình
    const orderInfoSection = document.querySelector('.order-info-section');
    if (!orderInfoSection) return;
    
    // Lấy trạng thái đơn hàng hiện tại
    const statusElement = document.querySelector('.status-dot').parentElement;
    const statusClass = statusElement ? statusElement.className : '';
    const statusCode = parseInt(statusClass.replace('value status-', '')) || 1;
    
    // Không hiển thị progress bar cho đơn hàng đã hủy hoặc hoàn trả
    if (statusCode === 6 || statusCode === 7) return;
    
    // Tạo progress bar
    const progressWrapper = document.createElement('div');
    progressWrapper.className = 'order-progress-wrapper';
    progressWrapper.style.marginTop = '20px';
    progressWrapper.style.overflow = 'hidden';
    progressWrapper.style.borderRadius = '8px';
    progressWrapper.style.boxShadow = '0 1px 3px rgba(0,0,0,0.1)';
    
    // Status steps
    const steps = [
        { code: 1, name: 'Đang xử lý', icon: 'fa-clipboard-list' },
        { code: 2, name: 'Đã xác nhận', icon: 'fa-check-circle' },
        { code: 3, name: 'Đang giao hàng', icon: 'fa-shipping-fast' },
        { code: 4, name: 'Đã giao hàng', icon: 'fa-box-open' },
        { code: 5, name: 'Đã hoàn thành', icon: 'fa-star' }
    ];
    
    // Tính phần trăm hoàn thành
    const progressPercent = ((statusCode - 1) / (steps.length - 1)) * 100;
    
    // Tạo thanh progress
    const progressContainer = document.createElement('div');
    progressContainer.className = 'progress-container';
    progressContainer.style.position = 'relative';
    progressContainer.style.height = '5px';
    progressContainer.style.background = '#e9ecef';
    
    const progressBar = document.createElement('div');
    progressBar.className = 'progress-bar';
    progressBar.style.position = 'absolute';
    progressBar.style.height = '100%';
    progressBar.style.width = '0%';
    progressBar.style.background = 'linear-gradient(to right, #4285f4, #34a853)';
    progressBar.style.transition = 'width 1.5s ease-in-out';
    
    progressContainer.appendChild(progressBar);
    
    // Tạo các step
    const stepsContainer = document.createElement('div');
    stepsContainer.className = 'steps-container';
    stepsContainer.style.display = 'flex';
    stepsContainer.style.justifyContent = 'space-between';
    stepsContainer.style.position = 'relative';
    stepsContainer.style.padding = '20px 0';
    
    steps.forEach((step, index) => {
        const stepElement = document.createElement('div');
        stepElement.className = `step-item ${statusCode >= step.code ? 'active' : ''}`;
        stepElement.style.display = 'flex';
        stepElement.style.flexDirection = 'column';
        stepElement.style.alignItems = 'center';
        stepElement.style.position = 'relative';
        stepElement.style.zIndex = '1';
        stepElement.style.flex = '1';
        stepElement.style.opacity = statusCode >= step.code ? '1' : '0.5';
        stepElement.style.transition = 'all 0.3s ease';
        
        const stepIcon = document.createElement('div');
        stepIcon.className = 'step-icon';
        stepIcon.style.width = '40px';
        stepIcon.style.height = '40px';
        stepIcon.style.borderRadius = '50%';
        stepIcon.style.background = statusCode >= step.code ? 
            'linear-gradient(to right bottom, #4285f4, #34a853)' : '#e9ecef';
        stepIcon.style.display = 'flex';
        stepIcon.style.alignItems = 'center';
        stepIcon.style.justifyContent = 'center';
        stepIcon.style.color = statusCode >= step.code ? '#fff' : '#adb5bd';
        stepIcon.style.boxShadow = statusCode >= step.code ? 
            '0 4px 10px rgba(66, 133, 244, 0.3)' : 'none';
        stepIcon.style.transition = 'all 0.5s ease';
        
        const icon = document.createElement('i');
        icon.className = `fas ${step.icon}`;
        stepIcon.appendChild(icon);
        
        const stepLabel = document.createElement('div');
        stepLabel.className = 'step-label';
        stepLabel.textContent = step.name;
        stepLabel.style.marginTop = '8px';
        stepLabel.style.fontSize = '12px';
        stepLabel.style.fontWeight = '500';
        stepLabel.style.color = statusCode >= step.code ? '#202124' : '#adb5bd';
        
        stepElement.appendChild(stepIcon);
        stepElement.appendChild(stepLabel);
        stepsContainer.appendChild(stepElement);
    });
    
    // Thêm các phần tử vào wrapper
    progressWrapper.appendChild(stepsContainer);
    progressWrapper.appendChild(progressContainer);
    
    // Thêm wrapper vào trang
    const infoCard = orderInfoSection.querySelector('.info-card');
    infoCard.appendChild(progressWrapper);
    
    // Hiệu ứng điền progress bar
    setTimeout(() => {
        progressBar.style.width = `${progressPercent}%`;
    }, 500);
}

/**
 * Cập nhật màu sắc và hiển thị trạng thái đơn hàng
 */
function updateOrderStatusDisplay() {
    // Tìm tất cả các phần tử trạng thái
    const statusElements = document.querySelectorAll('[class*="status-"]');
    
    statusElements.forEach(element => {
        // Xác định trạng thái từ class
        const statusClasses = Array.from(element.classList)
            .filter(cls => cls.startsWith('status-'));
            
        if (statusClasses.length > 0) {
            const statusClass = statusClasses[0];
            const statusCode = statusClass.replace('status-', '');
            
            // Thêm tooltip để hiển thị mô tả chi tiết
            if (element.classList.contains('status-dot') || 
                element.parentElement.classList.contains('order-status-badge')) {
                
                const tooltipText = getStatusDescription(parseInt(statusCode));
                element.setAttribute('title', tooltipText);
                element.setAttribute('data-toggle', 'tooltip');
            }
        }
    });
    
    // Khởi tạo tooltips nếu Bootstrap được sử dụng
    try {
        $('[data-toggle="tooltip"]').tooltip();
    } catch (e) {
        console.log('Bootstrap tooltip not available');
    }
}

/**
 * Lấy mô tả chi tiết cho trạng thái đơn hàng
 */
function getStatusDescription(statusCode) {
    switch (statusCode) {
        case 1: return 'Đơn hàng của bạn đang được xử lý';
        case 2: return 'Đơn hàng đã được xác nhận và đang chuẩn bị giao';
        case 3: return 'Đơn hàng đang được vận chuyển đến bạn';
        case 4: return 'Đơn hàng đã được giao đến địa chỉ của bạn';
        case 5: return 'Đơn hàng đã hoàn thành thành công';
        case 6: return 'Đơn hàng đã bị hủy';
        case 7: return 'Đơn hàng đã được hoàn trả';
        default: return 'Không xác định';
    }
}

/**
 * Thêm nút xuất đơn hàng ra PDF
 */
function addExportToPdfButton() {
    // Chỉ thêm nút vào trang chi tiết đơn hàng
    const orderDetailPage = document.querySelector('.order-detail-page');
    if (!orderDetailPage) return;
    
    // Tìm phần actions
    const actionsSection = document.querySelector('.actions-section');
    if (!actionsSection) return;
    
    // Tạo nút xuất PDF
    const exportBtn = document.createElement('button');
    exportBtn.type = 'button';
    exportBtn.className = 'btn-secondary';
    exportBtn.innerHTML = '<i class="fas fa-file-pdf"></i> <span>Xuất PDF</span>';
    exportBtn.addEventListener('click', handleExportToPdf);
    
    // Thêm nút vào đầu phần actions
    actionsSection.insertBefore(exportBtn, actionsSection.firstChild);
}

/**
 * Xử lý xuất đơn hàng ra PDF
 */
function handleExportToPdf() {
    // Hiển thị thông báo
    showNotification('Đang chuẩn bị tài liệu PDF...', 'info');
    
    // Trong thực tế, bạn sẽ gọi API hoặc backend để tạo PDF
    // Đây là mã giả để mô phỏng quá trình này
    setTimeout(() => {
        // Hiển thị thông báo thành công sau khi "xử lý"
        showNotification('Tệp PDF đã được tạo thành công!', 'success');
        
        // Trong thực tế, sau khi có file PDF, bạn sẽ tải xuống hoặc mở nó
        // window.open('/OrderHistory/ExportPdf/' + orderId, '_blank');
    }, 1500);
}

// Thêm hàm mới để hiển thị loading

/**
 * Thêm hiệu ứng loading trước khi tải dữ liệu
 */
function initLoadingEffect() {
    // Thêm một hàm để kiểm soát trạng thái loading
    window.showPageLoading = function() {
        // Kiểm tra nếu đã có loading overlay
        if (document.getElementById('page-loading-overlay')) return;
        
        // Tạo loading overlay
        const overlay = document.createElement('div');
        overlay.id = 'page-loading-overlay';
        overlay.style.position = 'fixed';
        overlay.style.top = '0';
        overlay.style.left = '0';
        overlay.style.width = '100%';
        overlay.style.height = '100%';
        overlay.style.backgroundColor = 'rgba(255, 255, 255, 0.8)';
        overlay.style.display = 'flex';
        overlay.style.alignItems = 'center';
        overlay.style.justifyContent = 'center';
        overlay.style.zIndex = '9999';
        overlay.style.opacity = '0';
        overlay.style.transition = 'opacity 0.3s ease';
        
        // Tạo spinner
        const spinner = document.createElement('div');
        spinner.className = 'loading-spinner';
        spinner.style.width = '50px';
        spinner.style.height = '50px';
        spinner.style.border = '5px solid #f3f3f3';
        spinner.style.borderTop = '5px solid var(--primary-color)';
        spinner.style.borderRadius = '50%';
        spinner.style.animation = 'spin 1s linear infinite';
        
        // Thêm style animation
        const style = document.createElement('style');
        style.textContent = `
            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
        `;
        document.head.appendChild(style);
        
        // Thêm spinner vào overlay
        overlay.appendChild(spinner);
        
        // Thêm overlay vào body
        document.body.appendChild(overlay);
        
        // Kích hoạt animation
        setTimeout(() => {
            overlay.style.opacity = '1';
        }, 10);
    };
    
    window.hidePageLoading = function() {
        const overlay = document.getElementById('page-loading-overlay');
        if (overlay) {
            overlay.style.opacity = '0';
            setTimeout(() => {
                if (overlay.parentNode) {
                    overlay.parentNode.removeChild(overlay);
                }
            }, 300);
        }
        
        // Xóa trạng thái loading khỏi sessionStorage
        sessionStorage.removeItem('pageIsLoading');
    };
    
    // Kiểm tra xem có đang hiển thị loading không khi trang được tải
    if (sessionStorage.getItem('pageIsLoading') === 'true') {
        // Ẩn loading nếu có
        window.hidePageLoading();
    }

    // Thêm loading khi submit form
    const filterForm = document.querySelector('.order-filter-form');
    if (filterForm) {
        filterForm.addEventListener('submit', function() {
            window.showPageLoading();
        });
    }
    
    // Thêm loading khi click vào các link phân trang
    const paginationLinks = document.querySelectorAll('.pagination a');
    if (paginationLinks.length) {
        paginationLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                // Chỉ hiển thị loading nếu link không bị disabled
                if (!this.classList.contains('disabled')) {
                    window.showPageLoading();
                }
            });
        });
    }
    
    // Thêm loading khi click vào các link chi tiết
    const detailLinks = document.querySelectorAll('.btn-view-detail');
    if (detailLinks.length) {
        detailLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                window.showPageLoading();
                // Lưu trạng thái loading vào sessionStorage
                sessionStorage.setItem('pageIsLoading', 'true');
            });
        });
    }
    
    // Thêm sự kiện pageshow để xử lý khi quay lại trang từ cache
    window.addEventListener('pageshow', function(event) {
        // Nếu trang được tải từ cache (người dùng nhấn Back/Forward)
        if (event.persisted) {
            window.hidePageLoading();
        }
    });
    
    // Thêm sự kiện popstate để xử lý khi nhấn nút Back
    window.addEventListener('popstate', function() {
        window.hidePageLoading();
    });
    
    // Ẩn loading khi trang đã tải xong
    window.addEventListener('load', function() {
        window.hidePageLoading();
    });
    
    // Đảm bảo loading bị ẩn khi người dùng rời khỏi trang
    window.addEventListener('beforeunload', function() {
        sessionStorage.removeItem('pageIsLoading');
    });
}

// Khởi tạo ngay sau khi tài liệu được tải
document.addEventListener('DOMContentLoaded', function() {
    // Ẩn loading nếu có
    if (typeof window.hidePageLoading === 'function') {
        window.hidePageLoading();
    }
});

// Thêm kiểm tra debugging cho timeline
document.addEventListener('DOMContentLoaded', function() {
    console.log('Timeline items: ' + document.querySelectorAll('.timeline-item').length);
    
    // Đảm bảo timeline items luôn hiển thị sau 2 giây
    setTimeout(function() {
        document.querySelectorAll('.timeline-item').forEach(function(item) {
            item.style.opacity = '1';
            item.style.transform = 'translateY(0)';
        });
    }, 2000);
});

/**
 * Khởi tạo hiệu ứng cho trạng thái trống
 */
function initEmptyStateEffects() {
    const emptyState = document.querySelector('.empty-state');
    
    if (emptyState) {
        console.log('Empty state found, initializing effects');
        
        // Thêm các khối trang trí
        const decorationBlock1 = document.createElement('div');
        decorationBlock1.className = 'decoration-block decoration-block-1';
        
        const decorationBlock2 = document.createElement('div');
        decorationBlock2.className = 'decoration-block decoration-block-2';
        
        const decorationBlock3 = document.createElement('div');
        decorationBlock3.className = 'decoration-block decoration-block-3';
        
        emptyState.appendChild(decorationBlock1);
        emptyState.appendChild(decorationBlock2);
        emptyState.appendChild(decorationBlock3);
        
        // Thêm hiệu ứng hover 3D
        emptyState.addEventListener('mousemove', function(e) {
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;
            
            const rotateX = (y - centerY) / 30;
            const rotateY = (centerX - x) / 30;
            
            this.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-5px)`;
        });
        
        emptyState.addEventListener('mouseleave', function() {
            this.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateY(0)';
        });
        
        // Thêm hiệu ứng cho icon
        const emptyIcon = emptyState.querySelector('.empty-icon i');
        if (emptyIcon) {
            // Thêm hiệu ứng rung nhẹ khi hover
            emptyIcon.parentElement.addEventListener('mouseenter', function() {
                emptyIcon.style.animation = 'tada 1s';
            });
            
            emptyIcon.parentElement.addEventListener('mouseleave', function() {
                emptyIcon.style.animation = '';
            });
        }
        
        // Hiệu ứng dành cho nút
        const shopNowBtn = emptyState.querySelector('.btn-primary, .btn-shop-now');
        if (shopNowBtn) {
            // Đánh dấu là button trạng thái trống
            shopNowBtn.classList.add('btn-shop-now');
            
            shopNowBtn.addEventListener('mouseenter', function() {
                // Thêm hiệu ứng highlight cho toàn bộ emptyState
                emptyState.style.boxShadow = '0 10px 25px rgba(66, 133, 244, 0.15)';
            });
            
            shopNowBtn.addEventListener('mouseleave', function() {
                // Trở về trạng thái bình thường
                emptyState.style.boxShadow = '';
            });
        }
        
        // Thêm animation
        document.head.insertAdjacentHTML('beforeend', `
            <style>
                @keyframes tada {
                    0% {transform: scale(1);}
                    10%, 20% {transform: scale(0.9) rotate(-3deg);}
                    30%, 50%, 70%, 90% {transform: scale(1.1) rotate(3deg);}
                    40%, 60%, 80% {transform: scale(1.1) rotate(-3deg);}
                    100% {transform: scale(1) rotate(0);}
                }
            </style>
        `);
    }
}

// Gọi hàm khởi tạo hiệu ứng empty state
document.addEventListener('DOMContentLoaded', function() {
    // Các hàm khởi tạo khác
    // ...
    
    // Khởi tạo hiệu ứng cho trạng thái trống
    initEmptyStateEffects();
});

/**
 * Khởi tạo chức năng đánh giá sản phẩm trực tiếp từ chi tiết đơn hàng
 */
function initProductReviews() {
    // Thêm modal đánh giá vào trang
    addReviewModal();
    
    // Tìm tất cả các nút đánh giá
    const reviewButtons = document.querySelectorAll('.btn-review');
    if (reviewButtons.length === 0) return;
}

/**
 * Thêm modal đánh giá vào DOM
 */
function addReviewModal() {
    // Kiểm tra xem có modal sẵn trong DOM không
    if (document.getElementById('reviewModal')) return;
    
    const modal = document.createElement('div');
    modal.id = 'reviewModal';
    modal.className = 'modal fade';
    modal.tabIndex = '-1';
    modal.setAttribute('role', 'dialog');
    modal.setAttribute('aria-hidden', 'true');
    
    // Lấy token từ trang
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    const tokenHtml = tokenInput ? 
        `<input type="hidden" name="__RequestVerificationToken" value="${tokenInput.value}">` : '';
    
    const modalHTML = `
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Đánh giá sản phẩm</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="product-to-review">
                        <div class="review-product-image">
                            <img src="" alt="Product" id="reviewProductImage">
                        </div>
                        <div class="review-product-info">
                            <h4 class="review-product-name" id="reviewProductName"></h4>
                        </div>
                    </div>
                    
                    <form id="reviewModalForm" action="/Review/Create" method="post">
                        ${tokenHtml}
                        <input type="hidden" name="ProductId" id="reviewProductId">
                        
                        <div class="rating-group">
                            <label>Đánh giá của bạn:</label>
                            <div class="star-rating">
                                <input type="radio" id="modalStar5" name="Rating" value="5" checked>
                                <label for="modalStar5"><i class="fas fa-star"></i></label>
                                <input type="radio" id="modalStar4" name="Rating" value="4">
                                <label for="modalStar4"><i class="fas fa-star"></i></label>
                                <input type="radio" id="modalStar3" name="Rating" value="3">
                                <label for="modalStar3"><i class="fas fa-star"></i></label>
                                <input type="radio" id="modalStar2" name="Rating" value="2">
                                <label for="modalStar2"><i class="fas fa-star"></i></label>
                                <input type="radio" id="modalStar1" name="Rating" value="1">
                                <label for="modalStar1"><i class="fas fa-star"></i></label>
                            </div>
                        </div>
                        
                        <div class="form-group">
                            <label for="modalComment">Nhận xét của bạn:</label>
                            <textarea class="form-control" id="modalComment" name="Comment" rows="4" placeholder="Chia sẻ trải nghiệm của bạn về sản phẩm này..."></textarea>
                        </div>
                    </form>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn-secondary" data-dismiss="modal">Huỷ bỏ</button>
                    <button type="button" class="btn-primary" id="submitReviewBtn">
                        <i class="fas fa-paper-plane"></i>
                        <span>Gửi đánh giá</span>
                    </button>
                </div>
            </div>
        </div>
    `;
    
    modal.innerHTML = modalHTML;
    document.body.appendChild(modal);
    
    // Xử lý nút submit
    const submitBtn = document.getElementById('submitReviewBtn');
    if (submitBtn) {
        submitBtn.addEventListener('click', function() {
            const form = document.getElementById('reviewModalForm');
            if (!form) return;
            
            // Validate
            const comment = document.getElementById('modalComment').value;
            if (!comment || comment.trim().length < 10) {
                // Mã validation không thay đổi...
                return;
            }
            
            // Thêm anti-forgery token vào form
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (tokenInput) {
                // Kiểm tra xem form đã có token chưa
                if (!form.querySelector('input[name="__RequestVerificationToken"]')) {
                    const clonedToken = tokenInput.cloneNode(true);
                    form.appendChild(clonedToken);
                }
            }
            
            // Submit form
            form.submit();
            
            // Disable button và hiển thị loading
            this.disabled = true;
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
        });
    }
}

/**
 * Mở modal đánh giá
 */
function openReviewModal(productId, productName, productImage) {
    // Đảm bảo modal đã được thêm vào DOM
    if (!document.getElementById('reviewModal')) {
        addReviewModal();
    }
    
    // Cập nhật thông tin sản phẩm
    document.getElementById('reviewProductId').value = productId;
    document.getElementById('reviewProductName').textContent = productName;
    document.getElementById('reviewProductImage').src = productImage;
    
    // Reset form
    const commentInput = document.getElementById('modalComment');
    if (commentInput) {
        commentInput.value = '';
        commentInput.style.borderColor = '';
    }
    
    // Xóa thông báo lỗi
    const errorMsg = document.querySelector('#reviewModalForm .text-danger');
    if (errorMsg) {
        errorMsg.remove();
    }
    
    // Mở modal
    try {
        $('#reviewModal').modal('show');
        
        // Khởi tạo CSS cho rating stars nếu cần
        initStarRatingCSS();
    } catch (e) {
        console.error('Error opening modal:', e);
    }
}

/**
 * Thêm CSS cho star rating nếu chưa có
 */
function initStarRatingCSS() {
    if (document.getElementById('star-rating-css')) return;
    
    const style = document.createElement('style');
    style.id = 'star-rating-css';
    style.innerHTML = `
        .star-rating {
            display: inline-flex;
            flex-direction: row-reverse;
            gap: 5px;
            font-size: 30px;
            position: relative;
        }
        
        .star-rating input {
            display: none;
        }
        
        .star-rating label {
            cursor: pointer;
            color: var(--gray-color, #dadce0);
            transition: color 0.2s ease, transform 0.2s ease;
        }
        
        .star-rating label i {
            transition: transform 0.2s ease;
        }
        
        .star-rating label:hover i,
        .star-rating label.hover i {
            transform: scale(1.2);
        }
        
        .star-rating label:hover,
        .star-rating label:hover ~ label,
        .star-rating label.hover,
        .star-rating label.hover ~ label,
        .star-rating label.selected,
        .star-rating label.selected ~ label {
            color: #FBBC05;
        }
        
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
            20%, 40%, 60%, 80% { transform: translateX(5px); }
        }
    `;
    document.head.appendChild(style);
    
    // Khởi tạo sao trong modal
    const starLabels = document.querySelectorAll('#reviewModal .star-rating label');
    starLabels.forEach(label => {
        label.addEventListener('click', function() {
            starLabels.forEach(l => l.classList.remove('selected'));
            this.classList.add('selected');
            let prev = this.previousElementSibling;
            while (prev && prev.tagName === 'LABEL') {
                prev.classList.add('selected');
                prev = prev.previousElementSibling;
            }
            
            // Find and check the radio input
            const radioId = this.getAttribute('for');
            document.getElementById(radioId).checked = true;
        });
        
        label.addEventListener('mouseenter', function() {
            this.classList.add('hover');
            let prev = this.previousElementSibling;
            while (prev && prev.tagName === 'LABEL') {
                prev.classList.add('hover');
                prev = prev.previousElementSibling;
            }
        });
        
        label.addEventListener('mouseleave', function() {
            starLabels.forEach(l => l.classList.remove('hover'));
        });
    });
    
    // Mặc định chọn 5 sao
    const fiveStarLabel = document.querySelector('label[for="modalStar5"]');
    if (fiveStarLabel) {
        fiveStarLabel.click();
    }
}

// Gọi hàm khởi tạo đánh giá sản phẩm khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    // Các hàm khởi tạo khác đã có
    
    // Khởi tạo chức năng đánh giá sản phẩm
    initProductReviews();
});