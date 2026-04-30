document.addEventListener("DOMContentLoaded", () => {
    const list = document.getElementById("notification-list");
    const badge = document.getElementById("notification-badge");
    const antiForgeryToken = document.querySelector('#notifications-antiforgery-form input[name="__RequestVerificationToken"]')?.value || "";

    if (!list || !badge) {
        return;
    }

    async function loadNotifications() {
        const response = await fetch("/Notifications/Unread?take=10", {
            method: "GET",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            list.innerHTML = '<div class="px-3 py-2 text-danger small">Не вдалося завантажити сповіщення.</div>';
            return;
        }

        const payload = await response.json();
        const items = Array.isArray(payload?.items) ? payload.items : [];
        const unreadCount = Number(payload?.unreadCount || 0);

        badge.textContent = unreadCount.toString();
        badge.classList.toggle("d-none", unreadCount <= 0);

        if (items.length === 0) {
            list.innerHTML = '<div class="px-3 py-2 text-muted small">Непрочитаних сповіщень немає.</div>';
            return;
        }

        list.innerHTML = "";
        items.forEach((item) => {
            const element = document.createElement("div");
            element.className = "notification-item";
            const createdAt = new Date(item.createdAt).toLocaleString("uk-UA");
            element.innerHTML = `
                <div class="fw-semibold">${escapeHtml(item.title || "")}</div>
                <div class="small text-muted mb-1">${escapeHtml(item.message || "")}</div>
                <div class="d-flex justify-content-between align-items-center">
                    <small class="text-secondary">${createdAt}</small>
                    <button type="button" class="btn btn-sm btn-outline-secondary" data-id="${item.id}">Прочитано</button>
                </div>
            `;

            const button = element.querySelector("button[data-id]");
            button?.addEventListener("click", async () => {
                await markAsRead(item.id);
                await loadNotifications();
            });

            list.appendChild(element);
        });
    }

    async function markAsRead(notificationId) {
        const payload = new FormData();
        payload.append("id", notificationId);
        payload.append("__RequestVerificationToken", antiForgeryToken);

        await fetch("/Notifications/MarkAsRead", {
            method: "POST",
            body: payload,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });
    }

    loadNotifications();
    setInterval(loadNotifications, 30000);
});

function escapeHtml(value) {
    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
