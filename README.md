# 🍔 Fast Food Shop (ASM1_NET)

Một ứng dụng web thương mại điện tử bán đồ ăn nhanh được xây dựng bằng **ASP.NET Core 8.0** và **Entity Framework Core**. Hệ thống cung cấp đầy đủ quy trình từ đặt hàng, giao hàng đến quản trị.

## 🚀 Tính Năng Chính

### 👤 Khách Hàng (Customer)

- **Đăng ký/Đăng nhập**: Hỗ trợ đăng nhập qua tài khoản Google.
- **Duyệt món ăn**: Xem danh sách món ăn, combo, tìm kiếm và lọc theo danh mục.
- **Giỏ hàng**: Thêm/sửa/xóa món trong giỏ hàng.
- **Đặt hàng (Checkout)**: Nhập thông tin giao hàng, chọn phương thức thanh toán.
- **Lịch sử đơn hàng**: Xem lại các đơn đã đặt và **hủy đơn hàng** (khi trạng thái là Pending).

### 🛠 Quản Trị Viên (Admin)

- **Dashboard**: Thống kê doanh thu, đơn hàng, hoạt động.
- **Quản lý Sản phẩm**: Thêm, sửa, xóa (Soft Delete) Món ăn và Combo.
- **Quản lý Đơn hàng**: Xem chi tiết, cập nhật trạng thái đơn hàng.
- **Nhật ký hoạt động (Activity Log)**: Theo dõi lích sử thao tác của hệ thống (Login, Order, CRUD).
- **Thùng rác**: Khôi phục các dữ liệu đã bị xóa tạm thời.

### 🚚 Nhân Viên Giao Hàng (Shipper)

- **Danh sách đơn**: Xem các đơn hàng được phân công hoặc cần giao.
- **Cập nhật**: Đổi trạng thái đơn hàng thành "Đang giao", "Đã giao".

---

## 🛠 Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 8.0 (MVC)
- **Database**: SQL Server (Entity Framework Core Code-First)
- **Frontend**: Razor Views, Bootstrap 5, CSS/JS tùy chỉnh.
- **Authentication**: ASP.NET Core Identity (Cookie Auth) & Google OAuth.
- **Logging**: Custom Activity Logging Service.

---

## ⚙️ Cài Đặt & Chạy Dự Án

### Yêu cầu

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server

### Các bước

1. **Clone dự án**

   ```bash
   git clone https://github.com/your-username/ASM1_NET.git
   cd ASM1_NET
   ```

2. **Cấu hình Database**
   Mở file `appsettings.json` và cập nhật chuỗi kết nối `DefaultConnection` phù hợp với SQL Server của bạn:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=FastFoodShopDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Cập nhật Database (Migrations)**
   Mở terminal tại thư mục dự án và chạy:

   ```bash
   dotnet ef database update
   ```

4. **Chạy ứng dụng**

   ```bash
   dotnet run
   ```

---

## 📂 Cấu Trúc Thư Mục

- `Areas/Admin`: Các trang quản trị (Dashboard, Products, Activity Logs).
- `Areas/Shipper`: Giao diện dành cho Shipper.
- `Controllers`: Các Controller chính (Home, Order, Cart).
- `Models`: Các Entity (User, Food, Order, ActivityLog...).
- `Services`: Các service xử lý logic (ActivityLogService, EmailService).
- `Views`: Giao diện người dùng (Razor Pages).

---

## 📝 Nhật Ký Cập Nhật (Gần đây)

- [x] Thêm tính năng **Activity Log** theo dõi toàn bộ hoạt động.
- [x] Cập nhật **Soft Delete** cho User, Food, Order.
- [x] Bổ sung tính năng **Hủy đơn hàng** cho khách hàng.
- [x] Fix lỗi hiển thị trang Activity Log.

---

**ASM1_NET - Đồ án lập trình web .NET**
