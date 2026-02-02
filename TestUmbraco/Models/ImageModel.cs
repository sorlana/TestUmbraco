namespace TestUmbraco.Models
{
    public class ImageModel
    {
        public string? Url { get; set; }
        public string? AltText { get; set; }
        public string? Title { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? FocalPoint { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public string? CropAlias { get; set; }
        public bool LazyLoad { get; set; } = true;
    }
}