(function () {
    const sidebar = document.getElementById('app-sidebar');
    const toggle = document.getElementById('sidebar-toggle');
    const overlay = document.getElementById('sidebar-overlay');
    const search = document.getElementById('sidebar-search');

    if (toggle) {
        toggle.addEventListener('click', () => document.body.classList.toggle('sidebar-open'));
    }
    if (overlay) {
        overlay.addEventListener('click', () => document.body.classList.remove('sidebar-open'));
    }

    document.querySelectorAll('.nav-group-toggle').forEach(btn => {
        btn.addEventListener('click', () => {
            const group = btn.closest('.nav-group');
            if (group) group.classList.toggle('collapsed');
        });
    });

    if (search) {
        search.addEventListener('input', () => {
            const q = search.value.trim().toLowerCase();
            document.querySelectorAll('.app-sidebar-link[data-label]').forEach(link => {
                const label = (link.getAttribute('data-label') || '').toLowerCase();
                link.style.display = !q || label.includes(q) ? '' : 'none';
            });
        });
    }

    if (sidebar) {
        const active = sidebar.querySelector('.app-sidebar-link.active');
        if (active) {
            const group = active.closest('.nav-group');
            if (group) group.classList.remove('collapsed');
        }
    }
})();
