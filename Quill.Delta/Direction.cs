namespace Quill.Delta
{
    public static class Direction
    {
        public static string GetStringValue(DirectionType? dirn) =>
            dirn.HasValue ? GetStringValue(dirn.Value) : "";

        public static string GetStringValue(DirectionType dirn) =>
            dirn == DirectionType.Rtl ? "rtl" : "";
    }
}
