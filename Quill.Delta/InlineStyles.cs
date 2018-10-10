using System.Collections.Generic;

namespace Quill.Delta
{
    public delegate string InlineStyleType(string value, DeltaInsertOp op);

    public class InlineStyles
    {
        public InlineStyleType Indent { get; set; }
        public InlineStyleType Align { get; set; }
        public InlineStyleType Color { get; set; }
        public InlineStyleType Background { get; set; }
        public InlineStyleType Direction { get; set; }
        public InlineStyleType Font { get; set; }
        public InlineStyleType Size { get; set; }

        public static string LookupValue(Dictionary<string, string> dic, string key, string def = "")
        {
            string value;
            return dic.TryGetValue(key, out value) ? value : def;
        }

        public static InlineStyleType MakeLookup(Dictionary<string, string> dic, string def = "") =>
            (value, op) => LookupValue(dic, value);
    }
}
