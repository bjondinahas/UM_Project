/* Stable Chart.js init — prevents infinite resize / shrinking */
window.UmCharts = {
    defaults() {
        if (typeof Chart === 'undefined') return;
        Chart.defaults.font.family = 'Inter, system-ui, sans-serif';
        Chart.defaults.responsive = true;
        Chart.defaults.maintainAspectRatio = true;
        Chart.defaults.animation.duration = 400;
    },
    create(canvasId, config) {
        const el = document.getElementById(canvasId);
        if (!el || typeof Chart === 'undefined') return null;
        this.defaults();
        const existing = Chart.getChart(el);
        if (existing) existing.destroy();
        const base = {
            responsive: true,
            maintainAspectRatio: true,
            aspectRatio: 1.6,
            plugins: { legend: { position: 'bottom' } }
        };
        config.options = Object.assign({}, base, config.options || {});
        return new Chart(el, config);
    }
};
