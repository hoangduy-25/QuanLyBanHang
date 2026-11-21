using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHang.Models.DTOs.Common
{
    /// <summary>
    /// Định dạng phản hồi thống nhất cho toàn bộ 71 RESTful API.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Thành công")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }

        public static ApiResponse<T> Fail(string message, object errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
        }
    }

    /// <summary>
    /// Kết quả phân trang chuẩn cho danh sách & tìm kiếm.
    /// </summary>
    public class PagedResult<T>
    {
        public PagedResult()
        {
            Items = new List<T>();
        }

        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages
        {
            get { return PageSize <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize); }
        }
    }

    /// <summary>
    /// Tham số phân trang cơ sở.
    /// </summary>
    public class PaginationQuery
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn hoặc bằng 1.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Số dòng mỗi trang từ 1 đến 100.")]
        public int PageSize { get; set; } = 10;
    }
}
