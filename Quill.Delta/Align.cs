namespace Quill.Delta
{
    public static class Align
    {
        public static string GetStringValue(AlignType? align) =>
            align.HasValue ? GetStringValue(align.Value) : "";

        public static string GetStringValue(AlignType align) =>
            align == AlignType.Center ? "center" :
            align == AlignType.Justify ? "justify" :
            align == AlignType.Right ? "right" : "";
    }
}
