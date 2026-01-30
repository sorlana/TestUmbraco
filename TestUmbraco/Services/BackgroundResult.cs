// Services/BackgroundResult.cs
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