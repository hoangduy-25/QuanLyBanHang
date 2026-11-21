using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>MVC Controller cho UC-04..07 (Hồ sơ cá nhân, Sổ địa chỉ, Đổi mật khẩu & Câu hỏi bảo mật).</summary>
    public class TaiKhoanController : BaseMvcController
    {
        public ActionResult Index() { return View(); }
    }
}
