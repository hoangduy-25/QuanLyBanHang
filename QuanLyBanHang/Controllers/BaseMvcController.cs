using System.Web.Mvc;
using QuanLyBanHang.Data;
using QuanLyBanHang.Repositories.Implementations;
using QuanLyBanHang.Repositories.Interfaces;

namespace QuanLyBanHang.Controllers
{
    public abstract class BaseMvcController : Controller
    {
        protected readonly IUnitOfWork UnitOfWork;

        protected BaseMvcController()
        {
            UnitOfWork = new UnitOfWork(new QuanLyBanHangDbContext());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UnitOfWork != null) UnitOfWork.Dispose();
            base.Dispose(disposing);
        }
    }
}
