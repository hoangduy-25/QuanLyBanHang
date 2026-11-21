using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// AuthPage (3 tab: Đăng nhập / Đăng ký / Quên mật khẩu) & CustomerAccountPage (3 tab dọc: Hồ sơ / Sổ địa chỉ / Bảo mật).
    /// </summary>
    public class AccountController : BaseMvcController
    {
        public ActionResult Auth() { return View(); }
        public ActionResult Index() { return View(); }
    }
}
