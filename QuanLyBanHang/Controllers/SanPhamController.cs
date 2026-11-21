using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyBanHang.Models.DTOs.SanPham;
using QuanLyBanHang.Services.Exceptions;
using QuanLyBanHang.Services.Implementations;
using QuanLyBanHang.Services.Interfaces;

namespace QuanLyBanHang.Controllers
{
    /// <summary>MVC Controller cho UC-18 (Chi tiết sản phẩm & Đánh giá).</summary>
    public class SanPhamController : BaseMvcController
    {
        private readonly ISanPhamService _sanPhamService;

        public SanPhamController()
        {
            _sanPhamService = new SanPhamService(UnitOfWork);
        }

        public async Task<ActionResult> ChiTiet(int id)
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
