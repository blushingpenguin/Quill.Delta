namespace Quill.Delta
{
    public static class AlignConverter
    {
        public static string GetStringValue(AlignType? align) =>
            align.HasValue ? GetStringValue(align.Value) : "";

        public static string GetStringValue(AlignType align) =>
            align == AlignType.Center ? "center" :
            align == AlignType.Justify ? "justify" :
            align == AlignType.Right ? "right" : "";

        public static AlignType? GetEnumValue(string align) =>
            align == "center" ? AlignType.Center :
            align == "right" ? AlignType.Right :
            align == "justify" ? (AlignType?)AlignType.Justify : null;
    }
}
