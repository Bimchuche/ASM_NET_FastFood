/* =====================================================
   PAGE LOADER SCRIPT
   ===================================================== */

// Hide loader when page is fully loaded
window.addEventListener('load', function() {
    const loader = document.getElementById('page-loader');
    if (loader) {
        // Delay a bit for smooth transition
        setTimeout(function() {
            loader.classList.add('hidden');
        }, 500);
        
        // Remove from DOM after animation
        setTimeout(function() {
            loader.style.display = 'none';
        }, 1000);
    }
});

// Fallback: Hide after max 5 seconds
setTimeout(function() {
    const loader = document.getElementById('page-loader');
    if (loader && !loader.classList.contains('hidden')) {
        loader.classList.add('hidden');
        setTimeout(function() {
            loader.style.display = 'none';
        }, 500);
    }
}, 5000);
