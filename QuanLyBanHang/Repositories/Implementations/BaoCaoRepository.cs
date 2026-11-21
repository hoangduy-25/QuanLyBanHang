using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.BaoCao;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class BaoCaoRepository : IBaoCaoRepository
    {
        private readonly QuanLyBanHangDbContext _context;

        public BaoCaoRepository(QuanLyBanHangDbContext context)
        {
            _context = context;
        }

        // UC-58: Gọi trực tiếp Table-Valued Function dbo.fn_BaoCaoTongQuan
        public BaoCaoTongQuanDTO GetBaoCaoTongQuan(DateTimeOffset? tuNgay, DateTimeOffset? denNgay)
        {
            return _context.LayBaoCaoTongQuan<BaoCaoTongQuanDTO>(tuNgay, denNgay) ?? new BaoCaoTongQuanDTO();
        }

        // UC-59: Tổng hợp doanh thu đơn Hoàn thành theo ngày / tuần / tháng / năm
        public async Task<List<DoanhThuDTO>> GetDoanhThuAsync(DateTimeOffset tuNgay, DateTimeOffset denNgay, string donVi)
        {
            var hoanThanh = TrangThaiDonHang.HoanThanh.ToString();
            var rawOrders = await _context.DonHangs.AsNoTracking()
                .Where(d => d.TrangThai == hoanThanh && d.NgayDat >= tuNgay && d.NgayDat <= denNgay)
                .Select(d => new { d.NgayDat, d.TongThanhToan })
                .ToListAsync();

            var unit = (donVi ?? "ngay").Trim().ToLower();
            var cal = CultureInfo.InvariantCulture.Calendar;

            var grouped = rawOrders
                .GroupBy(o =>
                {
                    var dt = o.NgayDat.LocalDateTime;
                    if (unit == "nam") return dt.ToString("yyyy");
                    if (unit == "thang") return dt.ToString("yyyy-MM");
                    if (unit == "tuan")
                    {
                        var week = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                        return string.Format("{0}-W{1:D2}", dt.Year, week);
                    }
                    return dt.ToString("yyyy-MM-dd");
                })
                .OrderBy(g => g.Key)
                .Select(g => new DoanhThuDTO
                {
                    MocThoiGian = g.Key,
                    SoDonHoanThanh = g.Count(),
                    DoanhThu = g.Sum(x => x.TongThanhToan)
                })
                .ToList();

            return grouped;
        }

        // UC-60: Top sản phẩm bán chạy từ các đơn Hoàn thành
        public async Task<List<SanPhamBanChayDTO>> GetSanPhamBanChayAsync(SanPhamBanChayQuery query)
        {
            var hoanThanh = TrangThaiDonHang.HoanThanh.ToString();

            var q = _context.ChiTietDonHangs.AsNoTracking()
                .Where(ct => ct.DonHang.TrangThai == hoanThanh);

            if (query.TuNgay.HasValue)
                q = q.Where(ct => ct.DonHang.NgayDat >= query.TuNgay.Value);

            if (query.DenNgay.HasValue)
                q = q.Where(ct => ct.DonHang.NgayDat <= query.DenNgay.Value);

            if (query.DanhMucId.HasValue)
                q = q.Where(ct => ct.SanPham.DanhMucId == query.DanhMucId.Value);

            var data = await q
                .GroupBy(ct => new { ct.SanPhamId, ct.SanPham.MaSanPham, ct.SanPham.Ten })
                .Select(g => new SanPhamBanChayDTO
                {
                    SanPhamId = g.Key.SanPhamId,
                    MaSanPham = g.Key.MaSanPham,
                    TenSanPham = g.Key.Ten,
                    SoLuongBan = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.ThanhTien)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .ThenByDescending(x => x.DoanhThu)
                .Take(query.Top)
                .ToListAsync();

            for (int i = 0; i < data.Count; i++)
                data[i].ThuHang = i + 1;

            return data;
        }

        // UC-61: Sản phẩm còn tồn > 0 nhưng không phát sinh đơn bán trong N ngày gần nhất
        public async Task<List<SanPhamTonLauDTO>> GetSanPhamTonLauAsync(SanPhamTonLauQuery query)
        {
            var now = DateTimeOffset.Now;
            var mocThoiGian = now.AddDays(-query.SoNgayKhongBan);
            var daHuy = TrangThaiDonHang.DaHuy.ToString();

            var q = _context.TonKhos.AsNoTracking()
                .Where(tk => !tk.SanPham.IsDeleted && tk.SoLuongTon > 0);

            if (query.DanhMucId.HasValue)
                q = q.Where(tk => tk.SanPham.DanhMucId == query.DanhMucId.Value);

            var candidates = await q
                .Select(tk => new
                {
                    tk.SanPhamId,
                    tk.SanPham.MaSanPham,
                    TenSanPham = tk.SanPham.Ten,
                    tk.SoLuongTon,
                    tk.SanPham.GiaBan,
                    tk.SanPham.NgayTao,
                    NgayBanGanNhat = _context.ChiTietDonHangs
                        .Where(ct => ct.SanPhamId == tk.SanPhamId && ct.DonHang.TrangThai != daHuy)
                        .Select(ct => (DateTimeOffset?)ct.DonHang.NgayDat)
                        .Max()
                })
                .Where(x => !x.NgayBanGanNhat.HasValue || x.NgayBanGanNhat.Value < mocThoiGian)
                .ToListAsync();

            return candidates
                .Select(x =>
                {
                    var mocSoSanh = x.NgayBanGanNhat ?? x.NgayTao;
                    var soNgay = Math.Max(0, (int)(now - mocSoSanh).TotalDays);
                    return new SanPhamTonLauDTO
                    {
                        SanPhamId = x.SanPhamId,
                        MaSanPham = x.MaSanPham,
                        TenSanPham = x.TenSanPham,
                        SoLuongTon = x.SoLuongTon,
                        NgayBanGanNhat = x.NgayBanGanNhat,
                        SoNgayTon = soNgay,
                        GiaTriTon = x.SoLuongTon * x.GiaBan
                    };
                })
                .Where(x => x.SoNgayTon >= query.SoNgayKhongBan)
                .OrderByDescending(x => x.SoNgayTon)
                .ToList();
        }
    }
}
