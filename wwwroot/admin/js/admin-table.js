// Admin table management JS - Shared functions for all admin index pages

// Toggle all checkboxes
function toggleAll(source) {
    document.querySelectorAll('.item-checkbox').forEach(cb => cb.checked = source.checked);
    updateCount();
}

// Update selected count display
function updateCount() {
    document.getElementById('selectedCount').textContent = document.querySelectorAll('.item-checkbox:checked').length;
}

// Submit bulk delete form
function submitBulkDelete(entityName = 'mục') {
    const checked = document.querySelectorAll('.item-checkbox:checked');
    if (checked.length === 0) {
        alert('Vui lòng chọn ít nhất 1 ' + entityName);
        return;
    }
    if (!confirm(`Bạn có chắc muốn xóa ${checked.length} ${entityName} đã chọn?`)) return;
    
    const container = document.getElementById('deleteIds');
    container.innerHTML = '';
    checked.forEach(cb => {
        const i = document.createElement('input');
        i.type = 'hidden';
        i.name = 'ids';
        i.value = cb.dataset.id;
        container.appendChild(i);
    });
    document.getElementById('bulkDeleteForm').submit();
}

// Generic table search filter
function filterTable(searchInputId, tableId) {
    var input = document.getElementById(searchInputId);
    var filter = input.value.toLowerCase();
    var table = document.getElementById(tableId);
    var rows = table.getElementsByTagName("tr");
    
    for (var i = 1; i < rows.length; i++) {
        var cells = rows[i].getElementsByTagName("td");
        var found = false;
        for (var j = 0; j < cells.length; j++) {
            if (cells[j].textContent.toLowerCase().indexOf(filter) > -1) {
                found = true;
                break;
            }
        }
        rows[i].style.display = found ? "" : "none";
    }
}

// Filter by category (for Food page)
function filterByCategory(categoryFilterId, searchInputId, tableId) {
    var categoryFilter = document.getElementById(categoryFilterId).value;
    var input = document.getElementById(searchInputId);
    var filter = input.value.toLowerCase();
    var table = document.getElementById(tableId);
    var rows = table.querySelectorAll("tbody tr");
    
    rows.forEach(function(row) {
        var matchesCategory = categoryFilter === "" || row.dataset.category === categoryFilter;
        var cells = row.getElementsByTagName("td");
        var matchesSearch = false;
        
        for (var j = 0; j < cells.length; j++) {
            if (cells[j].textContent.toLowerCase().indexOf(filter) > -1) {
                matchesSearch = true;
                break;
            }
        }
        
        row.style.display = (matchesCategory && matchesSearch) ? "" : "none";
    });
}

// Filter by status (for Order page)
function filterByStatus(statusFilterId, tableId) {
    var filterValue = document.getElementById(statusFilterId).value;
    var table = document.getElementById(tableId);
    var rows = table.querySelectorAll("tbody tr");
    
    rows.forEach(function(row) {
        if (filterValue === "") {
            row.style.display = "";
        } else if (filterValue.startsWith("status:")) {
            var status = filterValue.replace("status:", "");
            row.style.display = row.dataset.status === status ? "" : "none";
        } else if (filterValue.startsWith("payment:")) {
            var payment = filterValue.replace("payment:", "");
            row.style.display = row.dataset.payment === payment ? "" : "none";
        }
    });
}
