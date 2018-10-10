using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Quill.Delta
{
    public abstract class Converter
    {
        protected JArray _rawOps;
        protected ConverterOptions _options;

        public Converter(JArray ops, ConverterOptions options)
        {
            _rawOps = ops;
            _options = options;
        }

        internal string GetListTag(DeltaInsertOp op)
        {
            return op.IsOrderedList() ? _options.OrderedListTag :
                op.IsBulletList() ? _options.BulletListTag :
                op.IsCheckedList() ? _options.BulletListTag :
                op.IsUncheckedList() ? _options.BulletListTag :
                "";
        }

        protected IList<Group> GetGroupedOps()
        {
            var deltaOps = InsertOpsConverter.Convert(_rawOps);
            var pairedOps = Grouper.PairOpsWithTheirBlock(deltaOps);
            var groupedSameStyleBlocks = Grouper.GroupConsecutiveSameStyleBlocks(pairedOps,
                header: _options.MultiLineHeader,
                codeBlocks: _options.MultiLineCodeblock,
                blockquotes: _options.MultiLineBlockquote
            );
            var groupedOps = Grouper.ReduceConsecutiveSameStyleBlocksToOne(groupedSameStyleBlocks);
            // var listNester = new ListNester();
            return ListNester.Nest(groupedOps);
        }
    }
}
