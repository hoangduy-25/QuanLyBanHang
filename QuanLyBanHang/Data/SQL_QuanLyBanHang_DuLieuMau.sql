USE master;
GO

IF DB_ID(N'QuanLyBanHang') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLyBanHang SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyBanHang;
END;
GO

CREATE DATABASE QuanLyBanHang;
GO

USE QuanLyBanHang;
GO

-- =============================================================================
-- PHẦN 1: TẠO CẤU TRÚC 17 BẢNG (ID TỰ TĂNG INT IDENTITY(1,1))
-- =============================================================================

-- 1. BẢNG TÀI KHOẢN (UC-01 đến UC-10)
CREATE TABLE dbo.TaiKhoan (
    Id int IDENTITY(1,1) NOT NULL,
    HoTen nvarchar(100) NOT NULL,
    Email varchar(254) NOT NULL,
    SoDienThoai varchar(20) NOT NULL,
    MatKhauHash nvarchar(255) NOT NULL,
    VaiTro varchar(20) NOT NULL CONSTRAINT DF_TaiKhoan_VaiTro DEFAULT 'KhachHang',
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT 'HoatDong',
    NgaySinh date NULL,
    GioiTinh nvarchar(10) NULL,
    AnhDaiDien nvarchar(500) NULL,
    CauHoiBaoMat nvarchar(255) NULL,
    CauTraLoiHash nvarchar(255) NULL,
    LyDoKhoa nvarchar(500) NULL,
    SoLanDangNhapSai int NOT NULL CONSTRAINT DF_TaiKhoan_SoLanSai DEFAULT 0,
    KhoaTamDen datetimeoffset(0) NULL,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_TaiKhoan PRIMARY KEY (Id),
    CONSTRAINT UQ_TaiKhoan_Email UNIQUE (Email),
    CONSTRAINT UQ_TaiKhoan_SoDienThoai UNIQUE (SoDienThoai),
    CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN ('KhachHang','NhanVien','QuanLy')),
    CONSTRAINT CK_TaiKhoan_TrangThai CHECK (TrangThai IN ('HoatDong','BiKhoa')),
    CONSTRAINT CK_TaiKhoan_GioiTinh CHECK (GioiTinh IS NULL OR GioiTinh IN (N'Nam',N'Nu',N'Khac')),
    CONSTRAINT CK_TaiKhoan_SoLanSai CHECK (SoLanDangNhapSai >= 0),
    CONSTRAINT CK_TaiKhoan_BaoMat CHECK (
        (CauHoiBaoMat IS NULL AND CauTraLoiHash IS NULL)
        OR (CauHoiBaoMat IS NOT NULL AND CauTraLoiHash IS NOT NULL)
    )
);
GO

-- 2. BẢNG LỊCH SỬ KHÓA / MỞ KHÓA TÀI KHOẢN (UC-10)
CREATE TABLE dbo.LichSuKhoaTaiKhoan (
    Id int IDENTITY(1,1) NOT NULL,
    TaiKhoanId int NOT NULL,
    QuanLyId int NOT NULL,
    TrangThaiMoi varchar(20) NOT NULL,
    LyDo nvarchar(500) NULL,
    ThoiDiem datetimeoffset(0) NOT NULL CONSTRAINT DF_LichSuKhoaTaiKhoan_ThoiDiem DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_LichSuKhoaTaiKhoan PRIMARY KEY (Id),
    CONSTRAINT FK_LichSuKhoaTaiKhoan_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_LichSuKhoaTaiKhoan_QuanLy FOREIGN KEY (QuanLyId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_LichSuKhoaTaiKhoan_TrangThai CHECK (TrangThaiMoi IN ('HoatDong','BiKhoa'))
);
GO

-- 3. BẢNG ĐỊA CHỈ NHẬN HÀNG (UC-07, UC-34)
CREATE TABLE dbo.DiaChiNhanHang (
    Id int IDENTITY(1,1) NOT NULL,
    TaiKhoanId int NOT NULL,
    TenNguoiNhan nvarchar(100) NOT NULL,
    SoDienThoai varchar(20) NOT NULL,
    TinhThanh nvarchar(100) NOT NULL,
    QuanHuyen nvarchar(100) NOT NULL,
    PhuongXa nvarchar(100) NOT NULL,
    DiaChiCuThe nvarchar(300) NOT NULL,
    MacDinh bit NOT NULL CONSTRAINT DF_DiaChiNhanHang_MacDinh DEFAULT 0,
    CONSTRAINT PK_DiaChiNhanHang PRIMARY KEY (Id),
    CONSTRAINT FK_DiaChiNhanHang_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id)
);
GO

-- 4. BẢNG DANH MỤC (UC-11 đến UC-16)
CREATE TABLE dbo.DanhMuc (
    Id int IDENTITY(1,1) NOT NULL,
    Ten nvarchar(150) NOT NULL,
    MoTa nvarchar(1000) NULL,
    HinhAnh nvarchar(500) NULL,
    HienThi bit NOT NULL CONSTRAINT DF_DanhMuc_HienThi DEFAULT 1,
    IsDeleted bit NOT NULL CONSTRAINT DF_DanhMuc_IsDeleted DEFAULT 0,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_DanhMuc_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_DanhMuc PRIMARY KEY (Id),
    CONSTRAINT CK_DanhMuc_Ten CHECK (LEN(LTRIM(RTRIM(Ten))) > 0)
);
GO

-- 5. BẢNG SẢN PHẨM (UC-17 đến UC-23)
CREATE TABLE dbo.SanPham (
    Id int IDENTITY(1,1) NOT NULL,
    DanhMucId int NOT NULL,
    MaSanPham varchar(50) NOT NULL,
    Ten nvarchar(200) NOT NULL,
    MoTa nvarchar(2000) NOT NULL CONSTRAINT DF_SanPham_MoTa DEFAULT N'',
    HinhAnh nvarchar(500) NULL,
    GiaBan decimal(18,2) NOT NULL,
    HienThi bit NOT NULL CONSTRAINT DF_SanPham_HienThi DEFAULT 1,
    IsDeleted bit NOT NULL CONSTRAINT DF_SanPham_IsDeleted DEFAULT 0,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_SanPham_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_SanPham PRIMARY KEY (Id),
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (DanhMucId) REFERENCES dbo.DanhMuc(Id),
    CONSTRAINT CK_SanPham_MaSanPham CHECK (LEN(LTRIM(RTRIM(MaSanPham))) > 0),
    CONSTRAINT CK_SanPham_Ten CHECK (LEN(LTRIM(RTRIM(Ten))) > 0),
    CONSTRAINT CK_SanPham_GiaBan CHECK (GiaBan > 0)
);
GO

-- 6. BẢNG TỒN KHO (UC-20, UC-24 đến UC-27)
CREATE TABLE dbo.TonKho (
    Id int IDENTITY(1,1) NOT NULL,
    SanPhamId int NOT NULL,
    SoLuongTon int NOT NULL CONSTRAINT DF_TonKho_SoLuongTon DEFAULT 0,
    NguongToiThieu int NOT NULL CONSTRAINT DF_TonKho_NguongToiThieu DEFAULT 0,
    CONSTRAINT PK_TonKho PRIMARY KEY (Id),
    CONSTRAINT UQ_TonKho_SanPham UNIQUE (SanPhamId),
    CONSTRAINT FK_TonKho_SanPham FOREIGN KEY (SanPhamId) REFERENCES dbo.SanPham(Id),
    CONSTRAINT CK_TonKho_SoLuongTon CHECK (SoLuongTon >= 0),
    CONSTRAINT CK_TonKho_NguongToiThieu CHECK (NguongToiThieu >= 0)
);
GO

-- 7. BẢNG BIẾN ĐỘNG KHO (UC-25, UC-26, UC-28, UC-37, UC-41)
CREATE TABLE dbo.BienDongKho (
    Id int IDENTITY(1,1) NOT NULL,
    TonKhoId int NOT NULL,
    TaiKhoanId int NULL,
    DonHangId int NULL,
    Loai varchar(30) NOT NULL,
    SoLuongThayDoi int NOT NULL,
    SoLuongTruoc int NOT NULL,
    SoLuongSau int NOT NULL,
    GiaNhap decimal(18,2) NULL,
    NhaCungCap nvarchar(200) NULL,
    LyDo nvarchar(500) NOT NULL,
    ThoiDiem datetimeoffset(0) NOT NULL CONSTRAINT DF_BienDongKho_ThoiDiem DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_BienDongKho PRIMARY KEY (Id),
    CONSTRAINT FK_BienDongKho_TonKho FOREIGN KEY (TonKhoId) REFERENCES dbo.TonKho(Id),
    CONSTRAINT FK_BienDongKho_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_BienDongKho_Loai CHECK (Loai IN ('NhapTang','DieuChinhGiam','XuatBan','HoanDon')),
    CONSTRAINT CK_BienDongKho_SoLuongThayDoi CHECK (SoLuongThayDoi > 0),
    CONSTRAINT CK_BienDongKho_SoLuongTruoc CHECK (SoLuongTruoc >= 0),
    CONSTRAINT CK_BienDongKho_SoLuongSau CHECK (SoLuongSau >= 0),
    CONSTRAINT CK_BienDongKho_GiaNhap CHECK (GiaNhap IS NULL OR GiaNhap >= 0),
    CONSTRAINT CK_BienDongKho_LyDo CHECK (LEN(LTRIM(RTRIM(LyDo))) > 0)
);
GO

-- 8. BẢNG GIỎ HÀNG (UC-29 đến UC-32)
CREATE TABLE dbo.GioHang (
    Id int IDENTITY(1,1) NOT NULL,
    TaiKhoanId int NOT NULL,
    CONSTRAINT PK_GioHang PRIMARY KEY (Id),
    CONSTRAINT UQ_GioHang_TaiKhoan UNIQUE (TaiKhoanId),
    CONSTRAINT FK_GioHang_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id)
);
GO

-- 9. BẢNG CHI TIẾT GIỎ HÀNG (UC-29 đến UC-32)
CREATE TABLE dbo.ChiTietGioHang (
    Id int IDENTITY(1,1) NOT NULL,
    GioHangId int NOT NULL,
    SanPhamId int NOT NULL,
    SoLuong int NOT NULL,
    DonGiaLucThem decimal(18,2) NOT NULL,
    CONSTRAINT PK_ChiTietGioHang PRIMARY KEY (Id),
    CONSTRAINT UQ_ChiTietGioHang_GioHang_SanPham UNIQUE (GioHangId, SanPhamId),
    CONSTRAINT FK_ChiTietGioHang_GioHang FOREIGN KEY (GioHangId) REFERENCES dbo.GioHang(Id),
    CONSTRAINT FK_ChiTietGioHang_SanPham FOREIGN KEY (SanPhamId) REFERENCES dbo.SanPham(Id),
    CONSTRAINT CK_ChiTietGioHang_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_ChiTietGioHang_DonGiaLucThem CHECK (DonGiaLucThem > 0)
);
GO

-- 10. BẢNG YÊU THÍCH SẢN PHẨM (UC-33)
CREATE TABLE dbo.YeuThichSanPham (
    TaiKhoanId int NOT NULL,
    SanPhamId int NOT NULL,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_YeuThichSanPham_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_YeuThichSanPham PRIMARY KEY (TaiKhoanId, SanPhamId),
    CONSTRAINT FK_YeuThichSanPham_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_YeuThichSanPham_SanPham FOREIGN KEY (SanPhamId) REFERENCES dbo.SanPham(Id)
);
GO

-- 11. BẢNG MÃ GIẢM GIÁ (13 cột - UC-35, UC-45 đến UC-50)
CREATE TABLE dbo.MaGiamGia (
    Id int IDENTITY(1,1) NOT NULL,
    Ma varchar(50) NOT NULL,
    MoTa nvarchar(500) NULL,
    Loai varchar(20) NOT NULL,
    GiaTri decimal(18,2) NOT NULL,
    GiamToiDa decimal(18,2) NULL,
    DonToiThieu decimal(18,2) NOT NULL CONSTRAINT DF_MaGiamGia_DonToiThieu DEFAULT 0,
    NgayBatDau datetimeoffset(0) NOT NULL,
    NgayKetThuc datetimeoffset(0) NOT NULL,
    SoLuotToiDa int NOT NULL,
    SoLuotDaDung int NOT NULL CONSTRAINT DF_MaGiamGia_SoLuotDaDung DEFAULT 0,
    HienThi bit NOT NULL CONSTRAINT DF_MaGiamGia_HienThi DEFAULT 1,
    IsDeleted bit NOT NULL CONSTRAINT DF_MaGiamGia_IsDeleted DEFAULT 0,
    CONSTRAINT PK_MaGiamGia PRIMARY KEY (Id),
    CONSTRAINT CK_MaGiamGia_Loai CHECK (Loai IN ('SoTien','PhanTram')),
    CONSTRAINT CK_MaGiamGia_GiaTri CHECK (
        GiaTri > 0 AND (Loai <> 'PhanTram' OR GiaTri <= 100)
    ),
    CONSTRAINT CK_MaGiamGia_GiamToiDa CHECK (GiamToiDa IS NULL OR GiamToiDa > 0),
    CONSTRAINT CK_MaGiamGia_DonToiThieu CHECK (DonToiThieu >= 0),
    CONSTRAINT CK_MaGiamGia_ThoiGian CHECK (NgayKetThuc > NgayBatDau),
    CONSTRAINT CK_MaGiamGia_SoLuot CHECK (SoLuotToiDa > 0 AND SoLuotDaDung >= 0 AND SoLuotDaDung <= SoLuotToiDa)
);
GO

-- 12. BẢNG ĐƠN HÀNG (UC-34 đến UC-44)
CREATE TABLE dbo.DonHang (
    Id int IDENTITY(1,1) NOT NULL,
    TaiKhoanId int NOT NULL,
    MaGiamGiaId int NULL,
    MaDon varchar(50) NOT NULL,
    NgayDat datetimeoffset(0) NOT NULL CONSTRAINT DF_DonHang_NgayDat DEFAULT SYSDATETIMEOFFSET(),
    TrangThai varchar(30) NOT NULL CONSTRAINT DF_DonHang_TrangThai DEFAULT 'ChoXacNhan',
    PhuongThucThanhToan varchar(30) NOT NULL,
    DaThanhToan bit NOT NULL CONSTRAINT DF_DonHang_DaThanhToan DEFAULT 0,
    TenNguoiNhan nvarchar(100) NOT NULL,
    SoDienThoaiNhan varchar(20) NOT NULL,
    DiaChiGiaoHang nvarchar(500) NOT NULL,
    TamTinh decimal(18,2) NOT NULL,
    TienGiam decimal(18,2) NOT NULL CONSTRAINT DF_DonHang_TienGiam DEFAULT 0,
    TongThanhToan AS CONVERT(decimal(18,2), TamTinh - TienGiam),
    LyDoHuy nvarchar(500) NULL,
    CONSTRAINT PK_DonHang PRIMARY KEY (Id),
    CONSTRAINT UQ_DonHang_MaDon UNIQUE (MaDon),
    CONSTRAINT FK_DonHang_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_DonHang_MaGiamGia FOREIGN KEY (MaGiamGiaId) REFERENCES dbo.MaGiamGia(Id),
    CONSTRAINT CK_DonHang_TrangThai CHECK (TrangThai IN ('ChoXacNhan','DaXacNhan','DangChuanBi','DangGiao','HoanThanh','DaHuy')),
    CONSTRAINT CK_DonHang_TamTinh CHECK (TamTinh >= 0),
    CONSTRAINT CK_DonHang_TienGiam CHECK (TienGiam >= 0 AND TienGiam <= TamTinh),
    CONSTRAINT CK_DonHang_PhuongThuc CHECK (PhuongThucThanhToan IN ('KhiNhanHang','ChuyenKhoan','ViDienTu')),
    CONSTRAINT CK_DonHang_TenNguoiNhan CHECK (LEN(LTRIM(RTRIM(TenNguoiNhan))) > 0),
    CONSTRAINT CK_DonHang_SoDienThoaiNhan CHECK (LEN(LTRIM(RTRIM(SoDienThoaiNhan))) > 0),
    CONSTRAINT CK_DonHang_DiaChiGiaoHang CHECK (LEN(LTRIM(RTRIM(DiaChiGiaoHang))) > 0)
);
GO

-- 13. BẢNG CHI TIẾT ĐƠN HÀNG (UC-37, UC-39, UC-60)
CREATE TABLE dbo.ChiTietDonHang (
    Id int IDENTITY(1,1) NOT NULL,
    DonHangId int NOT NULL,
    SanPhamId int NOT NULL,
    DonGia decimal(18,2) NOT NULL,
    SoLuong int NOT NULL,
    ThanhTien AS CONVERT(decimal(18,2), DonGia * SoLuong),
    CONSTRAINT PK_ChiTietDonHang PRIMARY KEY (Id),
    CONSTRAINT UQ_ChiTietDonHang_DonHang_SanPham UNIQUE (DonHangId, SanPhamId),
    CONSTRAINT FK_ChiTietDonHang_DonHang FOREIGN KEY (DonHangId) REFERENCES dbo.DonHang(Id),
    CONSTRAINT FK_ChiTietDonHang_SanPham FOREIGN KEY (SanPhamId) REFERENCES dbo.SanPham(Id),
    CONSTRAINT CK_ChiTietDonHang_DonGia CHECK (DonGia > 0),
    CONSTRAINT CK_ChiTietDonHang_SoLuong CHECK (SoLuong > 0)
);
GO

-- 14. BẢNG LỊCH SỬ ĐƠN HÀNG (UC-37, UC-41 đến UC-44)
CREATE TABLE dbo.LichSuDonHang (
    Id int IDENTITY(1,1) NOT NULL,
    DonHangId int NOT NULL,
    TaiKhoanId int NULL,
    TrangThai varchar(30) NOT NULL,
    ThoiDiem datetimeoffset(0) NOT NULL CONSTRAINT DF_LichSuDonHang_ThoiDiem DEFAULT SYSDATETIMEOFFSET(),
    GhiChu nvarchar(500) NULL,
    CONSTRAINT PK_LichSuDonHang PRIMARY KEY (Id),
    CONSTRAINT FK_LichSuDonHang_DonHang FOREIGN KEY (DonHangId) REFERENCES dbo.DonHang(Id),
    CONSTRAINT FK_LichSuDonHang_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_LichSuDonHang_TrangThai CHECK (TrangThai IN ('ChoXacNhan','DaXacNhan','DangChuanBi','DangGiao','HoanThanh','DaHuy'))
);
GO

-- 15. BẢNG ĐÁNH GIÁ SẢN PHẨM (UC-51 đến UC-56)
CREATE TABLE dbo.DanhGia (
    Id int IDENTITY(1,1) NOT NULL,
    TaiKhoanId int NOT NULL,
    SanPhamId int NOT NULL,
    DonHangId int NOT NULL,
    SoSao int NOT NULL,
    NoiDung nvarchar(2000) NOT NULL,
    HinhAnh nvarchar(500) NULL,
    DaChinhSua bit NOT NULL CONSTRAINT DF_DanhGia_DaChinhSua DEFAULT 0,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_DanhGia_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    NgayCapNhat datetimeoffset(0) NULL,
    LyDoXoa nvarchar(500) NULL,
    IsDeleted bit NOT NULL CONSTRAINT DF_DanhGia_IsDeleted DEFAULT 0,
    CONSTRAINT PK_DanhGia PRIMARY KEY (Id),
    CONSTRAINT FK_DanhGia_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_DanhGia_SanPham FOREIGN KEY (SanPhamId) REFERENCES dbo.SanPham(Id),
    CONSTRAINT FK_DanhGia_DonHang FOREIGN KEY (DonHangId) REFERENCES dbo.DonHang(Id),
    CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5),
    CONSTRAINT CK_DanhGia_NoiDung CHECK (LEN(LTRIM(RTRIM(NoiDung))) > 0)
);
GO

-- 16. BẢNG PHẢN HỒI ĐÁNH GIÁ (UC-51, UC-57)
CREATE TABLE dbo.PhanHoiDanhGia (
    Id int IDENTITY(1,1) NOT NULL,
    DanhGiaId int NOT NULL,
    TaiKhoanId int NOT NULL,
    NoiDung nvarchar(2000) NOT NULL,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_PhanHoiDanhGia_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    NgayCapNhat datetimeoffset(0) NULL,
    CONSTRAINT PK_PhanHoiDanhGia PRIMARY KEY (Id),
    CONSTRAINT UQ_PhanHoiDanhGia_DanhGia UNIQUE (DanhGiaId),
    CONSTRAINT FK_PhanHoiDanhGia_DanhGia FOREIGN KEY (DanhGiaId) REFERENCES dbo.DanhGia(Id),
    CONSTRAINT FK_PhanHoiDanhGia_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT CK_PhanHoiDanhGia_NoiDung CHECK (LEN(LTRIM(RTRIM(NoiDung))) > 0)
);
GO

-- 17. BẢNG THÍCH ĐÁNH GIÁ (UC-55)
CREATE TABLE dbo.ThichDanhGia (
    TaiKhoanId int NOT NULL,
    DanhGiaId int NOT NULL,
    NgayTao datetimeoffset(0) NOT NULL CONSTRAINT DF_ThichDanhGia_NgayTao DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_ThichDanhGia PRIMARY KEY (TaiKhoanId, DanhGiaId),
    CONSTRAINT FK_ThichDanhGia_TaiKhoan FOREIGN KEY (TaiKhoanId) REFERENCES dbo.TaiKhoan(Id),
    CONSTRAINT FK_ThichDanhGia_DanhGia FOREIGN KEY (DanhGiaId) REFERENCES dbo.DanhGia(Id)
);
GO

ALTER TABLE dbo.BienDongKho
ADD CONSTRAINT FK_BienDongKho_DonHang
FOREIGN KEY (DonHangId) REFERENCES dbo.DonHang(Id);
GO

-- =============================================================================
-- PHẦN 2: FILTERED UNIQUE INDEXES, INDEXES, FUNCTION & VIEW
-- =============================================================================

CREATE UNIQUE INDEX UQ_DiaChiNhanHang_MacDinh ON dbo.DiaChiNhanHang(TaiKhoanId) WHERE MacDinh = 1;
CREATE UNIQUE INDEX UQ_DanhMuc_Ten ON dbo.DanhMuc(Ten) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX UQ_SanPham_MaSanPham ON dbo.SanPham(MaSanPham) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX UQ_MaGiamGia_Ma ON dbo.MaGiamGia(Ma) WHERE IsDeleted = 0;
CREATE UNIQUE INDEX UQ_DanhGia_DonHang_SanPham ON dbo.DanhGia(DonHangId, SanPhamId) WHERE IsDeleted = 0;
GO

CREATE INDEX IX_TaiKhoan_VaiTro_TrangThai ON dbo.TaiKhoan(VaiTro, TrangThai, NgayTao DESC);
CREATE INDEX IX_LichSuKhoaTaiKhoan_TaiKhoan_ThoiDiem ON dbo.LichSuKhoaTaiKhoan(TaiKhoanId, ThoiDiem DESC);
CREATE INDEX IX_DiaChiNhanHang_TaiKhoan ON dbo.DiaChiNhanHang(TaiKhoanId, MacDinh DESC);
CREATE INDEX IX_SanPham_DanhMuc_HienThi ON dbo.SanPham(DanhMucId, HienThi, IsDeleted) INCLUDE (MaSanPham, Ten, GiaBan, HinhAnh);
CREATE INDEX IX_BienDongKho_TonKho_ThoiDiem ON dbo.BienDongKho(TonKhoId, ThoiDiem DESC);
CREATE INDEX IX_BienDongKho_TaiKhoan ON dbo.BienDongKho(TaiKhoanId) WHERE TaiKhoanId IS NOT NULL;
CREATE INDEX IX_BienDongKho_DonHang ON dbo.BienDongKho(DonHangId) WHERE DonHangId IS NOT NULL;
CREATE INDEX IX_ChiTietGioHang_SanPham ON dbo.ChiTietGioHang(SanPhamId);
CREATE INDEX IX_YeuThichSanPham_SanPham ON dbo.YeuThichSanPham(SanPhamId, TaiKhoanId);
CREATE INDEX IX_MaGiamGia_HieuLuc ON dbo.MaGiamGia(HienThi, IsDeleted, NgayBatDau, NgayKetThuc);
CREATE INDEX IX_DonHang_TaiKhoan_NgayDat ON dbo.DonHang(TaiKhoanId, NgayDat DESC);
CREATE INDEX IX_DonHang_TrangThai_NgayDat ON dbo.DonHang(TrangThai, NgayDat DESC);
CREATE INDEX IX_DonHang_MaGiamGia ON dbo.DonHang(MaGiamGiaId) WHERE MaGiamGiaId IS NOT NULL;
CREATE INDEX IX_ChiTietDonHang_SanPham ON dbo.ChiTietDonHang(SanPhamId);
CREATE INDEX IX_LichSuDonHang_DonHang_ThoiDiem ON dbo.LichSuDonHang(DonHangId, ThoiDiem);
CREATE INDEX IX_DanhGia_SanPham_NgayTao ON dbo.DanhGia(SanPhamId, IsDeleted, NgayTao DESC) INCLUDE (SoSao, HinhAnh);
CREATE INDEX IX_DanhGia_TaiKhoan ON dbo.DanhGia(TaiKhoanId);
CREATE INDEX IX_PhanHoiDanhGia_TaiKhoan ON dbo.PhanHoiDanhGia(TaiKhoanId);
CREATE INDEX IX_ThichDanhGia_DanhGia ON dbo.ThichDanhGia(DanhGiaId, TaiKhoanId);
GO

CREATE FUNCTION dbo.fn_BaoCaoTongQuan (
    @TuNgay datetimeoffset(0) = NULL,
    @DenNgay datetimeoffset(0) = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        (SELECT COALESCE(SUM(TongThanhToan), CONVERT(decimal(18,2), 0))
           FROM dbo.DonHang
          WHERE TrangThai = 'HoanThanh'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS TongDoanhThu,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS TongDonHang,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE TrangThai = 'ChoXacNhan'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS DonChoXacNhan,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE TrangThai = 'DaXacNhan'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS DonDaXacNhan,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE TrangThai = 'DangChuanBi'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS DonDangChuanBi,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE TrangThai = 'DangGiao'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS DonDangGiao,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE TrangThai = 'HoanThanh'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS DonHoanThanh,
        (SELECT COUNT_BIG(*)
           FROM dbo.DonHang
          WHERE TrangThai = 'DaHuy'
            AND (@TuNgay IS NULL OR NgayDat >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayDat <= @DenNgay)) AS DonDaHuy,
        (SELECT COUNT_BIG(*)
           FROM dbo.TaiKhoan
          WHERE VaiTro = 'KhachHang'
            AND (@TuNgay IS NULL OR NgayTao >= @TuNgay)
            AND (@DenNgay IS NULL OR NgayTao <= @DenNgay)) AS TongKhachHangMoi,
        (SELECT COUNT_BIG(*)
           FROM dbo.TonKho tk
           JOIN dbo.SanPham sp ON sp.Id = tk.SanPhamId
          WHERE sp.IsDeleted = 0 AND tk.SoLuongTon <= tk.NguongToiThieu) AS SoSanPhamSapHetHang
);
GO

CREATE VIEW dbo.vw_BaoCaoTongQuan
AS
SELECT *
FROM dbo.fn_BaoCaoTongQuan(DATEADD(DAY, -30, SYSDATETIMEOFFSET()), SYSDATETIMEOFFSET());
GO

-- =============================================================================
-- PHẦN 3: THÊM DỮ LIỆU MẪU ĐẦY ĐỦ CHO TẤT CẢ 17 BẢNG (ID TỰ TĂNG 1, 2, 3...)
-- =============================================================================
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Now datetimeoffset(0) = SYSDATETIMEOFFSET();

    -- 1. TÀI KHOẢN (Id tự tăng: 1=QuanLy, 2-3=NhanVien, 4-6=KhachHang, 7=BiKhoa)
    INSERT INTO dbo.TaiKhoan (HoTen, Email, SoDienThoai, MatKhauHash, VaiTro, TrangThai, NgaySinh, GioiTinh, AnhDaiDien, CauHoiBaoMat, CauTraLoiHash, LyDoKhoa, SoLanDangNhapSai, KhoaTamDen, NgayTao)
    VALUES
    (N'Nguyễn Minh Quân',  'admin@techshop.vn',       '0901000001', N'HASH_001', 'QuanLy',    'HoatDong', '1990-05-15', N'Nam',  N'/images/avatars/avatar-admin.jpg',       N'Tên trường cấp 3 của bạn?', N'ANS_001', NULL, 0, NULL, DATEADD(DAY, -90, @Now)),
    (N'Trần Thị Lan Anh',  'lananh.nv@techshop.vn',   '0901000002', N'HASH_002', 'NhanVien',  'HoatDong', '1996-08-20', N'Nu',   N'/images/avatars/avatar-nv-lananh.jpg',   N'Tên thú cưng đầu tiên?',    N'ANS_002', NULL, 0, NULL, DATEADD(DAY, -60, @Now)),
    (N'Lê Hoàng Nam',      'hoangnam.nv@techshop.vn', '0901000003', N'HASH_003', 'NhanVien',  'HoatDong', '1998-02-10', N'Nam',  N'/images/avatars/avatar-nv-hoangnam.jpg', NULL, NULL, NULL, 0, NULL, DATEADD(DAY, -45, @Now)),
    (N'Phạm Đức Huy',      'duchuy.pham@gmail.com',   '0912345671', N'HASH_004', 'KhachHang', 'HoatDong', '1999-11-12', N'Nam',  N'/images/avatars/avatar-kh-duchuy.jpg',   N'Món ăn yêu thích?',         N'ANS_004', NULL, 0, NULL, DATEADD(DAY, -25, @Now)),
    (N'Vũ Thu Trang',      'thutrang.vu@gmail.com',   '0912345672', N'HASH_005', 'KhachHang', 'HoatDong', '2001-04-05', N'Nu',   N'/images/avatars/avatar-kh-thutrang.jpg', NULL, NULL, NULL, 1, NULL, DATEADD(DAY, -18, @Now)),
    (N'Đặng Quốc Bảo',     'quocbao.dang@gmail.com',  '0912345673', N'HASH_006', 'KhachHang', 'HoatDong', '1995-09-28', N'Khac', N'/images/avatars/avatar-kh-quocbao.jpg',  NULL, NULL, NULL, 0, NULL, DATEADD(DAY, -5,  @Now)),
    (N'Hoàng Văn Spam',    'spam.user@gmail.com',     '0912345679', N'HASH_007', 'KhachHang', 'BiKhoa',   '2000-01-01', N'Nam',  NULL,                                      NULL, NULL, N'Đặt đơn ảo nhiều lần không nhận hàng', 5, DATEADD(HOUR, 2, @Now), DATEADD(DAY, -20, @Now));

    -- 2. LỊCH SỬ KHÓA TÀI KHOẢN
    INSERT INTO dbo.LichSuKhoaTaiKhoan (TaiKhoanId, QuanLyId, TrangThaiMoi, LyDo, ThoiDiem)
    VALUES
    (7, 1, 'BiKhoa',   N'Khóa lần 1 do nghi vấn spam đánh giá',       DATEADD(DAY, -15, @Now)),
    (7, 1, 'HoatDong', N'Khách hàng cam kết không tái phạm, mở khóa', DATEADD(DAY, -12, @Now)),
    (7, 1, 'BiKhoa',   N'Đặt đơn ảo nhiều lần không nhận hàng',       DATEADD(DAY, -3,  @Now));

    -- 3. ĐỊA CHỈ NHẬN HÀNG
    INSERT INTO dbo.DiaChiNhanHang (TaiKhoanId, TenNguoiNhan, SoDienThoai, TinhThanh, QuanHuyen, PhuongXa, DiaChiCuThe, MacDinh)
    VALUES
    (4, N'Phạm Đức Huy',      '0912345671', N'Hà Nội',          N'Quận Cầu Giấy',    N'Phường Dịch Vọng',   N'Số 15 Ngõ 68 Cầu Giấy',        1),
    (4, N'Phạm Đức Huy (VP)', '0912345671', N'Hà Nội',          N'Quận Nam Từ Liêm', N'Phường Mỹ Đình 2',   N'Tòa nhà Keangnam Landmark 72', 0),
    (5, N'Vũ Thu Trang',      '0912345672', N'TP. Hồ Chí Minh', N'Quận 1',           N'Phường Bến Nghé',    N'Số 42 Nguyễn Huệ',             1),
    (6, N'Đặng Quốc Bảo',     '0912345673', N'Đà Nẵng',         N'Quận Hải Châu',    N'Phường Thạch Thang', N'Số 108 Bạch Đằng',             1);

    -- 4. DANH MỤC (Id tự tăng: 1..4 đang bán, 5 đã xóa mềm)
    INSERT INTO dbo.DanhMuc (Ten, MoTa, HinhAnh, HienThi, IsDeleted, NgayTao)
    VALUES
    (N'Điện thoại thông minh', N'Smartphone chính hãng Apple, Samsung, Xiaomi', N'/images/categories/cat-dien-thoai.png', 1, 0, DATEADD(DAY, -80, @Now)),
    (N'Laptop & Máy tính',     N'Laptop văn phòng, lập trình và đồ họa',        N'/images/categories/cat-laptop.png',     1, 0, DATEADD(DAY, -80, @Now)),
    (N'Thiết bị âm thanh',     N'Tai nghe True Wireless, loa Bluetooth',        N'/images/categories/cat-am-thanh.png',   1, 0, DATEADD(DAY, -75, @Now)),
    (N'Phụ kiện công nghệ',    N'Chuột, bàn phím, củ sạc nhanh GaN',            N'/images/categories/cat-phu-kien.png',   1, 0, DATEADD(DAY, -75, @Now)),
    (N'Máy ảnh cũ (Ngừng KD)', N'Danh mục đã ngừng kinh doanh',                 NULL,                                     0, 1, DATEADD(DAY, -90, @Now));

    -- 5. SẢN PHẨM (Id tự tăng: 1..7 đang bán, 8 đã xóa mềm)
    INSERT INTO dbo.SanPham (DanhMucId, MaSanPham, Ten, MoTa, HinhAnh, GiaBan, HienThi, IsDeleted, NgayTao)
    VALUES
    (1, 'SP001', N'iPhone 16 Pro Max 256GB',          N'Chip A18 Pro, khung Titan, màn hình 6.9 inch 120Hz', N'/images/products/iphone-16-pro-max.jpg',     32990000, 1, 0, DATEADD(DAY, -60, @Now)),
    (1, 'SP002', N'Samsung Galaxy S25 Ultra 256GB',   N'Camera 200MP, bút S-Pen tích hợp, Galaxy AI',        N'/images/products/samsung-s25-ultra.jpg',     29990000, 1, 0, DATEADD(DAY, -55, @Now)),
    (2, 'SP003', N'MacBook Air 13 inch M3 16GB/512GB',N'Siêu mỏng nhẹ, pin 18 tiếng, màn hình Liquid Retina',N'/images/products/macbook-air-m3.jpg',        28500000, 1, 0, DATEADD(DAY, -50, @Now)),
    (2, 'SP004', N'Lenovo ThinkPad X1 Carbon Gen 12', N'Intel Core Ultra 7, RAM 16GB, chuẩn bền quân đội',   N'/images/products/thinkpad-x1-carbon.jpg',    35000000, 1, 0, DATEADD(DAY, -45, @Now)),
    (3, 'SP005', N'AirPods 4 chống ồn chủ động ANC',  N'Chip H2, chống ồn chủ động, hộp sạc USB-C',          N'/images/products/airpods-4-anc.png',          4500000, 1, 0, DATEADD(DAY, -40, @Now)),
    (4, 'SP006', N'Chuột Logitech MX Master 3S',      N'Cảm biến 8000 DPI, cuộn điện từ MagSpeed, click êm', N'/images/products/logitech-mx-master-3s.webp', 2400000, 1, 0, DATEADD(DAY, -35, @Now)),
    (4, 'SP007', N'Củ sạc nhanh Anker GaN 65W 3 cổng',N'Công nghệ GaN II nhỏ gọn, hỗ trợ PD 3.0 sạc laptop', N'/images/products/anker-gan-65w.jpg',           950000, 1, 0, DATEADD(DAY, -30, @Now)),
    (1, 'SP008', N'iPhone 11 64GB (Ngừng sản xuất)',  N'Sản phẩm đã ngừng kinh doanh',                       NULL,                                          10000000, 0, 1, DATEADD(DAY, -80, @Now));

    -- 6. TỒN KHO (Id tự tăng: 1..7; sản phẩm 4 và 7 sắp hết hàng)
    INSERT INTO dbo.TonKho (SanPhamId, SoLuongTon, NguongToiThieu)
    VALUES
    (1, 18, 5),
    (2, 14, 5),
    (3, 10, 3),
    (4,  2, 3),  -- Sắp hết hàng
    (5, 25, 8),
    (6, 30, 10),
    (7,  4, 10); -- Sắp hết hàng

    -- 7. MÃ GIẢM GIÁ (Id tự tăng: 1..4)
    INSERT INTO dbo.MaGiamGia (Ma, MoTa, Loai, GiaTri, GiamToiDa, DonToiThieu, NgayBatDau, NgayKetThuc, SoLuotToiDa, SoLuotDaDung, HienThi, IsDeleted)
    VALUES
    ('KM001', N'Giảm 10% tối đa 1.500.000đ cho đơn từ 5.000.000đ', 'PhanTram', 10,     1500000, 5000000,  DATEADD(DAY, -30, @Now), DATEADD(DAY, 30, @Now),  100, 2,   1, 0),
    ('KM002', N'Giảm trực tiếp 500.000đ cho đơn từ 10.000.000đ',   'SoTien',   500000, NULL,    10000000, DATEADD(DAY, -20, @Now), DATEADD(DAY, 40, @Now),  50,  1,   1, 0),
    ('KM003', N'Mã khuyến mãi sự kiện cũ đã hết hạn',              'SoTien',   200000, NULL,    2000000,  DATEADD(DAY, -90, @Now), DATEADD(DAY, -31, @Now), 200, 150, 0, 0),
    ('KM004', N'Mã test nội bộ đã xóa',                            'PhanTram', 50,     100000,  0,        DATEADD(DAY, -10, @Now), DATEADD(DAY, 10, @Now),  10,  0,   0, 1);

    -- 8. ĐƠN HÀNG (Id tự tăng: 1..7, đủ 6 trạng thái)
    INSERT INTO dbo.DonHang (TaiKhoanId, MaGiamGiaId, MaDon, NgayDat, TrangThai, PhuongThucThanhToan, DaThanhToan, TenNguoiNhan, SoDienThoaiNhan, DiaChiGiaoHang, TamTinh, TienGiam, LyDoHuy)
    VALUES
    (4, 1,    'DH001', DATEADD(DAY, -15, @Now), 'HoanThanh',   'ChuyenKhoan', 1, N'Phạm Đức Huy',  '0912345671', N'Số 15 Ngõ 68 Cầu Giấy, Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội',     37490000, 1500000, NULL),
    (5, 2,    'DH002', DATEADD(DAY, -10, @Now), 'HoanThanh',   'ViDienTu',    1, N'Vũ Thu Trang',  '0912345672', N'Số 42 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',         28500000,  500000, NULL),
    (4, NULL, 'DH003', DATEADD(DAY, -3,  @Now), 'DangGiao',    'KhiNhanHang', 0, N'Phạm Đức Huy',  '0912345671', N'Tòa nhà Keangnam Landmark 72, Phường Mỹ Đình 2, Nam Từ Liêm, Hà Nội', 3350000,       0, NULL),
    (6, 1,    'DH004', DATEADD(DAY, -2,  @Now), 'DangChuanBi', 'ChuyenKhoan', 1, N'Đặng Quốc Bảo', '0912345673', N'Số 108 Bạch Đằng, Phường Thạch Thang, Quận Hải Châu, Đà Nẵng',       29990000, 1500000, NULL),
    (5, NULL, 'DH005', DATEADD(DAY, -1,  @Now), 'DaXacNhan',   'KhiNhanHang', 0, N'Vũ Thu Trang',  '0912345672', N'Số 42 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',          9000000,       0, NULL),
    (6, NULL, 'DH006', DATEADD(HOUR, -3, @Now), 'ChoXacNhan',  'ViDienTu',    1, N'Đặng Quốc Bảo', '0912345673', N'Số 108 Bạch Đằng, Phường Thạch Thang, Quận Hải Châu, Đà Nẵng',       35000000,       0, NULL),
    (4, NULL, 'DH007', DATEADD(DAY, -8,  @Now), 'DaHuy',       'KhiNhanHang', 0, N'Phạm Đức Huy',  '0912345671', N'Số 15 Ngõ 68 Cầu Giấy, Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội',     32990000,       0, N'Khách hàng muốn đổi sang phiên bản dung lượng 512GB');

    -- 9. CHI TIẾT ĐƠN HÀNG
    INSERT INTO dbo.ChiTietDonHang (DonHangId, SanPhamId, DonGia, SoLuong)
    VALUES
    (1, 1, 32990000, 1),
    (1, 5,  4500000, 1),
    (2, 3, 28500000, 1),
    (3, 6,  2400000, 1),
    (3, 7,   950000, 1),
    (4, 2, 29990000, 1),
    (5, 5,  4500000, 2),
    (6, 4, 35000000, 1),
    (7, 1, 32990000, 1);

    -- 10. LỊCH SỬ ĐƠN HÀNG
    INSERT INTO dbo.LichSuDonHang (DonHangId, TaiKhoanId, TrangThai, ThoiDiem, GhiChu)
    VALUES
    (1, 4, 'ChoXacNhan',  DATEADD(DAY, -15, @Now), N'Khách hàng tạo đơn hàng'),
    (1, 2, 'DaXacNhan',   DATEADD(HOUR, 2, DATEADD(DAY, -15, @Now)), N'Đã xác nhận thanh toán chuyển khoản'),
    (1, 3, 'DangChuanBi', DATEADD(HOUR, 5, DATEADD(DAY, -15, @Now)), N'Đang đóng gói sản phẩm và xuất hóa đơn VAT'),
    (1, 3, 'DangGiao',    DATEADD(DAY, -14, @Now), N'Đã bàn giao cho đơn vị vận chuyển GHN'),
    (1, 2, 'HoanThanh',   DATEADD(DAY, -12, @Now), N'Giao hàng thành công'),
    (2, 5, 'ChoXacNhan',  DATEADD(DAY, -10, @Now), N'Khách hàng đặt hàng qua ví điện tử'),
    (2, 2, 'HoanThanh',   DATEADD(DAY, -7,  @Now), N'Khách đã nhận hàng tại Quận 1'),
    (3, 4, 'ChoXacNhan',  DATEADD(DAY, -3,  @Now), N'Đơn hàng COD mới'),
    (3, 2, 'DangGiao',    DATEADD(DAY, -1,  @Now), N'Shipper đang trên đường giao'),
    (7, 4, 'ChoXacNhan',  DATEADD(DAY, -8,  @Now), N'Khách hàng tạo đơn hàng'),
    (7, 4, 'DaHuy',       DATEADD(HOUR, 1, DATEADD(DAY, -8, @Now)), N'Khách tự hủy đơn trước khi xác nhận');

    -- 11. BIẾN ĐỘNG KHO (Đủ 4 loại: NhapTang, XuatBan, HoanDon, DieuChinhGiam)
    INSERT INTO dbo.BienDongKho (TonKhoId, TaiKhoanId, DonHangId, Loai, SoLuongThayDoi, SoLuongTruoc, SoLuongSau, GiaNhap, NhaCungCap, LyDo, ThoiDiem)
    VALUES
    (1, 1, NULL, 'NhapTang',      20,  0, 20, 28500000, N'FPT Synnex',     N'Nhập lô hàng iPhone 16 Pro Max đợt 1',  DATEADD(DAY, -50, @Now)),
    (3, 1, NULL, 'NhapTang',      11,  0, 11, 24500000, N'Digiworld',      N'Nhập lô MacBook Air M3 chính hãng',     DATEADD(DAY, -45, @Now)),
    (7, 3, NULL, 'NhapTang',       6,  0,  6,   650000, N'Anker Việt Nam', N'Nhập phụ kiện sạc nhanh GaN',           DATEADD(DAY, -25, @Now)),
    (1, 3, 1,    'XuatBan',        1, 20, 19, NULL,     NULL,              N'Xuất kho cho đơn hàng DH001',           DATEADD(DAY, -15, @Now)),
    (3, 3, 2,    'XuatBan',        1, 11, 10, NULL,     NULL,              N'Xuất kho cho đơn hàng DH002',           DATEADD(DAY, -10, @Now)),
    (1, 3, 7,    'XuatBan',        1, 19, 18, NULL,     NULL,              N'Giữ hàng xuất kho cho đơn DH007',       DATEADD(DAY, -8,  @Now)),
    (1, 2, 7,    'HoanDon',        1, 18, 19, NULL,     NULL,              N'Hoàn tồn kho do khách hủy đơn DH007',   DATEADD(HOUR, 1, DATEADD(DAY, -8, @Now))),
    (1, 1, NULL, 'DieuChinhGiam',  1, 19, 18, NULL,     NULL,              N'Xuất chuyển bảo hành do rách seal',     DATEADD(DAY, -5,  @Now)),
    (7, 3, 3,    'XuatBan',        1,  6,  5, NULL,     NULL,              N'Xuất kho cho đơn hàng DH003',           DATEADD(DAY, -3,  @Now)),
    (7, 1, NULL, 'DieuChinhGiam',  1,  5,  4, NULL,     NULL,              N'Hàng mẫu trưng bày tại showroom',       DATEADD(DAY, -2,  @Now));

    -- 12. GIỎ HÀNG (Id tự tăng: 1..3)
    INSERT INTO dbo.GioHang (TaiKhoanId)
    VALUES (4), (5), (6);

    -- 13. CHI TIẾT GIỎ HÀNG
    INSERT INTO dbo.ChiTietGioHang (GioHangId, SanPhamId, SoLuong, DonGiaLucThem)
    VALUES
    (1, 3, 1, 28500000),
    (1, 7, 2,   950000),
    (2, 6, 1,  2400000),
    (3, 5, 1,  4500000);

    -- 14. YÊU THÍCH SẢN PHẨM
    INSERT INTO dbo.YeuThichSanPham (TaiKhoanId, SanPhamId, NgayTao)
    VALUES
    (4, 3, DATEADD(DAY, -10, @Now)),
    (4, 4, DATEADD(DAY, -5,  @Now)),
    (5, 1, DATEADD(DAY, -8,  @Now)),
    (6, 2, DATEADD(DAY, -2,  @Now));

    -- 15. ĐÁNH GIÁ SẢN PHẨM (Id tự tăng: 1..3)
    INSERT INTO dbo.DanhGia (TaiKhoanId, SanPhamId, DonHangId, SoSao, NoiDung, HinhAnh, DaChinhSua, NgayTao, NgayCapNhat, LyDoXoa, IsDeleted)
    VALUES
    (4, 1, 1, 5, N'Máy nguyên seal chính hãng VN/A, giao hàng cực nhanh, đóng gói chống sốc rất kỹ.', N'/images/reviews/review-iphone-16.jpg',  0, DATEADD(DAY, -11, @Now), NULL, NULL, 0),
    (4, 5, 1, 4, N'Chống ồn tốt hơn đời cũ nhiều, đeo thoải mái. Trừ 1 sao vì hộp hơi dễ xước.',   NULL,                                     1, DATEADD(DAY, -11, @Now), DATEADD(DAY, -10, @Now), NULL, 0),
    (5, 3, 2, 5, N'Máy nhẹ, pin trâu dùng cả ngày không cần sạc. Nhân viên tư vấn nhiệt tình!',     N'/images/reviews/review-macbook-m3.jpg', 0, DATEADD(DAY, -6,  @Now), NULL, NULL, 0);

    -- 16. PHẢN HỒI ĐÁNH GIÁ
    INSERT INTO dbo.PhanHoiDanhGia (DanhGiaId, TaiKhoanId, NoiDung, NgayTao)
    VALUES
    (1, 2, N'Cảm ơn anh Huy đã tin tưởng ủng hộ TechShop! Chúc anh có trải nghiệm tuyệt vời cùng iPhone 16 Pro Max ạ.', DATEADD(DAY, -10, @Now)),
    (3, 2, N'Cảm ơn chị Trang rất nhiều! Nếu cần hỗ trợ cài đặt phần mềm trên MacBook, chị cứ nhắn shop bất cứ lúc nào nhé.', DATEADD(DAY, -5, @Now));

    -- 17. THÍCH ĐÁNH GIÁ
    INSERT INTO dbo.ThichDanhGia (TaiKhoanId, DanhGiaId, NgayTao)
    VALUES
    (5, 1, DATEADD(DAY, -9, @Now)),
    (6, 1, DATEADD(DAY, -4, @Now)),
    (4, 3, DATEADD(DAY, -5, @Now));

    COMMIT TRANSACTION;
    PRINT N'Tạo CSDL QuanLyBanHang (INT IDENTITY) và thêm đầy đủ dữ liệu mẫu thành công!';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- Kiểm tra nhanh kết quả báo cáo tổng quan
SELECT * FROM dbo.vw_BaoCaoTongQuan;
GO