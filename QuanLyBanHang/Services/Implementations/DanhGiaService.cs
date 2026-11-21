using System;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.DanhGia;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class DanhGiaService : IDanhGiaService
    {
        private static readonly string[] BannedWords = { "lừa đảo", "rác rưởi", "đồ đểu", "spam" };
        private readonly IUnitOfWork _uow;

        public DanhGiaService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<DanhGiaResponse>> XemDanhSachAsync(DanhGiaFilterQuery query, int? currentUserId, string vaiTro)
        {
            var q = query ?? new DanhGiaFilterQuery();

            // UC-52 Ghi chú: Tiêu chí "chưa phản hồi" (CoPhanHoi == false) chỉ dành cho Nhân viên & Quản lý
            if (q.CoPhanHoi.HasValue && !q.CoPhanHoi.Value
                && vaiTro != VaiTro.NhanVien.ToString()
                && vaiTro != VaiTro.QuanLy.ToString())
            {
                throw new ForbiddenException("Tiêu chí lọc 'chưa phản hồi' chỉ dành cho Nhân viên và Quản lý.");
            }

            if (q.TuNgay.HasValue && q.DenNgay.HasValue && q.TuNgay.Value > q.DenNgay.Value)
                throw new BadRequestException("Khoảng thời gian lọc không hợp lệ.");

            var paged = await _uow.DanhGias.GetPagedAsync(q);
            return new PagedResult<DanhGiaResponse>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(dg => Map(dg, currentUserId)).ToList()
            };
        }

        public Task<PagedResult<DanhGiaResponse>> LocAsync(DanhGiaFilterQuery query, int? currentUserId, string vaiTro)
        {
            return XemDanhSachAsync(query, currentUserId, vaiTro);
        }

        // UC-53: Gửi đánh giá sản phẩm đã mua
        public async Task<DanhGiaResponse> TaoAsync(int khachHangId, TaoDanhGiaRequest request)
        {
            ValidateReviewContent(request.NoiDung);

            // Kiểm tra đã mua sản phẩm và đơn hàng đã Hoàn thành
            bool eligible = await _uow.DonHangs.DaMuaVaHoanThanhAsync(khachHangId, request.DonHangId, request.SanPhamId);
            if (!eligible)
                throw new ForbiddenException("Bạn chỉ có thể đánh giá sản phẩm thuộc đơn hàng đã hoàn thành.");

            var existing = await _uow.DanhGias.GetByDonHangAndSanPhamAsync(request.DonHangId, request.SanPhamId);
            if (existing != null)
                throw new ConflictException("Bạn đã đánh giá sản phẩm này cho đơn hàng này. Vui lòng chọn Chỉnh sửa đánh giá.");

            var entity = new DanhGia
            {
                TaiKhoanId = khachHangId,
                SanPhamId = request.SanPhamId,
                DonHangId = request.DonHangId,
                SoSao = request.SoSao,
                NoiDung = request.NoiDung.Trim(),
                HinhAnh = request.HinhAnh != null ? request.HinhAnh.Trim() : null,
                DaChinhSua = false,
                IsDeleted = false,
                NgayTao = DateTimeOffset.Now
            };

            _uow.DanhGias.Add(entity);
            await _uow.SaveChangesAsync();

            var reloaded = await _uow.DanhGias.GetActiveDetailByIdAsync(entity.Id);
            return Map(reloaded, khachHangId);
        }

        // UC-54: Chỉnh sửa đánh giá của mình
        public async Task<DanhGiaResponse> SuaAsync(int khachHangId, int danhGiaId, SuaDanhGiaRequest request)
        {
            ValidateReviewContent(request.NoiDung);

            var entity = await _uow.DanhGias.GetActiveDetailByIdAsync(danhGiaId);
            if (entity == null)
                throw new NotFoundException("Đánh giá không tồn tại hoặc đã bị xóa.");

            if (entity.TaiKhoanId != khachHangId)
                throw new ForbiddenException("Bạn chỉ có thể chỉnh sửa đánh giá do chính mình viết.");

            entity.SoSao = request.SoSao;
            entity.NoiDung = request.NoiDung.Trim();
            entity.HinhAnh = request.HinhAnh != null ? request.HinhAnh.Trim() : null;
            entity.DaChinhSua = true;
            entity.NgayCapNhat = DateTimeOffset.Now;

            await _uow.SaveChangesAsync();
            return Map(entity, khachHangId);
        }

        // UC-55: Thích / bỏ thích đánh giá
        public async Task<ThichDanhGiaResponse> ThichAsync(int nguoiDungId, int danhGiaId)
        {
            var dg = await _uow.DanhGias.GetActiveDetailByIdAsync(danhGiaId);
            if (dg == null)
                throw new NotFoundException("Đánh giá không tồn tại.");

            // Luồng 2b: Không cho phép tự thích đánh giá của chính mình
            if (dg.TaiKhoanId == nguoiDungId)
                throw new BadRequestException("Bạn không thể tự thích đánh giá của chính mình.");

            var existing = await _uow.DanhGias.GetThichAsync(nguoiDungId, danhGiaId);
            bool daThich;
            if (existing != null)
            {
                _uow.DanhGias.RemoveThich(existing);
                daThich = false;
            }
            else
            {
                _uow.DanhGias.AddThich(new ThichDanhGia
                {
                    TaiKhoanId = nguoiDungId,
                    DanhGiaId = danhGiaId,
                    NgayTao = DateTimeOffset.Now
                });
                daThich = true;
            }

            await _uow.SaveChangesAsync();
            int count = await _uow.DanhGias.CountThichAsync(danhGiaId);

            return new ThichDanhGiaResponse
            {
                DanhGiaId = danhGiaId,
                DaThich = daThich,
                SoLuotThich = count
            };
        }

        // UC-56: Xóa đánh giá (Soft Delete)
        public async Task XoaAsync(int nguoiDungId, string vaiTro, int danhGiaId, XoaDanhGiaRequest request)
        {
            var dg = await _uow.DanhGias.GetActiveDetailByIdAsync(danhGiaId);
            if (dg == null)
                throw new NotFoundException("Đánh giá không còn tồn tại.");

            bool isManager = vaiTro == VaiTro.QuanLy.ToString();
            if (!isManager && dg.TaiKhoanId != nguoiDungId)
                throw new ForbiddenException("Bạn không có quyền xóa đánh giá này.");

            dg.IsDeleted = true;
            dg.LyDoXoa = request != null && !string.IsNullOrWhiteSpace(request.LyDoXoa)
                ? request.LyDoXoa.Trim()
                : (isManager ? "Quản lý gỡ bỏ do vi phạm" : "Khách hàng tự xóa");

            await _uow.SaveChangesAsync();
        }

        // UC-57: Nhân viên / Quản lý trả lời hoặc cập nhật phản hồi đánh giá
        public async Task<PhanHoiDanhGiaResponse> TraLoiAsync(int nguoiTraLoiId, int danhGiaId, TraLoiDanhGiaRequest request)
        {
            var dg = await _uow.DanhGias.GetActiveDetailByIdAsync(danhGiaId);
            if (dg == null)
                throw new NotFoundException("Đánh giá không còn tồn tại.");

            var existing = await _uow.DanhGias.GetPhanHoiByDanhGiaIdAsync(danhGiaId);
            if (existing != null)
            {
                existing.NoiDung = request.NoiDung.Trim();
                existing.TaiKhoanId = nguoiTraLoiId;
                existing.NgayCapNhat = DateTimeOffset.Now;
            }
            else
            {
                existing = new PhanHoiDanhGia
                {
                    DanhGiaId = danhGiaId,
                    TaiKhoanId = nguoiTraLoiId,
                    NoiDung = request.NoiDung.Trim(),
                    NgayTao = DateTimeOffset.Now
                };
                _uow.DanhGias.AddPhanHoi(existing);
            }

            await _uow.SaveChangesAsync();

            var nguoiTraLoi = await _uow.TaiKhoans.GetByIdAsync(nguoiTraLoiId);
            return new PhanHoiDanhGiaResponse
            {
                Id = existing.Id,
                TaiKhoanId = existing.TaiKhoanId,
                TenNguoiTraLoi = nguoiTraLoi != null ? nguoiTraLoi.HoTen : "Cửa hàng",
                NoiDung = existing.NoiDung,
                NgayTao = existing.NgayTao,
                NgayCapNhat = existing.NgayCapNhat
            };
        }

        private static void ValidateReviewContent(string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                throw new BadRequestException("Nội dung đánh giá không được để trống.");

            var lower = noiDung.ToLowerInvariant();
            if (BannedWords.Any(w => lower.Contains(w)))
                throw new BadRequestException("Nội dung đánh giá chứa từ ngữ không phù hợp. Vui lòng chỉnh sửa.");
        }

        private static DanhGiaResponse Map(DanhGia dg, int? currentUserId)
        {
            var ph = dg.PhanHoiDanhGias != null ? dg.PhanHoiDanhGias.FirstOrDefault() : null;
            var likes = dg.ThichDanhGias != null ? dg.ThichDanhGias.ToList() : null;

            return new DanhGiaResponse
            {
                Id = dg.Id,
                TaiKhoanId = dg.TaiKhoanId,
                TenNguoiDanhGia = dg.TaiKhoan != null ? dg.TaiKhoan.HoTen : string.Empty,
                SanPhamId = dg.SanPhamId,
                TenSanPham = dg.SanPham != null ? dg.SanPham.Ten : string.Empty,
                DonHangId = dg.DonHangId,
                SoSao = dg.SoSao,
                NoiDung = dg.NoiDung,
                HinhAnh = dg.HinhAnh,
                DaChinhSua = dg.DaChinhSua,
                SoLuotThich = likes != null ? likes.Count : 0,
                DaThich = currentUserId.HasValue && likes != null && likes.Any(t => t.TaiKhoanId == currentUserId.Value),
                NgayTao = dg.NgayTao,
                NgayCapNhat = dg.NgayCapNhat,
                PhanHoi = ph == null ? null : new PhanHoiDanhGiaResponse
                {
                    Id = ph.Id,
                    TaiKhoanId = ph.TaiKhoanId,
                    TenNguoiTraLoi = ph.TaiKhoan != null ? ph.TaiKhoan.HoTen : "Cửa hàng",
                    NoiDung = ph.NoiDung,
                    NgayTao = ph.NgayTao,
                    NgayCapNhat = ph.NgayCapNhat
                }
            };
        }
    }
}
