using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// Trang chủ Storefront - khớp CatalogPage.tsx (UC-11, UC-12, UC-17, UC-19).
    /// </summary>
    public class HomeController : BaseMvcController
    {
        private readonly IDanhMucService _danhMucService;
        private readonly ISanPhamService _sanPhamService;

        public HomeController()
        {
            _danhMucService = new DanhMucService(UnitOfWork);
            _sanPhamService = new SanPhamService(UnitOfWork);
        }

        public async Task<ActionResult> Index(SanPhamFilterQuery query)
        {
            var q = query ?? new SanPhamFilterQuery { Page = 1, PageSize = 8 };
            if (q.PageSize <= 0) q.PageSize = 8;
            ViewBag.DanhMucs = await _danhMucService.XemDanhSachAsync(false);
            ViewBag.Query = q;
            var paged = await _sanPhamService.XemDanhSachAsync(q, false);
            return View(paged);
        }
    }
}
