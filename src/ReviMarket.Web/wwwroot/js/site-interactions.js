document.addEventListener('DOMContentLoaded', function () {
    var currentPath = window.location.pathname.toLowerCase();

    function normalizePath(href) {
        try {
            return new URL(href, window.location.origin).pathname.toLowerCase();
        } catch {
            return '';
        }
    }

    function isActive(linkPath) {
        if (!linkPath || linkPath === '/') {
            return currentPath === '/';
        }

        return currentPath === linkPath || currentPath.startsWith(linkPath + '/');
    }

    document.querySelectorAll('.nav a, .profile-tabs a, .admin-menu a, .site-footer a').forEach(function (link) {
        var linkPath = normalizePath(link.getAttribute('href'));
        if (!isActive(linkPath)) return;

        link.classList.add('active');
        link.setAttribute('aria-current', 'page');
    });

    document.querySelectorAll('.category-block.active, .withdraw-method.active').forEach(function (tab) {
        tab.setAttribute('aria-current', 'page');
    });

    document.querySelectorAll('.btn, .category-block, .role-card, .withdraw-method').forEach(function (control) {
        control.addEventListener('pointerdown', function () {
            control.classList.add('is-pressing');
        });

        ['pointerup', 'pointercancel', 'mouseleave', 'blur'].forEach(function (eventName) {
            control.addEventListener(eventName, function () {
                control.classList.remove('is-pressing');
            });
        });
    });

    var animatedItems = document.querySelectorAll(
        '.hero > div, .neon-card, .card, .section-block, .auth-card, .detail-layout, .review-row, .admin-row, .stat-card, .product-card'
    );

    if (!('IntersectionObserver' in window)) {
        animatedItems.forEach(function (item) {
            item.classList.add('in-view');
        });
        return;
    }

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (!entry.isIntersecting) return;

            entry.target.classList.add('in-view');
            observer.unobserve(entry.target);
        });
    }, { threshold: .08, rootMargin: '0px 0px -24px 0px' });

    animatedItems.forEach(function (item, index) {
        item.classList.add('js-animate');
        item.style.setProperty('--reveal-delay', Math.min(index * 35, 280) + 'ms');
        observer.observe(item);
    });
});
