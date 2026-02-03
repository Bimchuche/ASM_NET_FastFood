/* =====================================================
   POPUP & CART FUNCTIONS - FASTFOOD
   ===================================================== */

// ===== HELPER: TOAST NOTIFICATION =====
function showToast(message, type = 'success') {
    // Create toast container if not exists
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 99999; display: flex; flex-direction: column; gap: 10px;';
        document.body.appendChild(container);
    }

    // Create toast
    const toast = document.createElement('div');
    toast.className = `toast-notification ${type}`;
    toast.style.cssText = `
        background: ${type === 'success' ? '#10b981' : '#ef4444'};
        color: white;
        padding: 12px 24px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        display: flex;
        align-items: center;
        gap: 10px;
        font-weight: 500;
        animation: slideInRight 0.3s ease-out;
        min-width: 250px;
    `;
    
    const icon = type === 'success' ? '✅' : '⚠️';
    toast.innerHTML = `<span>${icon}</span> <span>${message}</span>`;

    container.appendChild(toast);

    // Remove after 3s
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        toast.style.transition = 'all 0.3s ease-in';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Add animation keyframes
const style = document.createElement('style');
style.innerHTML = `
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
`;
document.head.appendChild(style);

// ===== FOOD MODAL =====
function openFoodModal(id) {
    fetch('/Food/DetailPopup/' + id)
        .then(res => res.text())
        .then(html => {
            document.getElementById('foodModalBody').innerHTML = html;
            const modal = document.getElementById('foodModal');
            modal.style.display = 'flex';
            // Trigger reflow
            void modal.offsetWidth;
            modal.classList.add('active'); // Use CSS class for animation
        })
        .catch(err => console.error('Error loading food popup:', err));
}

function closeFoodModal() {
    const modal = document.getElementById('foodModal');
    modal.classList.remove('active');
    setTimeout(() => {
        modal.style.display = 'none';
        document.getElementById('foodModalBody').innerHTML = '';
    }, 300); // Wait for transition
}

// ===== COMBO MODAL =====
function openComboModal(id) {
    fetch('/Combo/DetailPopup/' + id)
        .then(res => res.text())
        .then(html => {
            document.getElementById('comboModalBody').innerHTML = html;
            const modal = document.getElementById('comboModal');
            modal.style.display = 'flex';
            void modal.offsetWidth;
            modal.classList.add('active');
        })
        .catch(err => console.error('Error loading combo popup:', err));
}

function closeComboModal() {
    const modal = document.getElementById('comboModal');
    modal.classList.remove('active');
    setTimeout(() => {
        modal.style.display = 'none';
        document.getElementById('comboModalBody').innerHTML = '';
    }, 300);
}

// ===== ADD FOOD TO CART =====
function addToCart(foodId) {
    // Prevent event bubbling if called from onclick
    if (event) event.stopPropagation();

    fetch('/Cart/Add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + foodId
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            showToast('Đã thêm món vào giỏ hàng!', 'success');
            reloadMiniCart();
            openMiniCart();
            // Close modal if open
            if (document.getElementById('foodModal').style.display === 'flex') closeFoodModal();
        } else {
            // Check if redirect due to no auth
            if (data.redirect) window.location.href = data.redirect;
            else showToast('Vui lòng đăng nhập để đặt món!', 'error');
        }
    })
    .catch(() => showToast('Có lỗi xảy ra, thử lại sau!', 'error'));
}

function addFoodToCart(foodId) {
    addToCart(foodId);
}

// ===== ADD COMBO TO CART =====
function addComboToCart(comboId) {
    if (event) event.stopPropagation();

    fetch('/Cart/AddCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + comboId
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            showToast('Đã thêm combo vào giỏ hàng!', 'success');
            reloadMiniCart();
            openMiniCart();
            if (document.getElementById('comboModal').style.display === 'flex') closeComboModal();
        } else {
            showToast('Vui lòng đăng nhập để đặt món!', 'error');
        }
    })
    .catch(() => showToast('Có lỗi xảy ra, thử lại sau!', 'error'));
}

// ===== MINI CART =====
function reloadMiniCart() {
    fetch('/Common/MiniCart')
        .then(res => res.text())
        .then(html => {
            const el = document.getElementById('miniCartContent');
            if (el) el.innerHTML = html;
        });
}

function openMiniCart() {
    const el = document.getElementById('cartDropdown');
    if (el && typeof bootstrap !== 'undefined') {
        const dropdown = bootstrap.Dropdown.getOrCreateInstance(el);
        dropdown.show();
    }
}

// ===== CART PAGE FUNCTIONS =====
// Similar updates for other cart functions to use arrow functions and better error handling
function foodIncrease(id) {
    fetch('/Cart/Increase', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + id
    }).then(() => location.reload());
}

function foodDecrease(id) {
    fetch('/Cart/Decrease', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + id
    }).then(() => location.reload());
}

function foodRemove(id) {
    fetch('/Cart/Remove', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + id
    }).then(() => location.reload());
}

function comboIncrease(id) {
    fetch('/Cart/IncreaseCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + id
    }).then(() => location.reload());
}

function comboDecrease(id) {
    fetch('/Cart/DecreaseCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + id
    }).then(() => location.reload());
}

function comboRemove(id) {
    fetch('/Cart/RemoveCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + id
    }).then(() => location.reload());
}

// ===== MINI CART ACTION FUNCTIONS (No Reload) =====
function miniFoodInc(id) {
    fetch('/Cart/Increase', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + id
    }).then(() => reloadMiniCart());
}

function miniFoodDec(id) {
    fetch('/Cart/Decrease', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + id
    }).then(() => reloadMiniCart());
}

function miniFoodRemove(id) {
    fetch('/Cart/Remove', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'foodId=' + id
    }).then(() => reloadMiniCart());
}

function miniComboInc(id) {
    fetch('/Cart/IncreaseCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + id
    }).then(() => reloadMiniCart());
}

function miniComboDec(id) {
    fetch('/Cart/DecreaseCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + id
    }).then(() => reloadMiniCart());
}

function miniComboRemove(id) {
    fetch('/Cart/RemoveCombo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'comboId=' + id
    }).then(() => reloadMiniCart());
}

// ===== CLOSE MODAL ON OVERLAY CLICK =====
document.addEventListener('click', function(e) {
    if (e.target.id === 'foodModal') closeFoodModal();
    if (e.target.id === 'comboModal') closeComboModal();
});

// ===== CLOSE MODAL ON ESC KEY =====
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        const food = document.getElementById('foodModal');
        const combo = document.getElementById('comboModal');
        if (food && food.style.display === 'flex') closeFoodModal();
        if (combo && combo.style.display === 'flex') closeComboModal();
    }
});

// (Legacy duplicates removed)

// =====================================================
// SHIPPER - DELIVERY PROOF FUNCTIONS
// =====================================================

// Hiển thị preview ảnh xác nhận giao hàng
function showProofPreview(orderId) {
    var input = document.getElementById('proofImage_' + orderId);
    var preview = document.getElementById('proofPreview_' + orderId);
    var img = document.getElementById('proofImg_' + orderId);
    var selectBtn = document.getElementById('selectPhotoBtn_' + orderId);
    var submitBtn = document.getElementById('submitBtn_' + orderId);

    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function(e) {
            img.src = e.target.result;
            preview.style.display = 'block';
            selectBtn.textContent = '📷 Chọn ảnh khác';
            selectBtn.style.background = '#6b7280';
            submitBtn.style.display = 'block';
        }
        reader.readAsDataURL(input.files[0]);
    }
}

// Validate ảnh trước khi submit
function validateProofImage(orderId) {
    var input = document.getElementById('proofImage_' + orderId);
    if (!input.files || input.files.length === 0) {
        showToast('Vui lòng chụp/chọn ảnh xác nhận giao hàng trước khi hoàn thành!', 'error');
        return false;
    }
    return confirm('Bạn xác nhận đã giao hàng thành công?');
}
function showProofPreviewDetail() {
        var input = document.getElementById('proofImageDetail');
        var preview = document.getElementById('proofPreviewDetail');
        var img = document.getElementById('proofImgDetail');
        var selectBtn = document.getElementById('selectPhotoBtnDetail');
        var submitBtn = document.getElementById('submitBtnDetail');

        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function(e) {
                img.src = e.target.result;
                preview.style.display = 'block';
                selectBtn.textContent = '📷 Chọn ảnh khác';
                selectBtn.style.background = '#6b7280';
                submitBtn.style.display = 'block';
            }
            reader.readAsDataURL(input.files[0]);
        }
    }

    function validateProofImageDetail() {
        var input = document.getElementById('proofImageDetail');
        if (!input.files || input.files.length === 0) {
            showToast('Vui lòng chụp/chọn ảnh xác nhận giao hàng!', 'error');
            return false;
        }
        return confirm('Bạn xác nhận đã giao hàng thành công?');
    }