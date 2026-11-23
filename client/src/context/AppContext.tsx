import React, { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import {
  AccountLockHistory,
  CartItem,
  Category,
  DiscountType,
  Gender,
  Order,
  OrderStatus,
  PaymentMethod,
  Product,
  Review,
  ShippingAddress,
  StockHistory,
  User,
  Voucher,
} from '../types/domain';
import {
  clearAccessToken,
  getAccessToken,
  setAccessToken,
  setUnauthorizedListener,
} from '../services/apiClient';
import {
  accountApi,
  addressApi,
  authApi,
  cartApi,
  categoryApi,
  checkoutApi,
  inventoryApi,
  orderApi,
  productApi,
  reviewApi,
  voucherApi,
} from '../services/hcshopApi';
import {
  mapBienDongKhoToStockHistory,
  mapDanhGiaToReview,
  mapDanhMucToCategory,
  mapDiaChiToShippingAddress,
  mapDonHangDetailToOrder,
  mapDonHangListItemToOrder,
  mapGioHangItemToCartItem,
  mapHoSoToUser,
  mapLichSuKhoaToModel,
  mapMaGiamGiaToVoucher,
  mapSanPhamToProduct,
  mapTaiKhoanItemToUser,
} from '../services/mappers';

export interface VoucherValidationResult {
  ok: boolean;
  voucher?: Voucher;
  discountAmount: number;
  message: string;
}

export interface ActionResult {
  ok: boolean;
  message: string;
}

interface AppContextType {
  isBootstrapping: boolean;
  apiConnectionError: string | null;
  currentUser: User | null;
  users: User[];
  accountLockHistories: AccountLockHistory[];
  categories: Category[];
  products: Product[];
  stockHistories: StockHistory[];
  vouchers: Voucher[];
  orders: Order[];
  reviews: Review[];
  addresses: ShippingAddress[];
  cart: CartItem[];
  wishlistProductIds: string[];
  searchQuery: string;
  selectedCategoryId: string;

  // UI & Data Refresh Helpers
  setSearchQuery: (q: string) => void;
  setSelectedCategoryId: (id: string) => void;
  refreshAllData: () => Promise<void>;
  loadProductDetail: (productId: string) => Promise<Product | null>;
  loadOrderDetail: (orderId: string) => Promise<Order | null>;
  loadAccountLockHistory: (userId: string) => Promise<AccountLockHistory[]>;

  // Cart & Wishlist (UC-29..33)
  addToCart: (productId: string, quantity?: number) => Promise<ActionResult>;
  updateCartQuantity: (
    productId: string,
    newQuantity: number
  ) => Promise<{ ok: boolean; confirmRemove?: boolean; message: string }>;
  removeFromCart: (productId: string) => Promise<ActionResult>;
  toggleWishlist: (productId: string) => Promise<{ added: boolean; message: string }>;

  // Addresses, Vouchers & Atomic Checkout (UC-34..37, UC-40)
  addShippingAddress: (
    input: Omit<ShippingAddress, 'id' | 'userId'>
  ) => Promise<{ ok: boolean; address?: ShippingAddress; message: string }>;
  updateShippingAddress: (
    id: string,
    input: Omit<ShippingAddress, 'id' | 'userId'>
  ) => Promise<ActionResult>;
  deleteShippingAddress: (id: string) => Promise<ActionResult>;
  setDefaultShippingAddress: (id: string) => Promise<ActionResult>;
  validateVoucherCode: (code: string, subtotal: number) => Promise<VoucherValidationResult>;
  placeOrder: (params: {
    addressId: string;
    paymentMethod: PaymentMethod;
    voucherCode?: string;
  }) => Promise<{ ok: boolean; order?: Order; message: string }>;
  cancelCustomerOrder: (orderId: string, cancelReason: string) => Promise<ActionResult>;

  // Auth & Profile (UC-01..06)
  loginUser: (
    identifier: string,
    password: string
  ) => Promise<{ ok: boolean; user?: User; message: string }>;
  registerUser: (input: {
    fullName: string;
    email: string;
    phone: string;
    password: string;
    confirmPassword: string;
    securityQuestion: string;
    securityAnswer: string;
  }) => Promise<{ ok: boolean; user?: User; message: string }>;
  logoutUser: () => void;
  lookupUserForRecovery: (identifier: string) => Promise<{
    ok: boolean;
    accountIdentifier?: string;
    securityQuestion?: string;
    isLockedOut?: boolean;
    message: string;
  }>;
  resetPasswordBySecurityAnswer: (params: {
    accountIdentifier: string;
    answer: string;
    newPassword: string;
    confirmPassword: string;
  }) => Promise<{ ok: boolean; isLockedOut?: boolean; message: string }>;
  changePassword: (params: {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
  }) => Promise<ActionResult>;
  updateSecurityQuestion: (params: {
    currentPassword: string;
    securityQuestion: string;
    securityAnswer: string;
  }) => Promise<ActionResult>;
  updateProfile: (params: {
    fullName: string;
    phone: string;
    dob?: string;
    gender?: Gender;
    avatar?: string;
  }) => Promise<ActionResult>;

  // Reviews (UC-53..56)
  submitReview: (
    productId: string,
    rating: number,
    comment: string,
    images?: string[],
    orderId?: string
  ) => Promise<ActionResult>;
  deleteReview: (reviewId: string) => Promise<ActionResult>;
  toggleReviewLike: (reviewId: string) => Promise<ActionResult>;

  // Staff / Admin Order Processing (UC-41, UC-43..44)
  staffConfirmOrRejectOrder: (
    orderId: string,
    action: 'CONFIRM' | 'REJECT',
    reasonOrNote?: string
  ) => Promise<ActionResult>;
  staffAdvanceOrderStatus: (
    orderId: string,
    nextStatus: OrderStatus,
    note?: string
  ) => Promise<ActionResult>;

  // Admin Category & Product Management (UC-13..16, UC-20..23)
  createCategory: (input: {
    name: string;
    description: string;
    imageUrl?: string;
    isVisible?: boolean;
  }) => Promise<ActionResult>;
  updateCategory: (
    id: string,
    input: { name: string; description: string; imageUrl?: string }
  ) => Promise<ActionResult>;
  toggleCategoryVisibility: (id: string) => Promise<ActionResult>;
  deleteCategory: (id: string) => Promise<ActionResult>;

  createProduct: (input: {
    code: string;
    name: string;
    categoryId: string;
    price: number;
    description: string;
    imageUrl?: string;
    minStockThreshold: number;
    initialStock: number;
    isVisible?: boolean;
  }) => Promise<ActionResult>;
  updateProduct: (
    id: string,
    input: {
      name: string;
      categoryId: string;
      price: number;
      description: string;
      imageUrl?: string;
    }
  ) => Promise<ActionResult>;
  toggleProductVisibility: (id: string) => Promise<ActionResult>;
  deleteProduct: (id: string) => Promise<ActionResult>;

  // Admin Inventory Management (UC-24..28)
  importStock: (params: {
    productId: string;
    quantity: number;
    importPrice?: number;
    supplier?: string;
    note: string;
  }) => Promise<ActionResult>;
  adjustStockDecrease: (params: {
    productId: string;
    quantityToSubtract: number;
    reason: string;
  }) => Promise<ActionResult>;
  updateMinStockThreshold: (productId: string, threshold: number) => Promise<ActionResult>;

  // Admin Voucher Management (UC-48..50)
  createVoucher: (input: {
    code: string;
    description?: string;
    discountType: DiscountType;
    discountValue: number;
    maxDiscountAmount?: number;
    minOrderValue: number;
    startDate: string;
    endDate: string;
    usageLimit: number;
    isActive?: boolean;
  }) => Promise<ActionResult>;
  updateVoucher: (
    id: string,
    input: {
      code: string;
      description?: string;
      discountType: DiscountType;
      discountValue: number;
      maxDiscountAmount?: number;
      minOrderValue: number;
      startDate: string;
      endDate: string;
      usageLimit: number;
    }
  ) => Promise<ActionResult>;
  toggleVoucherActive: (id: string) => Promise<ActionResult>;
  deleteVoucher: (id: string) => Promise<ActionResult>;

  // Staff / Admin Review Moderation (UC-56..57)
  replyToReview: (reviewId: string, replyContent: string) => Promise<ActionResult>;
  adminDeleteReview: (reviewId: string, reason: string) => Promise<ActionResult>;

  // Admin Account Management (UC-08..10)
  toggleAccountLock: (userId: string, lockReason?: string) => Promise<ActionResult>;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

function toIsoDateOffset(dateInput: string, endOfDay = false): string {
  if (!dateInput) return new Date().toISOString();
  if (dateInput.includes('T')) return dateInput;
  return endOfDay ? `${dateInput}T23:59:59+07:00` : `${dateInput}T00:00:00+07:00`;
}

function stripBaseUrlForSave(url?: string): string | null {
  if (!url || !url.trim()) return null;
  const trimmed = url.trim();
  // Không lưu placeholder SVG data URI xuống DB
  if (trimmed.startsWith('data:image/svg+xml')) return null;
  const apiBase = (import.meta.env.VITE_API_BASE_URL || '').replace(/\/+$/, '');
  if (apiBase && trimmed.startsWith(apiBase)) {
    return trimmed.slice(apiBase.length) || null;
  }
  return trimmed;
}

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isBootstrapping, setIsBootstrapping] = useState(true);
  const [apiConnectionError, setApiConnectionError] = useState<string | null>(null);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [accountLockHistories, setAccountLockHistories] = useState<AccountLockHistory[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [stockHistories, setStockHistories] = useState<StockHistory[]>([]);
  const [vouchers, setVouchers] = useState<Voucher[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [addresses, setAddresses] = useState<ShippingAddress[]>([]);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [wishlistProductIds, setWishlistProductIds] = useState<string[]>([]);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedCategoryId, setSelectedCategoryId] = useState<string>('ALL');

  const productsRef = useRef<Product[]>(products);
  productsRef.current = products;

  const ordersRef = useRef<Order[]>(orders);
  ordersRef.current = orders;

  useEffect(() => {
    document.documentElement.classList.remove('dark');
  }, []);

  const clearSessionState = useCallback(() => {
    clearAccessToken();
    setCurrentUser(null);
    setUsers([]);
    setAccountLockHistories([]);
    setStockHistories([]);
    setOrders([]);
    setAddresses([]);
    setCart([]);
    setWishlistProductIds([]);
  }, []);

  useEffect(() => {
    setUnauthorizedListener(() => {
      clearSessionState();
    });
    return () => setUnauthorizedListener(null);
  }, [clearSessionState]);

  const loadCatalogPublicData = useCallback(async (userIdForLikes?: string) => {
    const [catRes, prdRes, revRes, vchRes] = await Promise.all([
      categoryApi.list(),
      productApi.list({ page: 1, pageSize: 100 }),
      reviewApi.list({ page: 1, pageSize: 100 }),
      voucherApi.list(),
    ]);

    if (!catRes.ok && catRes.status === 0) {
      setApiConnectionError(catRes.message);
    } else {
      setApiConnectionError(null);
    }

    if (catRes.ok && catRes.data) {
      setCategories(catRes.data.map(mapDanhMucToCategory));
    }

    if (prdRes.ok && prdRes.data) {
      const existingById = new Map(productsRef.current.map((p) => [p.id, p]));
      setProducts(
        prdRes.data.items.map((dto) =>
          mapSanPhamToProduct(dto, existingById.get(String(dto.id)))
        )
      );
    }

    if (revRes.ok && revRes.data) {
      setReviews(revRes.data.items.map((dto) => mapDanhGiaToReview(dto, userIdForLikes)));
    }

    if (vchRes.ok && vchRes.data) {
      setVouchers(vchRes.data.map(mapMaGiamGiaToVoucher));
    }
  }, []);

  const hydrateOrdersWithDetails = useCallback(async () => {
    const listRes = await orderApi.list({ page: 1, pageSize: 100 });
    if (!listRes.ok || !listRes.data) return;

    const existingById = new Map(ordersRef.current.map((o) => [o.id, o]));
    const summaryOrders = listRes.data.items.map((dto) =>
      mapDonHangListItemToOrder(dto, existingById.get(String(dto.id)))
    );
    setOrders(summaryOrders);

    // Tải chi tiết song song theo nhóm nhỏ để các trang đơn hàng & kiểm tra quyền đánh giá có đủ items + timeline
    const detailed = await Promise.all(
      listRes.data.items.slice(0, 30).map(async (item) => {
        const detailRes = await orderApi.getDetail(item.id);
        return detailRes.ok && detailRes.data
          ? mapDonHangDetailToOrder(detailRes.data)
          : mapDonHangListItemToOrder(item, existingById.get(String(item.id)));
      })
    );

    const detailMap = new Map(detailed.map((o) => [o.id, o]));
    setOrders(summaryOrders.map((o) => detailMap.get(o.id) || o));
  }, []);

  const hydrateInventoryForManager = useCallback(async () => {
    const [invRes, histRes] = await Promise.all([
      inventoryApi.list({ page: 1, pageSize: 100 }),
      inventoryApi.listHistory({ page: 1, pageSize: 100 }),
    ]);

    if (invRes.ok && invRes.data) {
      const stockMap = new Map(invRes.data.items.map((tk) => [String(tk.sanPhamId), tk]));
      setProducts((prev) =>
        prev.map((p) => {
          const tk = stockMap.get(p.id);
          if (!tk) return p;
          return {
            ...p,
            tonKhoId: tk.tonKhoId,
            stock: tk.soLuongTon,
            minStockThreshold: tk.nguongToiThieu,
          };
        })
      );
    }

    if (histRes.ok && histRes.data) {
      setStockHistories(histRes.data.items.map(mapBienDongKhoToStockHistory));
    }
  }, []);

  const hydrateRoleData = useCallback(
    async (user: User) => {
      if (user.role === 'KhachHang') {
        const [addrRes, cartRes, wishRes] = await Promise.all([
          addressApi.list(),
          cartApi.getCart(),
          cartApi.listWishlist(),
        ]);

        if (addrRes.ok && addrRes.data) {
          setAddresses(addrRes.data.map((a) => mapDiaChiToShippingAddress(a, user.id)));
        }
        if (cartRes.ok && cartRes.data) {
          setCart(cartRes.data.items.map(mapGioHangItemToCartItem));
        }
        if (wishRes.ok && wishRes.data) {
          setWishlistProductIds(wishRes.data.map((w) => String(w.sanPhamId)));
        }
        await hydrateOrdersWithDetails();
      } else if (user.role === 'NhanVien') {
        await hydrateOrdersWithDetails();
      } else if (user.role === 'QuanLy') {
        const [accRes] = await Promise.all([
          accountApi.listAccounts({ page: 1, pageSize: 100 }),
          hydrateInventoryForManager(),
          hydrateOrdersWithDetails(),
        ]);
        if (accRes.ok && accRes.data) {
          setUsers(accRes.data.items.map(mapTaiKhoanItemToUser));
        }
      }
    },
    [hydrateInventoryForManager, hydrateOrdersWithDetails]
  );

  const refreshAllData = useCallback(async () => {
    const token = getAccessToken();
    let activeUser: User | null = null;

    if (token) {
      const profileRes = await accountApi.getProfile();
      if (profileRes.ok && profileRes.data) {
        activeUser = mapHoSoToUser(profileRes.data);
        setCurrentUser(activeUser);
      } else if (profileRes.status === 401) {
        clearSessionState();
      }
    }

    await loadCatalogPublicData(activeUser?.id);
    if (activeUser) {
      await hydrateRoleData(activeUser);
    }
  }, [clearSessionState, hydrateRoleData, loadCatalogPublicData]);

  useEffect(() => {
    let active = true;
    (async () => {
      try {
        await refreshAllData();
      } finally {
        if (active) setIsBootstrapping(false);
      }
    })();
    return () => {
      active = false;
    };
  }, [refreshAllData]);

  // ============================================================================
  // On-demand detail loaders
  // ============================================================================
  const loadProductDetail = useCallback(async (productId: string): Promise<Product | null> => {
    const numId = Number(productId);
    if (!Number.isFinite(numId) || numId <= 0) return null;

    const res = await productApi.getDetail(numId);
    if (!res.ok || !res.data) return null;

    const existing = productsRef.current.find((p) => p.id === productId);
    const mapped = mapSanPhamToProduct(res.data, existing);
    setProducts((prev) => {
      const exists = prev.some((p) => p.id === mapped.id);
      return exists ? prev.map((p) => (p.id === mapped.id ? mapped : p)) : [...prev, mapped];
    });
    return mapped;
  }, []);

  const loadOrderDetail = useCallback(async (orderId: string): Promise<Order | null> => {
    const numId = Number(orderId);
    if (!Number.isFinite(numId) || numId <= 0) return null;

    const res = await orderApi.getDetail(numId);
    if (!res.ok || !res.data) return null;

    const mapped = mapDonHangDetailToOrder(res.data);
    setOrders((prev) => {
      const exists = prev.some((o) => o.id === mapped.id);
      return exists ? prev.map((o) => (o.id === mapped.id ? mapped : o)) : [mapped, ...prev];
    });
    return mapped;
  }, []);

  const loadAccountLockHistory = useCallback(
    async (userId: string): Promise<AccountLockHistory[]> => {
      const numId = Number(userId);
      if (!Number.isFinite(numId) || numId <= 0) return [];

      const res = await accountApi.getLockHistory(numId);
      if (!res.ok || !res.data) return [];

      const mapped = res.data.map(mapLichSuKhoaToModel);
      setAccountLockHistories((prev) => {
        const others = prev.filter((h) => h.userId !== userId);
        return [...mapped, ...others];
      });
      return mapped;
    },
    []
  );

  // ============================================================================
  // Cart & Wishlist (UC-29..33)
  // ============================================================================
  const addToCart = async (productId: string, quantity = 1): Promise<ActionResult> => {
    if (!currentUser) {
      return { ok: false, message: 'Vui lòng đăng nhập bằng tài khoản Khách hàng để thêm vào giỏ hàng.' };
    }
    if (currentUser.role !== 'KhachHang') {
      return { ok: false, message: 'Chỉ tài khoản Khách hàng mới có thể mua sắm và sử dụng giỏ hàng.' };
    }
    const res = await cartApi.addItem(Number(productId), quantity);
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      setCart(res.data.items.map(mapGioHangItemToCartItem));
    }
    return { ok: true, message: res.message };
  };

  const updateCartQuantity = async (
    productId: string,
    newQuantity: number
  ): Promise<{ ok: boolean; confirmRemove?: boolean; message: string }> => {
    if (!currentUser || currentUser.role !== 'KhachHang') {
      return { ok: false, message: 'Vui lòng đăng nhập bằng tài khoản Khách hàng.' };
    }
    if (newQuantity <= 0) {
      return {
        ok: false,
        confirmRemove: true,
        message: 'Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?',
      };
    }
    const res = await cartApi.updateQuantity(Number(productId), newQuantity);
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      setCart(res.data.items.map(mapGioHangItemToCartItem));
    }
    return { ok: true, message: res.message };
  };

  const removeFromCart = async (productId: string): Promise<ActionResult> => {
    if (!currentUser || currentUser.role !== 'KhachHang') {
      return { ok: false, message: 'Vui lòng đăng nhập bằng tài khoản Khách hàng.' };
    }
    const res = await cartApi.removeItem(Number(productId));
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      setCart(res.data.items.map(mapGioHangItemToCartItem));
    }
    return { ok: true, message: res.message };
  };

  const toggleWishlist = async (
    productId: string
  ): Promise<{ added: boolean; message: string }> => {
    if (!currentUser || currentUser.role !== 'KhachHang') {
      return {
        added: false,
        message: 'Vui lòng đăng nhập bằng tài khoản Khách hàng để lưu sản phẩm yêu thích.',
      };
    }

    const exists = wishlistProductIds.includes(productId);
    const res = exists
      ? await cartApi.removeWishlist(Number(productId))
      : await cartApi.addWishlist(Number(productId));

    if (!res.ok) {
      return { added: exists, message: res.message };
    }

    setWishlistProductIds((prev) =>
      exists ? prev.filter((id) => id !== productId) : [...prev, productId]
    );
    return { added: !exists, message: res.message };
  };

  // ============================================================================
  // Addresses, Vouchers & Checkout (UC-07, UC-34..37, UC-40..41)
  // ============================================================================
  const addShippingAddress = async (
    input: Omit<ShippingAddress, 'id' | 'userId'>
  ): Promise<{ ok: boolean; address?: ShippingAddress; message: string }> => {
    if (!currentUser) return { ok: false, message: 'Vui lòng đăng nhập để thêm địa chỉ.' };

    const res = await addressApi.create({
      tenNguoiNhan: input.recipientName.trim(),
      soDienThoai: input.phone.trim(),
      tinhThanh: input.province.trim(),
      quanHuyen: input.district.trim(),
      phuongXa: input.ward.trim(),
      diaChiCuThe: input.detailAddress.trim(),
      macDinh: input.isDefault,
    });

    if (!res.ok || !res.data) {
      return { ok: false, message: res.message };
    }

    const mapped = mapDiaChiToShippingAddress(res.data, currentUser.id);
    const listRes = await addressApi.list();
    if (listRes.ok && listRes.data) {
      setAddresses(listRes.data.map((a) => mapDiaChiToShippingAddress(a, currentUser.id)));
    } else {
      setAddresses((prev) => [...prev, mapped]);
    }
    return { ok: true, address: mapped, message: res.message };
  };

  const updateShippingAddress = async (
    id: string,
    input: Omit<ShippingAddress, 'id' | 'userId'>
  ): Promise<ActionResult> => {
    if (!currentUser) return { ok: false, message: 'Vui lòng đăng nhập.' };

    const res = await addressApi.update(Number(id), {
      tenNguoiNhan: input.recipientName.trim(),
      soDienThoai: input.phone.trim(),
      tinhThanh: input.province.trim(),
      quanHuyen: input.district.trim(),
      phuongXa: input.ward.trim(),
      diaChiCuThe: input.detailAddress.trim(),
      macDinh: input.isDefault,
    });
    if (!res.ok) return { ok: false, message: res.message };

    const listRes = await addressApi.list();
    if (listRes.ok && listRes.data) {
      setAddresses(listRes.data.map((a) => mapDiaChiToShippingAddress(a, currentUser.id)));
    }
    return { ok: true, message: res.message };
  };

  const deleteShippingAddress = async (id: string): Promise<ActionResult> => {
    if (!currentUser) return { ok: false, message: 'Vui lòng đăng nhập.' };

    const res = await addressApi.remove(Number(id));
    if (!res.ok) return { ok: false, message: res.message };

    const listRes = await addressApi.list();
    if (listRes.ok && listRes.data) {
      setAddresses(listRes.data.map((a) => mapDiaChiToShippingAddress(a, currentUser.id)));
    } else {
      setAddresses((prev) => prev.filter((a) => a.id !== id));
    }
    return { ok: true, message: res.message };
  };

  const setDefaultShippingAddress = async (id: string): Promise<ActionResult> => {
    if (!currentUser) return { ok: false, message: 'Vui lòng đăng nhập.' };

    const res = await addressApi.setDefault(Number(id));
    if (!res.ok) return { ok: false, message: res.message };

    setAddresses((prev) => prev.map((a) => ({ ...a, isDefault: a.id === id })));
    return { ok: true, message: res.message };
  };

  const validateVoucherCode = async (
    code: string,
    _subtotal: number
  ): Promise<VoucherValidationResult> => {
    const normalized = code.trim().toUpperCase();
    if (!normalized) {
      return { ok: false, discountAmount: 0, message: 'Vui lòng nhập mã giảm giá.' };
    }

    const res = await checkoutApi.applyVoucher(normalized);
    if (!res.ok || !res.data) {
      return { ok: false, discountAmount: 0, message: res.message };
    }

    const matched = vouchers.find(
      (v) => v.id === String(res.data!.maGiamGiaId) || v.code.toUpperCase() === res.data!.ma.toUpperCase()
    );

    return {
      ok: true,
      voucher: matched,
      discountAmount: Number(res.data.tienGiam) || 0,
      message: res.message,
    };
  };

  const placeOrder = async (params: {
    addressId: string;
    paymentMethod: PaymentMethod;
    voucherCode?: string;
  }): Promise<{ ok: boolean; order?: Order; message: string }> => {
    if (!currentUser || currentUser.role !== 'KhachHang') {
      return { ok: false, message: 'Vui lòng đăng nhập bằng tài khoản Khách hàng để đặt hàng.' };
    }

    const res = await checkoutApi.confirmOrder({
      diaChiId: Number(params.addressId),
      maGiamGia: params.voucherCode?.trim() || null,
      phuongThucThanhToan: params.paymentMethod,
    });

    if (!res.ok || !res.data) {
      return { ok: false, message: res.message };
    }

    const createdOrder = await loadOrderDetail(String(res.data.donHangId));
    setCart([]);
    await loadCatalogPublicData(currentUser.id);
    await hydrateOrdersWithDetails();

    return {
      ok: true,
      order: createdOrder || undefined,
      message: res.message,
    };
  };

  const cancelCustomerOrder = async (
    orderId: string,
    cancelReason: string
  ): Promise<ActionResult> => {
    const res = await orderApi.cancel(Number(orderId), cancelReason.trim());
    if (!res.ok) return { ok: false, message: res.message };

    await loadOrderDetail(orderId);
    await loadCatalogPublicData(currentUser?.id);
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Auth & Profile (UC-01..06)
  // ============================================================================
  const loginUser = async (
    identifier: string,
    password: string
  ): Promise<{ ok: boolean; user?: User; message: string }> => {
    const res = await authApi.login({
      taiKhoan: identifier.trim(),
      matKhau: password,
    });
    if (!res.ok || !res.data) {
      return { ok: false, message: res.message };
    }

    setAccessToken(res.data.accessToken, res.data.expiresAt);

    let loggedInUser: User = {
      id: String(res.data.taiKhoanId),
      code: `${res.data.vaiTro === 'QuanLy' ? 'QL' : res.data.vaiTro === 'NhanVien' ? 'NV' : 'KH'}${String(res.data.taiKhoanId).padStart(4, '0')}`,
      fullName: res.data.hoTen,
      email: res.data.email,
      phone: '',
      passwordHash: '',
      role: res.data.vaiTro,
      isLocked: false,
      failedLoginAttempts: 0,
      createdAt: new Date().toISOString(),
    };

    const profileRes = await accountApi.getProfile();
    if (profileRes.ok && profileRes.data) {
      loggedInUser = mapHoSoToUser(profileRes.data);
    }

    setCurrentUser(loggedInUser);
    await loadCatalogPublicData(loggedInUser.id);
    await hydrateRoleData(loggedInUser);

    return { ok: true, user: loggedInUser, message: res.message };
  };

  const registerUser = async (input: {
    fullName: string;
    email: string;
    phone: string;
    password: string;
    confirmPassword: string;
    securityQuestion: string;
    securityAnswer: string;
  }): Promise<{ ok: boolean; user?: User; message: string }> => {
    const regRes = await authApi.register({
      hoTen: input.fullName.trim(),
      email: input.email.trim(),
      soDienThoai: input.phone.trim(),
      matKhau: input.password,
      xacNhanMatKhau: input.confirmPassword,
      cauHoiBaoMat: input.securityQuestion.trim(),
      cauTraLoiBaoMat: input.securityAnswer.trim(),
    });

    if (!regRes.ok) {
      return { ok: false, message: regRes.message };
    }

    // Tự động đăng nhập ngay sau khi đăng ký thành công
    const loginRes = await loginUser(input.email.trim(), input.password);
    if (loginRes.ok) {
      return {
        ok: true,
        user: loginRes.user,
        message: 'Đăng ký tài khoản thành công! Đang chuyển hướng...',
      };
    }

    return { ok: true, message: regRes.message };
  };

  const logoutUser = useCallback(() => {
    clearSessionState();
    void loadCatalogPublicData(undefined);
  }, [clearSessionState, loadCatalogPublicData]);

  const lookupUserForRecovery = async (
    identifier: string
  ): Promise<{
    ok: boolean;
    accountIdentifier?: string;
    securityQuestion?: string;
    isLockedOut?: boolean;
    message: string;
  }> => {
    const res = await authApi.getSecurityQuestion(identifier.trim());
    if (!res.ok || !res.data) {
      return {
        ok: false,
        isLockedOut: res.status === 403 || res.status === 423 || res.status === 429,
        message: res.message,
      };
    }
    return {
      ok: true,
      accountIdentifier: res.data.emailOrPhone,
      securityQuestion: res.data.cauHoiBaoMat,
      message: 'Đã tìm thấy tài khoản. Vui lòng trả lời câu hỏi bảo mật bên dưới.',
    };
  };

  const resetPasswordBySecurityAnswer = async (params: {
    accountIdentifier: string;
    answer: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<{ ok: boolean; isLockedOut?: boolean; message: string }> => {
    const res = await authApi.forgotPassword({
      taiKhoan: params.accountIdentifier.trim(),
      cauTraLoiBaoMat: params.answer.trim(),
      matKhauMoi: params.newPassword,
      xacNhanMatKhauMoi: params.confirmPassword,
    });
    if (!res.ok) {
      return {
        ok: false,
        isLockedOut: res.status === 403 || res.status === 423 || res.status === 429,
        message: res.message,
      };
    }
    return { ok: true, message: res.message };
  };

  const changePassword = async (params: {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<ActionResult> => {
    const res = await authApi.changePassword({
      matKhauCu: params.currentPassword,
      matKhauMoi: params.newPassword,
      xacNhanMatKhauMoi: params.confirmPassword,
    });
    return { ok: res.ok, message: res.message };
  };

  const updateSecurityQuestion = async (params: {
    currentPassword: string;
    securityQuestion: string;
    securityAnswer: string;
  }): Promise<ActionResult> => {
    const res = await authApi.updateSecurityQuestion({
      cauHoiBaoMat: params.securityQuestion.trim(),
      cauTraLoiBaoMat: params.securityAnswer.trim(),
      matKhauHienTai: params.currentPassword,
    });
    if (res.ok) {
      setCurrentUser((prev) =>
        prev ? { ...prev, securityQuestion: params.securityQuestion.trim() } : prev
      );
    }
    return { ok: res.ok, message: res.message };
  };

  const updateProfile = async (params: {
    fullName: string;
    phone: string;
    dob?: string;
    gender?: Gender;
    avatar?: string;
  }): Promise<ActionResult> => {
    const res = await accountApi.updateProfile({
      hoTen: params.fullName.trim(),
      soDienThoai: params.phone.trim(),
      ngaySinh: params.dob || null,
      gioiTinh: params.gender || null,
      anhDaiDien: params.avatar === '' ? '' : stripBaseUrlForSave(params.avatar),
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      setCurrentUser(mapHoSoToUser(res.data));
    }
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Reviews (UC-53..57)
  // ============================================================================
  const submitReview = async (
    productId: string,
    rating: number,
    comment: string,
    images?: string[],
    orderId?: string
  ): Promise<ActionResult> => {
    if (!currentUser || currentUser.role !== 'KhachHang') {
      return { ok: false, message: 'Vui lòng đăng nhập bằng tài khoản Khách hàng để đánh giá.' };
    }

    const firstImage =
      images && images.length > 0 ? stripBaseUrlForSave(images[0]) : null;

    // Kiểm tra xem người dùng đã có đánh giá cho (productId, orderId) chưa → PUT (UC-54) hay POST (UC-53)
    const existing = reviews.find(
      (r) =>
        r.productId === productId &&
        r.userId === currentUser.id &&
        (!orderId || r.orderId === orderId)
    );

    if (existing && (!orderId || existing.orderId === orderId)) {
      const res = await reviewApi.update(Number(existing.id), {
        soSao: rating,
        noiDung: comment.trim(),
        hinhAnh: firstImage,
      });
      if (!res.ok) return { ok: false, message: res.message };
      if (res.data) {
        const mapped = mapDanhGiaToReview(res.data, currentUser.id);
        setReviews((prev) => prev.map((r) => (r.id === mapped.id ? mapped : r)));
      }
      return { ok: true, message: res.message };
    }

    if (!orderId) {
      return {
        ok: false,
        message: 'Không xác định được đơn hàng hoàn thành để gửi đánh giá.',
      };
    }

    const res = await reviewApi.create({
      sanPhamId: Number(productId),
      donHangId: Number(orderId),
      soSao: rating,
      noiDung: comment.trim(),
      hinhAnh: firstImage,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapDanhGiaToReview(res.data, currentUser.id);
      setReviews((prev) => [mapped, ...prev]);
    }
    return { ok: true, message: res.message };
  };

  const deleteReview = async (reviewId: string): Promise<ActionResult> => {
    const res = await reviewApi.remove(Number(reviewId));
    if (!res.ok) return { ok: false, message: res.message };
    setReviews((prev) => prev.filter((r) => r.id !== reviewId));
    return { ok: true, message: res.message };
  };

  const toggleReviewLike = async (reviewId: string): Promise<ActionResult> => {
    if (!currentUser) {
      return { ok: false, message: 'Vui lòng đăng nhập để thích đánh giá.' };
    }
    const res = await reviewApi.toggleLike(Number(reviewId));
    if (!res.ok || !res.data) return { ok: false, message: res.message };

    const { daThich, soLuotThich } = res.data;
    setReviews((prev) =>
      prev.map((r) => {
        if (r.id !== reviewId) return r;
        const likedByUserIds: string[] = [];
        if (daThich) likedByUserIds.push(currentUser.id);
        const extra = Math.max(0, soLuotThich - likedByUserIds.length);
        for (let i = 0; i < extra; i += 1) {
          likedByUserIds.push(`__like_${reviewId}_${i}`);
        }
        return {
          ...r,
          likedByUserIds,
          likeCount: soLuotThich,
          isLikedByMe: daThich,
        };
      })
    );
    return { ok: true, message: daThich ? 'Đã thích đánh giá.' : 'Đã bỏ thích đánh giá.' };
  };

  const replyToReview = async (
    reviewId: string,
    replyContent: string
  ): Promise<ActionResult> => {
    const res = await reviewApi.reply(Number(reviewId), replyContent.trim());
    if (!res.ok || !res.data) return { ok: false, message: res.message };

    setReviews((prev) =>
      prev.map((r) =>
        r.id === reviewId
          ? {
              ...r,
              storeReply: res.data!.noiDung,
              storeReplyAt: res.data!.ngayCapNhat || res.data!.ngayTao,
              storeReplyBy: res.data!.tenNguoiTraLoi,
            }
          : r
      )
    );
    return { ok: true, message: res.message };
  };

  const adminDeleteReview = async (
    reviewId: string,
    reason: string
  ): Promise<ActionResult> => {
    const res = await reviewApi.remove(Number(reviewId), reason.trim());
    if (!res.ok) return { ok: false, message: res.message };
    setReviews((prev) => prev.filter((r) => r.id !== reviewId));
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Staff Order Processing (UC-41, UC-43, UC-44)
  // ============================================================================
  const staffConfirmOrRejectOrder = async (
    orderId: string,
    action: 'CONFIRM' | 'REJECT',
    reasonOrNote?: string
  ): Promise<ActionResult> => {
    const res =
      action === 'CONFIRM'
        ? await orderApi.confirm(Number(orderId))
        : await orderApi.cancel(Number(orderId), (reasonOrNote || '').trim());

    if (!res.ok) return { ok: false, message: res.message };

    await loadOrderDetail(orderId);
    if (action === 'REJECT') {
      await loadCatalogPublicData(currentUser?.id);
    }
    return { ok: true, message: res.message };
  };

  const staffAdvanceOrderStatus = async (
    orderId: string,
    nextStatus: OrderStatus,
    note?: string
  ): Promise<ActionResult> => {
    const res = await orderApi.updateStatus(Number(orderId), nextStatus, note?.trim());
    if (!res.ok) return { ok: false, message: res.message };

    await loadOrderDetail(orderId);
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Admin Category & Product Management (UC-13..16, UC-20..23)
  // ============================================================================
  const createCategory = async (input: {
    name: string;
    description: string;
    imageUrl?: string;
    isVisible?: boolean;
  }): Promise<ActionResult> => {
    const res = await categoryApi.create({
      ten: input.name.trim(),
      moTa: input.description.trim() || null,
      hinhAnh: stripBaseUrlForSave(input.imageUrl),
      hienThi: input.isVisible ?? true,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      setCategories((prev) => [...prev, mapDanhMucToCategory(res.data!)]);
    }
    return { ok: true, message: res.message };
  };

  const updateCategory = async (
    id: string,
    input: { name: string; description: string; imageUrl?: string }
  ): Promise<ActionResult> => {
    const existing = categories.find((c) => c.id === id);
    const res = await categoryApi.update(Number(id), {
      ten: input.name.trim(),
      moTa: input.description.trim() || null,
      hinhAnh: stripBaseUrlForSave(input.imageUrl),
      hienThi: existing?.isVisible ?? true,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapDanhMucToCategory(res.data);
      setCategories((prev) => prev.map((c) => (c.id === id ? mapped : c)));
    }
    return { ok: true, message: res.message };
  };

  const toggleCategoryVisibility = async (id: string): Promise<ActionResult> => {
    const existing = categories.find((c) => c.id === id);
    if (!existing) return { ok: false, message: 'Không tìm thấy danh mục.' };

    const res = await categoryApi.toggleVisibility(Number(id), !existing.isVisible);
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapDanhMucToCategory(res.data);
      setCategories((prev) => prev.map((c) => (c.id === id ? mapped : c)));
    }
    await loadCatalogPublicData(currentUser?.id);
    return { ok: true, message: res.message };
  };

  const deleteCategory = async (id: string): Promise<ActionResult> => {
    const res = await categoryApi.remove(Number(id));
    if (!res.ok) return { ok: false, message: res.message };
    setCategories((prev) => prev.filter((c) => c.id !== id));
    return { ok: true, message: res.message };
  };

  const createProduct = async (input: {
    code: string;
    name: string;
    categoryId: string;
    price: number;
    description: string;
    imageUrl?: string;
    minStockThreshold: number;
    initialStock: number;
    isVisible?: boolean;
  }): Promise<ActionResult> => {
    const res = await productApi.create({
      danhMucId: Number(input.categoryId),
      maSanPham: input.code.trim(),
      ten: input.name.trim(),
      moTa: input.description.trim(),
      hinhAnh: stripBaseUrlForSave(input.imageUrl),
      giaBan: input.price,
      soLuongBanDau: input.initialStock,
      nguongToiThieu: input.minStockThreshold,
      hienThi: input.isVisible ?? true,
    });
    if (!res.ok) return { ok: false, message: res.message };

    await loadCatalogPublicData(currentUser?.id);
    if (currentUser?.role === 'QuanLy') {
      await hydrateInventoryForManager();
    }
    return { ok: true, message: res.message };
  };

  const updateProduct = async (
    id: string,
    input: {
      name: string;
      categoryId: string;
      price: number;
      description: string;
      imageUrl?: string;
    }
  ): Promise<ActionResult> => {
    const existing = products.find((p) => p.id === id);
    if (!existing) return { ok: false, message: 'Không tìm thấy sản phẩm.' };

    const res = await productApi.update(Number(id), {
      danhMucId: Number(input.categoryId),
      maSanPham: existing.code,
      ten: input.name.trim(),
      moTa: input.description.trim(),
      hinhAnh: stripBaseUrlForSave(input.imageUrl),
      giaBan: input.price,
      hienThi: existing.isVisible,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapSanPhamToProduct(res.data, existing);
      setProducts((prev) => prev.map((p) => (p.id === id ? mapped : p)));
    }
    return { ok: true, message: res.message };
  };

  const toggleProductVisibility = async (id: string): Promise<ActionResult> => {
    const existing = products.find((p) => p.id === id);
    if (!existing) return { ok: false, message: 'Không tìm thấy sản phẩm.' };

    const res = await productApi.toggleVisibility(Number(id), !existing.isVisible);
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapSanPhamToProduct(res.data, existing);
      setProducts((prev) => prev.map((p) => (p.id === id ? mapped : p)));
    }
    return { ok: true, message: res.message };
  };

  const deleteProduct = async (id: string): Promise<ActionResult> => {
    const res = await productApi.remove(Number(id));
    if (!res.ok) return { ok: false, message: res.message };
    setProducts((prev) => prev.filter((p) => p.id !== id));
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Admin Inventory Management (UC-24..28)
  // ============================================================================
  const importStock = async (params: {
    productId: string;
    quantity: number;
    importPrice?: number;
    supplier?: string;
    note: string;
  }): Promise<ActionResult> => {
    const res = await inventoryApi.importStock({
      sanPhamId: Number(params.productId),
      soLuongNhap: params.quantity,
      giaNhap: params.importPrice ?? null,
      nhaCungCap: params.supplier?.trim() || null,
      ghiChu: params.note.trim() || null,
    });
    if (!res.ok) return { ok: false, message: res.message };

    await hydrateInventoryForManager();
    return { ok: true, message: res.message };
  };

  const adjustStockDecrease = async (params: {
    productId: string;
    quantityToSubtract: number;
    reason: string;
  }): Promise<ActionResult> => {
    const res = await inventoryApi.adjustDecrease({
      sanPhamId: Number(params.productId),
      soLuongGiam: params.quantityToSubtract,
      lyDo: params.reason.trim(),
    });
    if (!res.ok) return { ok: false, message: res.message };

    await hydrateInventoryForManager();
    return { ok: true, message: res.message };
  };

  const updateMinStockThreshold = async (
    productId: string,
    threshold: number
  ): Promise<ActionResult> => {
    let tonKhoId = products.find((p) => p.id === productId)?.tonKhoId;

    if (!tonKhoId) {
      const invRes = await inventoryApi.list({ page: 1, pageSize: 100 });
      if (invRes.ok && invRes.data) {
        tonKhoId = invRes.data.items.find((tk) => String(tk.sanPhamId) === productId)?.tonKhoId;
      }
    }

    if (!tonKhoId) {
      return { ok: false, message: 'Không tìm thấy bản ghi tồn kho của sản phẩm này.' };
    }

    const res = await inventoryApi.updateMinThreshold(tonKhoId, threshold);
    if (!res.ok) return { ok: false, message: res.message };

    await hydrateInventoryForManager();
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Admin Voucher Management (UC-48..50)
  // ============================================================================
  const createVoucher = async (input: {
    code: string;
    description?: string;
    discountType: DiscountType;
    discountValue: number;
    maxDiscountAmount?: number;
    minOrderValue: number;
    startDate: string;
    endDate: string;
    usageLimit: number;
    isActive?: boolean;
  }): Promise<ActionResult> => {
    const res = await voucherApi.create({
      ma: input.code.trim().toUpperCase(),
      moTa: input.description?.trim() || null,
      loai: input.discountType,
      giaTri: input.discountValue,
      giamToiDa: input.discountType === 'PhanTram' ? input.maxDiscountAmount ?? null : null,
      donToiThieu: input.minOrderValue,
      ngayBatDau: toIsoDateOffset(input.startDate, false),
      ngayKetThuc: toIsoDateOffset(input.endDate, true),
      soLuotToiDa: input.usageLimit,
      hienThi: input.isActive ?? true,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      setVouchers((prev) => [mapMaGiamGiaToVoucher(res.data!), ...prev]);
    }
    return { ok: true, message: res.message };
  };

  const updateVoucher = async (
    id: string,
    input: {
      code: string;
      description?: string;
      discountType: DiscountType;
      discountValue: number;
      maxDiscountAmount?: number;
      minOrderValue: number;
      startDate: string;
      endDate: string;
      usageLimit: number;
    }
  ): Promise<ActionResult> => {
    const existing = vouchers.find((v) => v.id === id);
    const res = await voucherApi.update(Number(id), {
      ma: input.code.trim().toUpperCase(),
      moTa: input.description?.trim() || null,
      loai: input.discountType,
      giaTri: input.discountValue,
      giamToiDa: input.discountType === 'PhanTram' ? input.maxDiscountAmount ?? null : null,
      donToiThieu: input.minOrderValue,
      ngayBatDau: toIsoDateOffset(input.startDate, false),
      ngayKetThuc: toIsoDateOffset(input.endDate, true),
      soLuotToiDa: input.usageLimit,
      hienThi: existing?.isActive ?? true,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapMaGiamGiaToVoucher(res.data);
      setVouchers((prev) => prev.map((v) => (v.id === id ? mapped : v)));
    }
    return { ok: true, message: res.message };
  };

  const toggleVoucherActive = async (id: string): Promise<ActionResult> => {
    const existing = vouchers.find((v) => v.id === id);
    if (!existing) return { ok: false, message: 'Không tìm thấy mã giảm giá.' };

    const res = await voucherApi.update(Number(id), {
      ma: existing.code,
      moTa: existing.description || null,
      loai: existing.discountType,
      giaTri: existing.discountValue,
      giamToiDa:
        existing.discountType === 'PhanTram' ? existing.maxDiscountAmount ?? null : null,
      donToiThieu: existing.minOrderValue,
      ngayBatDau: existing.startDate,
      ngayKetThuc: existing.endDate,
      soLuotToiDa: existing.usageLimit,
      hienThi: !existing.isActive,
    });
    if (!res.ok) return { ok: false, message: res.message };
    if (res.data) {
      const mapped = mapMaGiamGiaToVoucher(res.data);
      setVouchers((prev) => prev.map((v) => (v.id === id ? mapped : v)));
    }
    return {
      ok: true,
      message: existing.isActive
        ? `Đã ngừng áp dụng mã ${existing.code}.`
        : `Đã kích hoạt lại mã ${existing.code}.`,
    };
  };

  const deleteVoucher = async (id: string): Promise<ActionResult> => {
    const res = await voucherApi.remove(Number(id));
    if (!res.ok) return { ok: false, message: res.message };

    // Tải lại danh sách vì mã đã dùng trong đơn hàng sẽ chuyển sang hienThi=false thay vì xóa hẳn
    const listRes = await voucherApi.list();
    if (listRes.ok && listRes.data) {
      setVouchers(listRes.data.map(mapMaGiamGiaToVoucher));
    } else {
      setVouchers((prev) => prev.filter((v) => v.id !== id));
    }
    return { ok: true, message: res.message };
  };

  // ============================================================================
  // Admin Account Management (UC-08..10)
  // ============================================================================
  const toggleAccountLock = async (
    userId: string,
    lockReason?: string
  ): Promise<ActionResult> => {
    const target = users.find((u) => u.id === userId);
    if (!target) return { ok: false, message: 'Không tìm thấy tài khoản.' };

    const res = target.isLocked
      ? await accountApi.unlockAccount(Number(userId))
      : await accountApi.lockAccount(Number(userId), (lockReason || '').trim());

    if (!res.ok) return { ok: false, message: res.message };

    const accRes = await accountApi.listAccounts({ page: 1, pageSize: 100 });
    if (accRes.ok && accRes.data) {
      setUsers(accRes.data.items.map(mapTaiKhoanItemToUser));
    }

    await loadAccountLockHistory(userId);

    const warning = res.data?.canhBao ? ` ${res.data.canhBao}` : '';
    return { ok: true, message: `${res.message}${warning}` };
  };

  return (
    <AppContext.Provider
      value={{
        isBootstrapping,
        apiConnectionError,
        currentUser,
        users,
        accountLockHistories,
        categories,
        products,
        stockHistories,
        vouchers,
        orders,
        reviews,
        addresses,
        cart,
        wishlistProductIds,
        searchQuery,
        selectedCategoryId,
        setSearchQuery,
        setSelectedCategoryId,
        refreshAllData,
        loadProductDetail,
        loadOrderDetail,
        loadAccountLockHistory,
        addToCart,
        updateCartQuantity,
        removeFromCart,
        toggleWishlist,
        addShippingAddress,
        updateShippingAddress,
        deleteShippingAddress,
        setDefaultShippingAddress,
        validateVoucherCode,
        placeOrder,
        cancelCustomerOrder,
        loginUser,
        registerUser,
        logoutUser,
        lookupUserForRecovery,
        resetPasswordBySecurityAnswer,
        changePassword,
        updateSecurityQuestion,
        updateProfile,
        submitReview,
        deleteReview,
        toggleReviewLike,
        staffConfirmOrRejectOrder,
        staffAdvanceOrderStatus,
        createCategory,
        updateCategory,
        toggleCategoryVisibility,
        deleteCategory,
        createProduct,
        updateProduct,
        toggleProductVisibility,
        deleteProduct,
        importStock,
        adjustStockDecrease,
        updateMinStockThreshold,
        createVoucher,
        updateVoucher,
        toggleVoucherActive,
        deleteVoucher,
        replyToReview,
        adminDeleteReview,
        toggleAccountLock,
      }}
    >
      {children}
    </AppContext.Provider>
  );
};

export const useApp = (): AppContextType => {
  const ctx = useContext(AppContext);
  if (!ctx) {
    throw new Error('useApp must be used within AppProvider');
  }
  return ctx;
};
