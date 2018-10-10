using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class DirectionConverterTest
    {
        [Test]
        public void TestGetStringValue()
        {
            DirectionType dirn = DirectionType.Rtl;
            DirectionConverter.GetStringValue(dirn).Should().Be("rtl");
        }

        [Test]
        public void TestGetStringValueInvalidEnumValue()
        {
            DirectionType dirn = (DirectionType)1;
            DirectionConverter.GetStringValue(dirn).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableNull()
        {
            DirectionType? dirn = null;
            DirectionConverter.GetStringValue(dirn).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableValue()
        {
            DirectionType? dirn = DirectionType.Rtl;
            DirectionConverter.GetStringValue(dirn).Should().Be("rtl");
        }

        [Test]
        public void TestGetEnumValueDuffValue()
        {
            DirectionConverter.GetEnumValue("rubbish").Should().Be(null);
        }

        [Test]
        public void TestGetEnumValueRtl()
        {
            DirectionConverter.GetEnumValue("rtl").Should().Be(DirectionType.Rtl);
        }
    }
}
