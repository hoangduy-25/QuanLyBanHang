using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.Kho;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class KhoRepository : GenericRepository<TonKho>, IKhoRepository
    {
        public KhoRepository(QuanLyBanHangDbContext context) : base(context) { }

        public Task<TonKho> GetBySanPhamIdAsync(Guid sanPhamId)
        {
            return DbSet
                .Include(tk => tk.SanPham)
                .FirstOrDefaultAsync(tk => tk.SanPhamId == sanPhamId && !tk.SanPham.IsDeleted);
        }

        public async Task<PagedResult<TonKho>> GetPagedTonKhoAsync(TonKhoFilterQuery query)
        {
            var q = DbSet.AsNoTracking()
                .Include(tk => tk.SanPham)
                .Include(tk => tk.SanPham.DanhMuc)
                .Where(tk => !tk.SanPham.IsDeleted);

            if (query.DanhMucId.HasValue)
                q = q.Where(tk => tk.SanPham.DanhMucId == query.DanhMucId.Value);

            if (!string.IsNullOrWhiteSpace(query.TuKhoa))
            {
                var kw = query.TuKhoa.Trim().ToLower();
                q = q.Where(tk =>
                    tk.SanPham.Ten.ToLower().Contains(kw) ||
                    tk.SanPham.MaSanPham.ToLower().Contains(kw));
            }

            if (query.TrangThaiTon == "HetHang")
                q = q.Where(tk => tk.SoLuongTon <= 0);
            else if (query.TrangThaiTon == "SapHet")
                q = q.Where(tk => tk.SoLuongTon > 0 && tk.SoLuongTon <= tk.NguongToiThieu);
            else if (query.TrangThaiTon == "DuHang")
                q = q.Where(tk => tk.SoLuongTon > tk.NguongToiThieu);

            var total = await q.CountAsync();

            switch (query.SapXep)
            {
                case "SoLuongDesc": q = q.OrderByDescending(tk => tk.SoLuongTon); break;
                case "TenAsc": q = q.OrderBy(tk => tk.SanPham.Ten); break;
                default: q = q.OrderBy(tk => tk.SoLuongTon); break;
            }

            var items = await q
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<TonKho>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public async Task<PagedResult<BienDongKho>> GetPagedBienDongAsync(BienDongKhoFilterQuery query)
        {
            var q = Context.BienDongKhos.AsNoTracking()
                .Include(b => b.TonKho)
                .Include(b => b.TonKho.SanPham)
                .Include(b => b.TaiKhoan)
                .Include(b => b.DonHang)
                .AsQueryable();

            if (query.SanPhamId.HasValue)
                q = q.Where(b => b.TonKho.SanPhamId == query.SanPhamId.Value);

            if (!string.IsNullOrWhiteSpace(query.Loai))
                q = q.Where(b => b.Loai == query.Loai);

            if (query.TuNgay.HasValue)
                q = q.Where(b => b.ThoiDiem >= query.TuNgay.Value);

            if (query.DenNgay.HasValue)
                q = q.Where(b => b.ThoiDiem <= query.DenNgay.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(b => b.ThoiDiem)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<BienDongKho>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public void AddBienDong(BienDongKho bienDong)
        {
            Context.BienDongKhos.Add(bienDong);
        }
    }
}
