// Order History page JS - Animation

document.addEventListener('DOMContentLoaded', function() {
    // Animate table rows on scroll
    const rows = document.querySelectorAll('.order-table tbody tr');
    rows.forEach((row, index) => {
        row.style.animationDelay = (index * 0.05) + 's';
    });
});
