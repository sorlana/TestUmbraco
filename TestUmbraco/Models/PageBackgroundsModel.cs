// Models/PageBackgroundsModel.cs
using System.Collections.Generic;

namespace TestUmbraco.Models
{
    public class PageBackgroundsModel
    {
        public List<BackgroundItem> Backgrounds { get; set; } = new List<BackgroundItem>();
    }

    public class BackgroundItem
    {
        public string MediaGuid { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        public int MinHeight { get; set; } = 300;
        public string Size { get; set; } = "cover";
        public string Position { get; set; } = "center";
        public string AdditionalStyles { get; set; } = string.Empty;
    }
}