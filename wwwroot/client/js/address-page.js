// Address page JS - Modal and CRUD operations

function showAddModal() {
    document.getElementById('modalTitle').textContent = '➕ Thêm địa chỉ mới';
    document.getElementById('addressId').value = '';
    document.getElementById('addressName').value = '';
    document.getElementById('fullAddress').value = '';
    document.getElementById('addressPhone').value = '';
    document.getElementById('isDefault').checked = false;
    document.getElementById('addressModal').style.display = 'flex';
}

function editAddress(id, name, full, phone, isDefault) {
    document.getElementById('modalTitle').textContent = '✏️ Sửa địa chỉ';
    document.getElementById('addressId').value = id;
    document.getElementById('addressName').value = name;
    document.getElementById('fullAddress').value = full;
    document.getElementById('addressPhone').value = phone || '';
    document.getElementById('isDefault').checked = isDefault;
    document.getElementById('addressModal').style.display = 'flex';
}

function closeModal() {
    document.getElementById('addressModal').style.display = 'none';
}

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('addressForm').addEventListener('submit', async function(e) {
        e.preventDefault();
        const id = document.getElementById('addressId').value;
        const url = id ? '/Address/Update' : '/Address/Add';
        const body = new URLSearchParams({
            id: id,
            name: document.getElementById('addressName').value,
            fullAddress: document.getElementById('fullAddress').value,
            phone: document.getElementById('addressPhone').value,
            isDefault: document.getElementById('isDefault').checked
        });
        
        const res = await fetch(url, { method: 'POST', body: body });
        const data = await res.json();
        if (data.success) {
            location.reload();
        } else {
            alert(data.message);
        }
    });
});

async function setDefault(id) {
    const res = await fetch('/Address/SetDefault', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + id
    });
    const data = await res.json();
    if (data.success) location.reload();
}

async function deleteAddress(id) {
    if (!confirm('Bạn có chắc muốn xóa địa chỉ này?')) return;
    const res = await fetch('/Address/Delete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'id=' + id
    });
    const data = await res.json();
    if (data.success) location.reload();
}
