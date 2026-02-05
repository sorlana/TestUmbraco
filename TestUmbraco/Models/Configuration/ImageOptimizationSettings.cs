// Models/Configuration/ImageOptimizationSettings.cs
namespace TestUmbraco.Models.Configuration
{
    public class ImageOptimizationSettings
    {
        public int[] DefaultWidths { get; set; } = new[] { 400, 600, 800, 1000, 1200, 1920 };
        public int WebpQuality { get; set; } = 80;
        public int JpegQuality { get; set; } = 85;
        public bool EnableWebp { get; set; } = true;
        public bool EnableLazyLoading { get; set; } = true;
    }
}