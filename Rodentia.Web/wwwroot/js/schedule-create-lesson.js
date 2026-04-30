document.addEventListener("DOMContentLoaded", () => {
    const openButton = document.getElementById("open-create-lesson-modal");
    const modalRoot = document.getElementById("create-lesson-modal-root");

    if (!modalRoot) return;

    if (openButton) {
        openButton.addEventListener("click", async (event) => {
            event.preventDefault();
            const response = await fetch("/Schedule/CreateLessonModal", {
                method: "GET",
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            const responseText = await response.text();
            if (!response.ok) { alert("Не вдалося відкрити форму створення заняття."); return; }
            modalRoot.innerHTML = responseText;
            bindCreateLessonModal(modalRoot);
        });
    }

    document.querySelectorAll('.lesson-item').forEach(item => {
        item.addEventListener('click', async function () {
            const lessonId = this.dataset.lessonId;
            const isTeacher = Boolean(openButton);
            const endpoint = isTeacher
                ? `/Schedule/EditLessonModal?lessonId=${lessonId}`
                : `/Schedule/LessonDetailsModal?lessonId=${lessonId}`;
            const response = await fetch(endpoint, {
                method: "GET",
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            const responseText = await response.text();
            if (!response.ok) { alert("Не вдалося відкрити деталі заняття."); return; }
            modalRoot.innerHTML = responseText;
            if (isTeacher) {
                bindEditLessonModal(modalRoot);
                return;
            }
            bindLessonDetailsModal(modalRoot);
        });
    });
});

function bindCreateLessonModal(modalRoot) {
    const overlay = modalRoot.querySelector("#lesson-modal-overlay");
    const closeButtons = modalRoot.querySelectorAll("[data-lesson-close]");
    const form = modalRoot.querySelector("#create-lesson-form");
    const errorsBox = modalRoot.querySelector("#lesson-form-errors");

    if (!overlay || !form || !errorsBox) return;

    closeButtons.forEach(btn => btn.addEventListener("click", () => { modalRoot.innerHTML = ""; }));
    overlay.addEventListener("click", e => { if (e.target === overlay) modalRoot.innerHTML = ""; });
    bindRecurringLessonOptions(modalRoot);

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        errorsBox.classList.remove("is-visible");
        errorsBox.innerHTML = "";

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (response.ok) { modalRoot.innerHTML = ""; window.location.reload(); return; }

        let payload;
        try { payload = await response.json(); } catch { payload = { message: "Сталася помилка." }; }
        const errors = [];
        if (payload?.message) errors.push(payload.message);
        if (Array.isArray(payload?.errors)) errors.push(...payload.errors);
        if (errors.length === 0) errors.push("Сталася помилка.");
        errorsBox.innerHTML = errors.map(e => `<div>${escapeHtml(e)}</div>`).join("");
        errorsBox.classList.add("is-visible");
    });
}

function bindRecurringLessonOptions(modalRoot) {
    const recurringCheckbox = modalRoot.querySelector("#create-is-recurring");
    const recurringOptions = modalRoot.querySelector("#recurring-options");
    if (!recurringCheckbox || !recurringOptions) return;

    const toggleRecurringOptions = () => {
        recurringOptions.classList.toggle("d-none", !recurringCheckbox.checked);
    };

    recurringCheckbox.addEventListener("change", toggleRecurringOptions);
    toggleRecurringOptions();
}

function bindEditLessonModal(modalRoot) {
    const overlay = modalRoot.querySelector("#edit-lesson-modal-overlay");
    const closeButtons = modalRoot.querySelectorAll("[data-lesson-close]");
    const form = modalRoot.querySelector("#edit-lesson-form");
    const errorsBox = modalRoot.querySelector("#edit-lesson-form-errors");

    if (!overlay || !form || !errorsBox) return;

    closeButtons.forEach(btn => btn.addEventListener("click", () => { modalRoot.innerHTML = ""; }));
    overlay.addEventListener("click", e => { if (e.target === overlay) modalRoot.innerHTML = ""; });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        errorsBox.classList.remove("is-visible");
        errorsBox.innerHTML = "";

        const response = await fetch(form.action, {
            method: "POST",
            body: new FormData(form),
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (response.ok) { modalRoot.innerHTML = ""; window.location.reload(); return; }

        let payload;
        try { payload = await response.json(); } catch { payload = { message: "Сталася помилка." }; }
        const errors = [];
        if (payload?.message) errors.push(payload.message);
        if (Array.isArray(payload?.errors)) errors.push(...payload.errors);
        if (errors.length === 0) errors.push("Сталася помилка.");
        errorsBox.innerHTML = errors.map(e => `<div>${escapeHtml(e)}</div>`).join("");
        errorsBox.classList.add("is-visible");
    });

    const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value || "";
    const lessonId = form.querySelector('input[name="LessonId"]')?.value || "";
    bindRescheduleSection(modalRoot, lessonId, token);
}

function bindLessonDetailsModal(modalRoot) {
    const overlay = modalRoot.querySelector("#lesson-details-modal-overlay");
    const closeButtons = modalRoot.querySelectorAll("[data-lesson-close]");

    if (!overlay) return;

    closeButtons.forEach(btn => btn.addEventListener("click", () => { modalRoot.innerHTML = ""; }));
    overlay.addEventListener("click", e => { if (e.target === overlay) modalRoot.innerHTML = ""; });

    const token = modalRoot.querySelector('#lesson-details-actions-form input[name="__RequestVerificationToken"]')?.value || "";
    const lessonId = modalRoot.querySelector("#reschedule-section")?.dataset.lessonId || "";
    bindRescheduleSection(modalRoot, lessonId, token);
}

function bindRescheduleSection(modalRoot, lessonId, token) {
    const section = modalRoot.querySelector("#reschedule-section");
    const createButton = modalRoot.querySelector("#create-reschedule-btn");
    const dateInput = modalRoot.querySelector("#reschedule-date");
    const timeInput = modalRoot.querySelector("#reschedule-time");
    const reasonInput = modalRoot.querySelector("#reschedule-reason");
    const errorsBox = modalRoot.querySelector("#reschedule-errors");
    const list = modalRoot.querySelector("#reschedule-requests-list");

    if (!section || !createButton || !dateInput || !timeInput || !reasonInput || !errorsBox || !list || !lessonId) {
        return;
    }

    createButton.addEventListener("click", async () => {
        errorsBox.textContent = "";

        const payload = new FormData();
        payload.append("lessonId", lessonId);
        payload.append("lessonDate", dateInput.value);
        payload.append("startTime", timeInput.value);
        payload.append("reason", reasonInput.value);
        payload.append("__RequestVerificationToken", token);

        const response = await fetch("/Schedule/CreateRescheduleRequest", {
            method: "POST",
            body: payload,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            const responsePayload = await response.json().catch(() => ({ message: "Не вдалося створити запит." }));
            errorsBox.textContent = responsePayload.message || "Не вдалося створити запит.";
            return;
        }

        dateInput.value = "";
        timeInput.value = "";
        reasonInput.value = "";
        await loadRescheduleRequests(lessonId, token, list, errorsBox);
    });

    loadRescheduleRequests(lessonId, token, list, errorsBox);
}

async function loadRescheduleRequests(lessonId, token, list, errorsBox) {
    list.innerHTML = "";
    const response = await fetch(`/Schedule/RescheduleRequests?lessonId=${lessonId}`, {
        method: "GET",
        headers: { "X-Requested-With": "XMLHttpRequest" }
    });

    if (!response.ok) {
        const payload = await response.json().catch(() => ({ message: "Не вдалося отримати запити на перенесення." }));
        errorsBox.textContent = payload.message || "Не вдалося отримати запити на перенесення.";
        return;
    }

    const payload = await response.json();
    const items = Array.isArray(payload?.items) ? payload.items : [];

    if (items.length === 0) {
        list.innerHTML = '<div class="text-muted small">Активних запитів немає.</div>';
        return;
    }

    items.forEach(item => {
        const row = document.createElement("div");
        row.className = "border rounded p-2 mb-2";

        const dateText = new Date(item.proposedScheduledAt).toLocaleString("uk-UA");
        const reason = escapeHtml(item.reason || "");
        row.innerHTML = `
            <div><strong>Новий час:</strong> ${dateText}</div>
            <div><strong>Причина:</strong> ${reason}</div>
            <div class="d-flex gap-2 mt-2" data-actions></div>
        `;

        if (item.canReview) {
            const actions = row.querySelector("[data-actions]");
            const approveButton = document.createElement("button");
            approveButton.type = "button";
            approveButton.className = "btn btn-sm btn-success";
            approveButton.textContent = "Підтвердити";
            approveButton.addEventListener("click", async () => {
                await approveRescheduleRequest(item.requestId, token, lessonId, list, errorsBox);
            });

            const rejectButton = document.createElement("button");
            rejectButton.type = "button";
            rejectButton.className = "btn btn-sm btn-danger";
            rejectButton.textContent = "Відхилити";
            rejectButton.addEventListener("click", async () => {
                const rejectReason = prompt("Вкажіть причину відхилення:");
                if (rejectReason === null) return;
                await rejectRescheduleRequest(item.requestId, rejectReason, token, lessonId, list, errorsBox);
            });

            actions.appendChild(approveButton);
            actions.appendChild(rejectButton);
        } else {
            row.querySelector("[data-actions]").innerHTML = '<span class="text-muted small">Очікує рішення іншої сторони.</span>';
        }

        list.appendChild(row);
    });
}

async function approveRescheduleRequest(requestId, token, lessonId, list, errorsBox) {
    const payload = new FormData();
    payload.append("requestId", requestId);
    payload.append("__RequestVerificationToken", token);

    const response = await fetch("/Schedule/ApproveRescheduleRequest", {
        method: "POST",
        body: payload,
        headers: { "X-Requested-With": "XMLHttpRequest" }
    });

    if (!response.ok) {
        const responsePayload = await response.json().catch(() => ({ message: "Не вдалося підтвердити запит." }));
        errorsBox.textContent = responsePayload.message || "Не вдалося підтвердити запит.";
        return;
    }

    await loadRescheduleRequests(lessonId, token, list, errorsBox);
    window.location.reload();
}

async function rejectRescheduleRequest(requestId, reason, token, lessonId, list, errorsBox) {
    const payload = new FormData();
    payload.append("requestId", requestId);
    payload.append("reason", reason);
    payload.append("__RequestVerificationToken", token);

    const response = await fetch("/Schedule/RejectRescheduleRequest", {
        method: "POST",
        body: payload,
        headers: { "X-Requested-With": "XMLHttpRequest" }
    });

    if (!response.ok) {
        const responsePayload = await response.json().catch(() => ({ message: "Не вдалося відхилити запит." }));
        errorsBox.textContent = responsePayload.message || "Не вдалося відхилити запит.";
        return;
    }

    await loadRescheduleRequests(lessonId, token, list, errorsBox);
}

function requestDeleteLesson(lessonId) {
    const tokenEl = document.querySelector('#edit-lesson-form input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';

    const modalRoot = document.getElementById("create-lesson-modal-root");

    if (modalRoot) modalRoot.innerHTML = "";

    const existing = document.getElementById('delete-lesson-global-modal');
    if (existing) existing.remove();

    const wrapper = document.createElement('div');
    wrapper.id = 'delete-lesson-global-modal';
    wrapper.innerHTML = `
        <div class="modal fade" id="deleteLessonConfirmModal" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow">
                    <div class="modal-header"
                         style="background:#f4b667; border-bottom:1px solid #e8a142;">
                        <h5 class="modal-title fw-bold">Підтвердження видалення</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body py-4">
                        Ви впевнені, що хочете видалити це заняття? Дію неможливо скасувати.
                    </div>
                    <div class="modal-footer"
                         style="background:#f5efe4; border-top:1px solid #e8a142;">
                        <button type="button" class="btn btn-secondary"
                                data-bs-dismiss="modal">Скасувати</button>
                        <button type="button" class="btn btn-danger"
                                id="delete-confirm-ok">Видалити</button>
                    </div>
                </div>
            </div>
        </div>`;
    document.body.appendChild(wrapper);

    const bsModal = new bootstrap.Modal(
        document.getElementById('deleteLessonConfirmModal')
    );
    bsModal.show();

    document.getElementById('delete-confirm-ok').addEventListener('click', async () => {
        bsModal.hide();
        await deleteLesson(lessonId, token);
    });

    document.getElementById('deleteLessonConfirmModal')
        .addEventListener('hidden.bs.modal', () => wrapper.remove());
}

async function deleteLesson(lessonId, token) {
    if (!token) {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        token = el ? el.value : '';
    }

    try {
        const response = await fetch(`/Schedule/DeleteLesson?lessonId=${lessonId}`, {
            method: "POST",
            headers: {
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": token
            }
        });

        if (response.ok) { window.location.reload(); return; }

        const payload = await response.json().catch(() => ({}));
        alert(payload.message || "Сталася помилка при видаленні.");
    } catch (error) {
        console.error("Delete error:", error);
        alert("Не вдалося зв'язатися з сервером.");
    }
}

function escapeHtml(value) {
    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}