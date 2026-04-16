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
            const response = await fetch(`/Schedule/EditLessonModal?lessonId=${lessonId}`, {
                method: "GET",
                headers: { "X-Requested-With": "XMLHttpRequest" }
            });
            const responseText = await response.text();
            if (!response.ok) { alert("Не вдалося відкрити форму редагування заняття."); return; }
            modalRoot.innerHTML = responseText;
            bindEditLessonModal(modalRoot);
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