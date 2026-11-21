using System;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class SanPhamService : ISanPhamService
    {
        private readonly IUnitOfWork _uow;

        public SanPhamService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<PagedResult<SanPhamListItemResponse>> XemDanhSachAsync(SanPhamFilterQuery query, bool laQuanLy)
        {
            var q = query ?? new SanPhamFilterQuery();
            if (q.GiaTu.HasValue && q.GiaDen.HasValue && q.GiaTu.Value > q.GiaDen.Value)
                throw new BadRequestException("Khoảng giá lọc không hợp lệ (giá từ lớn hơn giá đến).");

            var paged = await _uow.SanPhams.GetPagedAsync(q, !laQuanLy);
            return new PagedResult<SanPhamListItemResponse>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(MapListItem).ToList()
            };
        }

        public async Task<SanPhamChiTietResponse> XemChiTietAsync(Guid id, bool laQuanLy)
        {
            var sp = await _uow.SanPhams.GetDetailByIdAsync(id, !laQuanLy);
            if (sp == null)
                throw new NotFoundException("Sản phẩm không khả dụng hoặc không tồn tại.");

            return MapDetail(sp);
        }

        public Task<PagedResult<SanPhamListItemResponse>> TimKiemAsync(SanPhamFilterQuery query, bool laQuanLy)
        {
            return XemDanhSachAsync(query, laQuanLy);
        }

        // UC-20: Thêm sản phẩm mới và khởi tạo bản ghi tồn kho ban đầu
        public async Task<SanPhamChiTietResponse> ThemAsync(TaoSanPhamRequest request, Guid quanLyId)
        {
            var danhMuc = await _uow.DanhMucs.GetActiveByIdAsync(request.DanhMucId);
            if (danhMuc == null)
                throw new NotFoundException("Danh mục đã chọn không tồn tại.");

            if (await _uow.SanPhams.ExistsMaSanPhamAsync(request.MaSanPham))
                throw new ConflictException("Mã sản phẩm đã tồn tại.");

            _uow.BeginTransaction();
            try
            {
                var sp = new SanPham
                {
                    DanhMucId = request.DanhMucId,
                    MaSanPham = request.MaSanPham.Trim(),
                    Ten = request.Ten.Trim(),
                    MoTa = request.MoTa != null ? request.MoTa.Trim() : string.Empty,
                    HinhAnh = request.HinhAnh != null ? request.HinhAnh.Trim() : null,
                    GiaBan = request.GiaBan,
                    HienThi = request.HienThi,
                    IsDeleted = false,
                    NgayTao = DateTimeOffset.Now
                };

                _uow.SanPhams.Add(sp);
                await _uow.SaveChangesAsync();

                var tonKho = new TonKho
                {
                    SanPhamId = sp.Id,
                    SoLuongTon = request.SoLuongBanDau,
                    NguongToiThieu = request.NguongToiThieu
                };
                _uow.Khos.Add(tonKho);
                await _uow.SaveChangesAsync();

                if (request.SoLuongBanDau > 0)
                {
                    _uow.Khos.AddBienDong(new BienDongKho
                    {
                        TonKhoId = tonKho.Id,
                        TaiKhoanId = quanLyId,
                        Loai = LoaiBienDongKho.NhapTang.ToString(),
                        SoLuongThayDoi = request.SoLuongBanDau,
                        SoLuongTruoc = 0,
                        SoLuongSau = request.SoLuongBanDau,
                        LyDo = "Khởi tạo tồn kho ban đầu khi thêm sản phẩm",
                        ThoiDiem = DateTimeOffset.Now
                    });
                    await _uow.SaveChangesAsync();
                }

                _uow.CommitTransaction();

                var created = await _uow.SanPhams.GetDetailByIdAsync(sp.Id, false);
                return MapDetail(created);
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        // UC-21: Sửa thông tin sản phẩm (không sửa số lượng tồn kho tại đây)
        public async Task<SanPhamChiTietResponse> SuaAsync(Guid id, SuaSanPhamRequest request)
        {
            var sp = await _uow.SanPhams.GetDetailByIdAsync(id, false);
            if (sp == null)
                throw new NotFoundException("Sản phẩm không tồn tại hoặc đã bị xóa.");

            var danhMuc = await _uow.DanhMucs.GetActiveByIdAsync(request.DanhMucId);
            if (danhMuc == null)
                throw new NotFoundException("Danh mục đã chọn không tồn tại.");

            if (await _uow.SanPhams.ExistsMaSanPhamAsync(request.MaSanPham, id))
                throw new ConflictException("Mã sản phẩm đã trùng với sản phẩm khác.");

            sp.DanhMucId = request.DanhMucId;
            sp.MaSanPham = request.MaSanPham.Trim();
            sp.Ten = request.Ten.Trim();
            sp.MoTa = request.MoTa != null ? request.MoTa.Trim() : string.Empty;
            sp.HinhAnh = request.HinhAnh != null ? request.HinhAnh.Trim() : null;
            sp.GiaBan = request.GiaBan;
            sp.HienThi = request.HienThi;

            await _uow.SaveChangesAsync();
            return MapDetail(sp);
        }

        // UC-22: Xóa sản phẩm
        public async Task XoaAsync(Guid id)
        {
            var sp = await _uow.SanPhams.GetDetailByIdAsync(id, false);
            if (sp == null)
                throw new NotFoundException("Sản phẩm không tồn tại hoặc đã bị xóa.");

            // Luồng 4a: Đã có trong đơn hàng -> từ chối xóa, đề nghị Ẩn
            if (await _uow.SanPhams.HasDonHangAsync(id))
                throw new ConflictException("Sản phẩm đã phát sinh trong đơn hàng nên không thể xóa. Vui lòng chọn Ẩn sản phẩm để bảo toàn lịch sử đơn hàng.");

            _uow.BeginTransaction();
            try
            {
                // Gỡ khỏi giỏ hàng và danh sách yêu thích của khách
                await _uow.GioHangs.RemoveAllBySanPhamIdAsync(id);
                sp.IsDeleted = true;
                sp.HienThi = false;
                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        // UC-23: Ẩn / Hiện sản phẩm
        public async Task<SanPhamChiTietResponse> AnHienAsync(Guid id, AnHienSanPhamRequest request)
        {
            var sp = await _uow.SanPhams.GetDetailByIdAsync(id, false);
            if (sp == null)
                throw new NotFoundException("Sản phẩm không tồn tại.");

            sp.HienThi = request.HienThi;
            await _uow.SaveChangesAsync();
            return MapDetail(sp);
        }

        private static SanPhamListItemResponse MapListItem(SanPham sp)
        {
            var tonKho = sp.TonKhos != null ? sp.TonKhos.FirstOrDefault() : null;
            var activeReviews = sp.DanhGias != null ? sp.DanhGias.Where(d => !d.IsDeleted).ToList() : null;
            int reviewCount = activeReviews != null ? activeReviews.Count : 0;
            double avgRating = reviewCount > 0 ? Math.Round(activeReviews.Average(d => d.SoSao), 1) : 0;

            return new SanPhamListItemResponse
            {
                Id = sp.Id,
                DanhMucId = sp.DanhMucId,
                TenDanhMuc = sp.DanhMuc != null ? sp.DanhMuc.Ten : string.Empty,
                MaSanPham = sp.MaSanPham,
                Ten = sp.Ten,
                HinhAnh = sp.HinhAnh,
                GiaBan = sp.GiaBan,
                SoLuongTon = tonKho != null ? tonKho.SoLuongTon : 0,
                HienThi = sp.HienThi,
                DiemTrungBinh = avgRating,
                SoLuotDanhGia = reviewCount,
                NgayTao = sp.NgayTao
            };
        }

        private static SanPhamChiTietResponse MapDetail(SanPham sp)
        {
            var baseItem = MapListItem(sp);
            var tonKho = sp.TonKhos != null ? sp.TonKhos.FirstOrDefault() : null;
            return new SanPhamChiTietResponse
            {
                Id = baseItem.Id,
                DanhMucId = baseItem.DanhMucId,
                TenDanhMuc = baseItem.TenDanhMuc,
                MaSanPham = baseItem.MaSanPham,
                Ten = baseItem.Ten,
                HinhAnh = baseItem.HinhAnh,
                GiaBan = baseItem.GiaBan,
                SoLuongTon = baseItem.SoLuongTon,
                HienThi = baseItem.HienThi,
                DiemTrungBinh = baseItem.DiemTrungBinh,
                SoLuotDanhGia = baseItem.SoLuotDanhGia,
                NgayTao = baseItem.NgayTao,
                MoTa = sp.MoTa,
                NguongToiThieu = tonKho != null ? tonKho.NguongToiThieu : 0
            };
        }
    }
}
