// Service Worker for Push Notifications
self.addEventListener('push', function(event) {
    if (!event.data) return;
    
    const data = event.data.json();
    
    const options = {
        body: data.body || 'Bạn có thông báo mới',
        icon: '/images/logo.png',
        badge: '/images/badge.png',
        vibrate: [100, 50, 100],
        data: {
            dateOfArrival: Date.now(),
            url: data.url || '/'
        },
        actions: [
            { action: 'open', title: 'Xem ngay' },
            { action: 'close', title: 'Đóng' }
        ]
    };

    event.waitUntil(
        self.registration.showNotification(data.title || 'FastFood', options)
    );
});

self.addEventListener('notificationclick', function(event) {
    event.notification.close();

    if (event.action === 'close') return;

    const url = event.notification.data?.url || '/';
    
    event.waitUntil(
        clients.matchAll({ type: 'window' }).then(function(clientList) {
            // Check if already open
            for (const client of clientList) {
                if (client.url === url && 'focus' in client) {
                    return client.focus();
                }
            }
            // Open new window
            if (clients.openWindow) {
                return clients.openWindow(url);
            }
        })
    );
});

// Install and activate
self.addEventListener('install', function(event) {
    self.skipWaiting();
});

self.addEventListener('activate', function(event) {
    event.waitUntil(clients.claim());
});
