using System;
using System.Collections.Generic;
using System.Text;

namespace Quill.Delta
{
    public struct TagKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
        
        public TagKeyValue(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public enum EncodeTarget
    {
        Html = 0,
        Url = 1
    }

    public static class HtmlHelpers
    {
        public static string MakeStartTag(string tag, IList<TagKeyValue> attrs = null)
        {
            if (String.IsNullOrEmpty(tag))
            {
                return "";
            }

            var result = new StringBuilder();
            result.Append('<');
            result.Append(tag);

            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    result.Append(' ');
                    result.Append(attr.Key);
                    if (!String.IsNullOrEmpty(attr.Value))
                    {
                        result.Append("=\"");
                        result.Append(attr.Value);
                        result.Append("\"");
                    }
                }
            }
            if (tag == "img" || tag == "br")
            {
                result.Append("/>");
            }
            else
            {
                result.Append('>');
            }
            return result.ToString();
        }

        public static string MakeEndTag(string tag)
        {
            if (String.IsNullOrEmpty(tag))
            {
                return "";
            }
            return $"</{tag}>";
        }

        
        public static string DecodeHtml(string str)
        {
            return str.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">")
                .Replace("&quot;", "\"").Replace("&#x27;", "'").Replace("&#x2F;", "/");
            // return WebUtility.HtmlDecode(str);
        }

        public static string EncodeHtml(string str, bool preventDoubleEncoding = true)
        {
            if (preventDoubleEncoding)
            {
                str = DecodeHtml(str);
            }
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("\"", "&quot;").Replace("'", "&#x27;").Replace("/", "&#x2F;");
            // return WebUtility.HtmlEncode(str);
        }

        public static string EncodeLink(string str)
        {

            return str.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">")
                .Replace("&#x27;", "'").Replace("&quot;", "\"").Replace("&#40;", "(")
                .Replace("&#41;", ")")
                .Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("'", "&#x27;").Replace("\"", "&quot;").Replace("(", "&#40;")
                .Replace(")", "&#41;");
            // return WebUtility.UrlEncode(str);
        }
    }
}
