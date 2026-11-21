using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>MVC Controller cho Cổng Nhân viên & Quản lý (7 màn hình Portal).</summary>
    public class QuanTriController : BaseMvcController
    {
        public ActionResult Index() { return View(); }      // Báo cáo & Tổng quan (UC-58..62)
        public ActionResult DonHang() { return View(); }    // Quản lý đơn hàng (UC-38..44)
        public ActionResult Kho() { return View(); }        // Quản lý kho hàng (UC-24..28)
        public ActionResult SanPham() { return View(); }    // Quản lý sản phẩm & danh mục (UC-11..23)
        public ActionResult MaGiamGia() { return View(); }  // Quản lý mã giảm giá (UC-45..50)
        public ActionResult DanhGia() { return View(); }    // Kiểm duyệt & trả lời đánh giá (UC-51..57)
        public ActionResult TaiKhoan() { return View(); }   // Quản lý tài khoản (UC-08..10)
    }
}
