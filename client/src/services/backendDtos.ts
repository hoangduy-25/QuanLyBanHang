import {
  AccountStatus,
  DiscountType,
  Gender,
  OrderStatus,
  PaymentMethod,
  StockMovementType,
  UserRole,
} from '../types/domain';

// ============================================================================
// Common DTOs
// ============================================================================
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: unknown;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

// ============================================================================
// Xác thực (UC-01..05)
// ============================================================================
export interface DangKyRequestDto {
  hoTen: string;
  email: string;
  soDienThoai: string;
  matKhau: string;
  xacNhanMatKhau: string;
  cauHoiBaoMat: string;
  cauTraLoiBaoMat: string;
}

export interface DangNhapRequestDto {
  taiKhoan: string;
  matKhau: string;
}

export interface DangNhapResponseDto {
  accessToken: string;
  expiresAt: string;
  taiKhoanId: number;
  hoTen: string;
  email: string;
  vaiTro: UserRole;
  redirectUrl: string;
}

export interface CauHoiBaoMatResponseDto {
  emailOrPhone: string;
  cauHoiBaoMat: string;
}

export interface QuenMatKhauRequestDto {
  taiKhoan: string;
  cauTraLoiBaoMat: string;
  matKhauMoi: string;
  xacNhanMatKhauMoi: string;
}

export interface DoiMatKhauRequestDto {
  matKhauCu: string;
  matKhauMoi: string;
  xacNhanMatKhauMoi: string;
}

export interface CapNhatCauHoiBaoMatRequestDto {
  cauHoiBaoMat: string;
  cauTraLoiBaoMat: string;
  matKhauHienTai: string;
}

// ============================================================================
// Tài khoản (UC-06, 08..10)
// ============================================================================
export interface HoSoResponseDto {
  id: number;
  hoTen: string;
  email: string;
  soDienThoai: string;
  vaiTro: UserRole;
  trangThai: AccountStatus;
  ngaySinh?: string | null;
  gioiTinh?: Gender | null;
  anhDaiDien?: string | null;
  daThietLapCauHoiBaoMat: boolean;
  cauHoiBaoMat?: string | null;
  ngayTao: string;
}

export interface CapNhatHoSoRequestDto {
  hoTen: string;
  soDienThoai: string;
  ngaySinh?: string | null;
  gioiTinh?: Gender | null;
  anhDaiDien?: string | null;
}

export interface TaiKhoanListItemResponseDto {
  id: number;
  hoTen: string;
  email: string;
  soDienThoai: string;
  vaiTro: UserRole;
  trangThai: AccountStatus;
  lyDoKhoa?: string | null;
  ngayTao: string;
}

export interface KhoaMoTaiKhoanResponseDto {
  taiKhoanId: number;
  trangThaiMoi: AccountStatus;
  lyDo?: string | null;
  soDonDangXuLy: number;
  canhBao?: string | null;
}

export interface LichSuKhoaResponseDto {
  id: number;
  taiKhoanId: number;
  quanLyId: number;
  tenQuanLy: string;
  trangThaiMoi: AccountStatus;
  lyDo: string;
  thoiDiem: string;
}

// ============================================================================
// Địa chỉ nhận hàng (UC-07)
// ============================================================================
export interface DiaChiResponseDto {
  id: number;
  tenNguoiNhan: string;
  soDienThoai: string;
  tinhThanh: string;
  quanHuyen: string;
  phuongXa: string;
  diaChiCuThe: string;
  macDinh: boolean;
  diaChiDayDu: string;
}

export interface TaoSuaDiaChiRequestDto {
  tenNguoiNhan: string;
  soDienThoai: string;
  tinhThanh: string;
  quanHuyen: string;
  phuongXa: string;
  diaChiCuThe: string;
  macDinh: boolean;
}

// ============================================================================
// Danh mục (UC-11..16)
// ============================================================================
export interface DanhMucResponseDto {
  id: number;
  ten: string;
  moTa?: string | null;
  hinhAnh?: string | null;
  hienThi: boolean;
  soSanPham: number;
  ngayTao: string;
}

export interface TaoSuaDanhMucRequestDto {
  ten: string;
  moTa?: string | null;
  hinhAnh?: string | null;
  hienThi: boolean;
}

// ============================================================================
// Sản phẩm (UC-17..23)
// ============================================================================
export interface SanPhamListItemResponseDto {
  id: number;
  danhMucId: number;
  tenDanhMuc: string;
  maSanPham: string;
  ten: string;
  hinhAnh?: string | null;
  giaBan: number;
  soLuongTon: number;
  conHang: boolean;
  hienThi: boolean;
  diemTrungBinh: number;
  soLuotDanhGia: number;
  ngayTao: string;
}

export interface SanPhamChiTietResponseDto extends SanPhamListItemResponseDto {
  moTa?: string | null;
  nguongToiThieu: number;
}

export interface TaoSanPhamRequestDto {
  danhMucId: number;
  maSanPham: string;
  ten: string;
  moTa: string;
  hinhAnh?: string | null;
  giaBan: number;
  soLuongBanDau: number;
  nguongToiThieu: number;
  hienThi: boolean;
}

export interface SuaSanPhamRequestDto {
  danhMucId: number;
  maSanPham: string;
  ten: string;
  moTa: string;
  hinhAnh?: string | null;
  giaBan: number;
  hienThi: boolean;
}

// ============================================================================
// Kho hàng (UC-24..28)
// ============================================================================
export interface TonKhoResponseDto {
  tonKhoId: number;
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  tenDanhMuc: string;
  soLuongTon: number;
  nguongToiThieu: number;
  trangThaiTon: 'DuHang' | 'SapHet' | 'HetHang';
}

export interface NhapTangKhoRequestDto {
  sanPhamId: number;
  soLuongNhap: number;
  giaNhap?: number | null;
  nhaCungCap?: string | null;
  ghiChu?: string | null;
}

export interface DieuChinhGiamKhoRequestDto {
  sanPhamId: number;
  soLuongGiam: number;
  lyDo: string;
}

export interface BienDongKhoResponseDto {
  id: number;
  tonKhoId: number;
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  donHangId?: number | null;
  maDon?: string | null;
  loai: StockMovementType;
  soLuongThayDoi: number;
  soLuongTruoc: number;
  soLuongSau: number;
  giaNhap?: number | null;
  nhaCungCap?: string | null;
  nguoiThucHien: string;
  lyDo: string;
  thoiDiem: string;
}

// ============================================================================
// Giỏ hàng & Yêu thích (UC-29..33)
// ============================================================================
export interface ChiTietGioHangItemResponseDto {
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  hinhAnh?: string | null;
  donGiaHienTai: number;
  donGiaLucThem: number;
  giaDaThayDoi: boolean;
  soLuong: number;
  soLuongTon: number;
  khaDung: boolean;
  lyDoKhongKhaDung?: string | null;
  thanhTien: number;
}

export interface GioHangResponseDto {
  gioHangId: number;
  items: ChiTietGioHangItemResponseDto[];
  tongSoLuongKhaDung: number;
  tongTienTamTinh: number;
}

export interface YeuThichItemResponseDto {
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  hinhAnh?: string | null;
  giaBan: number;
  conHang: boolean;
  hienThi: boolean;
  ngayTao: string;
}

// ============================================================================
// Đặt hàng (UC-34..37, 40)
// ============================================================================
export interface ChonDiaChiResponseDto {
  diaChiId: number;
  tenNguoiNhan: string;
  soDienThoai: string;
  diaChiGiaoHang: string;
  hoTroGiaoHang: boolean;
}

export interface ApDungMaGiamGiaResponseDto {
  maGiamGiaId: number;
  ma: string;
  tamTinh: number;
  tienGiam: number;
  tongThanhToan: number;
}

export interface ChonPhuongThucThanhToanResponseDto {
  phuongThucThanhToan: PaymentMethod;
  huongDanThanhToan: string;
}

export interface XacNhanDatHangRequestDto {
  diaChiId: number;
  maGiamGia?: string | null;
  phuongThucThanhToan: PaymentMethod;
}

export interface XacNhanDatHangResponseDto {
  donHangId: number;
  maDon: string;
  trangThai: OrderStatus;
  tongThanhToan: number;
  ngayDat: string;
}

// ============================================================================
// Đơn hàng (UC-38..39, 41..44)
// ============================================================================
export interface DonHangListItemResponseDto {
  id: number;
  maDon: string;
  taiKhoanId: number;
  tenKhachHang: string;
  ngayDat: string;
  trangThai: OrderStatus;
  phuongThucThanhToan: PaymentMethod;
  daThanhToan: boolean;
  tongThanhToan: number;
  soMatHang: number;
}

export interface ChiTietDonHangItemResponseDto {
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  hinhAnh?: string | null;
  donGia: number;
  soLuong: number;
  thanhTien: number;
}

export interface LichSuDonHangItemResponseDto {
  id: number;
  trangThai: OrderStatus;
  thoiDiem: string;
  nguoiThucHien: string;
  ghiChu: string;
}

export interface DonHangChiTietResponseDto {
  id: number;
  maDon: string;
  taiKhoanId: number;
  tenKhachHang: string;
  ngayDat: string;
  trangThai: OrderStatus;
  phuongThucThanhToan: PaymentMethod;
  daThanhToan: boolean;
  tenNguoiNhan: string;
  soDienThoaiNhan: string;
  diaChiGiaoHang: string;
  maGiamGia?: string | null;
  tamTinh: number;
  tienGiam: number;
  tongThanhToan: number;
  lyDoHuy?: string | null;
  chiTiets: ChiTietDonHangItemResponseDto[];
  hanhTrinh: LichSuDonHangItemResponseDto[];
}

// ============================================================================
// Mã giảm giá (UC-45..50)
// ============================================================================
export interface MaGiamGiaResponseDto {
  id: number;
  ma: string;
  moTa?: string | null;
  loai: DiscountType;
  giaTri: number;
  giamToiDa?: number | null;
  donToiThieu: number;
  ngayBatDau: string;
  ngayKetThuc: string;
  soLuotToiDa: number;
  soLuotDaDung: number;
  soLuotConLai: number;
  hienThi: boolean;
}

export interface TaoSuaMaGiamGiaRequestDto {
  ma: string;
  moTa?: string | null;
  loai: DiscountType;
  giaTri: number;
  giamToiDa?: number | null;
  donToiThieu: number;
  ngayBatDau: string;
  ngayKetThuc: string;
  soLuotToiDa: number;
  hienThi: boolean;
}

// ============================================================================
// Đánh giá (UC-51..57)
// ============================================================================
export interface PhanHoiDanhGiaResponseDto {
  id: number;
  taiKhoanId: number;
  tenNguoiTraLoi: string;
  noiDung: string;
  ngayTao: string;
  ngayCapNhat?: string | null;
}

export interface DanhGiaResponseDto {
  id: number;
  taiKhoanId: number;
  tenNguoiDanhGia: string;
  sanPhamId: number;
  tenSanPham: string;
  donHangId: number;
  soSao: number;
  noiDung: string;
  hinhAnh?: string | null;
  daChinhSua: boolean;
  soLuotThich: number;
  daThich: boolean;
  ngayTao: string;
  ngayCapNhat?: string | null;
  phanHoi?: PhanHoiDanhGiaResponseDto | null;
}

export interface TaoDanhGiaRequestDto {
  sanPhamId: number;
  donHangId: number;
  soSao: number;
  noiDung: string;
  hinhAnh?: string | null;
}

export interface SuaDanhGiaRequestDto {
  soSao: number;
  noiDung: string;
  hinhAnh?: string | null;
}

export interface ThichDanhGiaResponseDto {
  danhGiaId: number;
  daThich: boolean;
  soLuotThich: number;
}

// ============================================================================
// Báo cáo thống kê (UC-58..62)
// ============================================================================
export interface BaoCaoTongQuanDto {
  tongDoanhThu: number;
  tongDonHang: number;
  donChoXacNhan: number;
  donDaXacNhan: number;
  donDangChuanBi: number;
  donDangGiao: number;
  donHoanThanh: number;
  donDaHuy: number;
  tongKhachHangMoi: number;
  soSanPhamSapHetHang: number;
}

export interface DoanhThuItemDto {
  mocThoiGian: string;
  soDonHoanThanh: number;
  doanhThu: number;
}

export interface BieuDoDoanhThuResponseDto {
  duLieu: DoanhThuItemDto[];
  tongDoanhThu: number;
  giaTriCaoNhat: number;
  giaTriThapNhat: number;
}

export interface SanPhamBanChayDto {
  thuHang: number;
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  soLuongBan: number;
  doanhThu: number;
}

export interface SanPhamTonLauDto {
  sanPhamId: number;
  maSanPham: string;
  tenSanPham: string;
  soLuongTon: number;
  ngayBanGanNhat?: string | null;
  soNgayTon: number;
  giaTriTon: number;
}
