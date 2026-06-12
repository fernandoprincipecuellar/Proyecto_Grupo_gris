// Navbar scroll effect
document.addEventListener('DOMContentLoaded', function () {
    const nav = document.querySelector('.nav-main');
    if (!nav) return;

    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 50) {
            nav.classList.add('scrolled');
        } else {
            nav.classList.remove('scrolled');
        }
    }, { passive: true });
});
