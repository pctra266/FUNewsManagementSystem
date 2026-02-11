let skipCurrentNavigationLoading = false;

// Show/Hide Global Loading Indicator
function showLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.style.display = 'flex';
    }
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
}

// Show loading on any form submission
document.addEventListener('submit', function (e) {
    const form = e.target.closest('form');
    if (!form) {
        return;
    }

    if (form.dataset.skipLoading === 'true') {
        skipCurrentNavigationLoading = true;
        return;
    }

    showLoading();
});

// Show loading on page navigation
window.addEventListener('beforeunload', function () {
    if (skipCurrentNavigationLoading) {
        return;
    }

    showLoading();
});

// Ensure loading is hidden when page is fully loaded or restored from cache
window.addEventListener('load', function () {
    skipCurrentNavigationLoading = false;
    hideLoading();
});

window.addEventListener('pageshow', function (event) {
    skipCurrentNavigationLoading = false;
    if (event.persisted) {
        hideLoading();
    }
});
