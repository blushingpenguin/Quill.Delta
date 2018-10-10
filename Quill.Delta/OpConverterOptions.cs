namespace Quill.Delta
{
    public class OpConverterOptions
    {
        public string ClassPrefix { get; set; } = "ql";
        public InlineStyles InlineStyles { get; set; }
        public string ListItemTag { get; set; } = "li";
        public string ParagraphTag { get; set; } = "p";
        public string LinkRel { get; set; }
        public string LinkTarget { get; set; }
        public bool? AllowBackgroundClasses { get; set; }
    }
}
