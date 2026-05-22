(function () {
    var header = document.getElementById('home-header');
    var toggle = document.getElementById('home-nav-toggle');
    var panel = document.getElementById('home-nav-panel');

    function setScrolled() {
        if (!header) return;
        header.classList.toggle('is-scrolled', window.scrollY > 8);
    }

    window.addEventListener('scroll', setScrolled, { passive: true });
    setScrolled();

    function closePanel() {
        if (!panel || !toggle) return;
        panel.classList.remove('is-open');
        panel.setAttribute('hidden', '');
        toggle.setAttribute('aria-expanded', 'false');
        document.body.classList.remove('home-nav-open');
    }

    function openPanel() {
        if (!panel || !toggle) return;
        panel.classList.add('is-open');
        panel.removeAttribute('hidden');
        toggle.setAttribute('aria-expanded', 'true');
        document.body.classList.add('home-nav-open');
    }

    if (toggle && panel) {
        toggle.addEventListener('click', function () {
            if (panel.classList.contains('is-open')) {
                closePanel();
            } else {
                openPanel();
            }
        });

        panel.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', closePanel);
        });

        window.addEventListener('resize', function () {
            if (window.innerWidth >= 1280) {
                closePanel();
            }
        });
    }
})();
