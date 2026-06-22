namespace TaskManagementSystem.DTO.Response
{
    public class ApiResponse<T>
    {
        // Indicates if the request was successful
        public bool Success { get; set; }

        // Contains the actual payload (nullable for error states)
        public T? Data { get; set; }

        // Human-readable message or system error description
        public string? Message { get; set; }

        // Optional list of validation errors (useful for form submissions)
        public List<string>? Errors { get; set; }

        // Timetamp of when the response was generated
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Helper method for successful responses
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        // Helper method for error responses
        public static ApiResponse<T> FailureResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = message,
                Errors = errors
            };
        }
    }

}
