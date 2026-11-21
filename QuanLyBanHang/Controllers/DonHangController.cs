using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>MVC Controller cho Đơn hàng của tôi (UC-38, 39, 41, 42, 53).</summary>
    public class DonHangController : BaseMvcController
    {
        public ActionResult Index() { return View(); }
    }
}
