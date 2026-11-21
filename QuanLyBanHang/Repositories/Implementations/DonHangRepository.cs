using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.DonHang;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Models.Enums;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class DonHangRepository : GenericRepository<DonHang>, IDonHangRepository
    {
        public DonHangRepository(QuanLyBanHangDbContext context) : base(context) { }

        public async Task<PagedResult<DonHang>> GetPagedAsync(DonHangFilterQuery query, int? taiKhoanId = null)
        {
            var q = DbSet.AsNoTracking()
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonHangs)
                .AsQueryable();

            if (taiKhoanId.HasValue)
                q = q.Where(d => d.TaiKhoanId == taiKhoanId.Value);

            if (!string.IsNullOrWhiteSpace(query.MaDon))
            {
                var kw = query.MaDon.Trim().ToLower();
                q = q.Where(d => d.MaDon.ToLower().Contains(kw));
            }

            if (!string.IsNullOrWhiteSpace(query.TrangThai))
                q = q.Where(d => d.TrangThai == query.TrangThai);

            if (query.TuNgay.HasValue)
                q = q.Where(d => d.NgayDat >= query.TuNgay.Value);

            if (query.DenNgay.HasValue)
                q = q.Where(d => d.NgayDat <= query.DenNgay.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(d => d.NgayDat)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<DonHang>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public Task<DonHang> GetDetailByIdAsync(int id)
        {
            return DbSet
                .Include(d => d.TaiKhoan)
                .Include(d => d.MaGiamGia)
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
                .Include(d => d.LichSuDonHangs.Select(ls => ls.TaiKhoan))
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public Task<DonHang> GetByMaDonAsync(string maDon)
        {
            var normalized = (maDon ?? string.Empty).Trim().ToLower();
            return DbSet
                .Include(d => d.LichSuDonHangs.Select(ls => ls.TaiKhoan))
                .FirstOrDefaultAsync(d => d.MaDon.ToLower() == normalized);
        }

        public Task<List<LichSuDonHang>> GetHanhTrinhAsync(int donHangId)
        {
            return Context.LichSuDonHangs.AsNoTracking()
                .Include(ls => ls.TaiKhoan)
                .Where(ls => ls.DonHangId == donHangId)
                .OrderBy(ls => ls.ThoiDiem)
                .ToListAsync();
        }

        public Task<int> CountDonChuaHoanTatByTaiKhoanAsync(int taiKhoanId)
        {
            var hoanThanh = TrangThaiDonHang.HoanThanh.ToString();
            var daHuy = TrangThaiDonHang.DaHuy.ToString();
            return DbSet.CountAsync(d =>
                d.TaiKhoanId == taiKhoanId &&
                d.TrangThai != hoanThanh &&
                d.TrangThai != daHuy);
        }

        public Task<bool> DaMuaVaHoanThanhAsync(int taiKhoanId, int donHangId, int sanPhamId)
        {
            var hoanThanh = TrangThaiDonHang.HoanThanh.ToString();
            return DbSet.AnyAsync(d =>
                d.Id == donHangId &&
                d.TaiKhoanId == taiKhoanId &&
                d.TrangThai == hoanThanh &&
                d.ChiTietDonHangs.Any(ct => ct.SanPhamId == sanPhamId));
        }

        public void AddLichSu(LichSuDonHang lichSu)
        {
            Context.LichSuDonHangs.Add(lichSu);
        }
    }
}
