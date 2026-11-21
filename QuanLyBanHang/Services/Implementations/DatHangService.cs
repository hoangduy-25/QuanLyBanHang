using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.DatHang;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class DatHangService : IDatHangService
    {
        private const decimal MaxCodOrderValue = 20000000m;
        private readonly IUnitOfWork _uow;
        private readonly IGioHangService _gioHangService;

        public DatHangService(IUnitOfWork uow, IGioHangService gioHangService)
        {
            _uow = uow;
            _gioHangService = gioHangService;
        }

        // UC-34: Chọn và kiểm tra địa chỉ nhận hàng
        public async Task<ChonDiaChiResponse> ChonDiaChiAsync(Guid taiKhoanId, ChonDiaChiRequest request)
        {
            var dc = await _uow.DiaChis.GetByIdAndOwnerAsync(request.DiaChiId, taiKhoanId);
            if (dc == null)
                throw new NotFoundException("Địa chỉ nhận hàng không tồn tại trong sổ địa chỉ của bạn.");

            return new ChonDiaChiResponse
            {
                DiaChiId = dc.Id,
                TenNguoiNhan = dc.TenNguoiNhan,
                SoDienThoai = dc.SoDienThoai,
                DiaChiGiaoHang = string.Format("{0}, {1}, {2}, {3}", dc.DiaChiCuThe, dc.PhuongXa, dc.QuanHuyen, dc.TinhThanh),
                HoTroGiaoHang = true
            };
        }

        // UC-35: Áp dụng mã giảm giá (chưa trừ lượt dùng cho đến khi xác nhận đặt hàng UC-37)
        public async Task<ApDungMaGiamGiaResponse> ApDungMaGiamGiaAsync(Guid taiKhoanId, ApDungMaGiamGiaRequest request)
        {
            var gioHang = await _gioHangService.XemGioHangAsync(taiKhoanId);
            if (!gioHang.Items.Any(i => i.KhaDung))
                throw new BadRequestException("Giỏ hàng không có sản phẩm khả dụng để áp dụng mã giảm giá.");

            var voucher = await ValidateAndGetVoucherAsync(request.MaGiamGia, gioHang.TongTienTamTinh);
            decimal tienGiam = TinhTienGiam(voucher, gioHang.TongTienTamTinh);

            return new ApDungMaGiamGiaResponse
            {
                MaGiamGiaId = voucher.Id,
                Ma = voucher.Ma,
                TamTinh = gioHang.TongTienTamTinh,
                TienGiam = tienGiam,
                TongThanhToan = gioHang.TongTienTamTinh - tienGiam
            };
        }

        // UC-36: Chọn phương thức thanh toán
        public async Task<ChonPhuongThucThanhToanResponse> ChonPhuongThucThanhToanAsync(Guid taiKhoanId, ChonPhuongThucThanhToanRequest request)
        {
            var gioHang = await _gioHangService.XemGioHangAsync(taiKhoanId);

            // UC-36 1a: Thanh toán khi nhận hàng giới hạn giá trị đơn tối đa
            if (request.PhuongThucThanhToan == PhuongThucThanhToan.KhiNhanHang.ToString()
                && gioHang.TongTienTamTinh > MaxCodOrderValue)
            {
                throw new BadRequestException(string.Format("Thanh toán khi nhận hàng chỉ áp dụng cho đơn từ {0:N0} đ trở xuống. Vui lòng chọn Chuyển khoản hoặc Ví điện tử.", MaxCodOrderValue));
            }

            string huongDan = "Thanh toán tiền mặt trực tiếp cho nhân viên giao hàng khi nhận đơn.";
            if (request.PhuongThucThanhToan == PhuongThucThanhToan.ChuyenKhoan.ToString())
                huongDan = "Chuyển khoản ngân hàng theo mã đơn hàng hiển thị sau khi xác nhận đặt hàng.";
            else if (request.PhuongThucThanhToan == PhuongThucThanhToan.ViDienTu.ToString())
                huongDan = "Quét mã QR ví điện tử ngay sau khi xác nhận đặt hàng.";

            return new ChonPhuongThucThanhToanResponse
            {
                PhuongThucThanhToan = request.PhuongThucThanhToan,
                HuongDanThanhToan = huongDan
            };
        }

        // UC-40: Xem trước tóm tắt đơn hàng
        public async Task<XemTruocDatHangResponse> XemTruocAsync(Guid taiKhoanId, XemTruocDatHangRequest request)
        {
            var gioHang = await _gioHangService.XemGioHangAsync(taiKhoanId);
            var validItems = gioHang.Items.Where(i => i.KhaDung).ToList();
            if (!validItems.Any())
                throw new BadRequestException("Giỏ hàng trống hoặc không có sản phẩm khả dụng.");

            ChonDiaChiResponse diaChiResp = null;
            if (request != null && request.DiaChiId.HasValue)
            {
                diaChiResp = await ChonDiaChiAsync(taiKhoanId, new ChonDiaChiRequest { DiaChiId = request.DiaChiId.Value });
            }
            else
            {
                var macDinh = await _uow.DiaChis.GetMacDinhAsync(taiKhoanId);
                if (macDinh != null)
                {
                    diaChiResp = new ChonDiaChiResponse
                    {
                        DiaChiId = macDinh.Id,
                        TenNguoiNhan = macDinh.TenNguoiNhan,
                        SoDienThoai = macDinh.SoDienThoai,
                        DiaChiGiaoHang = string.Format("{0}, {1}, {2}, {3}", macDinh.DiaChiCuThe, macDinh.PhuongXa, macDinh.QuanHuyen, macDinh.TinhThanh),
                        HoTroGiaoHang = true
                    };
                }
            }

            decimal tamTinh = gioHang.TongTienTamTinh;
            decimal tienGiam = 0;
            string maApplied = null;

            if (request != null && !string.IsNullOrWhiteSpace(request.MaGiamGia))
            {
                var voucher = await ValidateAndGetVoucherAsync(request.MaGiamGia, tamTinh);
                tienGiam = TinhTienGiam(voucher, tamTinh);
                maApplied = voucher.Ma;
            }

            return new XemTruocDatHangResponse
            {
                DiaChi = diaChiResp,
                SanPhams = validItems,
                MaGiamGia = maApplied,
                PhuongThucThanhToan = request != null ? request.PhuongThucThanhToan : null,
                TamTinh = tamTinh,
                TienGiam = tienGiam,
                TongThanhToan = tamTinh - tienGiam
            };
        }

        // UC-37 & UC-40: Bấm xác nhận đặt hàng (Transaction nguyên tử: tạo đơn + trừ kho + ghi biến động + trừ lượt mã + xóa giỏ)
        public async Task<XacNhanDatHangResponse> XacNhanDatHangAsync(Guid taiKhoanId, XacNhanDatHangRequest request)
        {
            var diaChi = await _uow.DiaChis.GetByIdAndOwnerAsync(request.DiaChiId, taiKhoanId);
            if (diaChi == null)
                throw new NotFoundException("Vui lòng chọn địa chỉ nhận hàng hợp lệ.");

            var gh = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            if (gh == null || gh.ChiTietGioHangs == null || !gh.ChiTietGioHangs.Any())
                throw new BadRequestException("Giỏ hàng trống, không thể đặt hàng.");

            _uow.BeginTransaction();
            try
            {
                decimal tamTinh = 0;
                var chiTietDonHangs = new List<ChiTietDonHang>();
                var stockUpdates = new List<Tuple<TonKho, int, int>>();

                foreach (var ct in gh.ChiTietGioHangs.ToList())
                {
                    var sp = ct.SanPham;
                    if (sp == null || sp.IsDeleted || !sp.HienThi || (sp.DanhMuc != null && (!sp.DanhMuc.HienThi || sp.DanhMuc.IsDeleted)))
                        throw new BadRequestException(string.Format("Sản phẩm '{0}' không còn khả dụng. Vui lòng cập nhật lại giỏ hàng.", sp != null ? sp.Ten : ct.SanPhamId.ToString()));

                    var tonKho = await _uow.Khos.GetBySanPhamIdAsync(sp.Id);
                    if (tonKho == null || tonKho.SoLuongTon < ct.SoLuong)
                        throw new ConflictException(string.Format("Sản phẩm '{0}' không đủ tồn kho (còn lại: {1}). Vui lòng điều chỉnh giỏ hàng.", sp.Ten, tonKho != null ? tonKho.SoLuongTon : 0));

                    decimal donGia = sp.GiaBan;
                    tamTinh += donGia * ct.SoLuong;

                    chiTietDonHangs.Add(new ChiTietDonHang
                    {
                        SanPhamId = sp.Id,
                        DonGia = donGia,
                        SoLuong = ct.SoLuong
                    });

                    int truoc = tonKho.SoLuongTon;
                    tonKho.SoLuongTon = truoc - ct.SoLuong;
                    stockUpdates.Add(Tuple.Create(tonKho, ct.SoLuong, truoc));
                }

                if (request.PhuongThucThanhToan == PhuongThucThanhToan.KhiNhanHang.ToString() && tamTinh > MaxCodOrderValue)
                    throw new BadRequestException(string.Format("Đơn hàng vượt hạn mức thanh toán khi nhận hàng ({0:N0} đ).", MaxCodOrderValue));

                MaGiamGia voucher = null;
                decimal tienGiam = 0;
                if (!string.IsNullOrWhiteSpace(request.MaGiamGia))
                {
                    voucher = await ValidateAndGetVoucherAsync(request.MaGiamGia, tamTinh);
                    tienGiam = TinhTienGiam(voucher, tamTinh);
                    voucher.SoLuotDaDung += 1;
                }

                var maDon = string.Format("DH{0:yyyyMMddHHmmss}{1}", DateTime.Now, Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant());
                var donHang = new DonHang
                {
                    TaiKhoanId = taiKhoanId,
                    MaGiamGiaId = voucher != null ? (Guid?)voucher.Id : null,
                    MaDon = maDon,
                    NgayDat = DateTimeOffset.Now,
                    TrangThai = TrangThaiDonHang.ChoXacNhan.ToString(),
                    PhuongThucThanhToan = request.PhuongThucThanhToan,
                    DaThanhToan = false,
                    TenNguoiNhan = diaChi.TenNguoiNhan,
                    SoDienThoaiNhan = diaChi.SoDienThoai,
                    DiaChiGiaoHang = string.Format("{0}, {1}, {2}, {3}", diaChi.DiaChiCuThe, diaChi.PhuongXa, diaChi.QuanHuyen, diaChi.TinhThanh),
                    TamTinh = tamTinh,
                    TienGiam = tienGiam,
                    ChiTietDonHangs = chiTietDonHangs
                };

                _uow.DonHangs.Add(donHang);
                await _uow.SaveChangesAsync();

                // Ghi lịch sử biến động kho (XuatBan)
                foreach (var upd in stockUpdates)
                {
                    _uow.Khos.AddBienDong(new BienDongKho
                    {
                        TonKhoId = upd.Item1.Id,
                        TaiKhoanId = taiKhoanId,
                        DonHangId = donHang.Id,
                        Loai = LoaiBienDongKho.XuatBan.ToString(),
                        SoLuongThayDoi = upd.Item2,
                        SoLuongTruoc = upd.Item3,
                        SoLuongSau = upd.Item3 - upd.Item2,
                        LyDo = string.Format("Xuất bán cho đơn hàng {0}", donHang.MaDon),
                        ThoiDiem = DateTimeOffset.Now
                    });
                }

                // Ghi lịch sử trạng thái đơn hàng ban đầu
                _uow.DonHangs.AddLichSu(new LichSuDonHang
                {
                    DonHangId = donHang.Id,
                    TaiKhoanId = taiKhoanId,
                    TrangThai = TrangThaiDonHang.ChoXacNhan.ToString(),
                    ThoiDiem = DateTimeOffset.Now,
                    GhiChu = "Khách hàng tạo đơn hàng"
                });

                // Làm trống giỏ hàng
                foreach (var ct in gh.ChiTietGioHangs.ToList())
                    _uow.GioHangs.RemoveChiTiet(ct);

                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();

                return new XacNhanDatHangResponse
                {
                    DonHangId = donHang.Id,
                    MaDon = donHang.MaDon,
                    TrangThai = donHang.TrangThai,
                    TongThanhToan = tamTinh - tienGiam,
                    NgayDat = donHang.NgayDat
                };
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        private async Task<MaGiamGia> ValidateAndGetVoucherAsync(string ma, decimal tamTinh)
        {
            var voucher = await _uow.MaGiamGias.GetActiveByMaAsync(ma);
            if (voucher == null || !voucher.HienThi)
                throw new BadRequestException("Mã giảm giá không hợp lệ.");

            var now = DateTimeOffset.Now;
            if (now < voucher.NgayBatDau)
                throw new BadRequestException("Mã giảm giá chưa đến thời gian áp dụng.");
            if (now > voucher.NgayKetThuc)
                throw new BadRequestException("Mã giảm giá đã hết hạn sử dụng.");
            if (voucher.SoLuotDaDung >= voucher.SoLuotToiDa)
                throw new BadRequestException("Mã giảm giá đã hết lượt sử dụng.");
            if (tamTinh < voucher.DonToiThieu)
                throw new BadRequestException(string.Format("Đơn hàng chưa đạt giá trị tối thiểu {0:N0} đ để áp dụng mã này.", voucher.DonToiThieu));

            return voucher;
        }

        private static decimal TinhTienGiam(MaGiamGia voucher, decimal tamTinh)
        {
            decimal giam;
            if (voucher.Loai == LoaiGiamGia.PhanTram.ToString())
            {
                giam = Math.Round(tamTinh * voucher.GiaTri / 100m, 2);
                if (voucher.GiamToiDa.HasValue && giam > voucher.GiamToiDa.Value)
                    giam = voucher.GiamToiDa.Value;
            }
            else
            {
                giam = voucher.GiaTri;
            }

            return giam > tamTinh ? tamTinh : giam;
        }
    }
}
