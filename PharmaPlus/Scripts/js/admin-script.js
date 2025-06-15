document.addEventListener('DOMContentLoaded', function() {
    console.log('💼 Admin script loaded');
    
    // Sidebar Toggle Function
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.querySelector('.admin-sidebar');
    const adminMain = document.querySelector('.admin-main');
    const adminWrapper = document.querySelector('.admin-wrapper');
    
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
            adminWrapper.classList.toggle('sidebar-collapsed');
            
            // Check if we're in mobile view
            if (window.innerWidth <= 576) {
                if (sidebar.classList.contains('show')) {
                    // Add overlay when sidebar is shown on mobile
                    const overlay = document.createElement('div');
                    overlay.className = 'sidebar-overlay';
                    overlay.style.position = 'fixed';
                    overlay.style.top = '0';
                    overlay.style.left = '0';
                    overlay.style.width = '100%';
                    overlay.style.height = '100%';
                    overlay.style.background = 'rgba(0, 0, 0, 0.5)';
                    overlay.style.zIndex = '99';
                    document.body.appendChild(overlay);
                    
                    // Close sidebar when overlay is clicked
                    overlay.addEventListener('click', function() {
                        sidebar.classList.remove('show');
                        adminWrapper.classList.remove('sidebar-collapsed');
                        overlay.remove();
                    });
                } else {
                    // Remove overlay when sidebar is hidden
                    const overlay = document.querySelector('.sidebar-overlay');
                    if (overlay) overlay.remove();
                }
            }
            
            // Lưu trạng thái vào localStorage
            const isCollapsed = adminWrapper.classList.contains('sidebar-collapsed');
            localStorage.setItem('sidebar-collapsed', isCollapsed);
        });
        
        // Kiểm tra và khôi phục trạng thái từ localStorage khi tải trang
        const sidebarState = localStorage.getItem('sidebar-collapsed');
        if (sidebarState === 'true') {
            adminWrapper.classList.add('sidebar-collapsed');
        }
    }
    
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transition = 'opacity 0.5s ease';
            setTimeout(() => {
                alert.style.display = 'none';
            }, 500);
        }, 5000);
    });
    
    // Dropdown toggle functionality
    initDropdowns();
    
    // Add any data table initialization here if needed
    // Example:
    if (typeof $.fn.DataTable !== 'undefined') {
        $('.data-table').DataTable({
            responsive: true,
            language: {
                search: "Tìm kiếm:",
                lengthMenu: "Hiển thị _MENU_ mục",
                info: "Hiển thị _START_ đến _END_ trong _TOTAL_ mục",
                infoEmpty: "Hiển thị 0 đến 0 trong 0 mục",
                infoFiltered: "(lọc từ _MAX_ mục)",
                paginate: {
                    first: "Đầu",
                    previous: "Trước",
                    next: "Tiếp",
                    last: "Cuối"
                }
            }
        });
    }
    
    // Chart setup if Charts.js is available
    if (typeof Chart !== 'undefined') {
        setupDashboardCharts();
    }
});

function setupDashboardCharts() {
    // Implement chart setup if needed
    console.log('📊 Setting up dashboard charts');
}

// Khởi tạo các dropdowns
function initDropdowns() {
    const dropdownToggles = document.querySelectorAll('.dropdown-toggle');
    
    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const parent = this.parentElement;
            // Toggle class show thay vì chỉ thêm
            parent.classList.toggle('show');
            this.classList.toggle('active');
            
            // Đóng các dropdown khác
            dropdownToggles.forEach(otherToggle => {
                if (otherToggle !== toggle) {
                    otherToggle.parentElement.classList.remove('show');
                    otherToggle.classList.remove('active');
                }
            });
        });
    });
    
    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.user-dropdown')) {
            dropdownToggles.forEach(toggle => {
                toggle.parentElement.classList.remove('show');
                toggle.classList.remove('active');
            });
        }
    });
}