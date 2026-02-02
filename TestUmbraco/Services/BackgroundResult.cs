namespace TestUmbraco.Services
{
    public class BackgroundResult
    {
        public BackgroundType Type { get; set; } = BackgroundType.None;
        public string CssClass { get; set; } = string.Empty;
        public bool HasBackground { get; set; }
        public bool HasOverlay { get; set; }
        public bool IsLazyLoaded { get; set; }
        public bool IsMobileOptimized { get; set; }
        public string OverlayClass { get; set; } = string.Empty;
        public string VideoId { get; set; } = string.Empty;
        public string VideoPlaceholder { get; set; } = string.Empty;
        public bool UseVideoPlaceholder { get; set; }
    }

    public enum BackgroundType
    {
        None,
        Image,
        Color,
        Gradient,
        Video
    }
}