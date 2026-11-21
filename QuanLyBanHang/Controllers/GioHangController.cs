using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>MVC Controller cho Giỏ hàng (UC-29..32), Yêu thích (UC-33) và Thanh toán (UC-34..37, 40).</summary>
    public class GioHangController : BaseMvcController
    {
        public ActionResult Index() { return View(); }
        public ActionResult ThanhToan() { return View(); }
        public ActionResult YeuThich() { return View(); }
    }
}
