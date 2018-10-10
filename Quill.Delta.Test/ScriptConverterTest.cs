using FluentAssertions;
using NUnit.Framework;
using System;

namespace Quill.Delta.Test
{
    public class ScriptConverterTest
    {
        [Test]
        public void TestGetStringValueSub()
        {
            ScriptType type = ScriptType.Sub;
            ScriptConverter.GetStringValue(type).Should().Be("sub");
        }

        [Test]
        public void TestGetStringValueSuper()
        {
            ScriptType type = ScriptType.Super;
            ScriptConverter.GetStringValue(type).Should().Be("super");
        }

        [Test]
        public void TestGetStringValueInvalidEnumValue()
        {
            ScriptType type = (ScriptType)99;
            ScriptConverter.GetStringValue(type).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableNull()
        {
            ScriptType? type = null;
            ScriptConverter.GetStringValue(type).Should().Be("");
        }

        [Test]
        public void TestGetStringValueNullableValue()
        {
            ScriptType? type = ScriptType.Super;
            ScriptConverter.GetStringValue(type).Should().Be("super");
        }

        [Test]
        public void TestGetEnumValueDuffValue()
        {
            ScriptConverter.GetEnumValue("rubbish").Should().Be(null);
        }

        [Test]
        public void TestGetEnumValueSub()
        {
            ScriptConverter.GetEnumValue("sub").Should().Be(ScriptType.Sub);
        }

        [Test]
        public void TestGetEnumValueSuper()
        {
            ScriptConverter.GetEnumValue("super").Should().Be(ScriptType.Super);
        }

        [Test]
        public void TestGetTagSuper()
        {
            ScriptConverter.GetTag(ScriptType.Super).Should().Be("sup");
        }

        [Test]
        public void TestGetTagSub()
        {
            ScriptConverter.GetTag(ScriptType.Sub).Should().Be("sub");
        }

        [Test]
        public void TestGetTagInvalidEnumValue()
        {
            ScriptType type = (ScriptType)99;
            Action a = () => ScriptConverter.GetTag(type);
            a.Should().Throw<InvalidOperationException>().WithMessage("Unable to find a tag for*");
        }
    }
}
