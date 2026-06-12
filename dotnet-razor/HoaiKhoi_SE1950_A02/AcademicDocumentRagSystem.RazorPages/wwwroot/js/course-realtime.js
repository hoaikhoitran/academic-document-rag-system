// Real-time Course list for lecturers.
// Subscribes to the CourseHub at /hubs/courses and, whenever the admin creates,
// updates, or deletes a course, re-fetches just the course table fragment so the
// page never needs a manual reload.
(function () {
    "use strict";

    var listEl = document.getElementById("course-list");
    if (!listEl) {
        return;
    }

    if (typeof signalR === "undefined") {
        console.warn("SignalR client not loaded; live course updates are disabled.");
        return;
    }

    var refreshUrl = listEl.getAttribute("data-refresh-url");
    var indicator = document.getElementById("live-indicator");

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/courses")
        .withAutomaticReconnect()
        .build();

    function flash() {
        if (!indicator) {
            return;
        }
        indicator.classList.add("status-badge--active");
        setTimeout(function () {
            indicator.classList.remove("status-badge--active");
        }, 1500);
    }

    function refreshCourseList() {
        fetch(refreshUrl, { headers: { "X-Requested-With": "XMLHttpRequest" } })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Refresh failed with status " + response.status);
                }
                return response.text();
            })
            .then(function (html) {
                listEl.innerHTML = html;
                flash();
            })
            .catch(function (err) {
                console.error("Could not refresh course list:", err);
            });
    }

    // The hub broadcasts a coarse "CoursesChanged" event after every successful
    // create/update/delete; we simply re-pull the current list.
    connection.on("CoursesChanged", refreshCourseList);

    connection.start().catch(function (err) {
        console.error("Could not connect to CourseHub:", err);
    });
})();
