using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using QuanLyBanHang.Infrastructure.Filters;
using QuanLyBanHang.Services.Exceptions;

namespace QuanLyBanHang.Controllers.Api
{
    /// <summary>
    /// API tải ảnh lên thư mục ~/images/{products|categories|avatars|reviews}/ khớp 100% dữ liệu mẫu SQL.
    /// Kiểm tra định dạng (.jpg, .jpeg, .png, .webp) và dung lượng tối đa 5MB (UC-06, 13, 20, 53).
    /// </summary>
    [RoutePrefix("api/upload")]
    [JwtAuthorize(Roles = "KhachHang,NhanVien,QuanLy")]
    public class UploadController : BaseApiController
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly Dictionary<string, string> FolderMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "products", "products" },
            { "san-pham", "products" },
            { "categories", "categories" },
            { "danh-muc", "categories" },
            { "avatars", "avatars" },
            { "avatar", "avatars" },
            { "reviews", "reviews" },
            { "danh-gia", "reviews" }
        };
        private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        [HttpPost, Route("hinh-anh")]
        public IHttpActionResult UploadImage([FromUri] string folder = "products")
        {
            string targetFolder;
            if (string.IsNullOrWhiteSpace(folder) || !FolderMap.TryGetValue(folder.Trim(), out targetFolder))
                targetFolder = "products";

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files == null || httpRequest.Files.Count == 0)
                throw new BadRequestException("Vui lòng chọn tệp hình ảnh cần tải lên.");

            var file = httpRequest.Files[0];
            if (file == null || file.ContentLength <= 0)
                throw new BadRequestException("Tệp hình ảnh rỗng.");

            if (file.ContentLength > MaxFileSizeBytes)
                throw new BadRequestException("Dung lượng ảnh vượt quá giới hạn cho phép (tối đa 5 MB).");

            var ext = Path.GetExtension(file.FileName ?? string.Empty).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new BadRequestException("Định dạng ảnh không hợp lệ. Chỉ chấp nhận .jpg, .jpeg, .png, .webp.");

            var relativeDir = string.Format("/images/{0}/", targetFolder);
            var physicalDir = HttpContext.Current.Server.MapPath("~" + relativeDir);
            if (!Directory.Exists(physicalDir))
                Directory.CreateDirectory(physicalDir);

            var fileName = string.Format("{0:yyyyMMddHHmmss}_{1}{2}", DateTime.Now, Guid.NewGuid().ToString("N").Substring(0, 8), ext);
            var physicalPath = Path.Combine(physicalDir, fileName);
            file.SaveAs(physicalPath);

            var publicUrl = relativeDir + fileName;
            return OkApi(new { url = publicUrl, fileName = fileName }, "Tải ảnh lên thành công.");
        }
    }
}
