import React, { useEffect, useRef, useState } from 'react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import {
  AlertCircle,
  ChevronDown,
  Heart,
  LayoutDashboard,
  LogIn,
  LogOut,
  MapPin,
  Package,
  RefreshCw,
  Search,
  ShoppingBag,
  TicketPercent,
  User as UserIcon,
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

  const visibleCategories = categories.filter((c) => c.isVisible && !c.isDeleted);
  const totalCartItems = cart.reduce((sum, item) => sum + item.quantity, 0);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className="min-h-screen flex flex-col bg-[#f8f9fa] text-zinc-900">
      {/* Two-Row Header */}
      <header className="sticky top-0 z-30 bg-[#f8f9fa]/95 backdrop-blur-md border-b border-zinc-200/80">
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

            {/* Right Actions: Cart + User Profile */}
            <div className="flex items-center gap-2.5 shrink-0">
              <button
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

              {/* User Profile Dropdown */}
              <div className="relative" ref={menuRef}>
                <button
                  onClick={() => setMenuOpen((prev) => !prev)}
                  className="flex items-center gap-2.5 pl-1.5 pr-3 h-9 rounded-full bg-white border border-zinc-200/80 hover:bg-zinc-100 transition-colors focus:outline-none"
                >
                  {currentUser?.avatar ? (
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
                      {currentUser ? currentUser.fullName : 'Khách vãng lai'}
                    </p>
                    <p className="text-[10px] text-zinc-400 mt-0.5">
                      {currentUser ? currentUser.email : 'Đăng nhập / Đăng ký'}
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
                        navigate(currentUser ? '/account' : '/auth');
                      }}
                      className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                    >
                      {currentUser ? (
                        <UserIcon className="w-4 h-4 text-zinc-400" />
                      ) : (
                        <LogIn className="w-4 h-4 text-blue-500" />
                      )}
                      <span>{currentUser ? 'Hồ sơ cá nhân' : 'Đăng nhập / Đăng ký'}</span>
                    </button>
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

                    {(currentUser?.role === 'NhanVien' ||
                      currentUser?.role === 'QuanLy') && (
                      <>
                        <div className="my-1.5 border-t border-zinc-100" />
                        <button
                          onClick={() => {
                            setMenuOpen(false);
                            navigate(
                              currentUser.role === 'NhanVien'
                                ? '/portal/orders'
                                : '/portal'
                            );
                          }}
                          className="w-full flex items-center gap-2.5 px-3.5 py-2 hover:bg-zinc-100 text-zinc-700 text-left"
                        >
                          <LayoutDashboard className="w-4 h-4 text-zinc-400" />
                          <span>Mở trang Quản trị (/portal)</span>
                        </button>
                      </>
                    )}

                    {currentUser && (
                      <>
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
                      </>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Row 2: Minimal Category Bar */}
          <nav className="flex items-center gap-7 pl-[106px] pt-3 overflow-x-auto no-scrollbar">
            {visibleCategories.map((cat, idx) => {
              const shortLabel = String(idx + 1).padStart(2, '0');
              const isSelected = selectedCategoryId === cat.id;
              return (
                <button
                  key={cat.id}
                  onClick={() => {
                    setSelectedCategoryId(isSelected ? 'ALL' : cat.id);
                    if (location.pathname !== '/') navigate('/');
                  }}
                  title={cat.name}
                  className={`text-xs transition-colors focus:outline-none ${
                    isSelected
                      ? 'text-zinc-900 font-semibold'
                      : 'text-zinc-500 hover:text-zinc-900 font-normal'
                  }`}
                >
                  {shortLabel}
                </button>
              );
            })}
          </nav>
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
