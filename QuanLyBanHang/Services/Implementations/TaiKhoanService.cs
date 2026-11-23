using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.TaiKhoan;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;
using QuanLyBanHang.Services.Helpers;

namespace QuanLyBanHang.Services.Implementations
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly IUnitOfWork _uow;

        public TaiKhoanService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // UC-06: Xem hồ sơ cá nhân
        public async Task<HoSoResponse> LayHoSoAsync(int taiKhoanId)
        {
            var tk = await _uow.TaiKhoans.GetByIdAsync(taiKhoanId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            return MapHoSo(tk);
        }

        // UC-06: Cập nhật thông tin cá nhân
        public async Task<HoSoResponse> CapNhatThongTinCaNhanAsync(int taiKhoanId, CapNhatHoSoRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu cập nhật hồ sơ không hợp lệ.");
            var tk = await _uow.TaiKhoans.GetByIdAsync(taiKhoanId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            if (await _uow.TaiKhoans.ExistsPhoneAsync(request.SoDienThoai, taiKhoanId))
                throw new ConflictException("Số điện thoại này đã được sử dụng bởi tài khoản khác.");

            if (request.NgaySinh.HasValue && request.NgaySinh.Value.Date > DateTime.Today)
                throw new BadRequestException("Ngày sinh không được lớn hơn ngày hiện tại.");

            tk.HoTen = request.HoTen.Trim();
            tk.SoDienThoai = PasswordHasher.NormalizePhone(request.SoDienThoai);
            tk.NgaySinh = request.NgaySinh.HasValue ? request.NgaySinh.Value.Date : (DateTime?)null;
            tk.GioiTinh = string.IsNullOrWhiteSpace(request.GioiTinh) ? null : request.GioiTinh.Trim();
            if (request.AnhDaiDien != null)
                tk.AnhDaiDien = string.IsNullOrWhiteSpace(request.AnhDaiDien)
                    ? null
                    : request.AnhDaiDien.Trim();

            await _uow.SaveChangesAsync();
            return MapHoSo(tk);
        }

        // UC-08: Xem danh sách tài khoản
        public async Task<PagedResult<TaiKhoanListItemResponse>> XemDanhSachAsync(TaiKhoanFilterQuery query)
        {
            var paged = await _uow.TaiKhoans.GetPagedAsync(query ?? new TaiKhoanFilterQuery());
            return new PagedResult<TaiKhoanListItemResponse>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(t => new TaiKhoanListItemResponse
                {
                    Id = t.Id,
                    HoTen = t.HoTen,
                    Email = t.Email,
                    SoDienThoai = t.SoDienThoai,
                    VaiTro = t.VaiTro,
                    TrangThai = t.TrangThai,
                    LyDoKhoa = t.LyDoKhoa,
                    NgaySinh = t.NgaySinh,
                    GioiTinh = t.GioiTinh,
                    AnhDaiDien = t.AnhDaiDien,
                    DaThietLapCauHoiBaoMat = !string.IsNullOrWhiteSpace(t.CauHoiBaoMat),
                    NgayTao = t.NgayTao
                }).ToList()
            };
        }

        // UC-09: Tìm kiếm tài khoản
        public Task<PagedResult<TaiKhoanListItemResponse>> TimKiemAsync(TaiKhoanFilterQuery query)
        {
            return XemDanhSachAsync(query);
        }

        // UC-10: Khóa tài khoản khách hàng
        public async Task<KhoaMoTaiKhoanResponse> KhoaAsync(int quanLyId, int taiKhoanId, KhoaTaiKhoanRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LyDo))
                throw new BadRequestException("Vui lòng nhập lý do khóa tài khoản.");
            var tk = await _uow.TaiKhoans.GetByIdAsync(taiKhoanId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            // Luồng 2a: Chỉ cho phép khóa tài khoản Khách hàng
            if (tk.VaiTro != VaiTro.KhachHang.ToString())
                throw new ForbiddenException("Chỉ được phép khóa hoặc mở khóa tài khoản Khách hàng.");

            if (tk.TrangThai == TrangThaiTaiKhoan.BiKhoa.ToString())
                throw new BadRequestException("Tài khoản này hiện đã ở trạng thái bị khóa.");

            var soDonDangXuLy = await _uow.DonHangs.CountDonChuaHoanTatByTaiKhoanAsync(taiKhoanId);

            tk.TrangThai = TrangThaiTaiKhoan.BiKhoa.ToString();
            tk.LyDoKhoa = request.LyDo.Trim();

            _uow.TaiKhoans.AddLichSuKhoa(new LichSuKhoaTaiKhoan
            {
                TaiKhoanId = taiKhoanId,
                QuanLyId = quanLyId,
                TrangThaiMoi = TrangThaiTaiKhoan.BiKhoa.ToString(),
                LyDo = tk.LyDoKhoa,
                ThoiDiem = DateTimeOffset.Now
            });

            await _uow.SaveChangesAsync();

            return new KhoaMoTaiKhoanResponse
            {
                TaiKhoanId = taiKhoanId,
                TrangThaiMoi = tk.TrangThai,
                LyDo = tk.LyDoKhoa,
                SoDonDangXuLy = soDonDangXuLy,
                CanhBao = soDonDangXuLy > 0
                    ? string.Format("Khách hàng hiện còn {0} đơn hàng chưa hoàn tất; các đơn này vẫn tiếp tục được xử lý.", soDonDangXuLy)
                    : null
            };
        }

        // UC-10: Mở khóa tài khoản khách hàng
        public async Task<KhoaMoTaiKhoanResponse> MoKhoaAsync(int quanLyId, int taiKhoanId)
        {
            var tk = await _uow.TaiKhoans.GetByIdAsync(taiKhoanId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            if (tk.VaiTro != VaiTro.KhachHang.ToString())
                throw new ForbiddenException("Chỉ được phép khóa hoặc mở khóa tài khoản Khách hàng.");

            if (tk.TrangThai == TrangThaiTaiKhoan.HoatDong.ToString())
                throw new BadRequestException("Tài khoản này đang ở trạng thái hoạt động.");

            tk.TrangThai = TrangThaiTaiKhoan.HoatDong.ToString();
            tk.LyDoKhoa = null;
            tk.SoLanDangNhapSai = 0;
            tk.KhoaTamDen = null;

            _uow.TaiKhoans.AddLichSuKhoa(new LichSuKhoaTaiKhoan
            {
                TaiKhoanId = taiKhoanId,
                QuanLyId = quanLyId,
                TrangThaiMoi = TrangThaiTaiKhoan.HoatDong.ToString(),
                LyDo = "Quản lý mở khóa tài khoản",
                ThoiDiem = DateTimeOffset.Now
            });

            await _uow.SaveChangesAsync();

            return new KhoaMoTaiKhoanResponse
            {
                TaiKhoanId = taiKhoanId,
                TrangThaiMoi = tk.TrangThai
            };
        }

        public async Task<List<LichSuKhoaResponse>> XemLichSuKhoaAsync(int taiKhoanId)
        {
            var list = await _uow.TaiKhoans.GetLichSuKhoaAsync(taiKhoanId);
            return list.Select(x => new LichSuKhoaResponse
            {
                Id = x.Id,
                TaiKhoanId = x.TaiKhoanId,
                QuanLyId = x.QuanLyId,
                TenQuanLy = x.QuanLy != null ? x.QuanLy.HoTen : string.Empty,
                TrangThaiMoi = x.TrangThaiMoi,
                LyDo = x.LyDo,
                ThoiDiem = x.ThoiDiem
            }).ToList();
        }

        private static HoSoResponse MapHoSo(TaiKhoan tk)
        {
            return new HoSoResponse
            {
                Id = tk.Id,
                HoTen = tk.HoTen,
                Email = tk.Email,
                SoDienThoai = tk.SoDienThoai,
                VaiTro = tk.VaiTro,
                TrangThai = tk.TrangThai,
                NgaySinh = tk.NgaySinh,
                GioiTinh = tk.GioiTinh,
                AnhDaiDien = tk.AnhDaiDien,
                DaThietLapCauHoiBaoMat = !string.IsNullOrWhiteSpace(tk.CauHoiBaoMat),
                CauHoiBaoMat = tk.CauHoiBaoMat,
                NgayTao = tk.NgayTao
            };
        }
    }
}
