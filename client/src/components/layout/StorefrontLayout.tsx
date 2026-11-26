import React, { useEffect, useMemo, useRef, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import {
  AlertCircle,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  Heart,
  LayoutDashboard,
  LogOut,
  MapPin,
  Package,
  RefreshCw,
  Search,
  ShoppingBag,
  TicketPercent,
  User as UserIcon,
  X,
} from 'lucide-react';
import { useApp } from '../../context/AppContext';

export const StorefrontLayout: React.FC = () => {
  const {
    currentUser,
    apiConnectionError,
    categories,
    cart,
    wishlistProductIds,
    searchQuery,
    setSearchQuery,
    selectedCategoryId,
    setSelectedCategoryId,
    logoutUser,
    refreshAllData,
  } = useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const [catSearchOpen, setCatSearchOpen] = useState(false);
  const [categorySearch, setCategorySearch] = useState('');
  const catInputRef = useRef<HTMLInputElement>(null);
  const catNavRef = useRef<HTMLElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(false);

  const visibleCategories = useMemo(
    () => categories.filter((c) => c.isVisible && !c.isDeleted),
    [categories]
  );
  const matchedCategories = useMemo(() => {
    const q = categorySearch.trim().toLowerCase();
    if (!q) return visibleCategories;
    return visibleCategories.filter(
      (c) =>
        c.name.toLowerCase().includes(q) ||
        c.description.toLowerCase().includes(q)
    );
  }, [visibleCategories, categorySearch]);
  const totalCartItems = cart.reduce((sum, item) => sum + item.quantity, 0);

  const updateScrollButtons = () => {
    const el = catNavRef.current;
    if (!el) return;
    setCanScrollLeft(el.scrollLeft > 4);
    setCanScrollRight(el.scrollLeft + el.clientWidth < el.scrollWidth - 4);
  };

  useEffect(() => {
    updateScrollButtons();
    window.addEventListener('resize', updateScrollButtons);
    return () => window.removeEventListener('resize', updateScrollButtons);
  }, [matchedCategories.length, catSearchOpen]);

  useEffect(() => {
    if (catSearchOpen) {
      catInputRef.current?.focus();
    }
  }, [catSearchOpen]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const isAuthPage = location.pathname === '/auth';
  const isStaffOrAdmin =
    currentUser?.role === 'NhanVien' || currentUser?.role === 'QuanLy';
  const portalHomePath =
    currentUser?.role === 'NhanVien' ? '/portal/orders' : '/portal';

  return (
    <div
      className={`min-h-screen flex flex-col text-zinc-900 ${
        isAuthPage ? 'auth-doodle-bg' : 'bg-[#f8f9fa]'
      }`}
    >
      {/* Two-Row Header */}
      <header
        className={`sticky top-0 z-30 backdrop-blur-md border-b border-zinc-200/80 ${
          isAuthPage ? 'bg-white/95' : 'bg-[#f8f9fa]/95'
        }`}
      >
        <div className="max-w-5xl mx-auto px-6 pt-3.5 pb-2.5">
          {/* Row 1: Brand + Wide Search Input + Cart Pill + User Pill */}
          <div className="flex items-center gap-4">
            {/* Brand */}
            <button
              onClick={() => {
                setSelectedCategoryId('ALL');
                navigate('/');
              }}
              className="flex items-center gap-2.5 text-left focus:outline-none shrink-0"
            >
              <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-sky-200 via-blue-400 to-indigo-500 shrink-0" />
              <span className="font-bold text-base tracking-tight text-zinc-900">
                HCShop
              </span>
            </button>

            {/* Wide Search Bar */}
            <div className="relative flex-1">
              <Search className="w-3.5 h-3.5 text-zinc-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => {
                  setSearchQuery(e.target.value);
                  if (location.pathname !== '/') navigate('/');
                }}
                placeholder="Tìm kiếm..."
                className="w-full h-9 pl-9 pr-4 rounded-full bg-white border border-zinc-200/80 text-xs text-zinc-800 placeholder:text-zinc-400 focus:outline-none focus:border-zinc-400 transition-colors"
              />
            </div>

            {/* Right Actions: Cart (Customer/Guest) or Portal (Staff/Admin) + User Profile */}
            <div className="flex items-center gap-2.5 shrink-0">
              {isStaffOrAdmin ? (
                <button
                  type="button"
                  onClick={() => navigate(portalHomePath)}
                  className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-full bg-blue-50 text-blue-600 text-xs font-medium hover:bg-blue-100/80 transition-colors"
                >
                  <LayoutDashboard className="w-3.5 h-3.5" />
                  <span>Trang quản trị</span>
                </button>
              ) : (
                <button
                  type="button"
                  onClick={() => navigate('/cart')}
                  className="inline-flex items-center gap-1.5 h-9 px-3.5 rounded-full bg-blue-50 text-blue-600 text-xs font-medium hover:bg-blue-100/80 transition-colors"
                >
                  <ShoppingBag className="w-3.5 h-3.5" />
                  <span>Giỏ hàng</span>
                  {totalCartItems > 0 && (
                    <span className="ml-0.5 w-4 h-4 rounded-full bg-blue-600 text-white text-[10px] font-semibold inline-flex items-center justify-center">
                      {totalCartItems}
                    </span>
                  )}
                </button>
              )}

              {/* Guest Login Button OR Signed-In User Dropdown */}
              {!currentUser ? (
                <button
                  type="button"
                  onClick={() => navigate('/auth')}
                  className="inline-flex items-center gap-2 h-9 px-4 rounded-full bg-white border border-zinc-200/80 hover:bg-zinc-100 text-xs font-medium text-zinc-800 transition-colors focus:outline-none"
                >
                  <UserIcon className="w-3.5 h-3.5 text-zinc-700" />
                  <span>Đăng nhập</span>
                </button>
              ) : (
                <div className="relative" ref={menuRef}>
                  <button
                    onClick={() => setMenuOpen((prev) => !prev)}
                    className="flex items-center gap-2.5 pl-1.5 pr-3 h-9 rounded-full bg-white border border-zinc-200/80 hover:bg-zinc-100 transition-colors focus:outline-none"
                  >
                    {currentUser.avatar ? (
                      <img
                        src={currentUser.avatar}
                        alt={currentUser.fullName}
                        className="w-6 h-6 rounded-full object-cover border border-zinc-200 shrink-0"
                      />
                    ) : (
                      <div className="w-6 h-6 rounded-full bg-gradient-to-tr from-sky-200 via-indigo-300 to-purple-300 shrink-0" />
                    )}
                    <div className="hidden sm:block text-left leading-none">
                      <p className="text-[11px] font-semibold text-zinc-900">
                        {currentUser.fullName}
                      </p>
                      <p className="text-[10px] text-zinc-400 mt-0.5">
                        {currentUser.email}
                      </p>
                    </div>
                    <ChevronDown className="w-3.5 h-3.5 text-zinc-400 ml-1" />
                  </button>

                  {menuOpen && (
                    <div className="absolute right-0 mt-2 w-64 rounded-2xl bg-white border border-zinc-200/80 shadow-xl py-2 z-50 text-xs">
                      <div className="px-3.5 py-1.5 text-[11px] font-medium text-zinc-400">
                        Tài khoản cá nhân
                      </div>
                      <button
                        onClick={() => {
                          setMenuOpen(false);
                          navigate('/account');
                        }}
                        className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                      >
                        <UserIcon className="w-4 h-4 text-zinc-400" />
                        <span>Hồ sơ cá nhân</span>
                      </button>

                      {!isStaffOrAdmin && (
                        <>
                          <button
                            onClick={() => {
                              setMenuOpen(false);
                              navigate('/orders');
                            }}
                            className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                          >
                            <Package className="w-4 h-4 text-zinc-400" />
                            <span>Đơn hàng của tôi</span>
                          </button>
                          <button
                            onClick={() => {
                              setMenuOpen(false);
                              navigate('/wishlist');
                            }}
                            className="w-full flex items-center justify-between px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                          >
                            <span className="flex items-center gap-2.5">
                              <Heart className="w-4 h-4 text-zinc-400" />
                              <span>Sản phẩm yêu thích</span>
                            </span>
                            <span className="text-[11px] text-zinc-400">
                              {wishlistProductIds.length}
                            </span>
                          </button>
                          <button
                            onClick={() => {
                              setMenuOpen(false);
                              navigate('/vouchers');
                            }}
                            className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                          >
                            <TicketPercent className="w-4 h-4 text-zinc-400" />
                            <span>Kho mã giảm giá</span>
                          </button>
                          <button
                            onClick={() => {
                              setMenuOpen(false);
                              navigate('/account?tab=addresses');
                            }}
                            className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                          >
                            <MapPin className="w-4 h-4 text-zinc-400" />
                            <span>Sổ địa chỉ</span>
                          </button>
                        </>
                      )}

                      {isStaffOrAdmin && (
                        <>
                          <div className="my-1.5 border-t border-zinc-100" />
                          <button
                            onClick={() => {
                              setMenuOpen(false);
                              navigate(portalHomePath);
                            }}
                            className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                          >
                            <LayoutDashboard className="w-4 h-4 text-zinc-400" />
                            <span>Trang quản trị</span>
                          </button>
                        </>
                      )}

                      <div className="my-1.5 border-t border-zinc-100" />
                      <button
                        onClick={() => {
                          setMenuOpen(false);
                          logoutUser();
                          navigate('/');
                        }}
                        className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-rose-50 text-rose-600 text-left"
                      >
                        <LogOut className="w-4 h-4" />
                        <span>Đăng xuất</span>
                      </button>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>

          {/* Row 2: Category Navigation Bar + Compact UC-12 Search Icon */}
          <div className="flex items-center gap-2 pt-2.5">
            <div className="relative flex-1 min-w-0 flex items-center">
              {canScrollLeft && (
                <button
                  type="button"
                  onClick={() =>
                    catNavRef.current?.scrollBy({ left: -200, behavior: 'smooth' })
                  }
                  aria-label="Cuộn danh mục sang trái"
                  className="shrink-0 mr-1 w-6 h-6 rounded-full bg-white border border-zinc-200/80 text-zinc-500 hover:text-zinc-900 hover:bg-zinc-100 inline-flex items-center justify-center shadow-sm transition-colors"
                >
                  <ChevronLeft className="w-3.5 h-3.5" />
                </button>
              )}

              <nav
                ref={catNavRef}
                onScroll={updateScrollButtons}
                aria-label="Danh mục sản phẩm"
                className="flex items-center gap-1.5 overflow-x-auto no-scrollbar flex-1 py-0.5"
              >
                {!categorySearch.trim() && (
                  <button
                    type="button"
                    onClick={() => {
                      setSelectedCategoryId('ALL');
                      if (location.pathname !== '/') navigate('/');
                    }}
                    className={`h-7 px-3 rounded-full text-xs whitespace-nowrap transition-colors focus:outline-none ${
                      selectedCategoryId === 'ALL' && location.pathname === '/'
                        ? 'bg-zinc-900 text-white font-medium'
                        : 'text-zinc-600 hover:text-zinc-900 hover:bg-zinc-200/70 font-normal'
                    }`}
                  >
                    Tất cả
                  </button>
                )}

                {matchedCategories.length === 0 ? (
                  <span className="text-xs text-zinc-400 px-2 whitespace-nowrap">
                    Không tìm thấy danh mục &ldquo;{categorySearch}&rdquo;
                  </span>
                ) : (
                  matchedCategories.map((cat) => {
                    const isSelected =
                      selectedCategoryId === cat.id && location.pathname === '/';
                    return (
                      <button
                        key={cat.id}
                        type="button"
                        onClick={() => {
                          setSelectedCategoryId(isSelected ? 'ALL' : cat.id);
                          if (location.pathname !== '/') navigate('/');
                        }}
                        title={cat.description || cat.name}
                        className={`h-7 px-3 rounded-full text-xs whitespace-nowrap transition-colors focus:outline-none ${
                          isSelected
                            ? 'bg-zinc-900 text-white font-medium'
                            : 'text-zinc-600 hover:text-zinc-900 hover:bg-zinc-200/70 font-normal'
                        }`}
                      >
                        {cat.name}
                      </button>
                    );
                  })
                )}
              </nav>

              {canScrollRight && (
                <button
                  type="button"
                  onClick={() =>
                    catNavRef.current?.scrollBy({ left: 200, behavior: 'smooth' })
                  }
                  aria-label="Cuộn danh mục sang phải"
                  className="shrink-0 ml-1 w-6 h-6 rounded-full bg-white border border-zinc-200/80 text-zinc-500 hover:text-zinc-900 hover:bg-zinc-100 inline-flex items-center justify-center shadow-sm transition-colors"
                >
                  <ChevronRight className="w-3.5 h-3.5" />
                </button>
              )}
            </div>

            {/* UC-12: Compact Category Search Toggle */}
            <div className="shrink-0 flex items-center">
              {catSearchOpen ? (
                <div className="relative flex items-center">
                  <Search className="w-3.5 h-3.5 text-zinc-400 absolute left-2.5 pointer-events-none" />
                  <input
                    ref={catInputRef}
                    type="text"
                    maxLength={100}
                    value={categorySearch}
                    onChange={(e) => setCategorySearch(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Escape') {
                        setCategorySearch('');
                        setCatSearchOpen(false);
                      }
                    }}
                    onBlur={() => {
                      if (!categorySearch.trim()) {
                        setCatSearchOpen(false);
                      }
                    }}
                    placeholder="Tìm danh mục..."
                    aria-label="Tìm kiếm danh mục"
                    className="w-40 sm:w-52 h-7 pl-8 pr-7 rounded-full bg-white border border-zinc-300 text-xs text-zinc-800 placeholder:text-zinc-400 focus:outline-none focus:border-zinc-500 transition-all"
                  />
                  <button
                    type="button"
                    onMouseDown={(e) => e.preventDefault()}
                    onClick={() => {
                      setCategorySearch('');
                      setCatSearchOpen(false);
                    }}
                    aria-label="Đóng tìm kiếm danh mục"
                    className="absolute right-2 text-zinc-400 hover:text-zinc-700"
                  >
                    <X className="w-3.5 h-3.5" />
                  </button>
                </div>
              ) : (
                <button
                  type="button"
                  onClick={() => setCatSearchOpen(true)}
                  title="Tìm danh mục sản phẩm"
                  aria-label="Tìm danh mục sản phẩm"
                  className="w-7 h-7 rounded-full text-zinc-500 hover:text-zinc-900 hover:bg-zinc-200/70 inline-flex items-center justify-center transition-colors focus:outline-none"
                >
                  <Search className="w-3.5 h-3.5" />
                </button>
              )}
            </div>
          </div>
        </div>
      </header>

      {apiConnectionError && (
        <div className="max-w-5xl w-full mx-auto px-6 pt-4">
          <div className="flex items-center justify-between gap-3 px-4 py-3 rounded-2xl bg-amber-50 border border-amber-200/80 text-xs text-amber-900">
            <div className="flex items-center gap-2">
              <AlertCircle className="w-4 h-4 text-amber-600 shrink-0" />
              <span>{apiConnectionError}</span>
            </div>
            <button
              type="button"
              onClick={() => void refreshAllData()}
              className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-white border border-amber-300 text-amber-800 font-medium hover:bg-amber-100 shrink-0"
            >
              <RefreshCw className="w-3 h-3" />
              Thử lại
            </button>
          </div>
        </div>
      )}

      {/* Main Content */}
      <main className="flex-1 max-w-5xl w-full mx-auto px-6 py-8">
        <Outlet />
      </main>
    </div>
  );
};
