using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class AlignConverterTest
    {
        [Test]
        public void TestGetStringValueCenter()
        {
            AlignType dirn = AlignType.Center;
            AlignConverter.GetStringValue(dirn).Should().Be("center");
        }

        [Test]
        public void TestGetStringValueJustify()
        {
            AlignType dirn = AlignType.Justify;
            AlignConverter.GetStringValue(dirn).Should().Be("justify");
        }

        [Test]
        public void TestGetStringValueRight()
        {
            AlignType dirn = AlignType.Right;
            AlignConverter.GetStringValue(dirn).Should().Be("right");
        }

        [Test]
        public void TestGetStringValueInvalidEnumValue()
        {
            AlignType dirn = (AlignType)99;
            AlignConverter.GetStringValue(dirn).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableNull()
        {
            AlignType? dirn = null;
            AlignConverter.GetStringValue(dirn).Should().Be("");
        }

        [Test]
        public void TestGetEnumValueDuffValue()
        {
            AlignConverter.GetEnumValue("rubbish").Should().Be(null);
        }

        [Test]
        public void TestGetEnumValueCenter()
        {
            AlignConverter.GetEnumValue("center").Should().Be(AlignType.Center);
        }

        [Test]
        public void TestGetEnumValueJustify()
        {
            AlignConverter.GetEnumValue("justify").Should().Be(AlignType.Justify);
        }

        [Test]
        public void TestGetEnumValueRight()
        {
            AlignConverter.GetEnumValue("right").Should().Be(AlignType.Right);
        }
    }
}
