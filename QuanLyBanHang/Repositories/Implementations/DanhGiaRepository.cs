using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBanHang.Data;
using QuanLyBanHang.Models.DTOs.Common;
using QuanLyBanHang.Models.DTOs.DanhGia;
using QuanLyBanHang.Models.Entities;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Repositories.Implementations
{
    public class DanhGiaRepository : GenericRepository<DanhGia>, IDanhGiaRepository
    {
        public DanhGiaRepository(QuanLyBanHangDbContext context) : base(context) { }

        public async Task<PagedResult<DanhGia>> GetPagedAsync(DanhGiaFilterQuery query)
        {
            var q = DbSet.AsNoTracking()
                .Include(dg => dg.TaiKhoan)
                .Include(dg => dg.SanPham)
                .Include(dg => dg.PhanHoiDanhGias.Select(ph => ph.TaiKhoan))
                .Include(dg => dg.ThichDanhGias)
                .Where(dg => !dg.IsDeleted);

            if (query.SanPhamId.HasValue)
                q = q.Where(dg => dg.SanPhamId == query.SanPhamId.Value);

            if (query.SoSao.HasValue)
                q = q.Where(dg => dg.SoSao == query.SoSao.Value);

            if (query.CoHinhAnh.HasValue)
            {
                q = query.CoHinhAnh.Value
                    ? q.Where(dg => dg.HinhAnh != null && dg.HinhAnh != "")
                    : q.Where(dg => dg.HinhAnh == null || dg.HinhAnh == "");
            }

            if (query.CoPhanHoi.HasValue)
            {
                q = query.CoPhanHoi.Value
                    ? q.Where(dg => dg.PhanHoiDanhGias.Any())
                    : q.Where(dg => !dg.PhanHoiDanhGias.Any());
            }

            if (query.TuNgay.HasValue)
                q = q.Where(dg => dg.NgayTao >= query.TuNgay.Value);

            if (query.DenNgay.HasValue)
                q = q.Where(dg => dg.NgayTao <= query.DenNgay.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(dg => dg.NgayTao)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<DanhGia>
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = total
            };
        }

        public Task<DanhGia> GetActiveDetailByIdAsync(int id)
        {
            return DbSet
                .Include(dg => dg.TaiKhoan)
                .Include(dg => dg.SanPham)
                .Include(dg => dg.PhanHoiDanhGias.Select(ph => ph.TaiKhoan))
                .Include(dg => dg.ThichDanhGias)
                .FirstOrDefaultAsync(dg => dg.Id == id && !dg.IsDeleted);
        }

        public Task<DanhGia> GetByDonHangAndSanPhamAsync(int donHangId, int sanPhamId)
        {
            return DbSet.FirstOrDefaultAsync(dg =>
                dg.DonHangId == donHangId && dg.SanPhamId == sanPhamId && !dg.IsDeleted);
        }

        public Task<ThichDanhGia> GetThichAsync(int taiKhoanId, int danhGiaId)
        {
            return Context.ThichDanhGias
                .FirstOrDefaultAsync(t => t.TaiKhoanId == taiKhoanId && t.DanhGiaId == danhGiaId);
        }

        public void AddThich(ThichDanhGia thich)
        {
            Context.ThichDanhGias.Add(thich);
        }

        public void RemoveThich(ThichDanhGia thich)
        {
            Context.ThichDanhGias.Remove(thich);
        }

        public Task<int> CountThichAsync(int danhGiaId)
        {
            return Context.ThichDanhGias.CountAsync(t => t.DanhGiaId == danhGiaId);
        }

        public Task<PhanHoiDanhGia> GetPhanHoiByDanhGiaIdAsync(int danhGiaId)
        {
            return Context.PhanHoiDanhGias.FirstOrDefaultAsync(p => p.DanhGiaId == danhGiaId);
        }

        public void AddPhanHoi(PhanHoiDanhGia phanHoi)
        {
            Context.PhanHoiDanhGias.Add(phanHoi);
        }
    }
}
