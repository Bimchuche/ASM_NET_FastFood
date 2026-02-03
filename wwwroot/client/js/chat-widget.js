/* ============================================
   CHAT WIDGET - SignalR Chat Client
   ============================================ */

(function() {
    'use strict';
    
    let connection = null;
    let sessionId = null;
    let isOpen = false;
    
    // Get user ID from session (will be set by the partial view)
    const userId = window.chatUserId || null;
    
    // Initialize SignalR connection
    async function initConnection() {
        if (connection) return;
        
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chathub")
            .withAutomaticReconnect()
            .build();
        
        // Handle incoming messages
        connection.on("ReceiveMessage", function(msg) {
            addMessage(msg.message, msg.isFromCustomer, msg.createdAt, msg.senderName);
            scrollToBottom();
        });
        
        // Handle session closed
        connection.on("SessionClosed", function() {
            sessionId = null; // Reset để user có thể tạo session mới
            addSessionClosedMessage();
        });
        
        try {
            await connection.start();
            console.log("Chat connected!");
        } catch (err) {
            console.error("Chat connection failed:", err);
        }
    }
    
    // Start or resume chat session
    async function startSession() {
        if (!userId) {
            addSystemMessage("Vui lòng đăng nhập để sử dụng chat hỗ trợ.");
            return;
        }
        
        await initConnection();
        
        if (connection.state === signalR.HubConnectionState.Connected) {
            try {
                const result = await connection.invoke("StartSession", userId);
                sessionId = result.sessionId;
                console.log("Session info:", result);
                
                // Load chat history if exists
                if (result.messages && result.messages.length > 0) {
                    loadChatHistory(result.messages);
                }
            } catch (err) {
                console.error("StartSession error:", err);
            }
        }
    }
    
    // Load chat history
    function loadChatHistory(messages) {
        const container = document.getElementById('chatMessages');
        // Clear welcome message
        container.innerHTML = '';
        
        messages.forEach(msg => {
            addMessage(msg.message, msg.isFromCustomer, msg.createdAt, msg.senderName);
        });
        
        scrollToBottom();
    }
    
    // Send message
    async function sendMessage() {
        const input = document.getElementById('chatInput');
        const message = input.value.trim();
        
        if (!message || !sessionId) return;
        
        input.value = '';
        
        if (connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke("SendMessage", sessionId, message);
        }
    }
    
    // Add message to chat
    function addMessage(text, isCustomer, time, senderName) {
        const container = document.getElementById('chatMessages');
        const msgDiv = document.createElement('div');
        msgDiv.className = `chat-message ${isCustomer ? 'customer' : 'admin'}`;
        
        msgDiv.innerHTML = `
            <div>${escapeHtml(text)}</div>
            <div class="chat-message-time">${time || new Date().toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'})}</div>
        `;
        
        container.appendChild(msgDiv);
    }
    
    // Add system message
    function addSystemMessage(text) {
        const container = document.getElementById('chatMessages');
        const msgDiv = document.createElement('div');
        msgDiv.className = 'chat-welcome';
        msgDiv.innerHTML = `<p>${text}</p>`;
        container.appendChild(msgDiv);
    }
    
    // Add session closed message with restart button
    function addSessionClosedMessage() {
        const container = document.getElementById('chatMessages');
        const msgDiv = document.createElement('div');
        msgDiv.className = 'chat-welcome chat-closed';
        msgDiv.innerHTML = `
            <p>🔒 Cuộc trò chuyện đã kết thúc.</p>
            <button onclick="window.startNewChat()" class="chat-restart-btn">💬 Bắt đầu cuộc trò chuyện mới</button>
        `;
        container.appendChild(msgDiv);
        scrollToBottom();
    }
    
    // Start new chat (exposed globally)
    window.startNewChat = async function() {
        // Clear old messages
        const container = document.getElementById('chatMessages');
        container.innerHTML = `
            <div class="chat-welcome">
                <h5>👋 Xin chào!</h5>
                <p>Chúng tôi sẵn sàng hỗ trợ bạn.<br>Hãy gửi tin nhắn để bắt đầu.</p>
            </div>
        `;
        // Start new session
        await startSession();
    };

    
    // Scroll to bottom
    function scrollToBottom() {
        const container = document.getElementById('chatMessages');
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }
    
    // Escape HTML
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
    
    // Toggle chat window
    function toggleChat() {
        const window = document.getElementById('chatWindow');
        const toggle = document.getElementById('chatToggle');
        
        isOpen = !isOpen;
        
        if (isOpen) {
            window.classList.add('active');
            toggle.innerHTML = '✕';
            
            if (!sessionId) {
                startSession();
            }
            
            setTimeout(() => {
                document.getElementById('chatInput')?.focus();
            }, 300);
        } else {
            window.classList.remove('active');
            toggle.innerHTML = '💬';
        }
    }
    
    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        // Create chat widget HTML
        createChatWidget();
        
        // Set up event listeners
        document.getElementById('chatToggle')?.addEventListener('click', toggleChat);
        document.getElementById('chatClose')?.addEventListener('click', toggleChat);
        document.getElementById('chatSend')?.addEventListener('click', sendMessage);
        document.getElementById('chatInput')?.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                sendMessage();
            }
        });
    });
    
    // Create chat widget DOM
    function createChatWidget() {
        if (document.getElementById('chatWidget')) return;
        
        const widget = document.createElement('div');
        widget.id = 'chatWidget';
        widget.innerHTML = `
            <button id="chatToggle" class="chat-toggle" aria-label="Open chat">💬</button>
            
            <div id="chatWindow" class="chat-window">
                <div class="chat-header">
                    <div class="chat-header-info">
                        <div class="chat-header-avatar">🎧</div>
                        <div class="chat-header-text">
                            <h4>Hỗ trợ FastFood</h4>
                            <span>Thường trả lời trong vài phút</span>
                        </div>
                    </div>
                    <button id="chatClose" class="chat-close">✕</button>
                </div>
                
                <div id="chatMessages" class="chat-messages">
                    <div class="chat-welcome">
                        <h5>👋 Xin chào!</h5>
                        <p>Chúng tôi sẵn sàng hỗ trợ bạn.<br>Hãy gửi tin nhắn để bắt đầu.</p>
                    </div>
                </div>
                
                <div class="chat-input-area">
                    <input type="text" id="chatInput" class="chat-input" placeholder="Nhập tin nhắn..." maxlength="500">
                    <button id="chatSend" class="chat-send">➤</button>
                </div>
            </div>
        `;
        
        document.body.appendChild(widget);
    }
    
    // Expose toggle function
    window.toggleChat = toggleChat;
})();
