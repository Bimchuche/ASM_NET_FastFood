// Profile page JS

// Password confirmation validation
document.getElementById('passwordForm').addEventListener('submit', function(e) {
    var pwd = document.getElementById('newPassword').value;
    var confirm = document.getElementById('confirmPassword').value;
    
    if (pwd !== confirm) {
        e.preventDefault();
        alert('Mật khẩu xác nhận không khớp!');
        return false;
    }
});

function closeOTPModal() {
    document.getElementById('otpModal').style.display = 'none';
}

// Auto focus OTP input
document.addEventListener('DOMContentLoaded', function() {
    var otpInput = document.getElementById('otpInput');
    if (otpInput && document.getElementById('otpModal').style.display === 'flex') {
        otpInput.focus();
    }
});

// Fill address input from dropdown selection
function fillAddressFromDropdown(select) {
    var addressInput = document.getElementById('addressInput');
    if (select.value && addressInput) {
        addressInput.value = select.value;
    }
}
