document.addEventListener("DOMContentLoaded", () => {
    const openButton = document.getElementById("open-profile-modal");
    const modalRoot = document.getElementById("profile-modal-root");

    if (!openButton || !modalRoot) return;

    openButton.addEventListener("click", async (event) => {
        event.preventDefault();

        const response = await fetch("/Profiles/OwnModal", {
            method: "GET",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        const responseText = await response.text();

        if (!response.ok) {
            console.error("OwnModal failed", response.status, responseText);
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
    const fileInput = modalRoot.querySelector("#avatar-file-input");
    const changeLink = modalRoot.querySelector("#avatar-change-link");
    const previewStatus = modalRoot.querySelector("#avatar-preview-status");

    if (!overlay || !form || !errorsBox) return;

    const originalAvatarSrc = modalRoot.querySelector("#profile-avatar-img")?.src ?? null;

    const clearPasswords = () => {
        const cur = form.querySelector('input[name="CurrentPassword"]');
        const nw = form.querySelector('input[name="NewPassword"]');
        if (cur) cur.value = "";
        if (nw) nw.value = "";
    };
    clearPasswords();
    setTimeout(clearPasswords, 50);
    setTimeout(clearPasswords, 300);

    const closeModal = () => {
        const avatarImg = modalRoot.querySelector("#profile-avatar-img");
        const placeholder = modalRoot.querySelector("#profile-avatar-placeholder");

        if (avatarImg && originalAvatarSrc) {
            avatarImg.src = originalAvatarSrc;
        } else if (avatarImg) {
            avatarImg.classList.add("d-none");
            if (placeholder) placeholder.style.display = "";
        }

        if (fileInput) fileInput.value = "";
        if (previewStatus) { previewStatus.textContent = ""; }

        modalRoot.innerHTML = "";
    };

    closeButtons.forEach(btn => btn.addEventListener("click", closeModal));
    overlay.addEventListener("click", e => {
        if (e.target === overlay) closeModal();
    });

    if (changeLink && fileInput) {
        changeLink.addEventListener("click", e => {
            e.preventDefault();
            fileInput.click();
        });

        fileInput.addEventListener("change", () => {
            const file = fileInput.files[0];
            if (!file) return;

            if (!["image/jpeg", "image/png", "image/webp"].includes(file.type)) {
                previewStatus.textContent = "Дозволені лише JPG, PNG, WEBP.";
                previewStatus.style.color = "#c00";
                fileInput.value = "";
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                previewStatus.textContent = "Файл занадто великий (макс. 5 МБ).";
                previewStatus.style.color = "#c00";
                fileInput.value = "";
                return;
            }

            const previewUrl = URL.createObjectURL(file);
            const wrapper = modalRoot.querySelector(".profile-avatar-wrapper");
            const placeholder = modalRoot.querySelector("#profile-avatar-placeholder");
            let avatarImg = modalRoot.querySelector("#profile-avatar-img");

            if (!avatarImg) {
                avatarImg = document.createElement("img");
                avatarImg.id = "profile-avatar-img";
                avatarImg.alt = "Фото профілю";
                avatarImg.className = "profile-avatar-img";
                if (wrapper) wrapper.appendChild(avatarImg);
            }

            avatarImg.src = previewUrl;
            avatarImg.classList.remove("d-none");
            if (placeholder) placeholder.style.display = "none";

            previewStatus.textContent = "📎 Фото буде збережено після натискання «Зберегти зміни»";
            previewStatus.style.color = "#555";
        });
    }

    form.addEventListener("submit", async event => {
        event.preventDefault();

        errorsBox.classList.remove("is-visible");
        errorsBox.innerHTML = "";

        const formData = new FormData(form);

        const response = await fetch(form.action, {
            method: "POST",
            body: formData,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (response.ok) {
            modalRoot.innerHTML = "";
            window.location.reload();
            return;
        }

        let payload;
        try { payload = await response.json(); }
        catch { payload = { message: "Сталася помилка." }; }

        const errors = [];
        if (payload?.message) errors.push(payload.message);
        if (Array.isArray(payload?.errors)) errors.push(...payload.errors);
        if (errors.length === 0) errors.push("Сталася помилка.");

        errorsBox.innerHTML = errors.map(e => `<div>${escapeHtml(e)}</div>`).join("");
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