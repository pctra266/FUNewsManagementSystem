// notification.js
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7196/hub/notifications") // Adjust port if Backend is different
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Store notifications (max 10)
let notifications = [];
const MAX_NOTIFICATIONS = 10;

// Update notification badge
function updateNotificationBadge() {
    const badge = document.getElementById('notificationCount');
    if (badge) {
        const count = notifications.length;
        if (count > 0) {
            badge.textContent = count;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }
}

// Update notification dropdown list
function updateNotificationList() {
    const notificationList = document.getElementById('notificationList');
    if (!notificationList) return;

    // Clear existing items (keep header and divider)
    while (notificationList.children.length > 2) {
        notificationList.removeChild(notificationList.lastChild);
    }

    if (notifications.length === 0) {
        const emptyItem = document.createElement('li');
        emptyItem.innerHTML = '<span class="dropdown-item-text text-muted small">No notifications</span>';
        notificationList.appendChild(emptyItem);
    } else {
        notifications.forEach(notification => {
            const li = document.createElement('li');
            const timeAgo = getTimeAgo(new Date(notification.timestamp));
            li.innerHTML = `
                <a class="dropdown-item small" href="#">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1">${notification.message}</div>
                    </div>
                    <small class="text-muted">${timeAgo}</small>
                </a>
            `;
            notificationList.appendChild(li);
        });
    }
}

// Get time ago string
function getTimeAgo(date) {
    const seconds = Math.floor((new Date() - date) / 1000);

    if (seconds < 60) return 'Just now';
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    const days = Math.floor(hours / 24);
    return `${days}d ago`;
}

// Load initial notifications from API
async function loadInitialNotifications() {
    try {
        const response = await fetch('https://localhost:7196/api/notifications');
        if (response.ok) {
            const data = await response.json();
            notifications = data.map(n => ({
                id: n.id,
                message: n.message,
                timestamp: n.timestamp
            }));
            updateNotificationBadge();
            updateNotificationList();
        }
    } catch (error) {
        console.error('Failed to load notifications:', error);
    }
}

connection.on("ReceiveMessage", (message) => {
    console.log("Notification received: " + message);

    // Add to notifications array
    notifications.unshift({
        id: Date.now().toString(),
        message: message,
        timestamp: new Date().toISOString()
    });

    // Keep only last 10
    if (notifications.length > MAX_NOTIFICATIONS) {
        notifications = notifications.slice(0, MAX_NOTIFICATIONS);
    }

    // Update UI
    updateNotificationBadge();
    updateNotificationList();

    // Create toast container if not exists (should be in Layout)
    const toastContainer = document.getElementById('toastContainer');

    if (toastContainer) {
        // Create new toast element
        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header">
                    <strong class="me-auto">Notification</strong>
                    <small>Just now</small>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        // Append to container (using wrapper to parse HTML)
        const wrapper = document.createElement('div');
        wrapper.innerHTML = toastHtml;
        const toastElement = wrapper.firstElementChild;
        toastContainer.appendChild(toastElement);

        // Show
        const bsToast = new bootstrap.Toast(toastElement);
        bsToast.show();

        // Cleanup after remove
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
});

connection.start()
    .then(() => {
        console.log('SignalR Connected');
        loadInitialNotifications();
    })
    .catch(err => console.error(err.toString()));
