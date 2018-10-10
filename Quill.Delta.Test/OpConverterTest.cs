using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class OpConverterTest
    {
#if FALSE
            Font = (value, op) => InlineStyles.LookupValue(DEFAULT_INLINE_FONTS, value, "font-family:" + value),
            Size = InlineStyles.MakeLookup(DEFAULT_SIZES),
            Indent = (value, op) =>
            {
                int indentSize = Int32.Parse(value) * 3;
                var side = op.Attributes != null &&
                    op.Attributes.Direction == DirectionType.Rtl ? "right" : "left";
                return "padding-" + side + ":" + indentSize + "em";
            },
            Direction = (value, op) =>
            {
                if (value == "rtl")
                {
                    var hasAlign = op.Attributes != null && op.Attributes.Align.HasValue;
                    return $"direction:rtl{(hasAlign ? "" : "; text-align:inherit")}";
                }
                else
                {
                    return null;
                }
            }
#endif
        static InlineStyles s = OpConverter.DEFAULT_INLINE_STYLES;

        [Test]
        public void TestDefaultStylesDirectionNull()
        {
            s.Direction(null, null).Should().BeNull();
        }

        [Test]
        public void TestDefaultStylesDirectionRtlNullAttributes()
        {
            DeltaInsertOp op = new DeltaInsertOp("thing");
            s.Direction("rtl", op).Should().Be("direction:rtl; text-align:inherit");
        }

        [Test]
        public void TestDefaultStylesDirectionRtlAttributesNoAlign()
        {
            DeltaInsertOp op = new DeltaInsertOp("thing", new OpAttributes());
            s.Direction("rtl", op).Should().Be("direction:rtl; text-align:inherit");
        }

        [Test]
        public void TestDefaultStylesDirectionRtlAttributesHasAlign()
        {
            DeltaInsertOp op = new DeltaInsertOp("thing",
                new OpAttributes { Align = AlignType.Center });
            s.Direction("rtl", op).Should().Be("direction:rtl");
        }

        [Test]
        public void TestFontPresent()
        {
            s.Font("serif", null).Should().Be("font-family: Georgia, Times New Roman, serif");
        }

        [Test]
        public void TestFontNotPresent()
        {
            s.Font("roboto", null).Should().Be("font-family:roboto");
        }

        [Test]
        public void TestSizePresent()
        {
            s.Size("small", null).Should().Be("font-size: 0.75em");
        }

        [Test]
        public void TestSizeNotPresent()
        {
            s.Size("whatever", null).Should().Be("");
        }

        [Test]
        public void TestIndentNoAttributes()
        {
            DeltaInsertOp op = new DeltaInsertOp("thing");
            s.Indent("2", op).Should().Be("padding-left:6em");
        }

        [Test]
        public void TestIndentAttributesNoDirection()
        {
            DeltaInsertOp op = new DeltaInsertOp("thing",
                new OpAttributes());
            s.Indent("2", op).Should().Be("padding-left:6em");
        }

        [Test]
        public void TestIndentAttributesRtl()
        {
            DeltaInsertOp op = new DeltaInsertOp("thing",
                new OpAttributes { Direction = DirectionType.Rtl } );
            s.Indent("2", op).Should().Be("padding-right:6em");
        }
    }
}
