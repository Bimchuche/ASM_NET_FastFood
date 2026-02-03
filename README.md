# 🍔 Fast Food Shop

Hệ thống website đặt món ăn nhanh trực tuyến với đầy đủ tính năng cho khách hàng, admin và shipper. Xây dựng trên **ASP.NET Core 8.0**.

---

## ✨ Tính Năng Chính

### 👤 Khách Hàng

- Đăng ký/Đăng nhập (Email + Google OAuth)
- Duyệt menu món ăn & combo
- Tìm kiếm, lọc theo danh mục, khoảng giá, đánh giá
- Giỏ hàng & Thanh toán (COD/QR)
- Theo dõi & hủy đơn hàng
- Đánh giá đơn hàng đã nhận
- Chat hỗ trợ trực tuyến (SignalR)
- Wishlist yêu thích
- Tích điểm loyalty

### 🛠️ Quản Trị (Admin)

- Dashboard thống kê doanh thu, biểu đồ
- CRUD Món ăn, Combo, Danh mục
- Quản lý đơn hàng, gán shipper
- Quản lý người dùng & phân quyền
- Activity Logs - theo dõi hoạt động
- Soft Delete & Trash (khôi phục/xóa vĩnh viễn)
- Chat hỗ trợ khách hàng

### 🚚 Shipper

- Xem đơn được phân công
- Cập nhật trạng thái giao hàng
- Xem thông tin khách hàng

---

## 🛠️ Công Nghệ

| Layer     | Tech                                    |
| --------- | --------------------------------------- |
| Backend   | ASP.NET Core 8.0 MVC                    |
| Database  | SQL Server + EF Core                    |
| Frontend  | Razor Views, Bootstrap 5, Custom CSS/JS |
| Real-time | SignalR (Chat)                          |
| Auth      | Cookie Auth + Google OAuth              |
| Pattern   | Repository, DI, Async Services          |

---

## 📁 Cấu Trúc Thư Mục

```
ASM1_NET/
├── Areas/Admin/           # Admin area (Controllers, Views)
├── Controllers/           # Client controllers
├── Models/                # Entity models
├── Repositories/          # Data access layer
├── Services/              # Business logic
├── Views/                 # Client views
├── wwwroot/
│   ├── admin/
│   │   ├── css/           # Admin styles
│   │   └── js/            # Admin scripts
│   └── client/
│       ├── css/           # Client styles
│       └── js/            # Client scripts
└── Hubs/                  # SignalR hubs
```

---

## 🚀 Cài Đặt

### Yêu cầu

- .NET 8.0 SDK
- SQL Server 2019+
- Visual Studio 2022 / VS Code

### Các bước

```bash
# 1. Clone repo
git clone https://github.com/Bimchuche/ASM_NET_FastFood.git
cd ASM_NET_FastFood

# 2. Cấu hình database trong appsettings.json
# Sửa ConnectionStrings:DefaultConnection

# 3. Chạy migration
dotnet ef database update

# 4. Chạy ứng dụng
dotnet run
```

Truy cập: `https://localhost:5001`

---

## 👥 Tài Khoản Test

| Role     | Email              | Password  |
| -------- | ------------------ | --------- |
| Admin    | admin@fastfood.com | Admin@123 |
| Customer | user@test.com      | User@123  |

---

## 📸 Screenshots

### Trang chủ

![Home](wwwroot/images/screenshots/home.png)

### Admin Dashboard

![Dashboard](wwwroot/images/screenshots/dashboard.png)

---

## 📄 License

MIT License © 2026 Phạm Nguyễn Bảo Minh
