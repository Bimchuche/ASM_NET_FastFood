// Wishlist page JS

async function removeFromWishlist(foodId) {
    try {
        const response = await fetch('/Wishlist/Remove', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'foodId=' + foodId
        });
        const data = await response.json();
        if (data.success) {
            document.querySelector(`[data-food-id="${foodId}"]`).remove();
            updateWishlistCount(data.count);
            if (data.count === 0) location.reload();
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
        }
        else {
            alert(data.message || 'Có lỗi xảy ra!');
        }
    } catch (e) {
        console.error('Error:', e);
    }
}   

function updateWishlistCount(count) {
    const badge = document.querySelector('.wishlist-count');
    if (badge) badge.textContent = count;
}
