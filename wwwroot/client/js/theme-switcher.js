/* ============================================
   THEME SWITCHER - Dark/Light Mode Toggle
   ============================================ */

(function() {
    'use strict';
    
    const THEME_KEY = 'fastfood-theme';
    
    // Get saved theme or default to dark
    function getSavedTheme() {
        return localStorage.getItem(THEME_KEY) || 'dark';
    }
    
    // Apply theme to document
    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        updateToggleIcon(theme);
        localStorage.setItem(THEME_KEY, theme);
    }
    
    // Update toggle button icon
    function updateToggleIcon(theme) {
        const toggle = document.getElementById('themeToggle');
        if (toggle) {
            toggle.innerHTML = theme === 'dark' ? '☀️' : '🌙';
            toggle.title = theme === 'dark' ? 'Chuyển sang chế độ sáng' : 'Chuyển sang chế độ tối';
        }
    }
    
    // Toggle between themes
    function toggleTheme() {
        const current = getSavedTheme();
        const newTheme = current === 'dark' ? 'light' : 'dark';
        applyTheme(newTheme);
    }
    
    // Create toggle button
    function createToggleButton() {
        if (document.getElementById('themeToggle')) return;
        
        const button = document.createElement('button');
        button.id = 'themeToggle';
        button.className = 'theme-toggle';
        button.setAttribute('aria-label', 'Toggle theme');
        button.onclick = toggleTheme;
        
        document.body.appendChild(button);
        updateToggleIcon(getSavedTheme());
    }
    
    // Initialize on page load
    function init() {
        // Apply saved theme immediately
        applyTheme(getSavedTheme());
        
        // Create toggle button when DOM ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', createToggleButton);
        } else {
            createToggleButton();
        }
    }
    
    // Run init
    init();
    
    // Expose to global scope if needed
    window.toggleTheme = toggleTheme;
    window.setTheme = applyTheme;
})();
