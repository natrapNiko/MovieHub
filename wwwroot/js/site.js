// MovieHub client-side behaviour.
// Kept intentionally small: server-side rendering (Razor) does the heavy
// lifting, this file only layers on small progressive-enhancement touches.

(function () {
    "use strict";

    // Auto-dismiss success/info alerts after a few seconds.
    document.addEventListener("DOMContentLoaded", function () {
        var alerts = document.querySelectorAll(".alert-dismissible");
        alerts.forEach(function (alertEl) {
            setTimeout(function () {
                var bsAlert = bootstrap && bootstrap.Alert ? bootstrap.Alert.getOrCreateInstance(alertEl) : null;
                if (bsAlert) {
                    bsAlert.close();
                }
            }, 6000);
        });

        // Confirm before destructive delete-form submissions that don't
        // already have their own inline confirm handler.
        var deleteForms = document.querySelectorAll("form[data-confirm]");
        deleteForms.forEach(function (form) {
            form.addEventListener("submit", function (event) {
                var message = form.getAttribute("data-confirm") || "Are you sure?";
                if (!window.confirm(message)) {
                    event.preventDefault();
                }
            });
        });
    });
})();
