document.addEventListener("DOMContentLoaded", () => {
    const openButton = document.getElementById("open-profile-modal");
    const modalRoot = document.getElementById("profile-modal-root");

    if (!openButton || !modalRoot) {
        return;
    }

    openButton.addEventListener("click", async (event) => {
        event.preventDefault();

        const response = await fetch("/Profiles/OwnModal", {
            method: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        const responseText = await response.text();

        if (!response.ok) {
            console.error("OwnModal failed");
            console.error("Status:", response.status);
            console.error("Body:", responseText);

            alert(`Не вдалося відкрити профіль. Status: ${response.status}`);
            return;
        }

        modalRoot.innerHTML = responseText;
        bindProfileModal(modalRoot);
    });
});

function bindProfileModal(modalRoot) {
    const overlay = modalRoot.querySelector("#profile-modal-overlay");
    const closeButtons = modalRoot.querySelectorAll("[data-profile-close]");
    const form = modalRoot.querySelector("#own-profile-form");
    const errorsBox = modalRoot.querySelector("#profile-form-errors");

    if (!overlay || !form || !errorsBox) {
        return;
    }

    const currentPasswordInput = form.querySelector('input[name="CurrentPassword"]');
    const newPasswordInput = form.querySelector('input[name="NewPassword"]');

    const clearPasswordInputs = () => {
        if (currentPasswordInput) {
            currentPasswordInput.value = "";
        }

        if (newPasswordInput) {
            newPasswordInput.value = "";
        }
    };

    clearPasswordInputs();
    setTimeout(clearPasswordInputs, 50);
    setTimeout(clearPasswordInputs, 300);

    closeButtons.forEach(button => {
        button.addEventListener("click", () => {
            modalRoot.innerHTML = "";
        });
    });

    overlay.addEventListener("click", event => {
        if (event.target === overlay) {
            modalRoot.innerHTML = "";
        }
    });

    form.addEventListener("submit", async event => {
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