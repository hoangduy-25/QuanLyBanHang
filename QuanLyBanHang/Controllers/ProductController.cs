using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// Chi tiết sản phẩm - khớp ProductDetailPage.tsx (UC-18, UC-30, UC-33, UC-51..55).
    /// </summary>
    public class ProductController : BaseMvcController
    {
        private readonly ISanPhamService _sanPhamService;

        public ProductController()
        {
            _sanPhamService = new SanPhamService(UnitOfWork);
        }

        public async Task<ActionResult> Detail(Guid id)
        {
            try
            {
                var sp = await _sanPhamService.XemChiTietAsync(id, false);
                return View(sp);
            }
            catch (NotFoundException)
            {
                ViewBag.ErrorMessage = "Sản phẩm không khả dụng hoặc không tồn tại.";
                return View((SanPhamChiTietResponse)null);
            }
        }
    }
}
