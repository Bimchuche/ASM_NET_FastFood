// Food page JS - toggleWishlist, addToCart, openFoodModal, closeFoodModal

async function toggleWishlist(foodId, btn) {
    try {
        const response = await fetch('/Wishlist/Toggle', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'foodId=' + foodId
        });
        const data = await response.json();
        if (data.requireLogin) {
            showToast('Vui lòng đăng nhập để sử dụng tính năng yêu thích!', 'error');
            return;
        }
        if (data.success) {
            btn.innerHTML = data.isAdded ? '❤️' : '🤍';
            btn.classList.toggle('active', data.isAdded);
            showToast(data.message, 'success');
        }
    } catch (e) {
        console.error('Error:', e);
        showToast('Có lỗi xảy ra, thử lại sau!', 'error');
    }
}

async function addToCart(foodId) {
    try {
        const response = await fetch('/Cart/Add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'foodId=' + foodId
        });
        const data = await response.json();
        if (data.success) {
            showToast('Đã thêm vào giỏ hàng!', 'success');
            if (typeof reloadMiniCart === 'function') reloadMiniCart();
            if (typeof openMiniCart === 'function') openMiniCart();
        } else {
            showToast('Vui lòng đăng nhập để thêm vào giỏ hàng!', 'error');
        }
    } catch (e) {
        console.error('Error:', e);
        showToast('Có lỗi xảy ra, thử lại sau!', 'error');
    }
}

function openFoodModal(id) {
    fetch('/Food/DetailPopup/' + id)
        .then(res => res.text())
        .then(html => {
            document.getElementById('foodModalBody').innerHTML = html;
            document.getElementById('foodModal').style.display = 'flex';
        });
}

function closeFoodModal() {
    document.getElementById('foodModal').style.display = 'none';
}
