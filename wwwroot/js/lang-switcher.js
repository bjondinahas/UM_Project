(function () {
    function closeAll() {
        document.querySelectorAll('[data-lang-dropdown].open').forEach(function (el) {
            el.classList.remove('open');
            var btn = el.querySelector('[data-lang-toggle]');
            if (btn) btn.setAttribute('aria-expanded', 'false');
        });
    }

    document.querySelectorAll('[data-lang-toggle]').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var wrap = btn.closest('[data-lang-dropdown]');
            if (!wrap) return;
            var isOpen = wrap.classList.contains('open');
            closeAll();
            if (!isOpen) {
                wrap.classList.add('open');
                btn.setAttribute('aria-expanded', 'true');
            }
        });
    });

    document.addEventListener('click', function () {
        closeAll();
    });

    document.querySelectorAll('[data-lang-menu]').forEach(function (menu) {
        menu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    });
})();
