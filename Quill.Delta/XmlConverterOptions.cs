using System.Xml;

namespace Quill.Delta
{
    public delegate XmlNode XmlCustomRenderer(XmlDocument doc, DeltaInsertOp op, DeltaInsertOp contextOp);
    public delegate XmlNode XmlBeforeRenderer(XmlDocument doc, GroupType groupType, Group group);
    public delegate XmlNode XmlAfterRenderer(XmlDocument doc, GroupType groupType, XmlNode node);

    public class XmlConverterOptions : ConverterOptions
    {
        public string RootNodeTag { get; set; } = "template";

        public XmlCustomRenderer CustomRenderer { get; set; }
        public XmlBeforeRenderer BeforeRenderer { get; set; }
        public XmlAfterRenderer AfterRenderer { get; set; }
    }
}
