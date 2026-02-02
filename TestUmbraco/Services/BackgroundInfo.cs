using System.Collections.Generic;

namespace TestUmbraco.Services
{
    public class BackgroundInfo
    {
        public string ComponentClass { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ComponentId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Size { get; set; } = "cover";
        public string Position { get; set; } = "center";
        public string VideoId { get; set; } = string.Empty;
        public bool UsePlaceholder { get; set; }
        public string PlaceholderUrl { get; set; } = string.Empty;
        public bool HasOverlay { get; set; }
        public string OverlayClass { get; set; } = string.Empty;
    }
}