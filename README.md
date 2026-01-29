# 🍔 Fast Food Shop (ASM1_NET)

**Fast Food Shop** là một hệ thống website thương mại điện tử chuyên nghiệp cung cấp giải pháp đặt món ăn nhanh, quản lý cửa hàng và giao hàng. Dự án được xây dựng trên nền tảng **ASP.NET Core 8.0** với kiến trúc hiện đại, bảo mật và dễ mở rộng.

## 🌟 Chức Năng Nổi Bật

### 🛒 Dành Cho Khách Hàng (Customer)

- **Tài Khoản & Bảo Mật**:
  - Đăng ký và Đăng nhập dễ dàng.
  - **Đăng nhập bằng Google** (OAuth 2.0) tiện lợi.
  - Quản lý thông tin cá nhân, cập nhật địa chỉ giao hàng.
- **Trải Nghiệm Mua Sắm**:
  - Xem danh sách Món ăn (Food) và Combo khuyến mãi.
  - Tìm kiếm thông minh và Lọc món ăn theo Danh mục.
  - Xem chi tiết món ăn với hình ảnh trực quan.
- **Giỏ Hàng & Đặt Hàng**:
  - Thêm/Sửa/Xóa món trong Giỏ hàng real-time.
  - **Checkout (Thanh toán)**: Quy trình đặt hàng 3 bước (Thông tin - Xác nhận - Hoàn tất).
  - Hỗ trợ nhiều phương thức thanh toán (COD, v.v.).
- **Quản Lý Đơn Hàng**:
  - Xem lại **Lịch sử đơn hàng** đã đặt.
  - Theo dõi trạng thái đơn hàng (Đang xử lý, Đang giao, Hoàn tất).
  - **Hủy đơn hàng chủ động**: Khách hàng có thể hủy đơn ngay lập tức nếu đơn chưa được xử lý.

### 🛠 Dành Cho Quản Trị Viên (Admin)

- **Dashboard (Bảng điều khiển)**:
  - Xem tổng quan báo cáo doanh thu, số lượng đơn hàng, món ăn bán chạy.
- **Quản Lý Sản Phẩm (Món ăn & Combo)**:
  - Thêm mới, Cập nhật, Xóa (Soft Delete) món ăn và Combo.
  - Quản lý danh mục món ăn (Category).
- **Quản Lý Đơn Hàng**:
  - Duyệt đơn hàng, Gán Shipper, Cập nhật trạng thái.
  - Xem chi tiết từng đơn hàng.
- **Hệ Thống Nhật Ký Hoạt Động (Activity Logs)**:
  - **Theo dõi toàn diện**: Ghi lại mọi hành động quan trọng (Đăng nhập, Tạo đơn, Xóa món, Restore...).
  - **Bộ lọc mạnh mẽ**: Lọc theo thời gian, loại hành động, người thực hiện.
- **Quản Lý Thùng Rác (Trash/Recycle Bin)**:
  - Cơ chế **Soft Delete** giữ lại dữ liệu an toàn.
  - Khôi phục (Restore) hoặc Xóa vĩnh viễn các đối tượng (User, Food, Order) đã xóa.
- **Quản Lý Tài Khoản**:
  - Quản lý danh sách người dùng, phân quyền (Admin, Staff, Customer).

### 🚚 Dành Cho Shipper (Nhân viên giao hàng)

- **Quản Lý Giao Vận**:
  - Xem danh sách đơn hàng được phân công.
  - Cập nhật trạng thái giao hàng (Đang giao $\to$ Thành công/Thất bại).
  - Xem chi tiết địa chỉ và số điện thoại khách hàng.

---

## 💻 Công Nghệ & Kỹ Thuật

- **Backend Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server 2019+
- **ORM**: Entity Framework Core (Code-First Approach)
- **Frontend**: Razor Views (CSHTML), Bootstrap 5, Custom CSS/JS
- **Authentication**: ASP.NET Core Identity & Cookie Auth
- **Logging**: Custom Async Activity Logging Service
- **Design Pattern**: Repository Pattern, Dependency Injection (DI), ViewModel

## 🚀 Hướng Dẫn Cài Đặt

1. **Clone Source Code**:

   ```bash
   git clone https://github.com/Bimchuche/ASM_NET_FastFood.git
   cd ASM_NET_FastFood
   ```

2. **Cấu Hình Database**:
   - Mở `appsettings.json`.
   - Chỉnh sửa `DefaultConnection` trỏ đến SQL Server của bạn.

3. **Khởi Tạo Database**:

   ```bash
   dotnet ef database update
   ```

4. **Chạy Dự Án**:

   ```bash
   dotnet run
   ```

   - Truy cập Web: `http://localhost:####`

---

**Developed by [Phạm Nguyễn Bảo Minh] - 2026**
