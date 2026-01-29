/**
 * FastFood Homepage Animations
 * Using anime.js library - https://animejs.com/
 */

// Wait for DOM to load
document.addEventListener('DOMContentLoaded', function() {
    
    // ===== HERO SECTION ANIMATION =====
    // Animate hero title
    if (document.querySelector('.hero-content h1')) {
        anime({
            targets: '.hero-content h1',
            opacity: [0, 1],
            translateY: [50, 0],
            duration: 1000,
            easing: 'easeOutExpo'
        });
    }
    
    // Animate hero subtitle
    if (document.querySelector('.hero-content p')) {
        anime({
            targets: '.hero-content p',
            opacity: [0, 1],
            translateY: [30, 0],
            duration: 1000,
            delay: 300,
            easing: 'easeOutExpo'
        });
    }
    
    // Animate hero buttons
    if (document.querySelector('.hero-actions')) {
        anime({
            targets: '.hero-actions a',
            opacity: [0, 1],
            translateY: [30, 0],
            duration: 800,
            delay: anime.stagger(150, {start: 500}),
            easing: 'easeOutExpo'
        });
    }
    
    // ===== FOOD CARDS ANIMATION =====
    // Animate cards on scroll using Intersection Observer
    const cards = document.querySelectorAll('.glass-card, .food-card');
    
    if (cards.length > 0) {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };
        
        const cardObserver = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    anime({
                        targets: entry.target,
                        opacity: [0, 1],
                        translateY: [60, 0],
                        scale: [0.9, 1],
                        duration: 800,
                        delay: index * 100,
                        easing: 'easeOutExpo'
                    });
                    cardObserver.unobserve(entry.target);
                }
            });
        }, observerOptions);
        
        cards.forEach(card => {
            card.style.opacity = '0';
            cardObserver.observe(card);
        });
    }
    
    // ===== SECTION TITLES ANIMATION =====
    const sectionTitles = document.querySelectorAll('.section-title, .food-title');
    
    if (sectionTitles.length > 0) {
        const titleObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    anime({
                        targets: entry.target,
                        opacity: [0, 1],
                        translateX: [-50, 0],
                        duration: 800,
                        easing: 'easeOutExpo'
                    });
                    titleObserver.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });
        
        sectionTitles.forEach(title => {
            title.style.opacity = '0';
            titleObserver.observe(title);
        });
    }
    
    // ===== CTA SECTION ANIMATION =====
    const ctaSection = document.querySelector('.cta-modern');
    if (ctaSection) {
        const ctaObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    anime({
                        targets: '.cta-modern h2, .cta-modern p, .cta-modern a',
                        opacity: [0, 1],
                        translateY: [40, 0],
                        duration: 800,
                        delay: anime.stagger(150),
                        easing: 'easeOutExpo'
                    });
                    ctaObserver.unobserve(entry.target);
                }
            });
        }, { threshold: 0.3 });
        
        ctaObserver.observe(ctaSection);
    }
    
    // ===== NAVBAR ANIMATION =====
    const navbar = document.querySelector('.navbar-glass');
    if (navbar) {
        anime({
            targets: '.navbar-glass',
            translateY: [-100, 0],
            opacity: [0, 1],
            duration: 800,
            easing: 'easeOutExpo'
        });
        
        anime({
            targets: '.navbar-glass .nav-link, .navbar-glass .btn',
            opacity: [0, 1],
            translateY: [-20, 0],
            duration: 600,
            delay: anime.stagger(50, {start: 400}),
            easing: 'easeOutExpo'
        });
    }
    
    // ===== HOVER EFFECTS FOR CARDS =====
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            anime({
                targets: this,
                scale: 1.03,
                boxShadow: '0 20px 40px rgba(0,0,0,0.15)',
                duration: 300,
                easing: 'easeOutQuad'
            });
        });
        
        card.addEventListener('mouseleave', function() {
            anime({
                targets: this,
                scale: 1,
                boxShadow: '0 4px 16px rgba(0,0,0,0.15)',
                duration: 300,
                easing: 'easeOutQuad'
            });
        });
    });
    
    // ===== BUTTON RIPPLE EFFECT =====
    const buttons = document.querySelectorAll('.btn-primary, .add-btn, .btn-auth');
    buttons.forEach(button => {
        button.addEventListener('click', function(e) {
            anime({
                targets: this,
                scale: [1, 0.95, 1],
                duration: 300,
                easing: 'easeInOutQuad'
            });
        });
    });
    
    // ===== FLOATING ANIMATION FOR ICONS =====
    const floatingElements = document.querySelectorAll('.floating-icon');
    floatingElements.forEach((el, index) => {
        anime({
            targets: el,
            translateY: [0, -15, 0],
            rotate: [0, 5, 0],
            duration: 3000 + (index * 500),
            loop: true,
            easing: 'easeInOutSine'
        });
    });
    
    // ===== PAGE LOAD COMPLETE ANIMATION =====
    anime({
        targets: 'body',
        opacity: [0.8, 1],
        duration: 500,
        easing: 'easeOutQuad'
    });
    
});

// ===== SCROLL ANIMATIONS =====
let lastScrollTop = 0;
window.addEventListener('scroll', function() {
    const navbar = document.querySelector('.navbar-glass');
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    
    // Add/remove scrolled class for navbar
    if (navbar) {
        if (scrollTop > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
    
    lastScrollTop = scrollTop;
});

// ===== STAGGER TEXT ANIMATION HELPER =====
function animateText(selector) {
    const element = document.querySelector(selector);
    if (element) {
        const text = element.textContent;
        element.innerHTML = '';
        
        text.split('').forEach((char, i) => {
            const span = document.createElement('span');
            span.textContent = char === ' ' ? '\u00A0' : char;
            span.style.opacity = '0';
            span.style.display = 'inline-block';
            element.appendChild(span);
        });
        
        anime({
            targets: element.querySelectorAll('span'),
            opacity: [0, 1],
            translateY: [20, 0],
            duration: 50,
            delay: anime.stagger(30),
            easing: 'easeOutQuad'
        });
    }
}
