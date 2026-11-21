using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class GioHangRepository : GenericRepository<GioHang>, IGioHangRepository
    {
        public GioHangRepository(QuanLyBanHangDbContext context) : base(context) { }

        public Task<GioHang> GetByTaiKhoanIdWithDetailsAsync(Guid taiKhoanId)
        {
            return DbSet
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.SanPham))
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.SanPham.DanhMuc))
                .Include(g => g.ChiTietGioHangs.Select(ct => ct.SanPham.TonKhos))
                .FirstOrDefaultAsync(g => g.TaiKhoanId == taiKhoanId);
        }

        public Task<ChiTietGioHang> GetChiTietAsync(Guid gioHangId, Guid sanPhamId)
        {
            return Context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.GioHangId == gioHangId && ct.SanPhamId == sanPhamId);
        }

        public void AddChiTiet(ChiTietGioHang chiTiet)
        {
            Context.ChiTietGioHangs.Add(chiTiet);
        }

        public void RemoveChiTiet(ChiTietGioHang chiTiet)
        {
            Context.ChiTietGioHangs.Remove(chiTiet);
        }

        public async Task RemoveAllBySanPhamIdAsync(Guid sanPhamId)
        {
            var cartItems = await Context.ChiTietGioHangs.Where(ct => ct.SanPhamId == sanPhamId).ToListAsync();
            if (cartItems.Any())
                Context.ChiTietGioHangs.RemoveRange(cartItems);

            var wishItems = await Context.YeuThichSanPhams.Where(yt => yt.SanPhamId == sanPhamId).ToListAsync();
            if (wishItems.Any())
                Context.YeuThichSanPhams.RemoveRange(wishItems);
        }

        public Task<List<YeuThichSanPham>> GetYeuThichByTaiKhoanAsync(Guid taiKhoanId)
        {
            return Context.YeuThichSanPhams.AsNoTracking()
                .Include(yt => yt.SanPham)
                .Include(yt => yt.SanPham.TonKhos)
                .Where(yt => yt.TaiKhoanId == taiKhoanId && !yt.SanPham.IsDeleted)
                .OrderByDescending(yt => yt.NgayTao)
                .ToListAsync();
        }

        public Task<YeuThichSanPham> GetYeuThichAsync(Guid taiKhoanId, Guid sanPhamId)
        {
            return Context.YeuThichSanPhams
                .FirstOrDefaultAsync(yt => yt.TaiKhoanId == taiKhoanId && yt.SanPhamId == sanPhamId);
        }

        public void AddYeuThich(YeuThichSanPham yeuThich)
        {
            Context.YeuThichSanPhams.Add(yeuThich);
        }

        public void RemoveYeuThich(YeuThichSanPham yeuThich)
        {
            Context.YeuThichSanPhams.Remove(yeuThich);
        }
    }
}
