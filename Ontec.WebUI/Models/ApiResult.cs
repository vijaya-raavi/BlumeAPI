namespace Ontec.WebUI.Models
{
    public class ApiResult
    {
        public bool Error { get; set; }
        public object? Result { get; set; }

    }
    public class BadRequestApiResult
    {
        public bool Error { get; set;}
        public Dictionary<string ,string[]>? Result { get; set; }
    }
}
