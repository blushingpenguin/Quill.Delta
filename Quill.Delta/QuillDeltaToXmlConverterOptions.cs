using System.Xml;

namespace Quill.Delta
{
    public delegate XmlNode XmlCustomRenderer(XmlDocument doc, DeltaInsertOp op, DeltaInsertOp contextOp);
    public delegate XmlNode XmlBeforeRenderer(XmlDocument doc, GroupType groupType, Group group);
    public delegate XmlNode XmlAfterRenderer(XmlDocument doc, GroupType groupType, XmlNode node);

    public class QuillDeltaToXmlConverterOptions
    {
        // no more allowing these to be customized; unnecessary
        public string OrderedListTag { get; set; } = "ol";
        public string BulletListTag { get; set; } = "ul";
        public string ListItemTag { get; set; } = "li";

        public string ParagraphTag { get; set; } = "p";
        public string ClassPrefix { get; set; } = "ql";

        public InlineStyles InlineStyles { get; set; }
        public bool EncodeHtml { get; set; } = true;
        public bool MultiLineBlockquote { get; set; } = true;
        public bool MultiLineHeader { get; set; } = true;
        public bool MultiLineCodeblock { get; set; } = true;
        public bool MultiLineParagraph { get; set; } = true;

        public string LinkRel { get; set; } = "";
        public string LinkTarget { get; set; } = "_blank";
        public bool AllowBackgroundClasses { get; set; } = false;

        public string RootNodeTag { get; set; } = "template";

        public XmlCustomRenderer CustomRenderer { get; set; }
        public XmlBeforeRenderer BeforeRenderer { get; set; }
        public XmlAfterRenderer AfterRenderer { get; set; }
    }
}
