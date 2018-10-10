using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Quill.Delta
{
    public class QuillDeltaToXmlConverter
    {
        QuillDeltaToXmlConverterOptions _options;
        JArray _rawOps;
        OpToXmlConverterOptions _converterOptions;
        internal XmlDocument _document;

        public QuillDeltaToXmlConverter(JArray ops, QuillDeltaToXmlConverterOptions options = null)
        {
            _options = options ?? new QuillDeltaToXmlConverterOptions();
            _rawOps = ops;

            _converterOptions = new OpToXmlConverterOptions()
            {
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
            return ListNester.Nest(groupedOps);
        }

        public XmlDocument Convert()
        {
            _document = new XmlDocument();
            var rootNode = _document.CreateElement(_options.RootNodeTag);
            _document.AppendChild(rootNode);

            var groups = GetGroupedOps();
            foreach (var group in groups)
            {
                XmlNode node;
                if (group is ListGroup lg)
                {
                    node = RenderWithCallbacks(
                        GroupType.List, group, () => RenderList(lg));

                }
                else if (group is BlockGroup bg)
                {
                    node = RenderWithCallbacks(
                       GroupType.Block, group, () => RenderBlock(bg.Op, bg.Ops));

                }
                else if (group is BlotBlock bb)
                {
                    node = RenderCustom(bb.Op, null);
                }
                else if (group is VideoItem vi)
                {
                    node = RenderWithCallbacks(GroupType.Video, group, () =>
                    {
                        var converter = new OpToXmlConverter(vi.Op, _converterOptions);
                        return converter.GetXml(_document);
                    });
                }
                else // InlineGroup
                {
                    node = RenderWithCallbacks(GroupType.InlineGroup, group, () =>
                        RenderInlines(((InlineGroup)group).Ops, true));
                }
                if (node != null)
                {
                    rootNode.AppendChild(node);
                }
            }
            return _document;
        }

        XmlNode RenderWithCallbacks(GroupType groupType, Group group, Func<XmlNode> myRenderFn)
        {
            XmlNode node = null;
            if (_options.BeforeRenderer != null)
            {
                node = _options.BeforeRenderer(_document, groupType, group);
            }
            if (node== null)
            {
                node = myRenderFn();
            }
            if (_options.AfterRenderer != null)
            {
                node = _options.AfterRenderer(_document, groupType, node);
            }
            return node;
        }

        XmlNode RenderList(ListGroup list)
        {
            var firstItem = list.Items[0];
            var node = XmlHelpers.MakeElement(_document, GetListTag(firstItem.Item.Op)) ??
                _document.CreateDocumentFragment();
            foreach (var listItem in list.Items)
            {
                var child = RenderListItem(listItem);
                if (child != null)
                {
                    node.AppendChild(child);
                }
            }
            return node;
        }

        XmlNode RenderListItem(ListItem li)
        {
            //if (!isOuterMost) {
            li.Item.Op.Attributes.Indent = 0;
            //}
            var converter = new OpToXmlConverter(li.Item.Op, _converterOptions);
            var parts = converter.GetXmlParts(_document);
            var listElements = RenderInlines(li.Item.Ops, false);
            if (listElements != null)
            {
                parts.InnerNode.AppendChild(listElements);
            }
            if (li.InnerList != null)
            {
                var innerList = RenderList(li.InnerList);
                if (innerList != null)
                {
                    parts.InnerNode.AppendChild(innerList);
                }
            }
            return parts.OuterNode;
        }

        internal XmlNode RenderBlock(DeltaInsertOp bop, IList<DeltaInsertOp> ops)
        {
            var converter = new OpToXmlConverter(bop, _converterOptions);
            var parts = converter.GetXmlParts(_document);

            if (bop.IsCodeBlock())
            {
                foreach (var iop in ops)
                {
                    parts.InnerNode.AppendChild(
                        iop.IsCustom() ? RenderCustom(iop, bop) :
                            _document.CreateTextNode(((InsertDataString)iop.Insert).Value));
                }
                return parts.OuterNode;
            }
            if (!ops.Any())
            {
                parts.InnerNode.AppendChild(_document.CreateElement("br"));
            }
            else
            {
                foreach (var op in ops)
                {
                    parts.InnerNode.AppendChild(RenderInline(op, bop));
                }
            }
            return parts.OuterNode;
        }

        internal XmlNode RenderInlines(IList<DeltaInsertOp> ops, bool isInlineGroup = true)
        {
            XmlNode inlines = null;
            for (int i = 0; i < ops.Count; ++i)
            {
                var op = ops[i];
                if (!(i > 0 && i == ops.Count - 1 && op.IsJustNewline()))
                {
                    var node = RenderInline(op, null);
                    if (node != null)
                    {
                        if (inlines == null)
                        {
                            inlines = _document.CreateDocumentFragment();
                        }
                        inlines.AppendChild(node);
                    }
                }
            }
            if (!isInlineGroup || String.IsNullOrEmpty(_options.ParagraphTag))
            {
                return inlines;
            }
            // wrap in a paragraph node
            var p = _document.CreateElement(_options.ParagraphTag);
            if (_options.MultiLineParagraph || inlines == null ||
                inlines.ChildNodes.Count == 1)
            {
                if (inlines != null)
                {
                    p.AppendChild(inlines);
                }
                return p;
            }

            // split any lines off into separate paragraph tags
            var ps = _document.CreateDocumentFragment();
            ps.AppendChild(p);
            var inline = inlines.FirstChild;
            while (inline != null)
            {
                var nextInline = inline.NextSibling;
                if (inline is XmlElement el && el.Name == "br" &&
                    nextInline != null)
                {
                    if (p.ChildNodes.Count == 0)
                    {
                        p.AppendChild(_document.CreateElement("br"));
                    }
                    p = _document.CreateElement(_options.ParagraphTag);
                    ps.AppendChild(p);
                }
                else
                {
                    p.AppendChild(inline);
                }
                inline = nextInline;
            }

            return ps;
        }

        XmlNode TextNodeOrBr(string text)
        {
            return String.IsNullOrEmpty(text) ?
                (XmlNode)_document.CreateElement("br") :
                _document.CreateTextNode(text);
        }

        XmlNode RenderInline(DeltaInsertOp op, DeltaInsertOp contextOp)
        {
            if (op.IsCustom())
            {
                return RenderCustom(op, contextOp);
            }
            var converter = new OpToXmlConverter(op, _converterOptions);
            var xmlParts = converter.GetXmlParts(_document);
            // replace new lines with br
            var lines = xmlParts.Content.Split("\n");
            if (lines.Length == 1)
            {
                var child = _document.CreateTextNode(lines[0]);
                if (xmlParts.InnerNode == null)
                {
                    return child;
                }
                xmlParts.InnerNode.AppendChild(child);
            }
            else
            {
                if (xmlParts.InnerNode == null)
                {
                    xmlParts.OuterNode = xmlParts.InnerNode =
                        _document.CreateDocumentFragment();
                }
                for (int i = 0; i < lines.Length; ++i)
                {
                    var line = lines[i];
                    if (!String.IsNullOrEmpty(line))
                    {
                        xmlParts.InnerNode.AppendChild(
                            _document.CreateTextNode(line));
                    }
                    if (i < lines.Length - 1)
                    {
                        xmlParts.InnerNode.AppendChild(
                            _document.CreateElement("br"));
                    }
                }
            }
            return xmlParts.OuterNode;
        }

        XmlNode RenderCustom(DeltaInsertOp op, DeltaInsertOp contextOp)
        {
            if (_options.CustomRenderer != null)
            {
                return _options.CustomRenderer(_document, op, contextOp);
            }
            return null;
        }
    }
}
