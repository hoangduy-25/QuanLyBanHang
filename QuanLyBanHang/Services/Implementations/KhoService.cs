using System;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.Kho;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class KhoService : IKhoService
    {
        private readonly IUnitOfWork _uow;

        public KhoService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // UC-24: Xem danh sách tồn kho
        public async Task<PagedResult<TonKhoResponse>> XemTonKhoAsync(TonKhoFilterQuery query)
        {
            var paged = await _uow.Khos.GetPagedTonKhoAsync(query ?? new TonKhoFilterQuery());
            return new PagedResult<TonKhoResponse>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(MapTonKho).ToList()
            };
        }

        // UC-25: Nhập hàng tăng số lượng tồn kho
        public async Task<TonKhoResponse> NhapTangAsync(int quanLyId, NhapTangKhoRequest request)
        {
            var tk = await _uow.Khos.GetBySanPhamIdAsync(request.SanPhamId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy bản ghi tồn kho của sản phẩm.");

            _uow.BeginTransaction();
            try
            {
                int truoc = tk.SoLuongTon;
                int sau = truoc + request.SoLuongNhap;
                tk.SoLuongTon = sau;

                _uow.Khos.AddBienDong(new BienDongKho
                {
                    TonKhoId = tk.Id,
                    TaiKhoanId = quanLyId,
                    Loai = LoaiBienDongKho.NhapTang.ToString(),
                    SoLuongThayDoi = request.SoLuongNhap,
                    SoLuongTruoc = truoc,
                    SoLuongSau = sau,
                    GiaNhap = request.GiaNhap,
                    NhaCungCap = request.NhaCungCap != null ? request.NhaCungCap.Trim() : null,
                    LyDo = string.IsNullOrWhiteSpace(request.GhiChu) ? "Nhập hàng tăng tồn kho" : request.GhiChu.Trim(),
                    ThoiDiem = DateTimeOffset.Now
                });

                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();
                return MapTonKho(tk);
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        // UC-26: Điều chỉnh giảm tồn kho
        public async Task<TonKhoResponse> DieuChinhGiamAsync(int quanLyId, DieuChinhGiamKhoRequest request)
        {
            var tk = await _uow.Khos.GetBySanPhamIdAsync(request.SanPhamId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy bản ghi tồn kho của sản phẩm.");

            if (tk.SoLuongTon <= 0 || request.SoLuongGiam > tk.SoLuongTon)
                throw new BadRequestException(string.Format("Số lượng giảm ({0}) vượt quá tồn kho hiện tại ({1}). Không cho phép tồn kho âm.", request.SoLuongGiam, tk.SoLuongTon));

            _uow.BeginTransaction();
            try
            {
                int truoc = tk.SoLuongTon;
                int sau = truoc - request.SoLuongGiam;
                tk.SoLuongTon = sau;

                _uow.Khos.AddBienDong(new BienDongKho
                {
                    TonKhoId = tk.Id,
                    TaiKhoanId = quanLyId,
                    Loai = LoaiBienDongKho.DieuChinhGiam.ToString(),
                    SoLuongThayDoi = request.SoLuongGiam,
                    SoLuongTruoc = truoc,
                    SoLuongSau = sau,
                    LyDo = request.LyDo.Trim(),
                    ThoiDiem = DateTimeOffset.Now
                });

                await _uow.SaveChangesAsync();
                _uow.CommitTransaction();
                return MapTonKho(tk);
            }
            catch
            {
                _uow.RollbackTransaction();
                throw;
            }
        }

        // UC-27: Thiết lập ngưỡng tồn kho tối thiểu
        public async Task<TonKhoResponse> DatNguongToiThieuAsync(int tonKhoId, DatNguongToiThieuRequest request)
        {
            var tk = await _uow.Khos.GetByIdAsync(tonKhoId);
            if (tk == null)
                throw new NotFoundException("Không tìm thấy bản ghi tồn kho.");

            tk.NguongToiThieu = request.NguongToiThieu;
            await _uow.SaveChangesAsync();

            var reloaded = await _uow.Khos.GetBySanPhamIdAsync(tk.SanPhamId);
            return MapTonKho(reloaded ?? tk);
        }

        // UC-28: Xem lịch sử biến động kho
        public async Task<PagedResult<BienDongKhoResponse>> XemLichSuAsync(BienDongKhoFilterQuery query)
        {
            var q = query ?? new BienDongKhoFilterQuery();
            if (q.TuNgay.HasValue && q.DenNgay.HasValue && q.TuNgay.Value > q.DenNgay.Value)
                throw new BadRequestException("Khoảng thời gian không hợp lệ (ngày bắt đầu sau ngày kết thúc).");

            var paged = await _uow.Khos.GetPagedBienDongAsync(q);
            return new PagedResult<BienDongKhoResponse>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(b => new BienDongKhoResponse
                {
                    Id = b.Id,
                    TonKhoId = b.TonKhoId,
                    SanPhamId = b.TonKho != null ? b.TonKho.SanPhamId : 0,
                    MaSanPham = b.TonKho != null && b.TonKho.SanPham != null ? b.TonKho.SanPham.MaSanPham : string.Empty,
                    TenSanPham = b.TonKho != null && b.TonKho.SanPham != null ? b.TonKho.SanPham.Ten : string.Empty,
                    DonHangId = b.DonHangId,
                    MaDon = b.DonHang != null ? b.DonHang.MaDon : null,
                    Loai = b.Loai,
                    SoLuongThayDoi = b.SoLuongThayDoi,
                    SoLuongTruoc = b.SoLuongTruoc,
                    SoLuongSau = b.SoLuongSau,
                    GiaNhap = b.GiaNhap,
                    NhaCungCap = b.NhaCungCap,
                    NguoiThucHien = b.TaiKhoan != null ? b.TaiKhoan.HoTen : "Hệ thống",
                    LyDo = b.LyDo,
                    ThoiDiem = b.ThoiDiem
                }).ToList()
            };
        }

        private static TonKhoResponse MapTonKho(TonKho tk)
        {
            return new TonKhoResponse
            {
                TonKhoId = tk.Id,
                SanPhamId = tk.SanPhamId,
                MaSanPham = tk.SanPham != null ? tk.SanPham.MaSanPham : string.Empty,
                TenSanPham = tk.SanPham != null ? tk.SanPham.Ten : string.Empty,
                TenDanhMuc = tk.SanPham != null && tk.SanPham.DanhMuc != null ? tk.SanPham.DanhMuc.Ten : string.Empty,
                SoLuongTon = tk.SoLuongTon,
                NguongToiThieu = tk.NguongToiThieu
            };
        }
    }
}
