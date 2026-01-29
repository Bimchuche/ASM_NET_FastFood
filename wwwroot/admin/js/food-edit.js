document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("foodEditForm");
    const errorBox = document.getElementById("errorBox");

    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(form);

        fetch("/Admin/Food/EditAjax", {
            method: "POST",
            body: formData
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                window.location.href = "/Admin/Food/Index";
            } else {
                errorBox.innerText = data.message || "Cập nhật thất bại";
                errorBox.classList.remove("d-none");
            }
        })
        .catch(() => {
            errorBox.innerText = "Có lỗi xảy ra khi gửi dữ liệu";
            errorBox.classList.remove("d-none");
        });
    });

});
