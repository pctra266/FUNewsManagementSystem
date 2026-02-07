// Show/Hide Global Loading Indicator
function showLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) overlay.style.display = 'flex';
}

function hideLoading() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) overlay.style.display = 'none';
}

// Show loading on any form submission
document.addEventListener('submit', function (e) {
    const target = e.target;
    // Don't show if validation fails (optional check)
    if (target.tagName === 'FORM') {
        // If it's a delete confirm that was cancelled, don't show
        // This is tricky if using native confirm(), but native confirm blocks the thread.
        showLoading();
    }
});

// Show loading on page navigation
window.addEventListener('beforeunload', function () {
    showLoading();
});

// Ensure loading is hidden when page is fully loaded or restored from cache
window.addEventListener('load', hideLoading);
window.addEventListener('pageshow', function (event) {
    if (event.persisted) {
        hideLoading();
    }
});
