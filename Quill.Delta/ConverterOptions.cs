namespace Quill.Delta
{
    public class ConverterOptions
    {
        // no more allowing these to be customized; unnecessary
        public string OrderedListTag { get; set; } = "ol";
        public string BulletListTag { get; set; } = "ul";
        public string ListItemTag { get; set; } = "li";

        public string ParagraphTag { get; set; } = "p";
        public string ClassPrefix { get; set; } = "ql";

        public InlineStyles InlineStyles { get; set; }
        public bool MultiLineBlockquote { get; set; } = true;
        public bool MultiLineHeader { get; set; } = true;
        public bool MultiLineCodeblock { get; set; } = true;
        public bool MultiLineParagraph { get; set; } = true;

        public string LinkRel { get; set; } = "";
        public string LinkTarget { get; set; } = "_blank";
        public bool AllowBackgroundClasses { get; set; } = false;
    }
}
