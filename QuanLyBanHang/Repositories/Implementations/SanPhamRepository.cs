using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class SanPhamRepository : GenericRepository<SanPham>, ISanPhamRepository
    {
        public SanPhamRepository(QuanLyBanHangDbContext context) : base(context) { }

        public async Task<PagedResult<SanPham>> GetPagedAsync(SanPhamFilterQuery query, bool chiHienThi)
        {
            var q = DbSet.AsNoTracking()
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.TonKhos)
                .Include(sp => sp.DanhGias)
                .Where(sp => !sp.IsDeleted);

            if (chiHienThi)
                q = q.Where(sp => sp.HienThi && !sp.DanhMuc.IsDeleted && sp.DanhMuc.HienThi);
            else if (query.HienThi.HasValue)
                q = q.Where(sp => sp.HienThi == query.HienThi.Value);

            if (query.DanhMucId.HasValue)
                q = q.Where(sp => sp.DanhMucId == query.DanhMucId.Value);

            if (query.GiaTu.HasValue)
                q = q.Where(sp => sp.GiaBan >= query.GiaTu.Value);

            if (query.GiaDen.HasValue)
                q = q.Where(sp => sp.GiaBan <= query.GiaDen.Value);

            if (!string.IsNullOrWhiteSpace(query.TuKhoa))
            {
                var kw = query.TuKhoa.Trim().ToLower();
                q = q.Where(sp =>
                    sp.Ten.ToLower().Contains(kw) ||
                    sp.MaSanPham.ToLower().Contains(kw) ||
                    sp.MoTa.ToLower().Contains(kw));
            }

            var total = await q.CountAsync();

            switch (query.SapXep)
            {
                case "GiaAsc": q = q.OrderBy(sp => sp.GiaBan); break;
                case "GiaDesc": q = q.OrderByDescending(sp => sp.GiaBan); break;
                case "TenAsc": q = q.OrderBy(sp => sp.Ten); break;
                default: q = q.OrderByDescending(sp => sp.NgayTao); break;
            }

            var items = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<SanPham>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public Task<SanPham> GetDetailByIdAsync(Guid id, bool chiHienThi)
        {
            var q = DbSet
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.TonKhos)
                .Include(sp => sp.DanhGias)
                .Where(sp => sp.Id == id && !sp.IsDeleted);

            if (chiHienThi)
                q = q.Where(sp => sp.HienThi && !sp.DanhMuc.IsDeleted && sp.DanhMuc.HienThi);

            return q.FirstOrDefaultAsync();
        }

        public Task<bool> ExistsMaSanPhamAsync(string maSanPham, Guid? excludeId = null)
        {
            var normalized = (maSanPham ?? string.Empty).Trim().ToLower();
            return DbSet.AnyAsync(sp =>
                !sp.IsDeleted &&
                sp.MaSanPham.ToLower() == normalized &&
                (!excludeId.HasValue || sp.Id != excludeId.Value));
        }

        public Task<bool> ExistsTenTrongDanhMucAsync(Guid danhMucId, string ten, Guid? excludeId = null)
        {
            var normalized = (ten ?? string.Empty).Trim().ToLower();
            return DbSet.AnyAsync(sp =>
                !sp.IsDeleted &&
                sp.DanhMucId == danhMucId &&
                sp.Ten.ToLower() == normalized &&
                (!excludeId.HasValue || sp.Id != excludeId.Value));
        }

        public Task<bool> HasDonHangAsync(Guid sanPhamId)
        {
            return Context.ChiTietDonHangs.AnyAsync(ct => ct.SanPhamId == sanPhamId);
        }
    }
}
