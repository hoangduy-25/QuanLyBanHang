using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.MaGiamGia;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class MaGiamGiaService : IMaGiamGiaService
    {
        private readonly IUnitOfWork _uow;

        public MaGiamGiaService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<MaGiamGiaResponse>> XemDanhSachAsync(bool laQuanLy)
        {
            var list = await _uow.MaGiamGias.GetAllActiveAsync(!laQuanLy);
            return list.Select(Map).ToList();
        }

        public async Task<List<MaGiamGiaResponse>> TimKiemAsync(string tuKhoa, bool laQuanLy)
        {
            var list = await _uow.MaGiamGias.GetAllActiveAsync(!laQuanLy, tuKhoa);
            return list.Select(Map).ToList();
        }

        public async Task<MaGiamGiaResponse> XemChiTietAsync(Guid id, bool laQuanLy)
        {
            var m = await _uow.MaGiamGias.GetActiveByIdAsync(id);
            var now = DateTimeOffset.Now;
            if (m == null || (!laQuanLy && (!m.HienThi || m.NgayBatDau > now || m.NgayKetThuc < now || m.SoLuotDaDung >= m.SoLuotToiDa)))
                throw new NotFoundException("Mã giảm giá không khả dụng.");

            return Map(m);
        }

        public async Task<MaGiamGiaResponse> ThemAsync(TaoMaGiamGiaRequest request)
        {
            ValidateVoucherRules(request);

            if (await _uow.MaGiamGias.ExistsMaAsync(request.Ma))
                throw new ConflictException("Mã giảm giá đã tồn tại.");

            var entity = new MaGiamGia
            {
                Ma = request.Ma.Trim().ToUpperInvariant(),
                MoTa = request.MoTa != null ? request.MoTa.Trim() : null,
                Loai = request.Loai,
                GiaTri = request.GiaTri,
                GiamToiDa = request.GiamToiDa,
                DonToiThieu = request.DonToiThieu,
                NgayBatDau = request.NgayBatDau,
                NgayKetThuc = request.NgayKetThuc,
                SoLuotToiDa = request.SoLuotToiDa,
                SoLuotDaDung = 0,
                HienThi = request.HienThi,
                IsDeleted = false
            };

            _uow.MaGiamGias.Add(entity);
            await _uow.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<MaGiamGiaResponse> SuaAsync(Guid id, SuaMaGiamGiaRequest request)
        {
            var entity = await _uow.MaGiamGias.GetActiveByIdAsync(id);
            if (entity == null)
                throw new NotFoundException("Mã giảm giá không tồn tại.");

            ValidateVoucherRules(request);

            if (request.SoLuotToiDa < entity.SoLuotDaDung)
                throw new BadRequestException(string.Format("Tổng số lượt ({0}) không được nhỏ hơn số lượt đã dùng ({1}).", request.SoLuotToiDa, entity.SoLuotDaDung));

            // UC-49 4b: Mã đã được dùng -> không cho đổi Ma và Loai
            if (entity.SoLuotDaDung > 0)
            {
                if (!string.Equals(entity.Ma, request.Ma.Trim(), StringComparison.OrdinalIgnoreCase) || entity.Loai != request.Loai)
                    throw new BadRequestException("Mã giảm giá đã có lượt sử dụng nên không được đổi tên mã hoặc loại giảm.");
            }
            else
            {
                if (await _uow.MaGiamGias.ExistsMaAsync(request.Ma, id))
                    throw new ConflictException("Mã giảm giá đã trùng với mã khác.");
                entity.Ma = request.Ma.Trim().ToUpperInvariant();
                entity.Loai = request.Loai;
            }

            entity.MoTa = request.MoTa != null ? request.MoTa.Trim() : null;
            entity.GiaTri = request.GiaTri;
            entity.GiamToiDa = request.GiamToiDa;
            entity.DonToiThieu = request.DonToiThieu;
            entity.NgayBatDau = request.NgayBatDau;
            entity.NgayKetThuc = request.NgayKetThuc;
            entity.SoLuotToiDa = request.SoLuotToiDa;
            entity.HienThi = request.HienThi;

            await _uow.SaveChangesAsync();
            return Map(entity);
        }

        // UC-50: Xóa hoặc ngừng áp dụng nếu đã được dùng trong đơn hàng
        public async Task<string> XoaAsync(Guid id)
        {
            var entity = await _uow.MaGiamGias.GetActiveByIdAsync(id);
            if (entity == null)
                throw new NotFoundException("Mã giảm giá không tồn tại.");

            if (await _uow.MaGiamGias.HasBeenUsedInDonHangAsync(id))
            {
                entity.HienThi = false;
                await _uow.SaveChangesAsync();
                return "Mã giảm giá đã từng được dùng trong đơn hàng nên được chuyển sang trạng thái Ngừng áp dụng.";
            }

            entity.IsDeleted = true;
            entity.HienThi = false;
            await _uow.SaveChangesAsync();
            return "Xóa mã giảm giá thành công.";
        }

        private static void ValidateVoucherRules(TaoMaGiamGiaRequest request)
        {
            if (request.NgayKetThuc <= request.NgayBatDau)
                throw new BadRequestException("Ngày kết thúc phải sau ngày bắt đầu.");

            if (request.Loai == LoaiGiamGia.PhanTram.ToString() && (request.GiaTri < 1 || request.GiaTri > 100))
                throw new BadRequestException("Mức giảm theo phần trăm phải từ 1 đến 100.");
        }

        private static MaGiamGiaResponse Map(MaGiamGia m)
        {
            return new MaGiamGiaResponse
            {
                Id = m.Id,
                Ma = m.Ma,
                MoTa = m.MoTa,
                Loai = m.Loai,
                GiaTri = m.GiaTri,
                GiamToiDa = m.GiamToiDa,
                DonToiThieu = m.DonToiThieu,
                NgayBatDau = m.NgayBatDau,
                NgayKetThuc = m.NgayKetThuc,
                SoLuotToiDa = m.SoLuotToiDa,
                SoLuotDaDung = m.SoLuotDaDung,
                HienThi = m.HienThi
            };
        }
    }
}
