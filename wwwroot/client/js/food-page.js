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
            window.location.href = '/Account/Login';
            return;
        }
        if (data.success) {
            btn.innerHTML = data.isAdded ? '❤️' : '🤍';
            btn.classList.toggle('active', data.isAdded);
        }
    } catch (e) {
        console.error('Error:', e);
    }
}

async function addToCart(foodId) {
    try {
        const response = await fetch('/Cart/AddFood', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'foodId=' + foodId + '&quantity=1'
        });
        const data = await response.json();
        if (data.success) {
            alert('✅ Đã thêm vào giỏ hàng!');
        } else {
            if (data.message?.includes('đăng nhập')) {
                window.location.href = '/Account/Login';
            } else {
                alert(data.message || 'Có lỗi xảy ra!');
            }
        }
    } catch (e) {
        console.error('Error:', e);
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
