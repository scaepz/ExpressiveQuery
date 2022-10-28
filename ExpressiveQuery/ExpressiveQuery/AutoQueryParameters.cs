namespace MirkaApi.Lab.AutoQuery
{
    public class AutoQueryParameters : IAutoQueryParameters
    {
        public string Where { get; set; }

        public string OrderBy { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }
    }
    public interface IAutoQueryParameters
    {
        public string Where { get; }
        public string OrderBy { get; }
        public int? Take { get; }
        public int? Skip { get; }
    }
}
