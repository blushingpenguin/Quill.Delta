namespace Quill.Delta
{
    public class ListConverter
    {
        public static string GetStringValue(ListType? list) =>
            list.HasValue ? GetStringValue(list.Value) : "";

        public static string GetStringValue(ListType list) =>
            list == ListType.Ordered ? "ordered" :
            list == ListType.Bullet ? "bullet" :
            list == ListType.Checked ? "checked" :
            list == ListType.Unchecked ? "unchecked" : "";

        public static ListType? GetEnumValue(string list) =>
            list == "ordered" ? ListType.Ordered :
            list == "bullet" ? ListType.Bullet :
            list == "checked" ? ListType.Checked :
            list == "unchecked" ? (ListType?)ListType.Unchecked : null;
    }
}
