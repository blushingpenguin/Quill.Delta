using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class InsertOpsConverterTest
    {
        [Test]
        public void ManyOpsTest()
        {
            var data = JObject.Parse(@"{
            ops: [
                {""insert"":""This ""},
                {""attributes"":{""font"":""monospace""},""insert"":""is""},
                {""insert"":"" a ""},{""attributes"":{""size"":""large""},""insert"":""test""},
                {""insert"":"" ""},
                {""attributes"":{""italic"":true,""bold"":true},""insert"":""data""},
                {""insert"":"" ""},
                {""attributes"":{""underline"":true,""strike"":true},""insert"":""that""},
                {""insert"":"" is ""},{""attributes"":{""color"":""#e60000""},""insert"":""will""},
                {""insert"":"" ""},{""attributes"":{""background"":""#ffebcc""},""insert"":""test""},
                {""insert"":"" ""},{""attributes"":{""script"":""sub""},""insert"":""the""},
                {""insert"":"" ""},{""attributes"":{""script"":""super""},""insert"":""rendering""},
                {""insert"":"" of ""},{""attributes"":{""link"":""yahoo""},""insert"":""inline""},
                {""insert"":"" ""},
                {""insert"":{""formula"":""x=data""}},
                {""insert"":""  formats.\n""}
            ]}");

            var objs = InsertOpsConverter.Convert((JArray)data["ops"]);
            objs.Count.Should().Be(22);
        }

        [Test]
        public void NullOpsTest()
        {
            InsertOpsConverter.Convert(null).Should().Equal(new DeltaInsertOp[0]);
        }

        [Test]
        public void EmptyOpTest()
        {
            var data = JArray.Parse("[{insert:''}]");
            InsertOpsConverter.Convert(data).Should().Equal(new DeltaInsertOp[0]);
        }
        
        [Test]
        public void NullOpTest()
        {
            InsertOpsConverter.Convert(null).Should().Equal(new DeltaInsertOp[0]);
        }

        [Test]
        public void CakeOpTest()
        {
            var data = JArray.Parse("[{insert:{cake: ''}}]");
            InsertOpsConverter.Convert(data).Should().BeEquivalentTo(new DeltaInsertOp[] {
                new DeltaInsertOp(new InsertDataCustom("cake", new JValue("")))
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void DuffOpTest()
        {
            var data = JArray.Parse("[{\"insert\": 2}]");
            InsertOpsConverter.Convert(data).Should().Equal(new DeltaInsertOp[0]);
        }

        [Test]
        public void InsertValueConversionToNull()
        {
            var values = new JToken[] {
                JValue.CreateNull(),
                JValue.CreateUndefined(),
                new JValue(3),
                new JObject()
            };
            foreach (var value in values)
            {
                InsertOpsConverter.ConvertInsertVal(value).Should().Be(null);
            }
        }

        [Test]
        public void InsertNullConversion()
        {
            InsertOpsConverter.ConvertInsertVal(null).Should().BeNull();
        }

        [Test]
        public void InsertStringConversion()
        {
            var result = InsertOpsConverter.ConvertInsertVal("fdsf");
            result.Should().BeOfType<InsertDataText>();
            ((InsertDataText)result).Value.Should().Be("fdsf");
        }

        [Test]
        public void InsertImageConversion()
        {
            var input = JObject.Parse("{image: 'ff'}");
            var result = InsertOpsConverter.ConvertInsertVal(input);
            result.Should().BeOfType<InsertDataImage>();
            ((InsertDataImage)result).Value.Should().Be("ff");
        }

        [Test]
        public void InsertVideoConversion()
        {
            var input = JObject.Parse("{video: ''}");
            var result = InsertOpsConverter.ConvertInsertVal(input);
            result.Should().BeOfType<InsertDataVideo>();
            ((InsertDataVideo)result).Value.Should().Be("");
        }

        [Test]
        public void InsertFormulaConversion()
        {
            var input = JObject.Parse("{formula: ''}");
            var result = InsertOpsConverter.ConvertInsertVal(input);
            result.Should().BeOfType<InsertDataFormula>();
            ((InsertDataFormula)result).Value.Should().Be("");
        }
    }
}
