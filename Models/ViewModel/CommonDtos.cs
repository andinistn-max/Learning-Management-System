using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models.ViewModel
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }

        public static ApiResponse Ok(string message = "Sukses") =>
            new ApiResponse { Success = true, Message = message, Errors = null };

        public static ApiResponse Fail(string message, object errors = null) =>
            new ApiResponse { Success = false, Message = message, Errors = errors };
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Sukses") =>
            new ApiResponse<T> { Success = true, Message = message, Data = data, Errors = null };

        public static ApiResponse<T> Fail(string message, object errors = null) =>
            new ApiResponse<T> { Success = false, Message = message, Data = default(T), Errors = errors };
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}