using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Quill.Delta
{
    public static class XmlHelpers
    {
        public static XmlNode MakeElement(XmlDocument doc, string tag,
            IList<TagKeyValue> attrs = null)
        {
            if (String.IsNullOrEmpty(tag))
            {
                return null;
            }

            var el = doc.CreateElement(tag);

            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    el.SetAttribute(attr.Key, attr.Value ?? "");
                }
            }
            return el;
        }

        public static string EncodeXml(string xml)
        {
            var result = new StringBuilder(xml.Length);
            foreach (char c in xml)
            {
                switch (c)
                {
                    case '\'':
                        result.Append("&apos;");
                        break;
                    case '"':
                        result.Append("&quot;");
                        break;
                    case '<':
                        result.Append("&lt;");
                        break;
                    case '>':
                        result.Append("&gt;");
                        break;
                    case '&':
                        result.Append("&amp;");
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            return result.ToString();
        }
    }
}
