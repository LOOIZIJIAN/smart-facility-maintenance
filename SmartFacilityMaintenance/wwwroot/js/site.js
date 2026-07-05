// Site-wide JavaScript for the Smart Facility Maintenance system.
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        // Auto-dismiss success / info alerts after 4 seconds.
        setTimeout(function () {
            document.querySelectorAll(".alert-success, .alert-info").forEach(function (el) {
                if (window.bootstrap && window.bootstrap.Alert) {
                    window.bootstrap.Alert.getOrCreateInstance(el).close();
                }
            });
        }, 4000);
    });
})();
