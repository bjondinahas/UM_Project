(function () {

    const sidebar = document.getElementById('app-sidebar');

    const toggle = document.getElementById('sidebar-toggle');

    const overlay = document.getElementById('sidebar-overlay');

    const search = document.getElementById('sidebar-search');

    const searchEmpty = document.getElementById('sidebar-search-empty');



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

            let visibleCount = 0;



            document.querySelectorAll('.nav-group').forEach(group => {

                let groupVisible = false;



                group.querySelectorAll('.app-sidebar-link[data-label]').forEach(link => {

                    const label = (link.getAttribute('data-label') || link.textContent || '').toLowerCase();

                    const show = !q || label.includes(q);

                    link.style.display = show ? '' : 'none';

                    if (show) {

                        groupVisible = true;

                        visibleCount++;

                    }

                });



                group.style.display = !q || groupVisible ? '' : 'none';

                if (q && groupVisible) {

                    group.classList.remove('collapsed');

                }

            });



            if (searchEmpty) {

                searchEmpty.hidden = !q || visibleCount > 0;

            }

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

