// Offline Mode Detection and Management
// Handles connectivity checking, UI updates, and CRUD operation disabling

let isOffline = false;
let checkInterval = null;
const CHECK_INTERVAL_MS = 30000; // 30 seconds

const apiBaseUrl = (() => {
    const base = document.body?.dataset?.apiBase ?? '';
    return base.endsWith('/') ? base.slice(0, -1) : base;
})();

/**
 * Initialize offline mode detection
 */
function initializeOfflineMode() {
    console.log('[Offline Mode] Initializing...');

    // Initial check
    checkConnectivity();

    // Set up periodic checks
    if (checkInterval) {
        clearInterval(checkInterval);
    }
    checkInterval = setInterval(checkConnectivity, CHECK_INTERVAL_MS);

    // Listen for online/offline events (browser native)
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
}

/**
 * Check API connectivity by pinging health endpoint
 */
async function checkConnectivity() {
    const healthUrl = apiBaseUrl
        ? `${apiBaseUrl}/api/health`
        : '/api/health';

    try {
        const response = await fetch(healthUrl, {
            method: 'HEAD',
            cache: 'no-cache',
            headers: { 'Cache-Control': 'no-cache' }
        });

        setOfflineMode(!response.ok);
    } catch (error) {
        console.warn('[Offline Mode] API unreachable:', error.message);
        setOfflineMode(true);
    }
}

/**
 * Update offline mode state and UI
 */
function setOfflineMode(offline) {
    // Only update if state changed
    if (isOffline === offline) {
        return;
    }

    isOffline = offline;
    console.log(`[Offline Mode] State changed: ${offline ? 'OFFLINE' : 'ONLINE'}`);

    // Update UI
    updateOfflineBanner(offline);

    if (offline) {
        disableCRUDOperations();
    } else {
        enableCRUDOperations();
    }
}

/**
 * Show or hide offline mode banner
 */
function updateOfflineBanner(show) {
    const banner = document.getElementById('offlineBanner');
    if (!banner) {
        console.warn('[Offline Mode] Banner element not found');
        return;
    }

    if (show) {
        banner.style.display = 'block';
        // Smooth fade in
        banner.style.opacity = '0';
        setTimeout(() => {
            banner.style.transition = 'opacity 0.3s';
            banner.style.opacity = '1';
        }, 10);
    } else {
        // Smooth fade out
        banner.style.transition = 'opacity 0.3s';
        banner.style.opacity = '0';
        setTimeout(() => {
            banner.style.display = 'none';
        }, 300);
    }
}

/**
 * Disable all CRUD operations (Create, Edit, Delete, Modify)
 */
function disableCRUDOperations() {
    console.log('[Offline Mode] Disabling CRUD operations...');

    // Disable buttons with data-crud-action attribute
    const crudButtons = document.querySelectorAll('[data-crud-action]');
    crudButtons.forEach(button => {
        button.disabled = true;
        button.dataset.originalTitle = button.title || '';
        button.title = 'Disabled in offline mode';
        button.classList.add('offline-disabled');
    });

    // Disable modal triggers for create/edit
    const modalTriggers = document.querySelectorAll('[data-bs-toggle="modal"]');
    modalTriggers.forEach(trigger => {
        const target = trigger.getAttribute('data-bs-target');
        if (target && (target.includes('create') || target.includes('edit') || target.includes('Create') || target.includes('Edit'))) {
            trigger.disabled = true;
            trigger.dataset.originalTitle = trigger.title || '';
            trigger.title = 'Disabled in offline mode';
            trigger.classList.add('offline-disabled');
        }
    });

    // Disable form submissions
    const forms = document.querySelectorAll('form[method="post"]');
    forms.forEach(form => {
        const submitButtons = form.querySelectorAll('button[type="submit"]');
        submitButtons.forEach(button => {
            button.disabled = true;
            button.dataset.originalTitle = button.title || '';
            button.title = 'Disabled in offline mode';
            button.classList.add('offline-disabled');
        });
    });

    // Add visual indicator to button groups
    const buttonGroups = document.querySelectorAll('.btn-group');
    buttonGroups.forEach(group => {
        group.style.opacity = '0.5';
        group.style.pointerEvents = 'none';
    });
}

/**
 * Re-enable all CRUD operations
 */
function enableCRUDOperations() {
    console.log('[Offline Mode] Enabling CRUD operations...');

    // Re-enable all disabled elements
    const disabledElements = document.querySelectorAll('.offline-disabled');
    disabledElements.forEach(element => {
        element.disabled = false;
        element.title = element.dataset.originalTitle || '';
        element.classList.remove('offline-disabled');
        delete element.dataset.originalTitle;
    });

    // Restore button groups
    const buttonGroups = document.querySelectorAll('.btn-group');
    buttonGroups.forEach(group => {
        group.style.opacity = '1';
        group.style.pointerEvents = 'auto';
    });
}

/**
 * Handle browser online event
 */
function handleOnline() {
    console.log('[Offline Mode] Browser detected online');
    // Verify with API check
    checkConnectivity();
}

/**
 * Handle browser offline event
 */
function handleOffline() {
    console.log('[Offline Mode] Browser detected offline');
    setOfflineMode(true);
}

/**
 * Cleanup on page unload
 */
function cleanupOfflineMode() {
    if (checkInterval) {
        clearInterval(checkInterval);
    }
    window.removeEventListener('online', handleOnline);
    window.removeEventListener('offline', handleOffline);
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeOfflineMode);
} else {
    initializeOfflineMode();
}

// Cleanup on page unload
window.addEventListener('beforeunload', cleanupOfflineMode);
