namespace TestUmbraco.Services
{
    public interface IStaticCssGeneratorService
    {
        Task<string> GenerateBackgroundCssFileAsync();
        Task UpdateCssForMediaAsync(Guid mediaKey);
        Task RemoveCssForMediaAsync(Guid mediaKey);
        Task RegenerateAllCssAsync();
        Task<string> AddInlineStyleAsync(string css, string styleType = "custom");
        Task<string> GetOrAddMediaClassAsync(Guid mediaKey, string className, int minHeight = 400, string size = "cover", string position = "center");
        Task<string> GetOrAddColorClassAsync(string colorValue, int minHeight = 400);
        Task<string> GetOrAddGradientClassAsync(string colorStart, string colorEnd, string direction = "to bottom", int minHeight = 400);
        Task<string> AddOverlayStyleAsync(string overlayClass, string css);
    }
}