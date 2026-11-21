import { PaymentMethod } from '../types/domain';
import { apiFetchHtmlReport, apiRequest, apiUploadImage } from './apiClient';
import {
  ApDungMaGiamGiaResponseDto,
  BaoCaoTongQuanDto,
  BienDongKhoResponseDto,
  BieuDoDoanhThuResponseDto,
  CapNhatCauHoiBaoMatRequestDto,
  CapNhatHoSoRequestDto,
  CauHoiBaoMatResponseDto,
  ChonDiaChiResponseDto,
  ChonPhuongThucThanhToanResponseDto,
  DangKyRequestDto,
  DangNhapRequestDto,
  DangNhapResponseDto,
  DanhGiaResponseDto,
  DanhMucResponseDto,
  DiaChiResponseDto,
  DieuChinhGiamKhoRequestDto,
  DoiMatKhauRequestDto,
  DonHangChiTietResponseDto,
  DonHangListItemResponseDto,
  GioHangResponseDto,
  HoSoResponseDto,
  KhoaMoTaiKhoanResponseDto,
  LichSuDonHangItemResponseDto,
  LichSuKhoaResponseDto,
  MaGiamGiaResponseDto,
  NhapTangKhoRequestDto,
  PagedResult,
  PhanHoiDanhGiaResponseDto,
  QuenMatKhauRequestDto,
  SanPhamBanChayDto,
  SanPhamChiTietResponseDto,
  SanPhamListItemResponseDto,
  SanPhamTonLauDto,
  SuaDanhGiaRequestDto,
  SuaSanPhamRequestDto,
  TaiKhoanListItemResponseDto,
  TaoDanhGiaRequestDto,
  TaoSanPhamRequestDto,
  TaoSuaDanhMucRequestDto,
  TaoSuaDiaChiRequestDto,
  TaoSuaMaGiamGiaRequestDto,
  ThichDanhGiaResponseDto,
  TonKhoResponseDto,
  XacNhanDatHangRequestDto,
  XacNhanDatHangResponseDto,
  YeuThichItemResponseDto,
} from './backendDtos';

export const authApi = {
  register: (payload: DangKyRequestDto) =>
    apiRequest<{ taiKhoanId: number }>('/api/xac-thuc/dang-ky', {
      method: 'POST',
      body: payload,
      skipAuthRedirect: true,
    }),

  login: (payload: DangNhapRequestDto) =>
    apiRequest<DangNhapResponseDto>('/api/xac-thuc/dang-nhap', {
      method: 'POST',
      body: payload,
      skipAuthRedirect: true,
    }),

  getSecurityQuestion: (taiKhoan: string) =>
    apiRequest<CauHoiBaoMatResponseDto>('/api/xac-thuc/cau-hoi-bao-mat', {
      method: 'GET',
      query: { taiKhoan },
      skipAuthRedirect: true,
    }),

  forgotPassword: (payload: QuenMatKhauRequestDto) =>
    apiRequest<null>('/api/xac-thuc/quen-mat-khau', {
      method: 'POST',
      body: payload,
      skipAuthRedirect: true,
    }),

  changePassword: (payload: DoiMatKhauRequestDto) =>
    apiRequest<null>('/api/xac-thuc/doi-mat-khau', {
      method: 'POST',
      body: payload,
    }),

  updateSecurityQuestion: (payload: CapNhatCauHoiBaoMatRequestDto) =>
    apiRequest<null>('/api/xac-thuc/cap-nhat-cau-hoi-bao-mat', {
      method: 'PUT',
      body: payload,
    }),
};

export const accountApi = {
  getProfile: () =>
    apiRequest<HoSoResponseDto>('/api/tai-khoan/ho-so', {
      method: 'GET',
      skipAuthRedirect: true,
    }),

  updateProfile: (payload: CapNhatHoSoRequestDto) =>
    apiRequest<HoSoResponseDto>('/api/tai-khoan/ho-so', {
      method: 'PUT',
      body: payload,
    }),

  listAccounts: (query?: {
    page?: number;
    pageSize?: number;
    tuKhoa?: string;
    vaiTro?: string;
    trangThai?: string;
    sapXep?: string;
  }) =>
    apiRequest<PagedResult<TaiKhoanListItemResponseDto>>('/api/tai-khoan', {
      method: 'GET',
      query,
    }),

  lockAccount: (id: number, lyDo: string) =>
    apiRequest<KhoaMoTaiKhoanResponseDto>(`/api/tai-khoan/${id}/khoa`, {
      method: 'PATCH',
      body: { lyDo },
    }),

  unlockAccount: (id: number) =>
    apiRequest<KhoaMoTaiKhoanResponseDto>(`/api/tai-khoan/${id}/mo-khoa`, {
      method: 'PATCH',
    }),

  getLockHistory: (id: number) =>
    apiRequest<LichSuKhoaResponseDto[]>(`/api/tai-khoan/${id}/lich-su-khoa`, {
      method: 'GET',
    }),
};

export const addressApi = {
  list: () => apiRequest<DiaChiResponseDto[]>('/api/dia-chi', { method: 'GET' }),

  create: (payload: TaoSuaDiaChiRequestDto) =>
    apiRequest<DiaChiResponseDto>('/api/dia-chi', {
      method: 'POST',
      body: payload,
    }),

  update: (id: number, payload: TaoSuaDiaChiRequestDto) =>
    apiRequest<DiaChiResponseDto>(`/api/dia-chi/${id}`, {
      method: 'PUT',
      body: payload,
    }),

  remove: (id: number) =>
    apiRequest<null>(`/api/dia-chi/${id}`, { method: 'DELETE' }),

  setDefault: (id: number) =>
    apiRequest<DiaChiResponseDto>(`/api/dia-chi/${id}/mac-dinh`, {
      method: 'PATCH',
    }),
};

export const categoryApi = {
  list: () =>
    apiRequest<DanhMucResponseDto[]>('/api/danh-muc', {
      method: 'GET',
      skipAuthRedirect: true,
    }),

  create: (payload: TaoSuaDanhMucRequestDto) =>
    apiRequest<DanhMucResponseDto>('/api/danh-muc', {
      method: 'POST',
      body: payload,
    }),

  update: (id: number, payload: TaoSuaDanhMucRequestDto) =>
    apiRequest<DanhMucResponseDto>(`/api/danh-muc/${id}`, {
      method: 'PUT',
      body: payload,
    }),

  remove: (id: number) =>
    apiRequest<null>(`/api/danh-muc/${id}`, { method: 'DELETE' }),

  toggleVisibility: (id: number, hienThi: boolean) =>
    apiRequest<DanhMucResponseDto>(`/api/danh-muc/${id}/an-hien`, {
      method: 'PATCH',
      body: { hienThi },
    }),
};

export const productApi = {
  list: (query?: {
    page?: number;
    pageSize?: number;
    tuKhoa?: string;
    danhMucId?: number;
    giaTu?: number;
    giaDen?: number;
    hienThi?: boolean;
    sapXep?: string;
  }) =>
    apiRequest<PagedResult<SanPhamListItemResponseDto>>('/api/san-pham', {
      method: 'GET',
      query,
      skipAuthRedirect: true,
    }),

  getDetail: (id: number) =>
    apiRequest<SanPhamChiTietResponseDto>(`/api/san-pham/${id}`, {
      method: 'GET',
      skipAuthRedirect: true,
    }),

  create: (payload: TaoSanPhamRequestDto) =>
    apiRequest<SanPhamChiTietResponseDto>('/api/san-pham', {
      method: 'POST',
      body: payload,
    }),

  update: (id: number, payload: SuaSanPhamRequestDto) =>
    apiRequest<SanPhamChiTietResponseDto>(`/api/san-pham/${id}`, {
      method: 'PUT',
      body: payload,
    }),

  remove: (id: number) =>
    apiRequest<null>(`/api/san-pham/${id}`, { method: 'DELETE' }),

  toggleVisibility: (id: number, hienThi: boolean) =>
    apiRequest<SanPhamChiTietResponseDto>(`/api/san-pham/${id}/an-hien`, {
      method: 'PATCH',
      body: { hienThi },
    }),
};

export const inventoryApi = {
  list: (query?: {
    page?: number;
    pageSize?: number;
    tuKhoa?: string;
    danhMucId?: number;
    trangThaiTon?: string;
    sapXep?: string;
  }) =>
    apiRequest<PagedResult<TonKhoResponseDto>>('/api/kho', {
      method: 'GET',
      query,
    }),

  importStock: (payload: NhapTangKhoRequestDto) =>
    apiRequest<TonKhoResponseDto>('/api/kho/nhap-tang', {
      method: 'POST',
      body: payload,
    }),

  adjustDecrease: (payload: DieuChinhGiamKhoRequestDto) =>
    apiRequest<TonKhoResponseDto>('/api/kho/dieu-chinh-giam', {
      method: 'POST',
      body: payload,
    }),

  updateMinThreshold: (tonKhoId: number, nguongToiThieu: number) =>
    apiRequest<TonKhoResponseDto>(`/api/kho/${tonKhoId}/nguong-toi-thieu`, {
      method: 'PATCH',
      body: { nguongToiThieu },
    }),

  listHistory: (query?: {
    page?: number;
    pageSize?: number;
    sanPhamId?: number;
    loai?: string;
    tuNgay?: string;
    denNgay?: string;
  }) =>
    apiRequest<PagedResult<BienDongKhoResponseDto>>('/api/kho/lich-su', {
      method: 'GET',
      query,
    }),
};

export const cartApi = {
  getCart: () => apiRequest<GioHangResponseDto>('/api/gio-hang', { method: 'GET' }),

  addItem: (sanPhamId: number, soLuong: number) =>
    apiRequest<GioHangResponseDto>('/api/gio-hang/san-pham', {
      method: 'POST',
      body: { sanPhamId, soLuong },
    }),

  updateQuantity: (sanPhamId: number, soLuong: number) =>
    apiRequest<GioHangResponseDto>(`/api/gio-hang/san-pham/${sanPhamId}`, {
      method: 'PUT',
      body: { soLuong },
    }),

  removeItem: (sanPhamId: number) =>
    apiRequest<GioHangResponseDto>(`/api/gio-hang/san-pham/${sanPhamId}`, {
      method: 'DELETE',
    }),

  listWishlist: () =>
    apiRequest<YeuThichItemResponseDto[]>('/api/gio-hang/yeu-thich', {
      method: 'GET',
    }),

  addWishlist: (sanPhamId: number) =>
    apiRequest<null>(`/api/gio-hang/yeu-thich/${sanPhamId}`, {
      method: 'POST',
    }),

  removeWishlist: (sanPhamId: number) =>
    apiRequest<null>(`/api/gio-hang/yeu-thich/${sanPhamId}`, {
      method: 'DELETE',
    }),
};

export const checkoutApi = {
  selectAddress: (diaChiId: number) =>
    apiRequest<ChonDiaChiResponseDto>('/api/dat-hang/chon-dia-chi', {
      method: 'POST',
      body: { diaChiId },
    }),

  applyVoucher: (maGiamGia: string) =>
    apiRequest<ApDungMaGiamGiaResponseDto>('/api/dat-hang/ap-dung-ma-giam-gia', {
      method: 'POST',
      body: { maGiamGia },
    }),

  selectPaymentMethod: (phuongThucThanhToan: PaymentMethod) =>
    apiRequest<ChonPhuongThucThanhToanResponseDto>(
      '/api/dat-hang/chon-phuong-thuc-thanh-toan',
      {
        method: 'POST',
        body: { phuongThucThanhToan },
      }
    ),

  confirmOrder: (payload: XacNhanDatHangRequestDto) =>
    apiRequest<XacNhanDatHangResponseDto>('/api/dat-hang/xac-nhan', {
      method: 'POST',
      body: payload,
    }),
};

export const orderApi = {
  list: (query?: {
    page?: number;
    pageSize?: number;
    maDon?: string;
    trangThai?: string;
    tuNgay?: string;
    denNgay?: string;
  }) =>
    apiRequest<PagedResult<DonHangListItemResponseDto>>('/api/don-hang', {
      method: 'GET',
      query,
    }),

  getDetail: (id: number) =>
    apiRequest<DonHangChiTietResponseDto>(`/api/don-hang/${id}`, {
      method: 'GET',
    }),

  cancel: (id: number, lyDoHuy: string) =>
    apiRequest<null>(`/api/don-hang/${id}/huy`, {
      method: 'PATCH',
      body: { lyDoHuy },
    }),

  getTimeline: (id: number) =>
    apiRequest<LichSuDonHangItemResponseDto[]>(`/api/don-hang/${id}/hanh-trinh`, {
      method: 'GET',
    }),

  confirm: (id: number) =>
    apiRequest<null>(`/api/don-hang/${id}/xac-nhan`, {
      method: 'PATCH',
    }),

  updateStatus: (id: number, trangThaiMoi: string, ghiChu?: string) =>
    apiRequest<null>(`/api/don-hang/${id}/trang-thai`, {
      method: 'PATCH',
      body: { trangThaiMoi, ghiChu },
    }),
};

export const voucherApi = {
  list: () =>
    apiRequest<MaGiamGiaResponseDto[]>('/api/ma-giam-gia', {
      method: 'GET',
      skipAuthRedirect: true,
    }),

  create: (payload: TaoSuaMaGiamGiaRequestDto) =>
    apiRequest<MaGiamGiaResponseDto>('/api/ma-giam-gia', {
      method: 'POST',
      body: payload,
    }),

  update: (id: number, payload: TaoSuaMaGiamGiaRequestDto) =>
    apiRequest<MaGiamGiaResponseDto>(`/api/ma-giam-gia/${id}`, {
      method: 'PUT',
      body: payload,
    }),

  remove: (id: number) =>
    apiRequest<null>(`/api/ma-giam-gia/${id}`, { method: 'DELETE' }),
};

export const reviewApi = {
  list: (query?: {
    page?: number;
    pageSize?: number;
    sanPhamId?: number;
    soSao?: number;
    coHinhAnh?: boolean;
    coPhanHoi?: boolean;
  }) =>
    apiRequest<PagedResult<DanhGiaResponseDto>>('/api/danh-gia', {
      method: 'GET',
      query,
      skipAuthRedirect: true,
    }),

  create: (payload: TaoDanhGiaRequestDto) =>
    apiRequest<DanhGiaResponseDto>('/api/danh-gia', {
      method: 'POST',
      body: payload,
    }),

  update: (id: number, payload: SuaDanhGiaRequestDto) =>
    apiRequest<DanhGiaResponseDto>(`/api/danh-gia/${id}`, {
      method: 'PUT',
      body: payload,
    }),

  toggleLike: (id: number) =>
    apiRequest<ThichDanhGiaResponseDto>(`/api/danh-gia/${id}/thich`, {
      method: 'POST',
    }),

  remove: (id: number, lyDoXoa?: string) =>
    apiRequest<null>(`/api/danh-gia/${id}`, {
      method: 'DELETE',
      body: lyDoXoa ? { lyDoXoa } : undefined,
    }),

  reply: (id: number, noiDung: string) =>
    apiRequest<PhanHoiDanhGiaResponseDto>(`/api/danh-gia/${id}/tra-loi`, {
      method: 'POST',
      body: { noiDung },
    }),
};

export const reportApi = {
  getOverview: (query?: { tuNgay?: string; denNgay?: string }) =>
    apiRequest<BaoCaoTongQuanDto>('/api/bao-cao/tong-quan', {
      method: 'GET',
      query,
    }),

  getRevenueChart: (query?: {
    tuNgay?: string;
    denNgay?: string;
    donVi?: 'ngay' | 'tuan' | 'thang' | 'nam';
  }) =>
    apiRequest<BieuDoDoanhThuResponseDto>('/api/bao-cao/doanh-thu', {
      method: 'GET',
      query,
    }),

  getBestSellers: (query?: {
    tuNgay?: string;
    denNgay?: string;
    danhMucId?: number;
    top?: number;
  }) =>
    apiRequest<SanPhamBanChayDto[]>('/api/bao-cao/san-pham-ban-chay', {
      method: 'GET',
      query,
    }),

  getDeadStock: (query?: { soNgayKhongBan?: number; danhMucId?: number }) =>
    apiRequest<SanPhamTonLauDto[]>('/api/bao-cao/san-pham-ton-lau', {
      method: 'GET',
      query,
    }),

  printHtmlReport: (query: {
    loaiBaoCao: 'tong-quan' | 'doanh-thu' | 'ban-chay' | 'ton-lau';
    tuNgay?: string;
    denNgay?: string;
    donVi?: 'ngay' | 'tuan' | 'thang' | 'nam';
    top?: number;
    soNgayTonLau?: number;
  }) => apiFetchHtmlReport(query),
};

export const uploadApi = {
  uploadImage: apiUploadImage,
};
