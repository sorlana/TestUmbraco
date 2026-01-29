namespace TestUmbraco.Services
{
    public class BackgroundResult
    {
        public BackgroundType Type { get; set; } = BackgroundType.None;
        public string CssClass { get; set; } = string.Empty;
        public bool HasBackground { get; set; }
        public bool HasOverlay { get; set; }
        public string HtmlContent { get; set; } = string.Empty; // Для видео HTML
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