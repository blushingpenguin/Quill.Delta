using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quill.Delta
{
    public class QuillDeltaToHtmlConverter
    {
        const string BrTag = "<br/>";

        QuillDeltaToHtmlConverterOptions _options;
        JArray _rawOps;
        OpToHtmlConverterOptions _converterOptions;

        public QuillDeltaToHtmlConverter(JArray ops, QuillDeltaToHtmlConverterOptions options = null)
        {
            _options = options ?? new QuillDeltaToHtmlConverterOptions();
            _rawOps = ops;

            _converterOptions = new OpToHtmlConverterOptions()
            {
                EncodeHtml = _options.EncodeHtml,
                ClassPrefix = _options.ClassPrefix,
                InlineStyles = _options.InlineStyles,
                ListItemTag = _options.ListItemTag,
                ParagraphTag = _options.ParagraphTag,
                LinkRel = _options.LinkRel,
                LinkTarget = _options.LinkTarget,
                AllowBackgroundClasses = _options.AllowBackgroundClasses
            };
        }

        internal string GetListTag(DeltaInsertOp op)
        {
            return op.IsOrderedList() ? _options.OrderedListTag :
                op.IsBulletList() ? _options.BulletListTag :
                op.IsCheckedList() ? _options.BulletListTag :
                op.IsUncheckedList() ? _options.BulletListTag :
                "";
        }

        IList<Group> GetGroupedOps()
        {
            var deltaOps = InsertOpsConverter.Convert(_rawOps);
            var pairedOps = Grouper.PairOpsWithTheirBlock(deltaOps);
            var groupedSameStyleBlocks = Grouper.GroupConsecutiveSameStyleBlocks(pairedOps,
                header: _options.MultiLineHeader,
                codeBlocks: _options.MultiLineCodeblock,
                blockquotes: _options.MultiLineBlockquote
            );
            var groupedOps = Grouper.ReduceConsecutiveSameStyleBlocksToOne(groupedSameStyleBlocks);
            // var listNester = new ListNester();
            return ListNester.Nest(groupedOps);
        }

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
            if (_options.BeforeRenderer != null)
            {
                html = _options.BeforeRenderer(groupType, group);
            }
            if (String.IsNullOrEmpty(html))
            {
                html = myRenderFn();
            }
            if (_options.AfterRenderer != null)
            {
                html = _options.AfterRenderer(groupType, html);
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

            var startParaTag = HtmlHelpers.MakeStartTag(_options.ParagraphTag);
            var endParaTag = HtmlHelpers.MakeEndTag(_options.ParagraphTag);
            if (html == BrTag || _options.MultiLineParagraph)
            {
                return startParaTag + html + endParaTag;
            }
            return startParaTag + String.Join(endParaTag + startParaTag,
                html.Split(BrTag).Select(v => String.IsNullOrEmpty(v) ? BrTag : v)) +
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
            if (_options.CustomRenderer != null)
            {
                return _options.CustomRenderer(op, contextOp);
            }
            return "";
        }
    }
}
