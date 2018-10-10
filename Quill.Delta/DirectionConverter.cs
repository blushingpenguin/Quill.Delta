namespace Quill.Delta
{
    public static class DirectionConverter
    {
        public static string GetStringValue(DirectionType? dirn) =>
            dirn.HasValue ? GetStringValue(dirn.Value) : "";

        public static string GetStringValue(DirectionType dirn) =>
            dirn == DirectionType.Rtl ? "rtl" : "";

        public static DirectionType? GetEnumValue(string dirn) =>
            dirn == "rtl" ? (DirectionType?)DirectionType.Rtl : null;
    }
}
