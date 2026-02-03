// Trash management JS - Shared functions for trash pages

// Toggle all checkboxes
function toggleAll(source) {
    document.querySelectorAll('.item-checkbox').forEach(cb => cb.checked = source.checked);
    updateCount();
}

// Update selected count display
function updateCount() {
    document.getElementById('selectedCount').textContent = document.querySelectorAll('.item-checkbox:checked').length;
}

// Submit bulk restore/delete for trash items
function submitBulk(action, entityName = 'item') {
    const checked = document.querySelectorAll('.item-checkbox:checked');
    if (checked.length === 0) {
        alert('Vui lòng chọn ít nhất 1 ' + entityName);
        return;
    }
    
    const actionText = action === 'restore' ? 'khôi phục' : 'xóa vĩnh viễn';
    if (!confirm(`Bạn có chắc muốn ${actionText} ${checked.length} ${entityName}?`)) return;
    
    const form = document.getElementById(action === 'restore' ? 'bulkRestoreForm' : 'bulkDeleteForm');
    const container = document.getElementById(action === 'restore' ? 'restoreIds' : 'deleteIds');
    container.innerHTML = '';
    
    checked.forEach(cb => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'ids';
        input.value = cb.dataset.id;
        container.appendChild(input);
    });
    
    form.submit();
}
