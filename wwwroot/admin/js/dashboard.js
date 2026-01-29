document.addEventListener("DOMContentLoaded", function () {

    const canvas = document.getElementById("foodChart");
    if (!canvas) return;

    const labels = JSON.parse(canvas.dataset.labels);
    const values = JSON.parse(canvas.dataset.values);

    new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                label: "Số món ăn",
                data: values,
                backgroundColor: "#4e73df"
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            }
        }
    });
});
