using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.DanhMuc;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class DanhMucService : IDanhMucService
    {
        private readonly IUnitOfWork _uow;

        public DanhMucService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<DanhMucResponse>> XemDanhSachAsync(bool laQuanLy)
        {
            var list = await _uow.DanhMucs.GetAllActiveAsync(!laQuanLy);
            return list.Select(d => Map(d, laQuanLy)).ToList();
        }

        public async Task<List<DanhMucResponse>> TimKiemAsync(string tuKhoa, bool laQuanLy)
        {
            var list = await _uow.DanhMucs.GetAllActiveAsync(!laQuanLy, tuKhoa);
            return list.Select(d => Map(d, laQuanLy)).ToList();
        }

        public async Task<DanhMucResponse> ThemAsync(TaoSuaDanhMucRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Ten))
                throw new BadRequestException("Tên danh mục không được để trống.");

            var ten = request.Ten.Trim();
            if (await _uow.DanhMucs.ExistsTenAsync(ten))
                throw new ConflictException("Tên danh mục đã tồn tại.");

            var entity = new DanhMuc
            {
                Ten = ten,
                MoTa = string.IsNullOrWhiteSpace(request.MoTa) ? null : request.MoTa.Trim(),
                HinhAnh = string.IsNullOrWhiteSpace(request.HinhAnh) ? null : request.HinhAnh.Trim(),
                HienThi = request.HienThi,
                IsDeleted = false,
                NgayTao = DateTimeOffset.Now
            };

            _uow.DanhMucs.Add(entity);
            await _uow.SaveChangesAsync();
            return Map(entity, true);
        }

        public async Task<DanhMucResponse> SuaAsync(int id, TaoSuaDanhMucRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Ten))
                throw new BadRequestException("Tên danh mục không được để trống.");

            var entity = await _uow.DanhMucs.GetActiveByIdAsync(id);
            if (entity == null)
                throw new NotFoundException("Danh mục không tồn tại hoặc đã bị xóa.");

            var ten = request.Ten.Trim();
            if (await _uow.DanhMucs.ExistsTenAsync(ten, id))
                throw new ConflictException("Tên danh mục đã trùng với danh mục khác.");

            entity.Ten = ten;
            entity.MoTa = string.IsNullOrWhiteSpace(request.MoTa) ? null : request.MoTa.Trim();
            entity.HinhAnh = string.IsNullOrWhiteSpace(request.HinhAnh) ? null : request.HinhAnh.Trim();
            entity.HienThi = request.HienThi;

            await _uow.SaveChangesAsync();
            return Map(entity, true);
        }

        public async Task XoaAsync(int id)
        {
            var entity = await _uow.DanhMucs.GetActiveByIdAsync(id);
            if (entity == null)
                throw new NotFoundException("Danh mục không tồn tại hoặc đã bị xóa.");

            // UC-15 4a: Từ chối xóa nếu còn sản phẩm
            var soSanPham = await _uow.DanhMucs.CountSanPhamActiveAsync(id);
            if (soSanPham > 0)
                throw new ConflictException(string.Format("Không thể xóa danh mục vì đang có {0} sản phẩm thuộc danh mục này. Vui lòng chuyển sản phẩm sang danh mục khác hoặc chọn Ẩn danh mục.", soSanPham));

            entity.IsDeleted = true;
            entity.HienThi = false;
            await _uow.SaveChangesAsync();
        }

        public async Task<DanhMucResponse> AnHienAsync(int id, AnHienDanhMucRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu cập nhật trạng thái hiển thị không hợp lệ.");

            var entity = await _uow.DanhMucs.GetActiveByIdAsync(id);
            if (entity == null)
                throw new NotFoundException("Danh mục không tồn tại.");

            entity.HienThi = request.HienThi;
            await _uow.SaveChangesAsync();
            return Map(entity, true);
        }

        private static DanhMucResponse Map(DanhMuc d, bool laQuanLy)
        {
            var activeProducts = d.SanPhams != null
                ? d.SanPhams.Where(sp => !sp.IsDeleted && (laQuanLy || sp.HienThi)).Count()
                : 0;

            return new DanhMucResponse
            {
                Id = d.Id,
                Ten = d.Ten,
                MoTa = d.MoTa,
                HinhAnh = d.HinhAnh,
                HienThi = d.HienThi,
                SoSanPham = activeProducts,
                NgayTao = d.NgayTao
            };
        }
    }
}
