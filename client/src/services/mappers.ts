import {
  AccountLockHistory,
  CartItem,
  Category,
  Order,
  Product,
  Review,
  ShippingAddress,
  StockHistory,
  User,
  Voucher,
} from '../types/domain';
import { resolveAssetUrl } from './apiClient';
import {
  BienDongKhoResponseDto,
  ChiTietGioHangItemResponseDto,
  DanhGiaResponseDto,
  DanhMucResponseDto,
  DiaChiResponseDto,
  DonHangChiTietResponseDto,
  DonHangListItemResponseDto,
  HoSoResponseDto,
  LichSuKhoaResponseDto,
  MaGiamGiaResponseDto,
  SanPhamChiTietResponseDto,
  SanPhamListItemResponseDto,
  TaiKhoanListItemResponseDto,
} from './backendDtos';

export function createPlaceholderImage(label: string, accent = '#e4e4e7'): string {
  const safeLabel = (label || 'HCShop').slice(0, 18).replace(/[<>&"]/g, '');
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="400" height="400" viewBox="0 0 400 400">
    <rect width="400" height="400" rx="28" fill="#f4f4f5"/>
    <circle cx="200" cy="170" r="64" fill="${accent}" opacity="0.55"/>
    <text x="200" y="285" text-anchor="middle" font-family="system-ui,sans-serif" font-size="22" font-weight="600" fill="#52525b">${safeLabel}</text>
  </svg>`;
  return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`;
}

function formatUserCode(id: number, role: string): string {
  const prefix = role === 'QuanLy' ? 'QL' : role === 'NhanVien' ? 'NV' : 'KH';
  return `${prefix}${String(id).padStart(4, '0')}`;
}

export function mapHoSoToUser(dto: HoSoResponseDto): User {
  return {
    id: String(dto.id),
    code: formatUserCode(dto.id, dto.vaiTro),
    fullName: dto.hoTen,
    email: dto.email,
    phone: dto.soDienThoai || '',
    passwordHash: '',
    role: dto.vaiTro,
    isLocked: dto.trangThai === 'BiKhoa',
    securityQuestion: dto.cauHoiBaoMat || undefined,
    failedLoginAttempts: 0,
    avatar: dto.anhDaiDien ? resolveAssetUrl(dto.anhDaiDien) : undefined,
    dob: dto.ngaySinh ? dto.ngaySinh.slice(0, 10) : undefined,
    gender: dto.gioiTinh || undefined,
    createdAt: dto.ngayTao,
  };
}

export function mapTaiKhoanItemToUser(dto: TaiKhoanListItemResponseDto): User {
  return {
    id: String(dto.id),
    code: formatUserCode(dto.id, dto.vaiTro),
    fullName: dto.hoTen,
    email: dto.email,
    phone: dto.soDienThoai || '',
    passwordHash: '',
    role: dto.vaiTro,
    isLocked: dto.trangThai === 'BiKhoa',
    lockReason: dto.lyDoKhoa || undefined,
    failedLoginAttempts: 0,
    createdAt: dto.ngayTao,
  };
}

export function mapLichSuKhoaToModel(dto: LichSuKhoaResponseDto): AccountLockHistory {
  return {
    id: String(dto.id),
    userId: String(dto.taiKhoanId),
    newStatus: dto.trangThaiMoi,
    reason: dto.lyDo,
    actorName: dto.tenQuanLy || 'Quản lý',
    createdAt: dto.thoiDiem,
  };
}

export function mapDiaChiToShippingAddress(
  dto: DiaChiResponseDto,
  userId: string
): ShippingAddress {
  return {
    id: String(dto.id),
    userId,
    recipientName: dto.tenNguoiNhan,
    phone: dto.soDienThoai,
    province: dto.tinhThanh,
    district: dto.quanHuyen,
    ward: dto.phuongXa,
    detailAddress: dto.diaChiCuThe,
    isDefault: dto.macDinh,
  };
}

export function mapDanhMucToCategory(dto: DanhMucResponseDto): Category {
  return {
    id: String(dto.id),
    name: dto.ten,
    description: dto.moTa || '',
    imageUrl: dto.hinhAnh ? resolveAssetUrl(dto.hinhAnh) : '',
    iconName: 'Package',
    isVisible: dto.hienThi,
    isDeleted: false,
    createdAt: dto.ngayTao,
  };
}

export function mapSanPhamToProduct(
  dto: SanPhamListItemResponseDto | SanPhamChiTietResponseDto,
  existing?: Product
): Product {
  const detail = dto as SanPhamChiTietResponseDto;
  const imgUrl = dto.hinhAnh
    ? resolveAssetUrl(dto.hinhAnh)
    : existing?.imageUrl || createPlaceholderImage(dto.ten);

  return {
    id: String(dto.id),
    tonKhoId: existing?.tonKhoId,
    code: dto.maSanPham,
    name: dto.ten,
    categoryId: String(dto.danhMucId),
    price: Number(dto.giaBan) || 0,
    description:
      typeof detail.moTa === 'string'
        ? detail.moTa
        : existing?.description || '',
    imageUrl: imgUrl,
    stock: Number(dto.soLuongTon) || 0,
    minStockThreshold:
      typeof detail.nguongToiThieu === 'number'
        ? detail.nguongToiThieu
        : existing?.minStockThreshold ?? 5,
    isVisible: dto.hienThi,
    isDeleted: false,
    averageRating: dto.diemTrungBinh,
    reviewCount: dto.soLuotDanhGia,
    createdAt: dto.ngayTao,
  };
}

export function mapBienDongKhoToStockHistory(dto: BienDongKhoResponseDto): StockHistory {
  const isDecrease = dto.loai === 'DieuChinhGiam' || dto.loai === 'XuatBan';
  const rawQty = Math.abs(Number(dto.soLuongThayDoi) || 0);
  return {
    id: String(dto.id),
    productId: String(dto.sanPhamId),
    orderId: dto.donHangId ? String(dto.donHangId) : undefined,
    orderCode: dto.maDon || undefined,
    type: dto.loai,
    quantityChange: isDecrease ? -rawQty : rawQty,
    stockBefore: Number(dto.soLuongTruoc) || 0,
    stockAfter: Number(dto.soLuongSau) || 0,
    reasonOrNote: dto.lyDo || '',
    supplier: dto.nhaCungCap || undefined,
    importPrice: dto.giaNhap ?? undefined,
    actorName: dto.nguoiThucHien || 'Hệ thống',
    createdAt: dto.thoiDiem,
  };
}

export function mapGioHangItemToCartItem(dto: ChiTietGioHangItemResponseDto): CartItem {
  return {
    productId: String(dto.sanPhamId),
    quantity: dto.soLuong,
    addedPrice: Number(dto.donGiaLucThem) || Number(dto.donGiaHienTai) || 0,
    currentPrice: Number(dto.donGiaHienTai) || 0,
    isAvailable: dto.khaDung,
    unavailableReason: dto.lyDoKhongKhaDung || undefined,
    productName: dto.tenSanPham,
    productImage: dto.hinhAnh ? resolveAssetUrl(dto.hinhAnh) : undefined,
    stock: dto.soLuongTon,
  };
}

export function mapMaGiamGiaToVoucher(dto: MaGiamGiaResponseDto): Voucher {
  return {
    id: String(dto.id),
    code: dto.ma,
    description: dto.moTa || '',
    discountType: dto.loai,
    discountValue: Number(dto.giaTri) || 0,
    maxDiscountAmount: dto.giamToiDa ?? undefined,
    minOrderValue: Number(dto.donToiThieu) || 0,
    startDate: dto.ngayBatDau,
    endDate: dto.ngayKetThuc,
    usageLimit: dto.soLuotToiDa,
    usedCount: dto.soLuotDaDung,
    isActive: dto.hienThi,
    isDeleted: false,
    createdAt: dto.ngayBatDau,
  };
}

function parseFlatShippingAddress(
  orderId: string,
  userId: string,
  recipientName: string,
  phone: string,
  flatAddress: string
): ShippingAddress {
  const parts = (flatAddress || '')
    .split(',')
    .map((p) => p.trim())
    .filter(Boolean);

  if (parts.length >= 4) {
    return {
      id: `addr-ord-${orderId}`,
      userId,
      recipientName: recipientName || '',
      phone: phone || '',
      province: parts[parts.length - 1],
      district: parts[parts.length - 2],
      ward: parts[parts.length - 3],
      detailAddress: parts.slice(0, parts.length - 3).join(', '),
      isDefault: false,
    };
  }

  return {
    id: `addr-ord-${orderId}`,
    userId,
    recipientName: recipientName || '',
    phone: phone || '',
    province: '',
    district: '',
    ward: '',
    detailAddress: flatAddress || '',
    isDefault: false,
  };
}

export function mapDonHangListItemToOrder(
  dto: DonHangListItemResponseDto,
  existing?: Order
): Order {
  return {
    id: String(dto.id),
    code: dto.maDon,
    userId: String(dto.taiKhoanId),
    customerName: dto.tenKhachHang || existing?.customerName || '',
    status: dto.trangThai,
    items: existing?.items || [],
    shippingAddress: existing?.shippingAddress || {
      id: `addr-ord-${dto.id}`,
      userId: String(dto.taiKhoanId),
      recipientName: dto.tenKhachHang || '',
      phone: '',
      province: '',
      district: '',
      ward: '',
      detailAddress: '',
      isDefault: false,
    },
    paymentMethod: dto.phuongThucThanhToan,
    voucherId: existing?.voucherId,
    voucherCode: existing?.voucherCode,
    discountAmount: existing?.discountAmount ?? 0,
    subtotal: existing?.subtotal ?? (Number(dto.tongThanhToan) || 0),
    totalAmount: Number(dto.tongThanhToan) || 0,
    timeline: existing?.timeline || [
      {
        status: dto.trangThai,
        timestamp: dto.ngayDat,
        note: 'Đơn hàng đã được ghi nhận trên hệ thống',
      },
    ],
    cancelReason: existing?.cancelReason,
    isPaid: dto.daThanhToan,
    createdAt: dto.ngayDat,
  };
}

export function mapDonHangDetailToOrder(dto: DonHangChiTietResponseDto): Order {
  const userId = String(dto.taiKhoanId);
  return {
    id: String(dto.id),
    code: dto.maDon,
    userId,
    customerName: dto.tenKhachHang || dto.tenNguoiNhan || '',
    status: dto.trangThai,
    items: (dto.chiTiets || []).map((item) => ({
      productId: String(item.sanPhamId),
      productName: item.tenSanPham,
      productImage: item.hinhAnh
        ? resolveAssetUrl(item.hinhAnh)
        : createPlaceholderImage(item.tenSanPham),
      quantity: item.soLuong,
      priceAtOrder: Number(item.donGia) || 0,
    })),
    shippingAddress: parseFlatShippingAddress(
      String(dto.id),
      userId,
      dto.tenNguoiNhan,
      dto.soDienThoaiNhan,
      dto.diaChiGiaoHang
    ),
    paymentMethod: dto.phuongThucThanhToan,
    voucherCode: dto.maGiamGia || undefined,
    discountAmount: Number(dto.tienGiam) || 0,
    subtotal: Number(dto.tamTinh) || 0,
    totalAmount: Number(dto.tongThanhToan) || 0,
    timeline: (dto.hanhTrinh || []).map((step) => ({
      status: step.trangThai,
      timestamp: step.thoiDiem,
      note: step.ghiChu || '',
      actorName: step.nguoiThucHien,
    })),
    cancelReason: dto.lyDoHuy || undefined,
    isPaid: dto.daThanhToan,
    createdAt: dto.ngayDat,
  };
}

export function mapDanhGiaToReview(
  dto: DanhGiaResponseDto,
  currentUserId?: string
): Review {
  // Synthesize likedByUserIds so existing UI checks `.length` and `.includes(currentUser.id)` seamlessly
  const likedByUserIds: string[] = [];
  if (dto.daThich && currentUserId) {
    likedByUserIds.push(currentUserId);
  }
  const extraLikes = Math.max(0, (dto.soLuotThich || 0) - likedByUserIds.length);
  for (let i = 0; i < extraLikes; i += 1) {
    likedByUserIds.push(`__like_${dto.id}_${i}`);
  }

  return {
    id: String(dto.id),
    productId: String(dto.sanPhamId),
    orderId: String(dto.donHangId),
    userId: String(dto.taiKhoanId),
    userName: dto.tenNguoiDanhGia || 'Khách hàng',
    rating: dto.soSao,
    comment: dto.noiDung,
    images: dto.hinhAnh ? [resolveAssetUrl(dto.hinhAnh)] : [],
    isEdited: dto.daChinhSua,
    likedByUserIds,
    likeCount: dto.soLuotThich,
    isLikedByMe: dto.daThich,
    storeReply: dto.phanHoi?.noiDung,
    storeReplyAt: dto.phanHoi?.ngayCapNhat || dto.phanHoi?.ngayTao,
    storeReplyBy: dto.phanHoi?.tenNguoiTraLoi,
    createdAt: dto.ngayTao,
  };
}
