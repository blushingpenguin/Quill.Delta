using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quill.Delta
{
    public class HtmlConverter : Converter
    {
        const string BrTag = "<br/>";

        OpToHtmlConverterOptions _converterOptions;

        public HtmlConverter(JArray ops, HtmlConverterOptions options = null) :
            base(ops, options ?? new HtmlConverterOptions())
        {
            _converterOptions = new OpToHtmlConverterOptions()
            {
                EncodeHtml = Options.EncodeHtml,
                ClassPrefix = _options.ClassPrefix,
                InlineStyles = _options.InlineStyles,
                ListItemTag = _options.ListItemTag,
                ParagraphTag = _options.ParagraphTag,
                LinkRel = _options.LinkRel,
                LinkTarget = _options.LinkTarget,
                AllowBackgroundClasses = _options.AllowBackgroundClasses
            };
        }

        public HtmlConverterOptions Options { get => (HtmlConverterOptions)_options; }

        public string Convert()
        {
            var groups = GetGroupedOps();
            return String.Join("", groups.Select(group =>
            {
                if (group is ListGroup lg)
                {
                    return RenderWithCallbacks(
                        GroupType.List, group, () => RenderList(lg));

                }
                else if (group is BlockGroup bg)
                {
                    return RenderWithCallbacks(
                       GroupType.Block, group, () => RenderBlock(bg.Op, bg.Ops));

                }
                else if (group is BlotBlock bb)
                {
                    return RenderCustom(bb.Op, null);
                }
                else if (group is VideoItem vi)
                {
                    return RenderWithCallbacks(GroupType.Video, group, () =>
                    {
                        var converter = new OpToHtmlConverter(vi.Op, _converterOptions);
                        return converter.GetHtml();
                    });
                }
                else // InlineGroup
                {
                    return RenderWithCallbacks(GroupType.InlineGroup, group, () =>
                        RenderInlines(((InlineGroup)group).Ops, true));
                }
            }));
        }

        string RenderWithCallbacks(GroupType groupType, Group group, Func<string> myRenderFn)
        {
            string html = null;
            if (Options.BeforeRenderer != null)
            {
                html = Options.BeforeRenderer(groupType, group);
            }
            if (String.IsNullOrEmpty(html))
            {
                html = myRenderFn();
            }
            if (Options.AfterRenderer != null)
            {
                html = Options.AfterRenderer(groupType, html);
            }
            return html;
        }

        string RenderList(ListGroup list)
        {
            var firstItem = list.Items[0];
            return HtmlHelpers.MakeStartTag(GetListTag(firstItem.Item.Op))
               + String.Join("", list.Items.Select(li => RenderListItem(li)))
               + HtmlHelpers.MakeEndTag(GetListTag(firstItem.Item.Op));
        }

        string RenderListItem(ListItem li)
        {
            //if (!isOuterMost) {
            li.Item.Op.Attributes.Indent = 0;
            //}
            var converter = new OpToHtmlConverter(li.Item.Op, _converterOptions);
            var parts = converter.GetHtmlParts();
            var liElementsHtml = RenderInlines(li.Item.Ops, false);
            return parts.OpeningTag + (liElementsHtml) +
                (li.InnerList != null ? RenderList(li.InnerList) : "")
                + parts.ClosingTag;
        }

        internal string RenderBlock(DeltaInsertOp bop, IList<DeltaInsertOp> ops)
        {
            var converter = new OpToHtmlConverter(bop, _converterOptions);
            var htmlParts = converter.GetHtmlParts();

            if (bop.IsCodeBlock())
            {
                return htmlParts.OpeningTag +
                    HtmlHelpers.EncodeHtml(
                        String.Join("",
                            ops.Select(iop => iop.IsCustom() ?
                                RenderCustom(iop, bop) :
                                ((InsertDataString)iop.Insert).Value)))
                   + htmlParts.ClosingTag;
            }

            var inlines = String.Join("", ops.Select(op => RenderInline(op, bop)));
            return htmlParts.OpeningTag + (inlines.Length > 0 ? inlines : BrTag) +
                htmlParts.ClosingTag;
        }

        internal string RenderInlines(IList<DeltaInsertOp> ops, bool isInlineGroup = true)
        {
            var opsLen = ops.Count - 1;
            var html = String.Join("", ops.Select((op, i) => {
                if (i > 0 && i == opsLen && op.IsJustNewline())
                {
                    return "";
                }
                return RenderInline(op, null);
            }));
            if (!isInlineGroup)
            {
                return html;
            }

            var startParaTag = HtmlHelpers.MakeStartTag(Options.ParagraphTag);
            var endParaTag = HtmlHelpers.MakeEndTag(Options.ParagraphTag);
            if (html == BrTag || Options.MultiLineParagraph)
            {
                return startParaTag + html + endParaTag;
            }
            return startParaTag + String.Join(endParaTag + startParaTag,
                html.Split(new[] { BrTag }, StringSplitOptions.None)
                    .Select(v => String.IsNullOrEmpty(v) ? BrTag : v)) +
                endParaTag;
        }

        string RenderInline(DeltaInsertOp op, DeltaInsertOp contextOp)
        {
            if (op.IsCustom())
            {
                return RenderCustom(op, contextOp);
            }
            var converter = new OpToHtmlConverter(op, _converterOptions);
            return converter.GetHtml().Replace("\n", BrTag);
        }

        string RenderCustom(DeltaInsertOp op, DeltaInsertOp contextOp)
        {
            if (Options.CustomRenderer != null)
            {
                return Options.CustomRenderer(op, contextOp);
            }
            return "";
        }
    }
}
