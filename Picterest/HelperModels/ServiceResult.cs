namespace Picterest.HelperModels
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string? Error { get; set; } = string.Empty;
        public object? Result { get; set; }
        public string? Message { get; set; } = string.Empty;
        public int StatusCode { get; set; } = 200;
    }
}
