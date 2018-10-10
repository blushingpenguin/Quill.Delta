using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Quill.Delta
{
    public class XmlConverter : Converter
    {
        OpToXmlConverterOptions _converterOptions;
        internal XmlDocument _document;

        public XmlConverter(JArray ops, XmlConverterOptions options = null) :
            base(ops, options ?? new XmlConverterOptions())
        {
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

        public XmlConverterOptions Options { get => (XmlConverterOptions)_options; }

        public XmlDocument Convert()
        {
            _document = new XmlDocument();
            var rootNode = _document.CreateElement(Options.RootNodeTag);
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
            if (Options.BeforeRenderer != null)
            {
                node = Options.BeforeRenderer(_document, groupType, group);
            }
            if (node== null)
            {
                node = myRenderFn();
            }
            if (Options.AfterRenderer != null)
            {
                node = Options.AfterRenderer(_document, groupType, node);
            }
            return node;
        }

        internal XmlNode RenderList(ListGroup list)
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

        internal XmlNode RenderListItem(ListItem li)
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
                parts.InnerNode.AppendChild(RenderList(li.InnerList));
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
            if (!isInlineGroup || String.IsNullOrEmpty(Options.ParagraphTag))
            {
                return inlines;
            }
            // wrap in a paragraph node
            var p = _document.CreateElement(Options.ParagraphTag);
            if (Options.MultiLineParagraph || inlines == null ||
                inlines.ChildNodes.Count == 1)
            {
                if (inlines != null)
                {
                    p.AppendChild(inlines);
                }
                return p;
            }

            // split any lines off into separate paragraph tags
            // each <br/> tag makes one paragraph, empty paragraphs get br tags added
            var ps = _document.CreateDocumentFragment();
            ps.AppendChild(p);
            var inline = inlines.FirstChild;
            while (inline != null)
            {
                var nextInline = inline.NextSibling;
                if (inline is XmlElement el && el.Name == "br")
                {
                    if (p.ChildNodes.Count == 0)
                    {
                        p.AppendChild(_document.CreateElement("br"));
                    }
                    p = _document.CreateElement(Options.ParagraphTag);
                    ps.AppendChild(p);
                }
                else
                {
                    p.AppendChild(inline);
                }
                inline = nextInline;
            }
            if (p.ChildNodes.Count == 0)
            {
                p.AppendChild(_document.CreateElement("br"));
            }

            return ps;
        }

        internal XmlNode RenderInline(DeltaInsertOp op, DeltaInsertOp contextOp)
        {
            if (op.IsCustom())
            {
                return RenderCustom(op, contextOp);
            }
            var converter = new OpToXmlConverter(op, _converterOptions);
            var xmlParts = converter.GetXmlParts(_document);
            // replace new lines with br
            var lines = xmlParts.Content.Split('\n');
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
            if (Options.CustomRenderer != null)
            {
                return Options.CustomRenderer(_document, op, contextOp);
            }
            return null;
        }
    }
}
