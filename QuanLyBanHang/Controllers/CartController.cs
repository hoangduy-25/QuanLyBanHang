using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// Giỏ hàng (CartPage), Thanh toán (CheckoutPage), Yêu thích (WishlistPage).
    /// </summary>
    public class CartController : BaseMvcController
    {
        public ActionResult Index() { return View(); }
        public ActionResult Checkout() { return View(); }
        public ActionResult Wishlist() { return View(); }
    }
}
