// QuickPay animation helpers.
// 1) Reveal-on-load/scroll for anything marked .qp-reveal
//    (or .qp-stagger, whose children cascade via CSS nth-child delays).
// 2) A small animateCount() utility pages can call for balances
//    and stats - e.g. animateCount(el, 0, 12500.50, 800).
(function () {
    "use strict";

    function initReveal() {
        var targets = document.querySelectorAll(".qp-reveal, .qp-stagger");
        if (!targets.length) return;

        if (!("IntersectionObserver" in window)) {
            targets.forEach(function (el) { el.classList.add("qp-reveal-in"); });
            return;
        }

        var observer = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("qp-reveal-in");
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        targets.forEach(function (el) { observer.observe(el); });
    }

    function animateCount(el, from, to, durationMs, formatter) {
        if (!el) return;
        durationMs = durationMs || 700;
        formatter = formatter || function (n) {
            return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        };

        if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            el.textContent = formatter(to);
            return;
        }

        var start = null;

        function step(timestamp) {
            if (start === null) start = timestamp;
            var progress = Math.min((timestamp - start) / durationMs, 1);
            // ease-out cubic
            var eased = 1 - Math.pow(1 - progress, 3);
            var current = from + (to - from) * eased;
            el.textContent = formatter(current);
            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else {
                el.textContent = formatter(to);
            }
        }

        window.requestAnimationFrame(step);
    }

    // Auto-run for any element with data-qp-count-to="1234.56".
    function initAutoCounters() {
        var counters = document.querySelectorAll("[data-qp-count-to]");
        counters.forEach(function (el) {
            var to = parseFloat(el.getAttribute("data-qp-count-to"));
            if (isNaN(to)) return;
            animateCount(el, 0, to, 800);
        });
    }

    window.QuickPayAnimations = { animateCount: animateCount };

    document.addEventListener("DOMContentLoaded", function () {
        initReveal();
        initAutoCounters();
    });
})();
