using System;
using System.Collections.Generic;
using System.Linq;

namespace Quill.Delta
{
    public struct ArraySlice<T>
    {
        public int SliceStartsAt { get; set; }
        public IList<T> Elements { get; set; }
    }

    public class GroupGroup : Group
    {
        public IList<Group> Groups { get; internal set; } =
            new List<Group>();

        /// 
        /// Returns a new array by putting consecutive elements satisfying predicate into a new 
        /// array and returning others as they are. 
        /// Ex: [1, "ha", 3, "ha", "ha"] => [1, "ha", 3, ["ha", "ha"]] 
        ///      where predicate: (v, vprev) => typeof v === typeof vPrev
        /// 
        public static IList<Group> GroupConsecutiveElementsWhile(IList<Group> arr,
            Func<Group, Group, bool> predicate)
        {
            var groups = new List<GroupGroup>();

            for (int i = 0; i < arr.Count; ++i)
            {
                var currElm = arr[i];
                if (i > 0 && predicate(currElm, arr[i - 1]))
                {
                    groups.Last().Groups.Add(currElm);
                }
                else
                {
                    var group = new GroupGroup();
                    group.Groups.Add(currElm);
                    groups.Add(group);
                }
            }
            return groups.Select(g => g.Groups.Count == 1 ? g.Groups[0] : g).ToList();
        }
    }


    public static class ArrayHelpers
    {

        /// <summary>
        /// Returns consecutive list of elements satisfying the predicate starting from startIndex 
        /// and traversing the array in reverse order. 
        /// </summary>
        public static ArraySlice<T> SliceFromReverseWhile<T>(IList<T> arr,
            int startIndex, Func<T, bool> predicate)
        {
            var result = new ArraySlice<T>();

            int endIndex = startIndex;
            for (; endIndex >= 0; --endIndex)
            {
                if (!predicate(arr[endIndex]))
                {
                    break;
                }
            }
            if (endIndex < startIndex)
            {
                result.SliceStartsAt = endIndex + 1;
                result.Elements = new List<T>(
                    arr.Skip(endIndex + 1).Take(startIndex - endIndex));
            }
            else
            {
                result.SliceStartsAt = -1;
                result.Elements = new List<T>();
            }
            return result;
        }
    }

    public abstract class Group
    {
    }

    public class InlineGroup : Group
    {
        public IList<DeltaInsertOp> Ops { get; private set; }

        public InlineGroup(IList<DeltaInsertOp> ops)
        {
            Ops = ops;
        }
    }

    public class SingleItem : Group
    {
        public DeltaInsertOp Op { get; private set; }

        public SingleItem(DeltaInsertOp op)
        {
            Op = op;
        }
    }

    public class VideoItem : SingleItem
    {
        public VideoItem(DeltaInsertOp op) :
            base(op)
        {
        }
    }

    public class BlotBlock : SingleItem
    {
        public BlotBlock(DeltaInsertOp op) :
            base(op)
        {
        }
    }

    public class BlockGroup : Group
    {
        public DeltaInsertOp Op { get; private set; }
        public IList<DeltaInsertOp> Ops { get; set; }

        public BlockGroup(DeltaInsertOp op, IList<DeltaInsertOp> ops)
        {
            Op = op;
            Ops = ops;
        }
    }

    public class ListGroup : Group
    {
        public IList<ListItem> Items { get; private set; }

        public ListGroup(IList<ListItem> items)
        {
            Items = items;
        }
    }

    public class ListItem : Group
    {
        public BlockGroup Item { get; private set; }
        public ListGroup InnerList { get; set; }

        public ListItem(BlockGroup item, ListGroup innerList = null)
        {
            Item = item;
            InnerList = innerList;
        }
    }

    public static class Grouper
    {
        static bool CanBeInBlock(DeltaInsertOp op) =>
            !(op.IsJustNewline() || op.IsCustomBlock() || op.IsVideo() || op.IsContainerBlock());

        static bool IsInlineData(DeltaInsertOp op) =>
            op.IsInline();

        public static IList<Group> PairOpsWithTheirBlock(IList<DeltaInsertOp> ops)
        {
            var result = new List<Group>();

            for (int i = ops.Count - 1; i >= 0; --i)
            {
                var op = ops[i];

                if (op.IsVideo())
                {
                    result.Add(new VideoItem(op));
                }
                else if (op.IsCustomBlock())
                {
                    result.Add(new BlotBlock(op));
                }
                else if (op.IsContainerBlock())
                {
                    var opsSlice = ArrayHelpers.SliceFromReverseWhile(ops, i - 1, CanBeInBlock);
                    result.Add(new BlockGroup(op, opsSlice.Elements));
                    i = opsSlice.SliceStartsAt > -1 ? opsSlice.SliceStartsAt : i;
                }
                else
                {
                    var opsSlice = ArrayHelpers.SliceFromReverseWhile(ops, i - 1, IsInlineData);
                    opsSlice.Elements.Add(op);
                    result.Add(new InlineGroup(opsSlice.Elements));
                    i = opsSlice.SliceStartsAt > -1 ? opsSlice.SliceStartsAt : i;
                }
            }
            result.Reverse();
            return result;
        }


        public static IList<Group> GroupConsecutiveSameStyleBlocks(IList<Group> groups,
            bool header = true, bool codeBlocks = true, bool blockquotes = true)
        {
            return GroupGroup.GroupConsecutiveElementsWhile(groups,
                (Group g, Group gPrev) =>
                {
                    if (g is BlockGroup bg && gPrev is BlockGroup bgPrev)
                    {
                        return (codeBlocks && AreBothCodeblocks(bg, bgPrev)) ||
                            (blockquotes && AreBothBlockquotesWithSameAdi(bg, bgPrev)) ||
                            (header && AreBothSameHeadersWithSameAdi(bg, bgPrev));
                    }
                    return false;
                });
        }

        // Moves all ops of same style consecutive blocks to the ops of first block
        // and discards the rest. 
        public static IList<Group> ReduceConsecutiveSameStyleBlocksToOne(
            IList<Group> groups)
        {
            var newLineOp = DeltaInsertOp.CreateNewLineOp();
            var newLineOpSeq = Enumerable.Repeat(newLineOp, 1);
            return groups.Select((Group group) =>
            {
                if (group is GroupGroup gg)
                {
                    var elm = (BlockGroup)gg.Groups[0];
                    elm.Ops = gg.Groups.SelectMany((g, i) =>
                    {
                        var bg = (BlockGroup)g;
                        if (!bg.Ops.Any())
                        {
                            return newLineOpSeq;
                        }
                        return i < gg.Groups.Count - 1 ?
                            bg.Ops.Concat(newLineOpSeq) : bg.Ops;
                    }).ToList();
                    return elm;
                }
                if (group is BlockGroup bgt && !bgt.Ops.Any())
                {
                    bgt.Ops.Add(newLineOp);
                }
                return group;
            }).ToList();
        }

        public static bool AreBothCodeblocks(BlockGroup g1, BlockGroup gOther)
        {
            return g1.Op.IsCodeBlock() && gOther.Op.IsCodeBlock();
        }

        public static bool AreBothSameHeadersWithSameAdi(BlockGroup g1, BlockGroup gOther)
        {
            return g1.Op.IsSameHeaderAs(gOther.Op) && g1.Op.HasSameAdiAs(gOther.Op);
        }

        public static bool AreBothBlockquotesWithSameAdi(BlockGroup g, BlockGroup gOther)
        {
            return g.Op.IsBlockquote() && gOther.Op.IsBlockquote()
               && g.Op.HasSameAdiAs(gOther.Op);
        }
    }

}