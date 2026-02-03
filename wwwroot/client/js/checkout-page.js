// Checkout page JS

// Store layer values
var originalTotal = 0;
var currentDiscount = 0;
var currentShippingFee = 0;

// Initialize on load
document.addEventListener('DOMContentLoaded', function() {
    // Get original total from hidden input
    var subtotalInput = document.getElementById('subtotal');
    if (subtotalInput) {
        originalTotal = parseFloat(subtotalInput.value) || 0;
    }
    
    // Initialize map if no saved address
    const newSection = document.getElementById('newAddressSection');
    if (newSection && newSection.classList.contains('active')) {
        setTimeout(() => {
            if (typeof initAddressMap === 'function') {
                initAddressMap('checkoutMap', 'addressInput', { lat: 10.8231, lng: 106.6297, zoom: 13 });
            }
            if (typeof initAddressAutocomplete === 'function') {
                initAddressAutocomplete('addressInput', 'addressSuggestions');
            }
            window.addressMapInitialized = true;
        }, 100);
    }
    
    // Check if saved address is selected by default
    var addressType = document.getElementById('addressType');
    var savedAddress = document.getElementById('savedAddress');
    
    if (addressType && addressType.value === 'saved' && savedAddress && savedAddress.value) {
        geocodeSavedAddress(savedAddress.value);
    } else {
        var savedLat = document.querySelector('input[name="latitude"]');
        var savedLng = document.querySelector('input[name="longitude"]');
        if (savedLat && savedLng && savedLat.value && savedLng.value) {
            var lat = parseFloat(savedLat.value);
            var lng = parseFloat(savedLng.value);
            if (!isNaN(lat) && !isNaN(lng) && typeof updateShippingFeeFromCoordinates === 'function') {
                updateShippingFeeFromCoordinates(lat, lng);
            }
        }
    }
});

function selectPayment(element, type) {
    document.querySelectorAll('.payment-method').forEach(el => el.classList.remove('active'));
    element.classList.add('active');
    element.querySelector('input[type="radio"]').checked = true;
}

function selectAddressType(type, element) {
    document.querySelectorAll('.address-type-option').forEach(el => el.classList.remove('active'));
    element.classList.add('active');
    
    document.getElementById('addressType').value = type;
    
    const savedDisplay = document.getElementById('savedAddressDisplay');
    const newSection = document.getElementById('newAddressSection');
    const addressInput = document.getElementById('addressInput');
    
    if (type === 'saved') {
        savedDisplay.style.display = 'block';
        newSection.classList.remove('active');
        addressInput.removeAttribute('required');
        addressInput.value = '';
        
        var savedAddr = document.getElementById('savedAddress');
        if (savedAddr && savedAddr.value) {
            geocodeSavedAddress(savedAddr.value);
        }
    } else {
        savedDisplay.style.display = 'none';
        newSection.classList.add('active');
        addressInput.setAttribute('required', 'required');
        
        if (!window.addressMapInitialized) {
            setTimeout(() => {
                if (typeof initAddressMap === 'function') {
                    initAddressMap('checkoutMap', 'addressInput', { lat: 10.9452, lng: 106.8274, zoom: 14 });
                }
                if (typeof initAddressAutocomplete === 'function') {
                    initAddressAutocomplete('addressInput', 'addressSuggestions');
                }
                window.addressMapInitialized = true;
            }, 100);
        }
    }
}

function onSavedAddressChange(select) {
    var option = select.options[select.selectedIndex];
    var address = option.getAttribute('data-address');
    var phone = option.getAttribute('data-phone');
    var lat = option.getAttribute('data-lat');
    var lng = option.getAttribute('data-lng');
    
    var displayText = document.getElementById('selectedAddressText');
    if (displayText) {
        displayText.textContent = '✅ ' + address;
    }
    
    var savedAddrInput = document.getElementById('savedAddress');
    if (savedAddrInput) {
        savedAddrInput.value = address;
    }
    
    if (phone) {
        var phoneInput = document.querySelector('input[name="phone"]');
        if (phoneInput && !phoneInput.value) {
            phoneInput.value = phone;
        }
    }
    
    if (lat && lng && lat !== 'null' && lng !== 'null') {
        var latVal = parseFloat(lat);
        var lngVal = parseFloat(lng);
        if (!isNaN(latVal) && !isNaN(lngVal) && typeof updateShippingFeeFromCoordinates === 'function') {
            updateShippingFeeFromCoordinates(latVal, lngVal);
            return;
        }
    }
    
    if (address) {
        geocodeSavedAddress(address);
    }
}

function geocodeSavedAddress(address) {
    if (!address) return;
    
    var url = 'https://nominatim.openstreetmap.org/search?format=json&q=' + encodeURIComponent(address) + '&limit=1&accept-language=vi&countrycodes=vn';
    
    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data && data.length > 0) {
                var lat = parseFloat(data[0].lat);
                var lng = parseFloat(data[0].lon);
                console.log('Saved address geocoded:', lat, lng);
                
                if (typeof updateShippingFeeFromCoordinates === 'function') {
                    updateShippingFeeFromCoordinates(lat, lng);
                }
            } else {
                console.log('Could not geocode saved address, using default');
                if (typeof calculateShippingFee === 'function') {
                    calculateShippingFee(5);
                }
            }
        })
        .catch(err => {
            console.log('Geocode error:', err);
            if (typeof calculateShippingFee === 'function') {
                calculateShippingFee(5);
            }
        });
}

function applyCoupon() {
    var code = document.getElementById('couponCode').value.trim().toUpperCase();
    var messageDiv = document.getElementById('couponMessage');
    var btn = document.getElementById('applyCouponBtn');
    
    if (!code) {
        messageDiv.innerHTML = '<span style="color: #ef4444;">⚠️ Vui lòng nhập mã giảm giá!</span>';
        return;
    }
    
    btn.disabled = true;
    btn.innerText = 'Đang kiểm tra...';
    
    fetch('/Order/ValidateCoupon?code=' + encodeURIComponent(code) + '&totalAmount=' + originalTotal)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                currentDiscount = data.discountAmount;
                document.getElementById('discountAmount').value = currentDiscount;
                document.getElementById('couponId').value = data.couponId;
                messageDiv.innerHTML = '<span style="color: #22c55e;">✅ ' + data.message + '</span>';
                updateOrderSummary();
            } else {
                messageDiv.innerHTML = '<span style="color: #ef4444;">❌ ' + data.message + '</span>';
                currentDiscount = 0;
                document.getElementById('discountAmount').value = 0;
                document.getElementById('couponId').value = '';
                updateOrderSummary();
            }
        })
        .catch(error => {
            messageDiv.innerHTML = '<span style="color: #ef4444;">⚠️ Có lỗi xảy ra!</span>';
        })
        .finally(() => {
            btn.disabled = false;
            btn.innerText = 'Áp dụng';
        });
}

function updateOrderSummary() {
    var discountDisplay = document.getElementById('discountDisplay');
    var totalDisplay = document.getElementById('totalDisplay');
    var shippingFeeVal = parseFloat(document.getElementById('shippingFee').value) || 0;
    var subtotal = parseFloat(document.getElementById('subtotal').value) || originalTotal;
    
    if (discountDisplay) {
        discountDisplay.innerText = currentDiscount > 0 ? '-' + currentDiscount.toLocaleString('vi-VN') + '₫' : '-0₫';
        discountDisplay.style.color = currentDiscount > 0 ? '#22c55e' : 'inherit';
    }
    if (totalDisplay) {
        var finalTotal = subtotal + shippingFeeVal - currentDiscount;
        totalDisplay.innerText = finalTotal.toLocaleString('vi-VN') + '₫';
    }
}

function calculateShippingFee(distance) {
    if (!distance || distance <= 0) {
        document.getElementById('shippingFeeDisplay').innerText = 'Chọn địa chỉ để tính';
        document.getElementById('shippingFeeDisplay').style.color = '#94a3b8';
        document.getElementById('shippingFee').value = 0;
        currentShippingFee = 0;
        updateOrderSummary();
        return;
    }
    
    fetch('/Order/GetShippingFee?distance=' + distance)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                currentShippingFee = data.fee;
                document.getElementById('shippingFee').value = data.fee;
                
                var feeDisplay = document.getElementById('shippingFeeDisplay');
                if (data.fee == 0) {
                    feeDisplay.innerText = 'Miễn phí';
                    feeDisplay.style.color = '#22c55e';
                } else {
                    feeDisplay.innerText = '+' + data.fee.toLocaleString('vi-VN') + '₫';
                    feeDisplay.style.color = '#f59e0b';
                }
                
                updateOrderSummary();
            }
        })
        .catch(error => {
            console.error('Error calculating shipping:', error);
        });
}

// Form submit handler
document.addEventListener('DOMContentLoaded', function() {
    const checkoutForm = document.getElementById('checkoutForm');
    if (checkoutForm) {
        checkoutForm.addEventListener('submit', function(e) {
            const addressType = document.getElementById('addressType');
            const phone = document.querySelector('input[name="phone"]');
            
            if (!phone.value.trim()) {
                e.preventDefault();
                if (typeof showToast === 'function') {
                    showToast('Vui lòng nhập số điện thoại!', 'error');
                } else {
                    alert('Vui lòng nhập số điện thoại!');
                }
                return;
            }

            if (addressType && addressType.value === 'saved') {
                const savedAddress = document.getElementById('savedAddress').value;
                const addressInput = document.getElementById('addressInput');
                
                if (addressInput) {
                    addressInput.removeAttribute('required');
                    addressInput.value = savedAddress;
                } else {
                    const hidden = document.createElement('input');
                    hidden.type = 'hidden';
                    hidden.name = 'address';
                    hidden.value = savedAddress;
                    this.appendChild(hidden);
                }
            } else {
                const addressInput = document.getElementById('addressInput');
                if (!addressInput.value.trim()) {
                    e.preventDefault();
                    if (typeof showToast === 'function') {
                        showToast('Vui lòng nhập địa chỉ giao hàng!', 'error');
                    } else {
                        alert('Vui lòng nhập địa chỉ giao hàng!');
                    }
                    return;
                }
            }
        });
    }
});
