using System;

namespace Quill.Delta
{
    public class DeltaInsertOp
    {
        public InsertData Insert { get; private set; }
        public OpAttributes Attributes { get; private set; }

        public DeltaInsertOp(InsertData insert, OpAttributes attributes = null)
        {
            // if (typeof insertVal === 'string') {
            // insertVal = new InsertDataQuill(DataType.Text, insertVal + '');
            // }
            Insert = insert;
            Attributes = attributes ?? new OpAttributes();
        }

        public DeltaInsertOp(string insert, OpAttributes attributes = null) :
            this(new InsertDataText(insert ?? ""), attributes)
        {
        }

        public static DeltaInsertOp CreateNewLineOp()
        {
            return new DeltaInsertOp("\n");
        }

        public bool IsContainerBlock()
        {
            return
                (Attributes.Blockquote ?? false) ||
                Attributes.List.HasValue ||
                (Attributes.CodeBlock ?? false) ||
                (Attributes.Header ?? 0) > 0 ||
                Attributes.Align.HasValue ||
                Attributes.Direction.HasValue ||
                (Attributes.Indent ?? 0) > 0;
        }

        public bool IsBlockquote()
        {
            return Attributes.Blockquote ?? false;
        }

        public bool IsHeader()
        {
            return Attributes.Header.HasValue;
        }

        public bool IsSameHeaderAs(DeltaInsertOp op)
        {
            return Attributes.Header == op.Attributes.Header && IsHeader();
        }

        // adi: alignment direction indentation 
        public bool HasSameAdiAs(DeltaInsertOp op)
        {
            return Attributes.Align == op.Attributes.Align &&
                Attributes.Direction == op.Attributes.Direction &&
                Attributes.Indent == op.Attributes.Indent;
        }

        public bool HasSameIndentationAs(DeltaInsertOp op)
        {
            return Attributes.Indent == op.Attributes.Indent;
        }

        public bool HasHigherIndentThan(DeltaInsertOp op) {
            return (Attributes.Indent ?? 0) > (op.Attributes.Indent ?? 0);
        }

        public bool IsInline()
        {
            return !(IsContainerBlock() || IsVideo() || IsCustomBlock());
        }

        public bool IsCodeBlock()
        {
            return Attributes.CodeBlock ?? false;
        }

        public bool IsJustNewline()
        {
            if (Insert is InsertDataText idt)
            {
                return idt.Value == "\n";
            }
            return false;
        }

        public bool IsList()
        {
            return (
                IsOrderedList() ||
                IsBulletList() ||
                IsCheckedList() ||
                IsUncheckedList()
            );
        }

        public bool IsOrderedList()
        {
            return Attributes.List == ListType.Ordered;
        }

        public bool IsBulletList()
        {
            return Attributes.List == ListType.Bullet;
        }

        public bool IsCheckedList()
        {
            return Attributes.List == ListType.Checked;
        }

        public bool IsUncheckedList()
        {
            return Attributes.List == ListType.Unchecked;
        }

        public bool IsACheckList()
        {
            return Attributes.List == ListType.Unchecked ||
                   Attributes.List == ListType.Checked;
        }

        public bool IsSameListAs(DeltaInsertOp op)
        {
            return op.Attributes.List.HasValue &&
                (Attributes.List == op.Attributes.List ||
                    op.IsACheckList() && IsACheckList());
        }

        public bool IsText()
        {
            return Insert.Type == DataType.Text;
        }

        public bool IsImage()
        {
            return Insert.Type == DataType.Image;
        }

        public bool IsFormula()
        {
            return Insert.Type == DataType.Formula;
        }

        public bool IsVideo()
        {
            return Insert.Type == DataType.Video;
        }

        public bool IsLink()
        {
            return IsText() && !String.IsNullOrEmpty(Attributes.Link);
        }

        public bool IsCustom()
        {
            return Insert is InsertDataCustom;
        }

        public bool IsCustomBlock()
        {
            return IsCustom() && (Attributes.RenderAsBlock ?? false);
        }

        public bool IsMentions()
        {
            return IsText() && Attributes.Mentions;
        }
    }
}
