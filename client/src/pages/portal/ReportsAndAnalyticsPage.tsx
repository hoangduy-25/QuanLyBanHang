import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  AlertCircle,
  ArrowDownToLine,
  FileText,
  Loader2,
  PanelLeft,
  Printer,
  RefreshCw,
  Send,
  TrendingDown,
  TrendingUp,
  X,
} from 'lucide-react';
import {
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { useApp } from '../../context/AppContext';
import {
  BaoCaoTongQuanDto,
  BieuDoDoanhThuResponseDto,
  SanPhamBanChayDto,
  SanPhamTonLauDto,
} from '../../services/backendDtos';
import { reportApi } from '../../services/hcshopApi';
import { OrderStatus } from '../../types/domain';
import {
  formatDate,
  formatDateTime,
  formatVND,
  ORDER_STATUS_META,
} from '../../utils/formatters';

type GranularityKey = 'Ngay' | 'Tuan' | 'Thang' | 'Nam';
type PrintReportType = 'tong-quan' | 'doanh-thu' | 'ban-chay' | 'ton-lau';

const GRANULARITY_LABELS: Record<GranularityKey, string> = {
  Ngay: 'Ngày',
  Tuan: 'Tuần',
  Thang: 'Tháng',
  Nam: 'Năm',
};

const GRANULARITY_API_PARAM: Record<GranularityKey, 'ngay' | 'tuan' | 'thang' | 'nam'> = {
  Ngay: 'ngay',
  Tuan: 'tuan',
  Thang: 'thang',
  Nam: 'nam',
};

const PRINT_REPORT_OPTIONS: {
  id: PrintReportType;
  label: string;
  desc: string;
}[] = [
  {
    id: 'tong-quan',
    label: 'Báo cáo tổng quan',
    desc: 'Doanh thu, tổng đơn, hoàn thành, đã hủy, khách hàng mới và sản phẩm sắp hết hàng',
  },
  {
    id: 'doanh-thu',
    label: 'Báo cáo doanh thu theo kỳ',
    desc: 'Bảng doanh thu và số đơn hoàn thành theo đơn vị Ngày / Tuần / Tháng / Năm đang chọn',
  },
  {
    id: 'ban-chay',
    label: 'Báo cáo sản phẩm bán chạy',
    desc: 'Bảng xếp hạng Top N sản phẩm theo số lượng bán và doanh thu trong kỳ',
  },
  {
    id: 'ton-lau',
    label: 'Báo cáo hàng tồn kho lâu',
    desc: 'Danh sách sản phẩm chưa phát sinh bán từ 30 / 60 / 90 ngày trở lên',
  },
];

function computeDefault30DayRange(): { start: string; end: string } {
  const now = new Date();
  const start = new Date(now.getTime() - 29 * 24 * 60 * 60 * 1000);
  return {
    start: start.toISOString().slice(0, 10),
    end: now.toISOString().slice(0, 10),
  };
}

function toIsoRangeStart(dateStr: string): string | undefined {
  return dateStr ? `${dateStr}T00:00:00+07:00` : undefined;
}

function toIsoRangeEnd(dateStr: string): string | undefined {
  return dateStr ? `${dateStr}T23:59:59+07:00` : undefined;
}

export const ReportsAndAnalyticsPage: React.FC = () => {
  const { currentUser, categories, products, stockHistories } = useApp();
  const navigate = useNavigate();

  const initial30Days = useMemo(() => computeDefault30DayRange(), []);
  const [granularity, setGranularity] = useState<GranularityKey>('Ngay');
  const [reportStartDate, setReportStartDate] = useState(initial30Days.start);
  const [reportEndDate, setReportEndDate] = useState(initial30Days.end);
  const [bestSellerCategoryId, setBestSellerCategoryId] = useState('ALL');
  const [topN, setTopN] = useState<5 | 10 | 20>(5);
  const [deadStockCategoryId, setDeadStockCategoryId] = useState('ALL');
  const [rightPanelMode, setRightPanelMode] = useState<'BEST_SELLERS' | 'STOCK'>('BEST_SELLERS');
  const [deadStockDays, setDeadStockDays] = useState<30 | 60 | 90>(30);

  // Real API report states (UC-58..61)
  const [overview, setOverview] = useState<BaoCaoTongQuanDto | null>(null);
  const [revenueChart, setRevenueChart] = useState<BieuDoDoanhThuResponseDto | null>(null);
  const [bestSellers, setBestSellers] = useState<SanPhamBanChayDto[]>([]);
  const [deadStockItems, setDeadStockItems] = useState<SanPhamTonLauDto[]>([]);
  const [isLoadingReports, setIsLoadingReports] = useState(false);
  const [reportError, setReportError] = useState<string | null>(null);

  // UC-62 Server HTML Print Dialog state
  const [showPrintModal, setShowPrintModal] = useState(false);
  const [printReportType, setPrintReportType] = useState<PrintReportType>('tong-quan');
  const [isPrinting, setIsPrinting] = useState(false);
  const [printError, setPrintError] = useState<string | null>(null);

  const dateRangeError =
    reportStartDate && reportEndDate && reportStartDate > reportEndDate
      ? 'Ngày bắt đầu không được lớn hơn ngày kết thúc.'
      : null;

  const activeProducts = useMemo(
    () => products.filter((p) => !p.isDeleted),
    [products]
  );

  // UC-58 & UC-59: Fetch Overview + Revenue Chart when date range or granularity changes
  useEffect(() => {
    if (dateRangeError || currentUser?.role !== 'QuanLy') return;
    let cancelled = false;

    const tuNgay = toIsoRangeStart(reportStartDate);
    const denNgay = toIsoRangeEnd(reportEndDate);

    setIsLoadingReports(true);
    setReportError(null);

    void Promise.all([
      reportApi.getOverview({ tuNgay, denNgay }),
      reportApi.getRevenueChart({
        tuNgay,
        denNgay,
        donVi: GRANULARITY_API_PARAM[granularity],
      }),
    ]).then(([ovRes, revRes]) => {
      if (cancelled) return;
      if (ovRes.ok && ovRes.data) {
        setOverview(ovRes.data);
      } else if (!ovRes.ok) {
        setReportError(ovRes.message);
      }

      if (revRes.ok && revRes.data) {
        setRevenueChart(revRes.data);
      } else if (!revRes.ok) {
        setReportError(revRes.message);
      }
      setIsLoadingReports(false);
    });

    return () => {
      cancelled = true;
    };
  }, [reportStartDate, reportEndDate, granularity, dateRangeError, currentUser?.role]);

  // UC-60: Fetch Best-Selling Products
  useEffect(() => {
    if (dateRangeError || currentUser?.role !== 'QuanLy') return;
    let cancelled = false;

    void reportApi
      .getBestSellers({
        tuNgay: toIsoRangeStart(reportStartDate),
        denNgay: toIsoRangeEnd(reportEndDate),
        danhMucId: bestSellerCategoryId !== 'ALL' ? Number(bestSellerCategoryId) : undefined,
        top: topN,
      })
      .then((res) => {
        if (cancelled) return;
        if (res.ok && res.data) {
          setBestSellers(res.data);
        } else if (!res.ok) {
          setReportError(res.message);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [reportStartDate, reportEndDate, bestSellerCategoryId, topN, dateRangeError, currentUser?.role]);

  // UC-61: Fetch Slow-Moving / Dead-Stock Products
  useEffect(() => {
    if (currentUser?.role !== 'QuanLy') return;
    let cancelled = false;

    void reportApi
      .getDeadStock({
        soNgayKhongBan: deadStockDays,
        danhMucId: deadStockCategoryId !== 'ALL' ? Number(deadStockCategoryId) : undefined,
      })
      .then((res) => {
        if (cancelled) return;
        if (res.ok && res.data) {
          setDeadStockItems(res.data);
        } else if (!res.ok) {
          setReportError(res.message);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [deadStockDays, deadStockCategoryId, currentUser?.role]);

  const orderStatusBreakdown: { status: OrderStatus; count: number }[] = useMemo(
    () => [
      { status: 'ChoXacNhan', count: overview?.donChoXacNhan ?? 0 },
      { status: 'DaXacNhan', count: overview?.donDaXacNhan ?? 0 },
      { status: 'DangChuanBi', count: overview?.donDangChuanBi ?? 0 },
      { status: 'DangGiao', count: overview?.donDangGiao ?? 0 },
      { status: 'HoanThanh', count: overview?.donHoanThanh ?? 0 },
      { status: 'DaHuy', count: overview?.donDaHuy ?? 0 },
    ],
    [overview]
  );

  const revenueSeries = useMemo(
    () =>
      (revenueChart?.duLieu || []).map((pt) => ({
        label: pt.mocThoiGian,
        value: Number(pt.doanhThu) || 0,
        orders: pt.soDonHoanThanh,
      })),
    [revenueChart]
  );

  const { maxRevenuePoint, minRevenuePoint } = useMemo(() => {
    if (revenueSeries.length === 0) {
      return { maxRevenuePoint: null, minRevenuePoint: null };
    }
    let maxPt = revenueSeries[0];
    let minPt = revenueSeries[0];
    for (const pt of revenueSeries) {
      if (pt.value > maxPt.value) maxPt = pt;
      if (pt.value < minPt.value) minPt = pt;
    }
    return { maxRevenuePoint: maxPt, minRevenuePoint: minPt };
  }, [revenueSeries]);

  const topSeller = bestSellers[0];
  const totalRevenue = overview?.tongDoanhThu ?? revenueChart?.tongDoanhThu ?? 0;
  const totalOrders = overview?.tongDonHang ?? 0;
  const completedOrdersCount = overview?.donHoanThanh ?? 0;
  const newCustomersCount = overview?.tongKhachHangMoi ?? 0;
  const lowStockCount = overview?.soSanPhamSapHetHang ?? 0;

  const reportPeriodLabel =
    reportStartDate || reportEndDate
      ? `${reportStartDate || '...'} → ${reportEndDate || '...'}`
      : '30 ngày gần nhất';

  // UC-62: Gọi thẳng GET /api/bao-cao/in-bao-cao và mở cửa sổ in HTML từ Server
  const handlePrintServerHtml = async () => {
    if (isPrinting) return;
    if (dateRangeError) {
      setPrintError(dateRangeError);
      return;
    }

    setPrintError(null);
    setIsPrinting(true);
    try {
      const res = await reportApi.printHtmlReport({
        loaiBaoCao: printReportType,
        tuNgay: toIsoRangeStart(reportStartDate),
        denNgay: toIsoRangeEnd(reportEndDate),
        donVi: GRANULARITY_API_PARAM[granularity],
        top: topN,
        soNgayTonLau: deadStockDays,
      });

      if (!res.ok || !res.data) {
        setPrintError(res.message || 'Không có dữ liệu báo cáo để in.');
        return;
      }

      const printWin = window.open('', '_blank', 'width=960,height=720');
      if (!printWin) {
        setPrintError(
          'Trình duyệt đang chặn cửa sổ bật lên (pop-up). Vui lòng cho phép pop-up rồi bấm In lại.'
        );
        return;
      }

      printWin.document.open();
      printWin.document.write(res.data);
      printWin.document.close();
      printWin.focus();
      setTimeout(() => {
        printWin.print();
      }, 250);
      setShowPrintModal(false);
    } finally {
      setIsPrinting(false);
    }
  };

  return (
    <div className="space-y-6 pb-12">
      {/* Top Greeting & Quick Actions Bar */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <PanelLeft className="w-4 h-4 text-zinc-400" />
          <h1 className="text-base font-semibold text-zinc-900">
            Xin chào, {currentUser?.fullName || 'Quản lý'}
          </h1>
          {isLoadingReports && (
            <Loader2 className="w-3.5 h-3.5 text-zinc-400 animate-spin" />
          )}
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => navigate('/portal/orders')}
            className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-full bg-zinc-200/60 hover:bg-zinc-200 text-xs font-medium text-zinc-700 transition-colors"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Đơn hàng</span>
          </button>
          <button
            type="button"
            onClick={() => navigate('/portal/inventory')}
            className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-full bg-zinc-200/60 hover:bg-zinc-200 text-xs font-medium text-zinc-700 transition-colors"
          >
            <ArrowDownToLine className="w-3.5 h-3.5" />
            <span>Nhập kho</span>
          </button>
          <button
            type="button"
            onClick={() => {
              setPrintError(null);
              setShowPrintModal(true);
            }}
            className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-full bg-zinc-900 hover:bg-zinc-800 text-xs font-medium text-white transition-colors"
          >
            <Printer className="w-3.5 h-3.5" />
            <span>In báo cáo (UC-62)</span>
          </button>
          <button
            type="button"
            onClick={() => navigate('/')}
            className="inline-flex items-center gap-1.5 h-8 px-3.5 rounded-full bg-blue-50 hover:bg-blue-100 text-xs font-medium text-blue-600 transition-colors"
          >
            <Send className="w-3.5 h-3.5" />
            <span>HCShop</span>
          </button>
        </div>
      </div>

      {reportError && (
        <div className="flex items-center gap-2 px-4 py-3 rounded-2xl bg-rose-50 border border-rose-200 text-xs text-rose-700">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{reportError}</span>
        </div>
      )}

      {/* Custom Date Range Filter (default 30 days per UC-58) & 6-Status Order Breakdown */}
      <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 space-y-3">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex flex-wrap items-center gap-2 text-xs">
            <span className="font-semibold text-zinc-700">Kỳ báo cáo:</span>
            <input
              type="date"
              value={reportStartDate}
              onChange={(e) => setReportStartDate(e.target.value)}
              aria-label="Từ ngày"
              className="h-8 px-3 rounded-full bg-[#f4f4f5] text-zinc-800 border border-transparent focus:border-zinc-300 focus:bg-white focus:outline-none"
            />
            <span className="text-zinc-400">–</span>
            <input
              type="date"
              value={reportEndDate}
              onChange={(e) => setReportEndDate(e.target.value)}
              aria-label="Đến ngày"
              className="h-8 px-3 rounded-full bg-[#f4f4f5] text-zinc-800 border border-transparent focus:border-zinc-300 focus:bg-white focus:outline-none"
            />
            <button
              type="button"
              onClick={() => {
                const range = computeDefault30DayRange();
                setReportStartDate(range.start);
                setReportEndDate(range.end);
              }}
              className="h-8 px-3 rounded-full bg-zinc-100 hover:bg-zinc-200 text-zinc-700 font-medium"
            >
              30 ngày gần nhất
            </button>
            {(reportStartDate || reportEndDate) && (
              <button
                type="button"
                onClick={() => {
                  setReportStartDate('');
                  setReportEndDate('');
                }}
                className="h-8 px-3 rounded-full bg-zinc-100 hover:bg-zinc-200 text-zinc-600 font-medium"
              >
                Mặc định Server
              </button>
            )}
          </div>
          {dateRangeError && (
            <span className="text-xs text-rose-600 font-medium">{dateRangeError}</span>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-1.5 pt-1 border-t border-zinc-100">
          <span className="text-[11px] font-medium text-zinc-400 mr-1">
            Phân bổ trạng thái đơn:
          </span>
          {orderStatusBreakdown.map((item) => {
            const meta = ORDER_STATUS_META[item.status];
            return (
              <span
                key={item.status}
                className={`px-2.5 py-0.5 rounded-full text-[11px] font-medium ${meta.badgeClass}`}
              >
                {meta.label}: {item.count}
              </span>
            );
          })}
        </div>
      </div>

      {/* 6 KPI Cards (UC-58 from dbo.fn_BaoCaoTongQuan) */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
        <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 flex flex-col justify-between min-h-[112px]">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-zinc-500">Tổng doanh thu</span>
            <span className="text-[10px] font-medium px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-600 whitespace-nowrap shrink-0">
              Hoàn thành
            </span>
          </div>
          <div className="text-lg font-bold text-zinc-900 whitespace-nowrap">
            {formatVND(totalRevenue)}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 flex flex-col justify-between min-h-[112px]">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-zinc-500">Tổng đơn hàng</span>
            <span className="text-[10px] font-medium px-2 py-0.5 rounded-full bg-blue-50 text-blue-600 whitespace-nowrap shrink-0">
              {completedOrdersCount} hoàn thành
            </span>
          </div>
          <div className="text-2xl font-bold text-zinc-900 whitespace-nowrap">
            {totalOrders}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 flex flex-col justify-between min-h-[112px]">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-zinc-500">Khách hàng mới</span>
            <span className="text-[10px] font-medium px-2 py-0.5 rounded-full bg-indigo-50 text-indigo-600 whitespace-nowrap shrink-0">
              Tài khoản KH
            </span>
          </div>
          <div className="text-2xl font-bold text-zinc-900 whitespace-nowrap">
            {newCustomersCount}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 flex flex-col justify-between min-h-[112px]">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-zinc-500">Sắp hết hàng</span>
            <span className="text-[10px] font-medium px-2 py-0.5 rounded-full bg-amber-50 text-amber-700 whitespace-nowrap shrink-0">
              Cảnh báo tồn
            </span>
          </div>
          <div className="text-2xl font-bold text-amber-600 whitespace-nowrap">
            {lowStockCount}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 flex flex-col justify-between min-h-[112px]">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-zinc-500 truncate">
              Bán chạy · {topSeller?.maSanPham || 'N/A'}
            </span>
            <span className="text-[10px] font-medium px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-600 whitespace-nowrap shrink-0">
              Đã bán
            </span>
          </div>
          <div className="text-2xl font-bold text-zinc-900 whitespace-nowrap">
            {topSeller?.soLuongBan || 0}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 p-4 flex flex-col justify-between min-h-[112px]">
          <div className="flex items-center justify-between gap-2">
            <span className="text-xs text-zinc-500">Danh mục hàng</span>
            <span className="text-[10px] font-medium px-2 py-0.5 rounded-full bg-zinc-100 text-zinc-600 whitespace-nowrap shrink-0">
              Mặt hàng
            </span>
          </div>
          <div className="text-2xl font-bold text-zinc-900 whitespace-nowrap">
            {activeProducts.length}
          </div>
        </div>
      </div>

      {/* Middle Section: Revenue Chart (UC-59) + Right Ranking List (UC-60) */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left 7 Cols: Dynamic Revenue Chart Card */}
        <div className="lg:col-span-7 bg-white rounded-2xl border border-zinc-200/70 p-6 space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="text-sm font-semibold text-zinc-900">
              Biểu đồ doanh thu
            </span>
            <div className="flex items-center gap-1 bg-zinc-100 p-0.5 rounded-full text-xs">
              {(['Ngay', 'Tuan', 'Thang', 'Nam'] as GranularityKey[]).map((key) => (
                <button
                  key={key}
                  type="button"
                  onClick={() => setGranularity(key)}
                  className={`px-2.5 py-1 rounded-full font-medium transition-colors ${
                    granularity === key
                      ? 'bg-white text-blue-600 shadow-sm'
                      : 'text-zinc-500 hover:text-zinc-800'
                  }`}
                >
                  {GRANULARITY_LABELS[key]}
                </button>
              ))}
            </div>
          </div>

          <div className="flex flex-wrap items-end justify-between gap-3">
            <div className="space-y-1">
              <p className="text-xs text-zinc-400">Doanh thu lũy kế kỳ chọn</p>
              <p className="text-2xl font-semibold text-zinc-900 whitespace-nowrap">
                {formatVND(revenueChart?.tongDoanhThu ?? totalRevenue)}
              </p>
              <p className="text-xs text-zinc-500 font-medium">
                {completedOrdersCount} đơn hoàn thành · nhóm theo{' '}
                {GRANULARITY_LABELS[granularity].toLowerCase()}
              </p>
            </div>

            {maxRevenuePoint && minRevenuePoint && (
              <div className="flex flex-wrap items-center gap-2 text-[11px]">
                <div className="px-3 py-1.5 rounded-xl bg-emerald-50/80 border border-emerald-200/60 text-emerald-800 flex items-center gap-1.5">
                  <TrendingUp className="w-3.5 h-3.5 text-emerald-600 shrink-0" />
                  <span>
                    Cao nhất: <strong>{formatVND(maxRevenuePoint.value)}</strong> (
                    {maxRevenuePoint.label})
                  </span>
                </div>
                <div className="px-3 py-1.5 rounded-xl bg-amber-50/80 border border-amber-200/60 text-amber-800 flex items-center gap-1.5">
                  <TrendingDown className="w-3.5 h-3.5 text-amber-600 shrink-0" />
                  <span>
                    Thấp nhất: <strong>{formatVND(minRevenuePoint.value)}</strong> (
                    {minRevenuePoint.label})
                  </span>
                </div>
              </div>
            )}
          </div>

          <div className="h-56 w-full pt-2">
            {revenueSeries.length === 0 ? (
              <div className="h-full rounded-xl bg-[#f8f9fa] border border-dashed border-zinc-200 flex items-center justify-center text-xs text-zinc-400 px-4 text-center">
                Chưa có đơn hàng hoàn thành trong kỳ báo cáo ({reportPeriodLabel}) để hiển thị biểu đồ.
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={revenueSeries}>
                  <XAxis
                    dataKey="label"
                    axisLine={false}
                    tickLine={false}
                    tick={{ fontSize: 11, fill: '#a1a1aa' }}
                  />
                  <YAxis
                    axisLine={false}
                    tickLine={false}
                    tick={{ fontSize: 11, fill: '#a1a1aa' }}
                    tickFormatter={(v) =>
                      v >= 1000000 ? `${(v / 1000000).toFixed(1)}M` : `${Math.round(v / 1000)}k`
                    }
                    width={46}
                  />
                  <Tooltip
                    formatter={(val: number) => [formatVND(val), 'Doanh thu']}
                    contentStyle={{
                      borderRadius: '12px',
                      border: '1px solid #e4e4e7',
                      fontSize: '12px',
                    }}
                  />
                  <Line
                    type="monotone"
                    dataKey="value"
                    stroke="#0284c7"
                    strokeWidth={2}
                    dot={{ r: 4, fill: '#0284c7' }}
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Right 5 Cols: Best Sellers (UC-60) / Stock Holdings */}
        <div className="lg:col-span-5 space-y-4">
          <div className="flex flex-wrap items-center justify-between gap-2 px-1">
            <div className="flex items-center gap-2">
              <h3 className="text-sm font-semibold text-zinc-900">
                {rightPanelMode === 'BEST_SELLERS'
                  ? 'Sản phẩm bán chạy'
                  : 'Tồn kho nhanh'}
              </h3>
              <span className="text-xs text-zinc-400">
                {rightPanelMode === 'BEST_SELLERS'
                  ? bestSellers.length
                  : activeProducts.length}
              </span>
            </div>

            <div className="flex flex-wrap items-center gap-1.5">
              {rightPanelMode === 'BEST_SELLERS' && (
                <>
                  <select
                    value={topN}
                    onChange={(e) => setTopN(Number(e.target.value) as 5 | 10 | 20)}
                    aria-label="Số lượng Top sản phẩm"
                    className="h-7 px-2.5 rounded-full bg-white border border-zinc-200/80 text-[11px] font-medium text-zinc-700 focus:outline-none"
                  >
                    <option value={5}>Top 5</option>
                    <option value={10}>Top 10</option>
                    <option value={20}>Top 20</option>
                  </select>
                  <select
                    value={bestSellerCategoryId}
                    onChange={(e) => setBestSellerCategoryId(e.target.value)}
                    aria-label="Lọc danh mục bán chạy"
                    className="h-7 px-2.5 rounded-full bg-white border border-zinc-200/80 text-[11px] font-medium text-zinc-700 focus:outline-none"
                  >
                    <option value="ALL">Mọi danh mục</option>
                    {categories
                      .filter((c) => !c.isDeleted)
                      .map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.name}
                        </option>
                      ))}
                  </select>
                </>
              )}

              <div className="flex items-center gap-1 bg-zinc-200/60 p-0.5 rounded-full text-[11px]">
                <button
                  type="button"
                  onClick={() => setRightPanelMode('BEST_SELLERS')}
                  className={`px-2.5 py-0.5 rounded-full font-medium ${
                    rightPanelMode === 'BEST_SELLERS'
                      ? 'bg-white text-zinc-900 shadow-sm'
                      : 'text-zinc-500'
                  }`}
                >
                  Bán chạy
                </button>
                <button
                  type="button"
                  onClick={() => setRightPanelMode('STOCK')}
                  className={`px-2.5 py-0.5 rounded-full font-medium ${
                    rightPanelMode === 'STOCK'
                      ? 'bg-white text-zinc-900 shadow-sm'
                      : 'text-zinc-500'
                  }`}
                >
                  Tồn kho
                </button>
              </div>
            </div>
          </div>

          <div className="space-y-2.5 max-h-72 overflow-y-auto pr-1">
            {rightPanelMode === 'BEST_SELLERS' ? (
              bestSellers.length === 0 ? (
                <div className="p-6 text-center text-xs text-zinc-400 bg-white rounded-2xl border border-zinc-200/60">
                  Chưa có dữ liệu sản phẩm bán chạy trong kỳ chọn.
                </div>
              ) : (
                bestSellers.map((item, idx) => {
                  const colors = [
                    'from-amber-200 to-orange-400',
                    'from-sky-200 to-blue-500',
                    'from-zinc-200 to-zinc-500',
                    'from-yellow-200 to-amber-500',
                    'from-purple-200 to-indigo-500',
                  ];
                  return (
                    <div
                      key={item.sanPhamId}
                      className="flex items-center justify-between py-1.5 px-1"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div
                          className={`w-8 h-8 rounded-full bg-gradient-to-tr ${
                            colors[idx % colors.length]
                          } shrink-0 flex items-center justify-center text-[11px] font-bold text-zinc-900/80`}
                        >
                          #{item.thuHang || idx + 1}
                        </div>
                        <div className="min-w-0">
                          <p className="text-xs font-semibold text-zinc-900 truncate">
                            {item.maSanPham}
                          </p>
                          <p className="text-[11px] text-zinc-400 truncate">
                            {item.tenSanPham}
                          </p>
                        </div>
                      </div>

                      <div className="text-right shrink-0">
                        <p className="text-xs font-semibold text-zinc-900">
                          {formatVND(item.doanhThu)}
                        </p>
                        <p className="text-[11px] font-medium text-emerald-600">
                          Đã bán: {item.soLuongBan}
                        </p>
                      </div>
                    </div>
                  );
                })
              )
            ) : (
              activeProducts.slice(0, 5).map((item, idx) => {
                const colors = [
                  'from-amber-200 to-orange-400',
                  'from-sky-200 to-blue-500',
                  'from-zinc-200 to-zinc-500',
                  'from-yellow-200 to-amber-500',
                  'from-purple-200 to-indigo-500',
                ];
                const isLow = item.stock <= item.minStockThreshold;

                return (
                  <div
                    key={item.id}
                    className="flex items-center justify-between py-1.5 px-1"
                  >
                    <div className="flex items-center gap-3 min-w-0">
                      <div
                        className={`w-8 h-8 rounded-full bg-gradient-to-tr ${
                          colors[idx % colors.length]
                        } shrink-0`}
                      />
                      <div className="min-w-0">
                        <p className="text-xs font-semibold text-zinc-900 truncate">
                          {item.code}
                        </p>
                        <p className="text-[11px] text-zinc-400 truncate">
                          {item.name}
                        </p>
                      </div>
                    </div>

                    <div className="text-right shrink-0">
                      <p className="text-xs font-semibold text-zinc-900">
                        {formatVND(item.price)}
                      </p>
                      <p
                        className={`text-[11px] font-medium ${
                          isLow ? 'text-rose-500' : 'text-emerald-600'
                        }`}
                      >
                        {isLow ? `↓ Tồn: ${item.stock}` : `↑ Tồn: ${item.stock}`}
                      </p>
                    </div>
                  </div>
                );
              })
            )}
          </div>

          <button
            type="button"
            onClick={() => navigate('/portal/inventory')}
            className="w-full h-9 rounded-full bg-zinc-200/70 hover:bg-zinc-200 text-xs font-medium text-zinc-800 transition-colors"
          >
            Xem chi tiết kho hàng
          </button>
        </div>
      </div>

      {/* Slow-Moving / Dead-Stock Inventory Report (UC-61) */}
      <div className="space-y-3 pt-2">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <h3 className="text-sm font-semibold text-zinc-900">
              Hàng tồn đọng chậm luân chuyển
            </h3>
            <span className="text-xs text-zinc-400">{deadStockItems.length}</span>
          </div>

          <div className="flex flex-wrap items-center gap-2 text-xs">
            <select
              value={deadStockCategoryId}
              onChange={(e) => setDeadStockCategoryId(e.target.value)}
              className="h-7 px-3 rounded-full bg-white border border-zinc-200/80 text-xs font-medium text-zinc-700 focus:outline-none"
            >
              <option value="ALL">Tất cả danh mục</option>
              {categories
                .filter((c) => !c.isDeleted)
                .map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
            </select>

            {([30, 60, 90] as const).map((days) => (
              <button
                key={days}
                type="button"
                onClick={() => setDeadStockDays(days)}
                className={`px-3 py-1 rounded-full font-medium transition-colors ${
                  deadStockDays === days
                    ? 'bg-zinc-900 text-white'
                    : 'bg-zinc-200/60 text-zinc-600 hover:text-zinc-900'
                }`}
              >
                ≥ {days} ngày
              </button>
            ))}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 overflow-hidden">
          <div className="grid grid-cols-12 px-5 py-3 bg-zinc-100/70 text-[11px] font-medium text-zinc-500">
            <div className="col-span-4">Sản phẩm</div>
            <div className="col-span-2">Tồn hiện tại</div>
            <div className="col-span-2">Giá trị tồn</div>
            <div className="col-span-2">Lần bán cuối</div>
            <div className="col-span-2 text-right">Số ngày chưa bán</div>
          </div>

          <div className="divide-y divide-zinc-100">
            {deadStockItems.length === 0 ? (
              <div className="px-5 py-6 text-center text-xs text-zinc-400">
                Không có sản phẩm nào tồn đọng từ {deadStockDays} ngày trở lên trong bộ lọc hiện tại.
              </div>
            ) : (
              deadStockItems.map((item) => (
                <div
                  key={item.sanPhamId}
                  className="grid grid-cols-12 items-center px-5 py-3.5 text-xs"
                >
                  <div className="col-span-4">
                    <p className="font-semibold text-zinc-900">{item.tenSanPham}</p>
                    <p className="text-[11px] font-mono text-zinc-400">{item.maSanPham}</p>
                  </div>
                  <div className="col-span-2 font-semibold text-zinc-800">
                    {item.soLuongTon} đơn vị
                  </div>
                  <div className="col-span-2 font-medium text-zinc-800">
                    {formatVND(item.giaTriTon)}
                  </div>
                  <div className="col-span-2 text-zinc-500 text-[11px]">
                    {item.ngayBanGanNhat ? formatDate(item.ngayBanGanNhat) : 'Chưa từng bán'}
                  </div>
                  <div className="col-span-2 text-right">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full bg-amber-50 text-amber-700 text-[11px] font-medium">
                      {item.soNgayTon} ngày
                    </span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>

      {/* Bottom Section: Recent Stock Activity Table */}
      <div className="space-y-3 pt-2">
        <div className="flex items-center gap-2">
          <h3 className="text-sm font-semibold text-zinc-900">
            Hoạt động kho gần đây
          </h3>
          <span className="text-xs text-zinc-400">{stockHistories.length}</span>
        </div>

        <div className="bg-white rounded-2xl border border-zinc-200/70 overflow-hidden">
          <div className="grid grid-cols-12 px-5 py-3 bg-zinc-100/70 text-[11px] font-medium text-zinc-500">
            <div className="col-span-3">Mã giao dịch</div>
            <div className="col-span-3">Loại</div>
            <div className="col-span-2">Sản phẩm</div>
            <div className="col-span-2">Số lượng</div>
            <div className="col-span-2 text-right">Thời gian</div>
          </div>

          <div className="divide-y divide-zinc-100">
            {stockHistories.length === 0 ? (
              <div className="px-5 py-6 text-center text-xs text-zinc-400">
                Chưa có lịch sử biến động kho.
              </div>
            ) : (
              stockHistories.slice(0, 6).map((sh) => {
                const prod = products.find((p) => p.id === sh.productId);
                const isPositive = sh.quantityChange > 0;

                return (
                  <div
                    key={sh.id}
                    className="grid grid-cols-12 items-center px-5 py-3.5 text-xs"
                  >
                    <div className="col-span-3 font-mono text-zinc-600 truncate pr-2">
                      #{sh.id}
                    </div>
                    <div className="col-span-3">
                      {isPositive ? (
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-emerald-50 text-emerald-600 text-[11px] font-medium">
                          ↓ Nhập / Hoàn
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-zinc-100 text-zinc-700 text-[11px] font-medium">
                          <FileText className="w-3 h-3" /> Xuất / Giảm
                        </span>
                      )}
                    </div>
                    <div className="col-span-2 font-medium text-zinc-800">
                      {prod?.code || sh.productId}
                    </div>
                    <div className="col-span-2">
                      <p className="font-semibold text-zinc-900">
                        {isPositive ? `+${sh.quantityChange}` : sh.quantityChange} đơn vị
                      </p>
                      <p className="text-[11px] text-zinc-400">
                        Tồn sau: {sh.stockAfter}
                      </p>
                    </div>
                    <div className="col-span-2 text-right text-zinc-400 text-[11px]">
                      {formatDateTime(sh.createdAt)}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>

      {/* UC-62: Server HTML Print Report Selector Modal (calls GET /api/bao-cao/in-bao-cao) */}
      {showPrintModal && (
        <div className="fixed inset-0 z-50 bg-black/40 backdrop-blur-[2px] flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl border border-zinc-200/80 max-w-lg w-full p-6 shadow-xl space-y-5">
            <div className="flex items-start justify-between border-b border-zinc-100 pb-3.5">
              <div>
                <span className="text-[11px] font-semibold uppercase tracking-wider text-zinc-400">
                  UC-62 · GET /api/bao-cao/in-bao-cao
                </span>
                <h2 className="text-base font-bold text-zinc-900 mt-0.5">
                  Chọn loại bản in HTML từ máy chủ
                </h2>
                <p className="text-xs text-zinc-500 mt-0.5">
                  Kỳ áp dụng: {reportPeriodLabel} · Đơn vị: {GRANULARITY_LABELS[granularity]} · Top {topN} · Tồn ≥ {deadStockDays} ngày
                </p>
              </div>
              <button
                type="button"
                onClick={() => setShowPrintModal(false)}
                className="w-8 h-8 rounded-full bg-zinc-100 text-zinc-500 flex items-center justify-center"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="space-y-2">
              {PRINT_REPORT_OPTIONS.map((opt) => {
                const selected = printReportType === opt.id;
                return (
                  <label
                    key={opt.id}
                    className={`flex items-start gap-3 p-3.5 rounded-2xl border cursor-pointer transition-colors ${
                      selected
                        ? 'border-zinc-900 bg-zinc-50'
                        : 'border-zinc-200/80 hover:border-zinc-300'
                    }`}
                  >
                    <input
                      type="radio"
                      name="printReportType"
                      checked={selected}
                      onChange={() => setPrintReportType(opt.id)}
                      className="mt-0.5 accent-zinc-900"
                    />
                    <div className="space-y-0.5">
                      <p className="text-xs font-semibold text-zinc-900">{opt.label}</p>
                      <p className="text-[11px] text-zinc-500">{opt.desc}</p>
                    </div>
                  </label>
                );
              })}
            </div>

            {printError && (
              <div className="p-3 rounded-2xl bg-rose-50 border border-rose-200/80 flex items-start gap-2 text-xs text-rose-800">
                <AlertCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
                <span>{printError}</span>
              </div>
            )}

            <div className="flex items-center justify-end gap-2 pt-1">
              <button
                type="button"
                onClick={() => setShowPrintModal(false)}
                className="px-4 py-2 rounded-full border border-zinc-200 text-xs font-medium text-zinc-700"
              >
                Đóng
              </button>
              <button
                type="button"
                disabled={isPrinting || Boolean(dateRangeError)}
                onClick={() => void handlePrintServerHtml()}
                className="inline-flex items-center gap-1.5 px-5 py-2 rounded-full bg-zinc-900 hover:bg-zinc-800 disabled:opacity-50 text-white text-xs font-semibold"
              >
                {isPrinting ? (
                  <Loader2 className="w-3.5 h-3.5 animate-spin" />
                ) : (
                  <Printer className="w-3.5 h-3.5" />
                )}
                <span>{isPrinting ? 'Đang tạo bản in...' : 'Mở bản in HTML'}</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
