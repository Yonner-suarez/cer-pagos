namespace microPagos.API.Model
{
    public class GeneralResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public object data { get; set; }

        public GeneralResponse() { }

        public GeneralResponse(int _status, string _message, object _data)
        {
            status = _status;
            message = _message;
            data = _data;
        }
    }

    public class GeneralResponse<T>
    {
        public int status { get; set; }
        public string message { get; set; }
        public T data { get; set; }

        public GeneralResponse() { }
    }
}
