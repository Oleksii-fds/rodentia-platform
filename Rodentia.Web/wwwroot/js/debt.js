async function confirmPayment(lessonId, button) {
    button.disabled = true;
    button.innerHTML = "<span class=\"spinner-border spinner-border-sm\"></span>";

    const token = document.querySelector("input[name=\"__RequestVerificationToken\"]")?.value;

    try {
        const response = await fetch("/Debt/ConfirmPayment", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                RequestVerificationToken: token,
                "X-Requested-With": "XMLHttpRequest"
            },
            body: `lessonId=${lessonId}`
        });

        if (response.ok) {
            const row = document.getElementById(`lesson-row-${lessonId}`);
            row?.classList.add("debt-paid-row");
            button.outerHTML = "<span class=\"text-success fw-semibold\">Оплачено</span>";
            setTimeout(() => location.reload(), 1000);
            return;
        }

        const data = await response.json();
        alert(data.message || "Помилка підтвердження оплати.");
    } catch (error) {
        console.error(error);
    }

    button.disabled = false;
    button.innerHTML = "Оплачено";
}
