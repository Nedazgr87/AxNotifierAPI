namespace AxNotifierAPI
{
    public class ApiResult
    {
        public ApiResult(string? msg, bool isSuccess = true, TypeEnum type = TypeEnum.Success)
        {
            Msg = msg;
            IsSuccess = isSuccess;
            Type = type;
        }

        public string? Msg { get; set; }
        public bool IsSuccess { get; set; }
        public TypeEnum Type { get; set; }
    }

    public class ApiResult<T> : ApiResult
    {
        public ApiResult(string? msg, T data, bool isSuccess = true, TypeEnum type = TypeEnum.Success) : base(msg, isSuccess, type)
        {
            Data = data;
        }
        public T Data { get; set; }
    }


    public enum TypeEnum
    {
        Success = 1,
        Warning = 2,
        Error = 3
    }
}
