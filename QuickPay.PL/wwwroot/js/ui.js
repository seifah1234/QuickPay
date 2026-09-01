// QuickPay UI interactions: sidebar toggle (desktop collapse + mobile
// off-canvas) and the subtle card tilt effect. No framework, no
// dependencies beyond what's already loaded (bootstrap bundle).
(function () {
    "use strict";

    var COLLAPSE_KEY = "qp:sidebar-collapsed";

    function initSidebar() {
        var body = document.body;
        var toggleBtns = document.querySelectorAll("[data-qp-sidebar-toggle]");
        var backdrop = document.querySelector(".qp-sidebar-backdrop");

        if (!toggleBtns.length) return;

        // Restore desktop collapsed preference.
        if (window.innerWidth >= 992 && sessionStorage.getItem(COLLAPSE_KEY) === "1") {
            body.classList.add("qp-sidebar-collapsed");
        }

        toggleBtns.forEach(function (btn) {
            btn.addEventListener("click", function () {
                if (window.innerWidth < 992) {
                    // Mobile: off-canvas open/close.
                    var isOpen = body.classList.toggle("qp-sidebar-open");
                    if (backdrop) backdrop.classList.toggle("qp-show", isOpen);
                } else {
                    // Desktop: icon-only collapse, remembered per tab.
                    var collapsed = body.classList.toggle("qp-sidebar-collapsed");
                    sessionStorage.setItem(COLLAPSE_KEY, collapsed ? "1" : "0");
                }
            });
        });

        if (backdrop) {
            backdrop.addEventListener("click", function () {
                body.classList.remove("qp-sidebar-open");
                backdrop.classList.remove("qp-show");
            });
        }
    }

    // Very subtle 3D tilt on elements marked .qp-tilt - reads the
    // mouse position relative to the card and nudges two CSS
    // variables that animations.css uses for the rotation. Capped
    // to a couple of degrees so it stays elegant, not gimmicky.
    function initTilt() {
        var cards = document.querySelectorAll(".qp-tilt");
        if (!cards.length) return;

        var MAX_DEG = 3;

        cards.forEach(function (card) {
            card.addEventListener("mousemove", function (e) {
                var rect = card.getBoundingClientRect();
                var px = (e.clientX - rect.left) / rect.width - 0.5;
                var py = (e.clientY - rect.top) / rect.height - 0.5;
                card.style.setProperty("--tilt-x", (px * MAX_DEG * 2).toFixed(2) + "deg");
                card.style.setProperty("--tilt-y", (py * -MAX_DEG * 2).toFixed(2) + "deg");
            });

            card.addEventListener("mouseleave", function () {
                card.style.setProperty("--tilt-x", "0deg");
                card.style.setProperty("--tilt-y", "0deg");
            });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initSidebar();
        initTilt();
    });
})();
