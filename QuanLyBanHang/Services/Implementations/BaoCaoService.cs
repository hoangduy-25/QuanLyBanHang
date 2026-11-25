using QuanLyBanHang.Models.DTOs.BaoCao;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyBanHang.Services.Implementations
{
    public class BaoCaoService : IBaoCaoService
    {
        private readonly IUnitOfWork _uow;

        public BaoCaoService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // UC-58: Xem thống kê tổng quan (mặc định 30 ngày gần nhất)
        public Task<BaoCaoTongQuanDTO> XemTongQuanAsync(DateTimeOffset? tuNgay, DateTimeOffset? denNgay)
        {
            var den = denNgay ?? DateTimeOffset.Now;
            var tu = tuNgay ?? den.AddDays(-30);

            if (tu > den)
                throw new BadRequestException("Khoảng thời gian không hợp lệ (ngày bắt đầu sau ngày kết thúc).");

            var result = _uow.BaoCaos.GetBaoCaoTongQuan(tu, den);
            return Task.FromResult(result);
        }

        // UC-59: Xem biểu đồ doanh thu theo ngày / tuần / tháng / năm
        public async Task<BieuDoDoanhThuResponse> XemBieuDoDoanhThuAsync(DateTimeOffset? tuNgay, DateTimeOffset? denNgay, string donVi)
        {
            var den = denNgay ?? DateTimeOffset.Now;
            var tu = tuNgay ?? den.AddDays(-30);

            if (tu > den)
                throw new BadRequestException("Khoảng thời gian không hợp lệ (ngày bắt đầu sau ngày kết thúc).");

            var data = await _uow.BaoCaos.GetDoanhThuAsync(tu, den, donVi);
            return new BieuDoDoanhThuResponse
            {
                DuLieu = data,
                TongDoanhThu = data.Sum(x => x.DoanhThu),
                GiaTriCaoNhat = data.Any() ? data.Max(x => x.DoanhThu) : 0,
                GiaTriThapNhat = data.Any() ? data.Min(x => x.DoanhThu) : 0
            };
        }

        // UC-60: Báo cáo sản phẩm bán chạy
        public Task<List<SanPhamBanChayDTO>> XemSanPhamBanChayAsync(SanPhamBanChayQuery query)
        {
            var q = query ?? new SanPhamBanChayQuery();
            if (q.TuNgay.HasValue && q.DenNgay.HasValue && q.TuNgay.Value > q.DenNgay.Value)
                throw new BadRequestException("Khoảng thời gian không hợp lệ.");

            return _uow.BaoCaos.GetSanPhamBanChayAsync(q);
        }

        // UC-61: Báo cáo sản phẩm tồn kho lâu
        public Task<List<SanPhamTonLauDTO>> XemSanPhamTonLauAsync(SanPhamTonLauQuery query)
        {
            var q = query ?? new SanPhamTonLauQuery();
            if (q.SoNgayKhongBan <= 0)
                throw new BadRequestException("Số ngày không phát sinh bán phải lớn hơn 0.");

            return _uow.BaoCaos.GetSanPhamTonLauAsync(q);
        }

        // UC-62: Tạo bản in HTML chuẩn (sẵn sàng in hoặc xuất PDF trên trình duyệt/máy in)
        public async Task<string> InBaoCaoAsync(string loaiBaoCao, DateTimeOffset? tuNgay, DateTimeOffset? denNgay, string donVi, int top, int soNgayTonLau, string nguoiLap)
        {
            var type = (loaiBaoCao ?? "tong-quan").Trim().ToLowerInvariant();
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'/><title>Báo cáo thống kê</title>");
            sb.AppendLine("<style>body{font-family:Arial,sans-serif;padding:24px;}table{width:100%;border-collapse:collapse;margin-top:16px;}th,td{border:1px solid #ccc;padding:8px;text-align:left;}th{background:#f3f4f6;}</style>");
            sb.AppendLine("</head><body>");
            var safeAuthor = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(nguoiLap) ? "Quản lý" : nguoiLap);
            var kyBaoCao = (tuNgay.HasValue || denNgay.HasValue)
                ? string.Format("{0} → {1}", tuNgay.HasValue ? tuNgay.Value.ToString("dd/MM/yyyy") : "...", denNgay.HasValue ? denNgay.Value.ToString("dd/MM/yyyy") : "...")
                : "30 ngày gần nhất";

            sb.AppendFormat("<h2>BÁO CÁO THỐNG KÊ ({0})</h2>", WebUtility.HtmlEncode(type.ToUpperInvariant()));
            sb.AppendFormat("<p>Khoảng thời gian: <strong>{0}</strong> | Người lập: <strong>{1}</strong> | Thời điểm lập: {2:dd/MM/yyyy HH:mm}</p>",
                kyBaoCao, safeAuthor, DateTime.Now);

            if (type == "doanh-thu")
            {
                var r = await XemBieuDoDoanhThuAsync(tuNgay, denNgay, donVi);
                if (!r.DuLieu.Any())
                    throw new BadRequestException("Báo cáo không có dữ liệu để in.");

                sb.AppendFormat("<p>Tổng doanh thu: <strong>{0:N0} đ</strong></p>", r.TongDoanhThu);
                sb.AppendLine("<table><tr><th>Mốc thời gian</th><th>Số đơn hoàn thành</th><th>Doanh thu (VNĐ)</th></tr>");
                foreach (var row in r.DuLieu)
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2:N0}</td></tr>", row.MocThoiGian, row.SoDonHoanThanh, row.DoanhThu);
                sb.AppendLine("</table>");
            }
            else if (type == "ban-chay")
            {
                var list = await XemSanPhamBanChayAsync(new SanPhamBanChayQuery { TuNgay = tuNgay, DenNgay = denNgay, Top = top > 0 ? top : 10 });
                if (!list.Any())
                    throw new BadRequestException("Báo cáo không có dữ liệu để in.");

                sb.AppendLine("<table><tr><th>Hạng</th><th>Mã SP</th><th>Tên sản phẩm</th><th>SL bán</th><th>Doanh thu (VNĐ)</th></tr>");
                foreach (var row in list)
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4:N0}</td></tr>",
                        row.ThuHang,
                        WebUtility.HtmlEncode(row.MaSanPham),
                        WebUtility.HtmlEncode(row.TenSanPham),
                        row.SoLuongBan,
                        row.DoanhThu);
                sb.AppendLine("</table>");
            }
            else if (type == "ton-lau")
            {
                var list = await XemSanPhamTonLauAsync(new SanPhamTonLauQuery { SoNgayKhongBan = soNgayTonLau > 0 ? soNgayTonLau : 30 });
                if (!list.Any())
                    throw new BadRequestException("Báo cáo không có dữ liệu để in.");

                sb.AppendLine("<table><tr><th>Mã SP</th><th>Tên sản phẩm</th><th>Tồn kho</th><th>Số ngày tồn</th><th>Giá trị tồn (VNĐ)</th></tr>");
                foreach (var row in list)
                    sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4:N0}</td></tr>",
                        WebUtility.HtmlEncode(row.MaSanPham),
                        WebUtility.HtmlEncode(row.TenSanPham),
                        row.SoLuongTon,
                        row.SoNgayTon,
                        row.GiaTriTon);
                sb.AppendLine("</table>");
            }
            else
            {
                var r = await XemTongQuanAsync(tuNgay, denNgay);
                sb.AppendLine("<table><tr><th>Chỉ số</th><th>Giá trị</th></tr>");
                sb.AppendFormat("<tr><td>Tổng doanh thu</td><td>{0:N0} đ</td></tr>", r.TongDoanhThu);
                sb.AppendFormat("<tr><td>Tổng đơn hàng</td><td>{0}</td></tr>", r.TongDonHang);
                sb.AppendFormat("<tr><td>Đơn hoàn thành</td><td>{0}</td></tr>", r.DonHoanThanh);
                sb.AppendFormat("<tr><td>Đơn đã hủy</td><td>{0}</td></tr>", r.DonDaHuy);
                sb.AppendFormat("<tr><td>Khách hàng mới</td><td>{0}</td></tr>", r.TongKhachHangMoi);
                sb.AppendFormat("<tr><td>Sản phẩm sắp hết hàng</td><td>{0}</td></tr>", r.SoSanPhamSapHetHang);
                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
