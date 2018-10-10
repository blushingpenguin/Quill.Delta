using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class ListConverterTest
    {
        [Test]
        public void TestGetStringValueBullet()
        {
            ListType list = ListType.Bullet;
            ListConverter.GetStringValue(list).Should().Be("bullet");
        }

        [Test]
        public void TestGetStringValueOrdered()
        {
            ListType list = ListType.Ordered;
            ListConverter.GetStringValue(list).Should().Be("ordered");
        }

        [Test]
        public void TestGetStringValueChecked()
        {
            ListType list = ListType.Checked;
            ListConverter.GetStringValue(list).Should().Be("checked");
        }

        [Test]
        public void TestGetStringValueUnchecked()
        {
            ListType list = ListType.Unchecked;
            ListConverter.GetStringValue(list).Should().Be("unchecked");
        }

        [Test]
        public void TestGetStringValueInvalidEnumValue()
        {
            ListType list = (ListType)99;
            ListConverter.GetStringValue(list).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableNull()
        {
            ListType? list = null;
            ListConverter.GetStringValue(list).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableValue()
        {
            ListType? list = ListType.Ordered;
            ListConverter.GetStringValue(list).Should().Be("ordered");
        }

        [Test]
        public void TestGetEnumValueDuffValue()
        {
            ListConverter.GetEnumValue("rubbish").Should().Be(null);
        }

        [Test]
        public void TestGetEnumValueBullet()
        {
            ListConverter.GetEnumValue("bullet").Should().Be(ListType.Bullet);
        }

        [Test]
        public void TestGetEnumValueOrdered()
        {
            ListConverter.GetEnumValue("ordered").Should().Be(ListType.Ordered);
        }

        [Test]
        public void TestGetEnumValueChecked()
        {
            ListConverter.GetEnumValue("checked").Should().Be(ListType.Checked);
        }

        [Test]
        public void TestGetEnumValueUnchecked()
        {
            ListConverter.GetEnumValue("unchecked").Should().Be(ListType.Unchecked);
        }
    }
}
