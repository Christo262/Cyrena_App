(function () {
    'use strict';

    // Update footer year
    var yearEl = document.getElementById('year');
    if (yearEl) {
        yearEl.textContent = new Date().getFullYear();
    }

    // Mobile navigation toggle
    var navToggle = document.querySelector('.nav-toggle');
    var mainNav = document.querySelector('.main-nav');

    if (navToggle && mainNav) {
        navToggle.addEventListener('click', function () {
            var expanded = navToggle.getAttribute('aria-expanded') === 'true';
            navToggle.setAttribute('aria-expanded', String(!expanded));
            mainNav.classList.toggle('is-open');
        });
    }

    // Close mobile nav when a link is clicked
    var navLinks = document.querySelectorAll('.nav-list a');
    navLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            if (navToggle && mainNav && mainNav.classList.contains('is-open')) {
                navToggle.setAttribute('aria-expanded', 'false');
                mainNav.classList.remove('is-open');
            }
        });
    });
})();
