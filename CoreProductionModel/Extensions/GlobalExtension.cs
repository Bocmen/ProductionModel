using System.Drawing;

namespace ProductionModel.Extensions
{
    public static class GlobalExtension
    {
        public static string ToHex(this Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
