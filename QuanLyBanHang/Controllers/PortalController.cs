using System.Web.Mvc;

namespace QuanLyBanHang.Controllers
{
    /// <summary>
    /// Cổng Quản trị & Nhân viên - khớp 7 trang trong src/pages/portal/ của hcshop-ui.
    /// </summary>
    public class PortalController : BaseMvcController
    {
        public ActionResult Index() { return View(); }       // ReportsAndAnalyticsPage (/portal)
        public ActionResult Orders() { return View(); }      // OrderManagementPage (/portal/orders)
        public ActionResult Inventory() { return View(); }   // InventoryManagementPage (/portal/inventory)
        public ActionResult Products() { return View(); }    // CatalogManagementPage (/portal/products)
        public ActionResult Vouchers() { return View(); }    // VoucherManagementPage (/portal/vouchers)
        public ActionResult Reviews() { return View(); }     // ReviewModerationPage (/portal/reviews)
        public ActionResult Accounts() { return View(); }    // AccountManagementPage (/portal/accounts)
    }
}
