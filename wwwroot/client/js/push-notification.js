/* ============================================
   PUSH NOTIFICATION - Browser Notifications
   ============================================ */

(function() {
    'use strict';
    
    const VAPID_PUBLIC_KEY = 'YOUR_VAPID_PUBLIC_KEY'; // Replace with actual key
    
    // Check if push is supported
    function isPushSupported() {
        return 'serviceWorker' in navigator && 'PushManager' in window;
    }
    
    // Request notification permission
    async function requestPermission() {
        if (!isPushSupported()) {
            console.log('Push notifications not supported');
            return false;
        }
        
        const permission = await Notification.requestPermission();
        return permission === 'granted';
    }
    
    // Register service worker
    async function registerServiceWorker() {
        if (!isPushSupported()) return null;
        
        try {
            const registration = await navigator.serviceWorker.register('/sw.js');
            console.log('Service Worker registered');
            return registration;
        } catch (error) {
            console.error('Service Worker registration failed:', error);
            return null;
        }
    }
    
    // Subscribe to push
    async function subscribeToPush() {
        const permission = await requestPermission();
        if (!permission) return null;
        
        const registration = await registerServiceWorker();
        if (!registration) return null;
        
        try {
            // Get existing subscription or create new
            let subscription = await registration.pushManager.getSubscription();
            
            if (!subscription) {
                subscription = await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: urlBase64ToUint8Array(VAPID_PUBLIC_KEY)
                });
            }
            
            // Send subscription to server
            await saveSubscription(subscription);
            
            return subscription;
        } catch (error) {
            console.error('Push subscription failed:', error);
            return null;
        }
    }
    
    // Save subscription to server
    async function saveSubscription(subscription) {
        const userId = window.chatUserId || 0;
        if (!userId) return;
        
        try {
            await fetch('/api/push/subscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    userId: userId,
                    subscription: subscription
                })
            });
            console.log('Push subscription saved');
        } catch (error) {
            console.error('Failed to save subscription:', error);
        }
    }
    
    // Unsubscribe
    async function unsubscribe() {
        const registration = await navigator.serviceWorker.ready;
        const subscription = await registration.pushManager.getSubscription();
        
        if (subscription) {
            await subscription.unsubscribe();
            
            await fetch('/api/push/unsubscribe', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ endpoint: subscription.endpoint })
            });
        }
    }
    
    // Helper function
    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }
    
    // Show local notification (for testing)
    function showNotification(title, body, url) {
        if (Notification.permission === 'granted') {
            const notification = new Notification(title, {
                body: body,
                icon: '/images/logo.png'
            });
            
            notification.onclick = function() {
                window.focus();
                if (url) window.location.href = url;
            };
        }
    }
    
    // Auto-register on page load
    document.addEventListener('DOMContentLoaded', async function() {
        if (isPushSupported() && Notification.permission === 'default') {
            // Don't auto-ask, wait for user interaction
            // Show a prompt button instead
        } else if (Notification.permission === 'granted') {
            await registerServiceWorker();
        }
    });
    
    // Expose functions
    window.pushNotification = {
        subscribe: subscribeToPush,
        unsubscribe: unsubscribe,
        isSupported: isPushSupported,
        showLocal: showNotification,
        requestPermission: requestPermission
    };
})();
