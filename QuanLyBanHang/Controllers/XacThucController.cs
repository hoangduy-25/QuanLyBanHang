using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>MVC Controller cho UC-01..03 (Đăng nhập, Đăng ký, Quên mật khẩu).</summary>
    public class XacThucController : BaseMvcController
    {
        public ActionResult Index() { return View(); }
    }
}
