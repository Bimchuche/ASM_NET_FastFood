// Order Completed page JS

// Select all checkbox
document.getElementById('selectAll')?.addEventListener('change', function() {
    document.querySelectorAll('.row-checkbox').forEach(cb => cb.checked = this.checked);
    updateBulkActions();
});

// Individual checkboxes
document.querySelectorAll('.row-checkbox').forEach(cb => {
    cb.addEventListener('change', updateBulkActions);
});

function updateBulkActions() {
    const count = document.querySelectorAll('.row-checkbox:checked').length;
    document.getElementById('selectedCount').textContent = count;
    document.getElementById('bulkActions').style.display = count > 0 ? 'flex' : 'none';
}

function deleteOrder(id, code) {
    if (confirm(`Xác nhận xóa đơn hàng #${code}?`)) {
        document.getElementById('deleteId').value = id;
        document.getElementById('deleteForm').submit();
    }
}

// Search functionality
document.getElementById('searchInput')?.addEventListener('input', function() {
    const term = this.value.toLowerCase();
    document.querySelectorAll('.order-row').forEach(row => {
        const searchData = row.dataset.search?.toLowerCase() || '';
        row.style.display = searchData.includes(term) ? '' : 'none';
    });
});
