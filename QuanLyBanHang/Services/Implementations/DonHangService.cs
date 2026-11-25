using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.DonHang;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class DonHangService : IDonHangService
    {
        private readonly IUnitOfWork _uow;

        public DonHangService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // UC-38: Xem danh sách đơn hàng
        public async Task<PagedResult<DonHangListItemResponse>> XemDanhSachAsync(int nguoiDungId, string vaiTro, DonHangFilterQuery query)
        {
            var q = query ?? new DonHangFilterQuery();
            if (q.TuNgay.HasValue && q.DenNgay.HasValue && q.TuNgay.Value > q.DenNgay.Value)
                throw new BadRequestException("Khoảng thời gian lọc không hợp lệ.");

            int? filterTaiKhoan = vaiTro == VaiTro.KhachHang.ToString() ? (int?)nguoiDungId : null;
            var paged = await _uow.DonHangs.GetPagedAsync(q, filterTaiKhoan);

            return new PagedResult<DonHangListItemResponse>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(d => new DonHangListItemResponse
                {
                    Id = d.Id,
                    MaDon = d.MaDon,
                    TaiKhoanId = d.TaiKhoanId,
                    TenKhachHang = d.TaiKhoan != null ? d.TaiKhoan.HoTen : string.Empty,
                    NgayDat = d.NgayDat,
                    TrangThai = d.TrangThai,
                    PhuongThucThanhToan = d.PhuongThucThanhToan,
                    DaThanhToan = d.DaThanhToan,
                    TongThanhToan = d.TongThanhToan,
                    SoMatHang = d.ChiTietDonHangs != null ? d.ChiTietDonHangs.Count : 0
                }).ToList()
            };
        }

        // UC-39: Xem chi tiết đơn hàng
        public async Task<DonHangChiTietResponse> XemChiTietAsync(int nguoiDungId, string vaiTro, int donHangId)
        {
            var dh = await _uow.DonHangs.GetDetailByIdAsync(donHangId);
            if (dh == null || (vaiTro == VaiTro.KhachHang.ToString() && dh.TaiKhoanId != nguoiDungId))
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            return new DonHangChiTietResponse
            {
                Id = dh.Id,
                MaDon = dh.MaDon,
                TaiKhoanId = dh.TaiKhoanId,
                TenKhachHang = dh.TaiKhoan != null ? dh.TaiKhoan.HoTen : string.Empty,
                NgayDat = dh.NgayDat,
                TrangThai = dh.TrangThai,
                PhuongThucThanhToan = dh.PhuongThucThanhToan,
                DaThanhToan = dh.DaThanhToan,
                TenNguoiNhan = dh.TenNguoiNhan,
                SoDienThoaiNhan = dh.SoDienThoaiNhan,
                DiaChiGiaoHang = dh.DiaChiGiaoHang,
                MaGiamGia = dh.MaGiamGia != null ? dh.MaGiamGia.Ma : null,
                TamTinh = dh.TamTinh,
                TienGiam = dh.TienGiam,
                TongThanhToan = dh.TongThanhToan,
                LyDoHuy = dh.LyDoHuy,
                ChiTiets = dh.ChiTietDonHangs.Select(ct => new ChiTietDonHangItemResponse
                {
                    SanPhamId = ct.SanPhamId,
                    MaSanPham = ct.SanPham != null ? ct.SanPham.MaSanPham : string.Empty,
                    TenSanPham = ct.SanPham != null ? ct.SanPham.Ten : string.Empty,
                    HinhAnh = ct.SanPham != null ? ct.SanPham.HinhAnh : null,
                    DonGia = ct.DonGia,
                    SoLuong = ct.SoLuong,
                    ThanhTien = ct.ThanhTien
                }).ToList(),
                HanhTrinh = dh.LichSuDonHangs.OrderBy(ls => ls.ThoiDiem).Select(ls => new LichSuDonHangItemResponse
                {
                    Id = ls.Id,
                    TrangThai = ls.TrangThai,
                    ThoiDiem = ls.ThoiDiem,
                    NguoiThucHien = ls.TaiKhoan != null ? ls.TaiKhoan.HoTen : "Hệ thống",
                    GhiChu = ls.GhiChu
                }).ToList()
            };
        }

        // UC-41: Hủy đơn hàng (hoàn tồn kho + hoàn lượt mã giảm giá + ghi lịch sử)
        public async Task HuyAsync(int nguoiDungId, string vaiTro, int donHangId, HuyDonHangRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LyDoHuy))
                throw new BadRequestException("Vui lòng nhập lý do hủy đơn hàng.");
            var dh = await _uow.DonHangs.GetDetailByIdAsync(donHangId);
            if (dh == null)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (vaiTro == VaiTro.KhachHang.ToString() && dh.TaiKhoanId != nguoiDungId)
                throw new ForbiddenException("Bạn không có quyền hủy đơn hàng này.");

            if (dh.TrangThai != TrangThaiDonHang.ChoXacNhan.ToString() && dh.TrangThai != TrangThaiDonHang.DaXacNhan.ToString())
                throw new BadRequestException("Đơn hàng ở trạng thái hiện tại không thể hủy. Vui lòng liên hệ cửa hàng.");

            _uow.BeginTransaction();
            try
            {
                dh.TrangThai = TrangThaiDonHang.DaHuy.ToString();
                dh.LyDoHuy = request.LyDoHuy.Trim();

                // Hoàn tồn kho & ghi lịch sử biến động (HoanDon)
                foreach (var ct in dh.ChiTietDonHangs)
                {
                    var tonKho = await _uow.Khos.GetBySanPhamIdAsync(ct.SanPhamId);
                    if (tonKho != null)
                    {
                        int truoc = tonKho.SoLuongTon;
                        tonKho.SoLuongTon = truoc + ct.SoLuong;
                        _uow.Khos.AddBienDong(new BienDongKho
                        {
                            TonKhoId = tonKho.Id,
                            TaiKhoanId = nguoiDungId,
                            DonHangId = dh.Id,
                            Loai = LoaiBienDongKho.HoanDon.ToString(),
                            SoLuongThayDoi = ct.SoLuong,
                            SoLuongTruoc = truoc,
                            SoLuongSau = truoc + ct.SoLuong,
                            LyDo = string.Format("Hoàn kho do hủy đơn hàng {0}", dh.MaDon),
                            ThoiDiem = DateTimeOffset.Now
                        });
                    }
                }

                // Hoàn lượt mã giảm giá nếu có
                if (dh.MaGiamGiaId.HasValue)
                {
                    var voucher = await _uow.MaGiamGias.GetByIdAsync(dh.MaGiamGiaId.Value);
                    if (voucher != null && voucher.SoLuotDaDung > 0)
                        voucher.SoLuotDaDung -= 1;
                }

                _uow.DonHangs.AddLichSu(new LichSuDonHang
                {
                    DonHangId = dh.Id,
                    TaiKhoanId = nguoiDungId,
                    TrangThai = TrangThaiDonHang.DaHuy.ToString(),
                    ThoiDiem = DateTimeOffset.Now,
                    GhiChu = dh.LyDoHuy
                });

                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        // UC-42: Tra cứu hành trình đơn hàng
        public async Task<List<LichSuDonHangItemResponse>> TraCuuHanhTrinhAsync(int khachHangId, int donHangId)
        {
            var dh = await _uow.DonHangs.GetByIdAsync(donHangId);
            if (dh == null || dh.TaiKhoanId != khachHangId)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            var list = await _uow.DonHangs.GetHanhTrinhAsync(donHangId);
            return list.Select(ls => new LichSuDonHangItemResponse
            {
                Id = ls.Id,
                TrangThai = ls.TrangThai,
                ThoiDiem = ls.ThoiDiem,
                NguoiThucHien = ls.TaiKhoan != null ? ls.TaiKhoan.HoTen : "Hệ thống",
                GhiChu = ls.GhiChu
            }).ToList();
        }

        // UC-43: Nhân viên xác nhận đơn hàng
        public async Task XacNhanAsync(int nhanVienId, int donHangId)
        {
            var dh = await _uow.DonHangs.GetByIdAsync(donHangId);
            if (dh == null)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (dh.TrangThai != TrangThaiDonHang.ChoXacNhan.ToString())
                throw new ConflictException("Đơn hàng không còn ở trạng thái Chờ xác nhận.");

            dh.TrangThai = TrangThaiDonHang.DaXacNhan.ToString();
            _uow.DonHangs.AddLichSu(new LichSuDonHang
            {
                DonHangId = dh.Id,
                TaiKhoanId = nhanVienId,
                TrangThai = TrangThaiDonHang.DaXacNhan.ToString(),
                ThoiDiem = DateTimeOffset.Now,
                GhiChu = "Nhân viên xác nhận đơn hàng"
            });

            await _uow.SaveChangesAsync();
        }

        // UC-44: Cập nhật trạng thái đơn hàng theo đúng thứ tự: DaXacNhan -> DangChuanBi -> DangGiao -> HoanThanh
        public async Task CapNhatTrangThaiAsync(int nhanVienId, int donHangId, CapNhatTrangThaiDonHangRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TrangThaiMoi))
                throw new BadRequestException("Vui lòng chọn trạng thái đơn hàng mới.");
            var dh = await _uow.DonHangs.GetByIdAsync(donHangId);
            if (dh == null)
                throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (dh.TrangThai == TrangThaiDonHang.HoanThanh.ToString() || dh.TrangThai == TrangThaiDonHang.DaHuy.ToString())
                throw new BadRequestException("Đơn hàng đã hoàn thành hoặc đã hủy nên không thể đổi trạng thái.");

            string expectedNext = null;
            if (dh.TrangThai == TrangThaiDonHang.DaXacNhan.ToString()) expectedNext = TrangThaiDonHang.DangChuanBi.ToString();
            else if (dh.TrangThai == TrangThaiDonHang.DangChuanBi.ToString()) expectedNext = TrangThaiDonHang.DangGiao.ToString();
            else if (dh.TrangThai == TrangThaiDonHang.DangGiao.ToString()) expectedNext = TrangThaiDonHang.HoanThanh.ToString();

            if (expectedNext == null || request.TrangThaiMoi != expectedNext)
                throw new BadRequestException(string.Format("Chuyển trạng thái sai thứ tự. Trạng thái kế tiếp hợp lệ từ '{0}' là '{1}'.", dh.TrangThai, expectedNext ?? "không có"));

            dh.TrangThai = request.TrangThaiMoi;

            // UC-44 4a: Hoàn thành đơn COD -> tự động ghi nhận đã thu tiền
            if (request.TrangThaiMoi == TrangThaiDonHang.HoanThanh.ToString()
                && dh.PhuongThucThanhToan == PhuongThucThanhToan.KhiNhanHang.ToString())
            {
                dh.DaThanhToan = true;
            }

            _uow.DonHangs.AddLichSu(new LichSuDonHang
            {
                DonHangId = dh.Id,
                TaiKhoanId = nhanVienId,
                TrangThai = dh.TrangThai,
                ThoiDiem = DateTimeOffset.Now,
                GhiChu = string.IsNullOrWhiteSpace(request.GhiChu) ? string.Format("Cập nhật trạng thái sang {0}", dh.TrangThai) : request.GhiChu.Trim()
            });

            await _uow.SaveChangesAsync();
        }
    }
}
