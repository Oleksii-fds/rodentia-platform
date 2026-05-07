function confirmRemove(studentId, studentName) {
    const idInput = document.getElementById("removeStudentId");
    const nameElement = document.getElementById("removeStudentName");
    const modalElement = document.getElementById("removeStudentModal");
    if (!idInput || !nameElement || !modalElement) {
        return;
    }

    idInput.value = studentId;
    nameElement.textContent = studentName;
    new bootstrap.Modal(modalElement).show();
}

function viewStudentProfile(studentId) {
    fetch(`/Profiles/StudentProfile?id=${studentId}`, {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    })
        .then((response) => {
            if (!response.ok) {
                throw new Error("Не вдалося завантажити профіль");
            }

            return response.text();
        })
        .then((html) => {
            const content = document.getElementById("studentProfileContent");
            const modalElement = document.getElementById("studentProfileModal");
            if (!content || !modalElement) {
                return;
            }

            content.innerHTML = html;
            new bootstrap.Modal(modalElement).show();
        })
        .catch((error) => alert(`Помилка: ${error.message}`));
}
