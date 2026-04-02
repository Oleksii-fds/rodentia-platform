document.addEventListener("DOMContentLoaded", () => {
    const openButton = document.getElementById("open-create-lesson-modal");
    const modalRoot = document.getElementById("create-lesson-modal-root");

    if (!openButton || !modalRoot) {
        return;
    }

    openButton.addEventListener("click", async (event) => {
        event.preventDefault();

        const response = await fetch("/Schedule/CreateLessonModal", {
            method: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        const responseText = await response.text();

        if (!response.ok) {
            console.error("CreateLessonModal failed");
            console.error("Status:", response.status);
            console.error("Body:", responseText);
            alert("Не вдалося відкрити форму створення заняття.");
            return;
        }

        modalRoot.innerHTML = responseText;
        bindCreateLessonModal(modalRoot);
    });
});

function bindCreateLessonModal(modalRoot) {
    const overlay = modalRoot.querySelector("#lesson-modal-overlay");
    const closeButtons = modalRoot.querySelectorAll("[data-lesson-close]");
    const form = modalRoot.querySelector("#create-lesson-form");
    const errorsBox = modalRoot.querySelector("#lesson-form-errors");

    if (!overlay || !form || !errorsBox) {
        return;
    }

    closeButtons.forEach(button => {
        button.addEventListener("click", () => {
            modalRoot.innerHTML = "";
        });
    });

    overlay.addEventListener("click", (event) => {
        if (event.target === overlay) {
            modalRoot.innerHTML = "";
        }
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        errorsBox.classList.remove("is-visible");
        errorsBox.innerHTML = "";

        const formData = new FormData(form);

        const response = await fetch(form.action, {
            method: "POST",
            body: formData,
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        if (response.ok) {
            modalRoot.innerHTML = "";
            window.location.reload();
            return;
        }

        let payload;

        try {
            payload = await response.json();
        } catch {
            payload = { message: "Сталася помилка." };
        }

        const errors = [];

        if (payload?.message) {
            errors.push(payload.message);
        }

        if (Array.isArray(payload?.errors)) {
            errors.push(...payload.errors);
        }

        if (errors.length === 0) {
            errors.push("Сталася помилка.");
        }

        errorsBox.innerHTML = errors
            .map(error => `<div>${escapeHtml(error)}</div>`)
            .join("");

        errorsBox.classList.add("is-visible");
    });
}

function escapeHtml(value) {
    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}