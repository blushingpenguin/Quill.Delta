namespace Quill.Delta
{
    public delegate string CustomRenderer(DeltaInsertOp op, DeltaInsertOp contextOp);
    public delegate string BeforeRenderer(GroupType groupType, Group group);
    public delegate string AfterRenderer(GroupType groupType, string html);

    public class HtmlConverterOptions : ConverterOptions
    {
        public bool EncodeHtml { get; set; } = true;

        public CustomRenderer CustomRenderer { get; set; }
        public BeforeRenderer BeforeRenderer { get; set; }
        public AfterRenderer AfterRenderer { get; set; }
    }
}
