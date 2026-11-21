-- =============================================================================
-- CẬP NHẬT MẬT KHẨU & CÂU TRẢ LỜI BẢO MẬT ĐÚNG ĐỊNH DẠNG PBKDF2-HMACSHA256
-- Chạy script này sau file "SQL QUẢN LÝ BÁN HÀNG + DỮ LIỆU MẪU.sql" để đăng nhập test ngay:
--   - Quản lý / Nhân viên (Id 1, 2, 3): Mật khẩu = Admin@123
--     (admin@techshop.vn | lananh.nv@techshop.vn | hoangnam.nv@techshop.vn)
--   - Khách hàng (Id 4, 5, 6, 7):       Mật khẩu = User@123
--     (duchuy.pham@gmail.com | thutrang.vu@gmail.com | quocbao.dang@gmail.com)
--   - Câu trả lời bảo mật cho các tài khoản đã thiết lập (Id 1, 2, 4): hanoi
-- =============================================================================
USE QuanLyBanHang;
GO

UPDATE dbo.TaiKhoan
SET MatKhauHash = N'100000.SENTaG9wU2FsdEFkbWluMQ==.KfUDghJzu0QQzKwp90m/FrafnW91ZEgUXL/KeMNeST4='
WHERE VaiTro IN ('QuanLy', 'NhanVien');

UPDATE dbo.TaiKhoan
SET MatKhauHash = N'100000.SENTaG9wU2FsdFVzZXIwMQ==.Jnt3SIjVQ0tALkfTYVHWRol/G3qD6A2EyPU2guk6kcY='
WHERE VaiTro = 'KhachHang';

UPDATE dbo.TaiKhoan
SET CauTraLoiHash = N'100000.SENTaG9wU2FsdEFuczAwMQ==.neY+YxMAHEvuwFfGWHwr27lmGzAC+05/jMogJR4HZAo='
WHERE CauHoiBaoMat IS NOT NULL;
GO
