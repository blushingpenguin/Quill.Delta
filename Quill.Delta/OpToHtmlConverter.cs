using System;
using System.Collections.Generic;

namespace Quill.Delta
{
    public struct HtmlParts
    {
        public string OpeningTag { get; set; }
        public string Content { get; set; }
        public string ClosingTag { get; set; }
    }

    public class OpToHtmlConverterOptions : OpConverterOptions
    {
        public bool EncodeHtml { get; set; } = true;
    }

    public class OpToHtmlConverter : OpConverter
    {
        public OpToHtmlConverter(DeltaInsertOp op, OpToHtmlConverterOptions options = null) :
            base(op, options ?? new OpToHtmlConverterOptions())
        {
        }

        public OpToHtmlConverterOptions Options { get => (OpToHtmlConverterOptions)_options; }

        protected override string EncodeLink(string link) => HtmlHelpers.EncodeLink(link);

        protected override string EncodeContent(string content) =>
            Options.EncodeHtml ? HtmlHelpers.EncodeHtml(content) : content;

        public string GetHtml()
        {
            var parts = GetHtmlParts();
            return $"{parts.OpeningTag}{parts.Content}{parts.ClosingTag}";
        }

        public HtmlParts GetHtmlParts()
        {
            if (_op.IsJustNewline() && !_op.IsContainerBlock())
            {
                return new HtmlParts() { OpeningTag = "", ClosingTag = "", Content = "\n" };
            }

            var tags = GetTags();
            var attrs = GetTagAttributes();
            if (tags.Count == 0 && attrs.Count > 0)
            {
                tags.Add("span");
            }

            var beginTags = new List<string>();
            var endTags = new List<string>();

            foreach (var tag in tags)
            {
                beginTags.Add(HtmlHelpers.MakeStartTag(tag, attrs));
                endTags.Add(tag == "img" ? "" : HtmlHelpers.MakeEndTag(tag));
                // consumed in first tag
                attrs = null;
            }
            endTags.Reverse();

            return new HtmlParts()
            {
                OpeningTag = String.Join("", beginTags),
                Content = GetContent(),
                ClosingTag = String.Join("", endTags)
            };
        }
    }
}
