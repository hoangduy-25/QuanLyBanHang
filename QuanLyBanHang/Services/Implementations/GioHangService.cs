using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Models.DTOs.GioHang;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Services.Implementations
{
    public class GioHangService : IGioHangService
    {
        private readonly IUnitOfWork _uow;

        public GioHangService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // UC-29: Xem giỏ hàng
        public async Task<GioHangResponse> XemGioHangAsync(int taiKhoanId)
        {
            var gh = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            if (gh == null)
                return new GioHangResponse();

            return MapGioHang(gh);
        }

        // UC-30: Thêm sản phẩm vào giỏ hàng
        public async Task<GioHangResponse> ThemSanPhamAsync(int taiKhoanId, ThemVaoGioRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu thêm vào giỏ hàng không hợp lệ.");
            var sp = await _uow.SanPhams.GetDetailByIdAsync(request.SanPhamId, true);
            if (sp == null)
                throw new NotFoundException("Sản phẩm không khả dụng.");

            var tonKho = sp.TonKhos != null ? sp.TonKhos.FirstOrDefault() : null;
            int soLuongTon = tonKho != null ? tonKho.SoLuongTon : 0;
            if (soLuongTon <= 0)
                throw new BadRequestException("Sản phẩm đã hết hàng.");

            var gh = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            if (gh == null)
            {
                gh = new GioHang { TaiKhoanId = taiKhoanId };
                _uow.GioHangs.Add(gh);
                await _uow.SaveChangesAsync();
            }

            var existing = await _uow.GioHangs.GetChiTietAsync(gh.Id, request.SanPhamId);
            int soLuongMoi = (existing != null ? existing.SoLuong : 0) + request.SoLuong;
            if (soLuongMoi > soLuongTon)
                throw new BadRequestException(string.Format("Số lượng yêu cầu vượt quá tồn kho (tối đa còn lại: {0}).", soLuongTon));

            if (existing != null)
            {
                existing.SoLuong = soLuongMoi;
                existing.DonGiaLucThem = sp.GiaBan;
            }
            else
            {
                _uow.GioHangs.AddChiTiet(new ChiTietGioHang
                {
                    GioHangId = gh.Id,
                    SanPhamId = sp.Id,
                    SoLuong = request.SoLuong,
                    DonGiaLucThem = sp.GiaBan
                });
            }

            await _uow.SaveChangesAsync();
            var updated = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            return MapGioHang(updated);
        }

        // UC-31: Chỉnh sửa số lượng sản phẩm trong giỏ
        public async Task<GioHangResponse> SuaSoLuongAsync(int taiKhoanId, int sanPhamId, SuaSoLuongGioRequest request)
        {
            if (request == null)
                throw new BadRequestException("Dữ liệu cập nhật số lượng không hợp lệ.");
            if (request.SoLuong < 0)
                throw new BadRequestException("Số lượng sản phẩm không hợp lệ.");

            var gh = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            if (gh == null)
                throw new NotFoundException("Giỏ hàng trống.");

            var item = await _uow.GioHangs.GetChiTietAsync(gh.Id, sanPhamId);
            if (item == null)
                throw new NotFoundException("Sản phẩm không có trong giỏ hàng.");

            // UC-31 2b: Số lượng bằng 0 -> xóa khỏi giỏ (UC-32)
            if (request.SoLuong == 0)
            {
                _uow.GioHangs.RemoveChiTiet(item);
                await _uow.SaveChangesAsync();
            }
            else
            {
                var sp = await _uow.SanPhams.GetDetailByIdAsync(sanPhamId, true);
                if (sp == null)
                    throw new BadRequestException("Sản phẩm hiện không còn khả dụng.");

                var tonKho = sp.TonKhos != null ? sp.TonKhos.FirstOrDefault() : null;
                int soLuongTon = tonKho != null ? tonKho.SoLuongTon : 0;
                if (request.SoLuong > soLuongTon)
                    throw new BadRequestException(string.Format("Số lượng vượt quá tồn kho (tối đa còn lại: {0}).", soLuongTon));

                item.SoLuong = request.SoLuong;
                await _uow.SaveChangesAsync();
            }

            var updated = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            return MapGioHang(updated);
        }

        // UC-32: Xóa sản phẩm khỏi giỏ hàng
        public async Task<GioHangResponse> XoaSanPhamAsync(int taiKhoanId, int sanPhamId)
        {
            var gh = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            if (gh == null)
                return new GioHangResponse();

            var item = await _uow.GioHangs.GetChiTietAsync(gh.Id, sanPhamId);
            if (item != null)
            {
                _uow.GioHangs.RemoveChiTiet(item);
                await _uow.SaveChangesAsync();
            }

            var updated = await _uow.GioHangs.GetByTaiKhoanIdWithDetailsAsync(taiKhoanId);
            return updated != null ? MapGioHang(updated) : new GioHangResponse();
        }

        // UC-33: Xem danh sách yêu thích
        public async Task<List<YeuThichItemResponse>> XemYeuThichAsync(int taiKhoanId)
        {
            var list = await _uow.GioHangs.GetYeuThichByTaiKhoanAsync(taiKhoanId);
            return list.Select(yt =>
            {
                var tonKho = yt.SanPham != null && yt.SanPham.TonKhos != null ? yt.SanPham.TonKhos.FirstOrDefault() : null;
                return new YeuThichItemResponse
                {
                    SanPhamId = yt.SanPhamId,
                    MaSanPham = yt.SanPham != null ? yt.SanPham.MaSanPham : string.Empty,
                    TenSanPham = yt.SanPham != null ? yt.SanPham.Ten : string.Empty,
                    HinhAnh = yt.SanPham != null ? yt.SanPham.HinhAnh : null,
                    GiaBan = yt.SanPham != null ? yt.SanPham.GiaBan : 0,
                    ConHang = tonKho != null && tonKho.SoLuongTon > 0,
                    HienThi = yt.SanPham != null && yt.SanPham.HienThi &&
                              (yt.SanPham.DanhMuc == null || (yt.SanPham.DanhMuc.HienThi && !yt.SanPham.DanhMuc.IsDeleted)),
                    NgayTao = yt.NgayTao
                };
            }).ToList();
        }

        // UC-33: Thêm sản phẩm yêu thích
        public async Task ThemYeuThichAsync(int taiKhoanId, int sanPhamId)
        {
            var sp = await _uow.SanPhams.GetDetailByIdAsync(sanPhamId, true);
            if (sp == null)
                throw new NotFoundException("Sản phẩm không tồn tại hoặc đang ẩn.");

            var existing = await _uow.GioHangs.GetYeuThichAsync(taiKhoanId, sanPhamId);
            if (existing != null)
                return;

            _uow.GioHangs.AddYeuThich(new YeuThichSanPham
            {
                TaiKhoanId = taiKhoanId,
                SanPhamId = sanPhamId,
                NgayTao = DateTimeOffset.Now
            });
            await _uow.SaveChangesAsync();
        }

        // UC-33: Bỏ sản phẩm yêu thích
        public async Task XoaYeuThichAsync(int taiKhoanId, int sanPhamId)
        {
            var existing = await _uow.GioHangs.GetYeuThichAsync(taiKhoanId, sanPhamId);
            if (existing == null)
                return;

            _uow.GioHangs.RemoveYeuThich(existing);
            await _uow.SaveChangesAsync();
        }

        private static GioHangResponse MapGioHang(GioHang gh)
        {
            var resp = new GioHangResponse { GioHangId = gh.Id };
            foreach (var ct in gh.ChiTietGioHangs)
            {
                var sp = ct.SanPham;
                var tonKho = sp != null && sp.TonKhos != null ? sp.TonKhos.FirstOrDefault() : null;
                int soLuongTon = tonKho != null ? tonKho.SoLuongTon : 0;

                bool khaDung = true;
                string lyDo = null;

                if (sp == null || sp.IsDeleted || !sp.HienThi || (sp.DanhMuc != null && (!sp.DanhMuc.HienThi || sp.DanhMuc.IsDeleted)))
                {
                    khaDung = false;
                    lyDo = "Sản phẩm không còn khả dụng.";
                }
                else if (soLuongTon <= 0)
                {
                    khaDung = false;
                    lyDo = "Sản phẩm đã hết hàng.";
                }
                else if (ct.SoLuong > soLuongTon)
                {
                    khaDung = false;
                    lyDo = string.Format("Chỉ còn {0} sản phẩm trong kho.", soLuongTon);
                }

                var item = new ChiTietGioHangItemResponse
                {
                    SanPhamId = ct.SanPhamId,
                    MaSanPham = sp != null ? sp.MaSanPham : string.Empty,
                    TenSanPham = sp != null ? sp.Ten : string.Empty,
                    HinhAnh = sp != null ? sp.HinhAnh : null,
                    DonGiaHienTai = sp != null ? sp.GiaBan : ct.DonGiaLucThem,
                    DonGiaLucThem = ct.DonGiaLucThem,
                    SoLuong = ct.SoLuong,
                    SoLuongTon = soLuongTon,
                    KhaDung = khaDung,
                    LyDoKhongKhaDung = lyDo
                };

                resp.Items.Add(item);
                if (khaDung)
                {
                    resp.TongSoLuongKhaDung += item.SoLuong;
                    resp.TongTienTamTinh += item.ThanhTien;
                }
            }
            return resp;
        }
    }
}
