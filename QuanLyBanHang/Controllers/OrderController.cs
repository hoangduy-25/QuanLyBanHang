using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// Đơn hàng của tôi dạng Master-Detail - khớp CustomerOrdersPage.tsx (UC-38, 39, 41, 42, 53).
    /// </summary>
    public class OrderController : BaseMvcController
    {
        public ActionResult Index() { return View(); }
    }
}
