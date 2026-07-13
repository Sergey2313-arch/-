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
        var href = link.getAttribute('href') || '';
        if (href.charAt(0) === '#') return;

        var linkPath = normalizePath(href);
        if (!isActive(linkPath)) return;

        link.classList.add('active');
        link.setAttribute('aria-current', 'page');
    });

    document.querySelectorAll('.nav a[aria-current="page"]').forEach(function (link) {
        link.scrollIntoView({ block: 'nearest', inline: 'center' });
    });

    var sectionLinks = Array.prototype.slice.call(document.querySelectorAll('.profile-section-tabs a[href^="#"]'));
    function setSectionActive(hash) {
        if (!hash) return;

        sectionLinks.forEach(function (link) {
            var isCurrent = link.hash === hash;
            link.classList.toggle('active', isCurrent);

            if (isCurrent) {
                link.setAttribute('aria-current', 'page');
            } else {
                link.removeAttribute('aria-current');
            }
        });
    }

    if (sectionLinks.length) {
        setSectionActive(window.location.hash || sectionLinks[0].hash);

        sectionLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                setSectionActive(link.hash);
            });
        });
    }

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
        '.neon-card:not(.hero-card), .card, .section-block, .auth-card, .detail-layout, .review-row, .admin-row, .stat-card, .product-card'
    );

    if (!('IntersectionObserver' in window)) {
        animatedItems.forEach(function (item) {
            item.classList.add('in-view');
        });
        return;
    }

    if (sectionLinks.length) {
        var profileSections = sectionLinks
            .map(function (link) { return document.querySelector(link.hash); })
            .filter(Boolean);

        var sectionObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    setSectionActive('#' + entry.target.id);
                }
            });
        }, { rootMargin: '-35% 0px -50% 0px', threshold: 0 });

        profileSections.forEach(function (section) {
            sectionObserver.observe(section);
        });
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
