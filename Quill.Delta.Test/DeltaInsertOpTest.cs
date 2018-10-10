using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class DeltaInsertOpTest
    {
        [Test]
        public void ConstructorWithImage()
        {
            var embed = new InsertDataImage("https://");
            var t = new DeltaInsertOp(embed);
            t.Insert.Should().NotBeNull();
            t.Insert.Should().BeOfType<InsertDataImage>();
            t.Attributes.Should().NotBeNull();
        }

        [Test]
        public void ConstructorWithString()
        {
            var t = new DeltaInsertOp("test");
            t.Insert.Should().BeOfType<InsertDataText>();
            var idt = (InsertDataText)t.Insert;
            idt.Value.Should().Be("test");
        }

        [Test]
        public void ConstructorWithFormula()
        {
            var t = new DeltaInsertOp(new InsertDataFormula("x=data"));
            var f = (InsertDataFormula)t.Insert;
            f.Value.Should().Be("x=data");
        }

        [Test]
        public void ConstructorWithNoArgs()
        {
            var t = new DeltaInsertOp();
            t.Insert.Should().BeOfType<InsertDataText>();
            var idt = (InsertDataText)t.Insert;
            idt.Value.Should().Be("");
        }

        [Test]
        public void ConstructorWithAttributes()
        {
            var t = new DeltaInsertOp(attributes: new OpAttributes { Bold = true });
            t.Insert.Should().BeOfType<InsertDataText>();
            var idt = (InsertDataText)t.Insert;
            idt.Value.Should().Be("");
            t.Attributes.Should().BeEquivalentTo(new OpAttributes { Bold = true },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void IsContainerBlock()
        {
            var op = new DeltaInsertOp("test");
            op.IsContainerBlock().Should().BeFalse();

            op = new DeltaInsertOp("test", new OpAttributes() { Blockquote = true });
            op.IsContainerBlock().Should().BeTrue();
        }

        [Test]
        public void HasSameAdiAs()
        {
            var op1 = new DeltaInsertOp("\n", new OpAttributes() {
                Align = AlignType.Right, Indent = 2 });
            var op2 = new DeltaInsertOp("\n", new OpAttributes() {
                Align = AlignType.Right, Indent = 2 });

            op1.HasSameAdiAs(op2).Should().BeTrue();

            op2 = new DeltaInsertOp("\n", new OpAttributes() {
                Align = AlignType.Right, Indent = 3 });
            op1.HasSameAdiAs(op2).Should().BeFalse();
        }

        [Test]
        public void HasHigherIndentThan()
        {
            var op1 = new DeltaInsertOp("\n", new OpAttributes() { Indent = null });
            var op2 = new DeltaInsertOp("\n", new OpAttributes() { Indent = null });

            op1.HasHigherIndentThan(op2).Should().BeFalse();
        }

        [Test]
        public void IsInline()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes { });
            op.IsInline().Should().BeTrue();
        }

        [Test]
        public void IsJustNewline()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes { });
            op.IsJustNewline().Should().BeTrue();

            op = new DeltaInsertOp("\n\n ", new OpAttributes
            {
                List = ListType.Ordered
            });
            op.IsJustNewline().Should().BeFalse();
        }

        [Test]
        public void IsList()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes { });
            op.IsList().Should().BeFalse();

            op = new DeltaInsertOp("fds ", new OpAttributes
            { List = ListType.Ordered });
            op.IsList().Should().BeTrue();

            op = new DeltaInsertOp("fds ", new OpAttributes
            { List = ListType.Unchecked });
            op.IsList().Should().BeTrue();
        }

        [Test]
        public void IsBulletList()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes {
                List = ListType.Bullet });
            op.IsBulletList().Should().BeTrue();

            op = new DeltaInsertOp("fds ", new OpAttributes {
                List = ListType.Ordered });
            op.IsBulletList().Should().BeFalse();
        }

        [Test]
        public void IsOrderedList()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes {
                List = ListType.Bullet });
            op.IsOrderedList().Should().BeFalse();

            op = new DeltaInsertOp("fds ", new OpAttributes {
                List = ListType.Ordered });
            op.IsOrderedList().Should().BeTrue();
        }

        [Test]
        public void IsCheckedList()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes() {
                List = ListType.Unchecked });
            op.IsCheckedList().Should().BeFalse();

            op = new DeltaInsertOp("fds ", new OpAttributes {
                List = ListType.Checked });
            op.IsCheckedList().Should().BeTrue();
        }

        [Test]
        public void IsUncheckedList()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes {
                List = ListType.Bullet });
            op.IsUncheckedList().Should().BeFalse();

            op = new DeltaInsertOp("fds ", new OpAttributes {
                List = ListType.Unchecked });
            op.IsUncheckedList().Should().BeTrue();
        }

        [Test]
        public void IsSameListAs()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes {
                List = ListType.Bullet });
            var op2 = new DeltaInsertOp("ds", new OpAttributes {
                List = ListType.Bullet });
            op.IsSameListAs(op2).Should().BeTrue();

            var op3 = new DeltaInsertOp("fds ", new OpAttributes {
                List = ListType.Ordered });
            op.IsSameListAs(op3).Should().BeFalse();
        }

        [Test]
        public void IsText()
        {
            var op = new DeltaInsertOp("\n", new OpAttributes {
                List = ListType.Bullet });
            op.IsVideo().Should().BeFalse();
            op.IsText().Should().BeTrue();

            op = new DeltaInsertOp(new InsertDataImage("d"),
                new OpAttributes { List = ListType.Ordered });
            op.IsImage().Should().BeTrue();
            op.IsText().Should().BeFalse();
        }

        [Test]
        public void IsVideoImageFormula()
        {
            var op = new DeltaInsertOp(new InsertDataVideo(""));
            op.IsVideo().Should().BeTrue();
            op.IsFormula().Should().BeFalse();
            op.IsImage().Should().BeFalse();

            op = new DeltaInsertOp(new InsertDataImage("d"));
            op.IsImage().Should().BeTrue();
            op.IsFormula().Should().BeFalse();

            op = new DeltaInsertOp(new InsertDataFormula("d"));
            op.IsVideo().Should().BeFalse();
            op.IsFormula().Should().BeTrue();
        }

        [Test]
        public void IsLink()
        {
            var op = new DeltaInsertOp(new InsertDataVideo(""),
                new OpAttributes() { Link = "http://" });
            op.IsLink().Should().BeFalse();

            op = new DeltaInsertOp("http", new OpAttributes() { Link = "http://" });
            op.IsLink().Should().BeTrue();
        }

        [Test]
        public void IsSameHeaderAsDifferentN()
        {
            var op1 = new DeltaInsertOp("",
                new OpAttributes() { Header = 1 });
            var op2 = new DeltaInsertOp("",
                new OpAttributes() { Header = 2 });
            op1.IsSameHeaderAs(op2).Should().BeFalse();
        }

        [Test]
        public void IsSameHeaderAsSameN()
        {
            var op1 = new DeltaInsertOp("",
                new OpAttributes() { Header = 1 });
            var op2 = new DeltaInsertOp("",
                new OpAttributes() { Header = 1 });
            op1.IsSameHeaderAs(op2).Should().BeTrue();
        }

        [Test]
        public void IsSameHeaderAsNotHeaders()
        {
            var op1 = new DeltaInsertOp("", new OpAttributes());
            var op2 = new DeltaInsertOp("", new OpAttributes());
            op1.IsSameHeaderAs(op2).Should().BeFalse();
        }
    }
}
