document.addEventListener("DOMContentLoaded", () => {
    const roleButtons = document.querySelectorAll(".role-btn");
    const selectedRole = document.getElementById("selectedRole");
    if (!roleButtons.length || !selectedRole) {
        return;
    }

    roleButtons.forEach((button) => {
        button.addEventListener("click", () => {
            roleButtons.forEach((item) => item.classList.remove("active"));
            button.classList.add("active");
            selectedRole.value = button.dataset.role;
        });
    });
});
