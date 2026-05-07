document.addEventListener("DOMContentLoaded", () => {
    const select = document.getElementById("timezone-select");
    const addButton = document.getElementById("add-timezone-btn");
    const list = document.getElementById("timezone-list");
    if (!select || !addButton || !list) {
        return;
    }

    const serverTimeZone = document.getElementById("visitor-timezone")?.textContent?.trim();
    const visitorTimeElement = document.getElementById("visitor-local-time");
    const visitorTimeZoneElement = document.getElementById("visitor-timezone");
    const trackedZones = new Map();
    const browserTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    const visitorTimeZone = (!serverTimeZone || serverTimeZone === "UTC")
        ? browserTimeZone
        : serverTimeZone;

    function formatNow(timeZone) {
        return new Intl.DateTimeFormat("uk-UA", {
            timeZone,
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        }).format(new Date());
    }

    function refreshTimes() {
        trackedZones.forEach((value, timeZone) => {
            value.timeElement.textContent = formatNow(timeZone);
        });

        if (visitorTimeZone && visitorTimeElement) {
            visitorTimeElement.textContent = formatNow(visitorTimeZone);
        }
    }

    function addZone(timeZone, label) {
        if (!timeZone || trackedZones.has(timeZone)) {
            return;
        }

        const row = document.createElement("div");
        row.className = "d-flex justify-content-between align-items-center border rounded p-2 mb-2";

        const textWrap = document.createElement("div");
        textWrap.innerHTML = `<div class="fw-semibold">${label}</div><div class="text-muted small">${timeZone}</div>`;

        const timeElement = document.createElement("div");
        timeElement.className = "fw-bold";
        timeElement.textContent = formatNow(timeZone);

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-sm btn-outline-danger ms-2";
        removeButton.textContent = "x";
        removeButton.addEventListener("click", () => {
            trackedZones.delete(timeZone);
            row.remove();
        });

        const rightWrap = document.createElement("div");
        rightWrap.className = "d-flex align-items-center";
        rightWrap.appendChild(timeElement);
        rightWrap.appendChild(removeButton);

        row.appendChild(textWrap);
        row.appendChild(rightWrap);
        list.appendChild(row);

        trackedZones.set(timeZone, { timeElement });
    }

    addButton.addEventListener("click", () => {
        const option = select.selectedOptions[0];
        if (!option) {
            return;
        }

        addZone(option.value, option.text);
    });

    if (visitorTimeZoneElement) {
        visitorTimeZoneElement.textContent = visitorTimeZone;
    }

    setInterval(refreshTimes, 1000);
    refreshTimes();
});
