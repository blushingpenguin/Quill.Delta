using System.Xml;

namespace Quill.Delta
{
    public class XmlParts
    {
        public XmlNode OuterNode { get; set; }
        public XmlNode InnerNode { get; set; }
        public string Content { get; set; }
    }

    public class OpToXmlConverterOptions : OpConverterOptions
    {
    }

    public class OpToXmlConverter : OpConverter
    {
        public OpToXmlConverter(DeltaInsertOp op, OpToXmlConverterOptions options = null) :
            base(op, options ?? new OpToXmlConverterOptions())
        {
        }
        
        public OpToXmlConverterOptions Options { get => (OpToXmlConverterOptions)_options; }

        protected override string EncodeLink(string link) => link;
        protected override string EncodeContent(string content) => content;

        public XmlParts GetXmlParts(XmlDocument doc)
        {
            if (_op.IsJustNewline() && !_op.IsContainerBlock())
            {
                return new XmlParts { Content = "\n" };
            }

            var tags = GetTags();
            var attrs = GetTagAttributes();
            if (tags.Count == 0 && attrs.Count > 0)
            {
                tags.Add("span");
            }

            var result = new XmlParts();
            foreach (var tag in tags)
            {
                var child = XmlHelpers.MakeElement(doc, tag, attrs);
                if (result.InnerNode == null)
                {
                    result.OuterNode = result.InnerNode = child;
                }
                else
                {
                    result.InnerNode.AppendChild(child);
                }
                result.InnerNode = child;
                // consumed in first tag
                attrs = null;
            }
            result.Content = GetContent();

            return result;
        }

        public XmlNode GetXml(XmlDocument doc)
        {
            var parts = GetXmlParts(doc);
            var content = doc.CreateTextNode(parts.Content ?? "");
            if (parts.InnerNode != null)
            {
                parts.InnerNode.AppendChild(content);
                return parts.OuterNode;
            }
            return content;
        }
    }
}
