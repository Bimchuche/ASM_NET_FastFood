// Chat Session page JS - SignalR handling
// Variables sessionId, adminId, customerInitial are set from the CSHTML page

let connection = null;

async function initChat() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chathub")
        .withAutomaticReconnect()
        .build();
    
    connection.on("ReceiveMessage", function(msg) {
        addMessage(msg.message, msg.isFromCustomer, msg.createdAt, msg.senderName);
        scrollToBottom();
    });
    
    try {
        await connection.start();
        await connection.invoke("JoinSession", sessionId);
        await connection.invoke("JoinAdminGroup");
        console.log("Admin connected to chat");
    } catch (err) {
        console.error("Connection failed:", err);
    }
}

function addMessage(text, isCustomer, time, sender) {
    const area = document.getElementById('messagesArea');
    
    // Remove no messages placeholder if exists
    const noMsg = area.querySelector('.no-messages');
    if (noMsg) noMsg.remove();
    
    const wrapper = document.createElement('div');
    wrapper.className = 'message-wrapper ' + (isCustomer ? 'customer' : 'admin');
    
    const avatarHtml = isCustomer 
        ? `<div class="msg-avatar customer-avatar-small">${customerInitial}</div>`
        : `<div class="msg-avatar admin-avatar-small">🎧</div>`;
    
    wrapper.innerHTML = `
        ${isCustomer ? avatarHtml : ''}
        <div class="message-bubble">
            <div class="msg-text">${escapeHtml(text)}</div>
            <div class="msg-info">
                <span class="msg-sender">${sender}</span>
                <span class="msg-time">${time}</span>
            </div>
        </div>
        ${!isCustomer ? avatarHtml : ''}
    `;
    
    area.appendChild(wrapper);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function scrollToBottom() {
    const area = document.getElementById('messagesArea');
    area.scrollTop = area.scrollHeight;
}

async function sendMessage() {
    const input = document.getElementById('adminMessageInput');
    const message = input.value.trim();
    if (!message) return;
    
    console.log("Sending message:", { sessionId, message, adminId });
    input.value = '';
    
    try {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke("AdminSendMessage", sessionId, message, adminId);
            console.log("Message sent successfully");
        } else {
            console.error("Connection not ready:", connection?.state);
        }
    } catch (err) {
        console.error("Error sending message:", err);
        alert("Lỗi gửi tin nhắn: " + err.message);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    initChat();
    scrollToBottom();
    
    document.getElementById('sendAdminMessage')?.addEventListener('click', sendMessage);
    document.getElementById('adminMessageInput')?.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') sendMessage();
    });
});
