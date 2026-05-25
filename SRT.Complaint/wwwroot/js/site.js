// Auto-dismiss flash alerts after 6 seconds
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('[role="alert"]').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'opacity 0.4s';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 400);
        }, 6000);
    });
});

// HTMX: re-run alert dismiss on every htmx:afterSwap
document.addEventListener('htmx:afterSwap', (e) => {
    e.detail.elt.querySelectorAll('[role="alert"]').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'opacity 0.4s';
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 400);
        }, 6000);
    });
});
