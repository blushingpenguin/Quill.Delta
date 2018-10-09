using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class GrouperTest
    {
        [Test]
        public void ReturnsOpsGroupedByGroupType()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp(new InsertDataVideo("http://")),
                new DeltaInsertOp("hello"),
                new DeltaInsertOp("\n"),
                new DeltaInsertOp("how are you?"),
                new DeltaInsertOp("\n"),
                new DeltaInsertOp("Time is money"),
                new DeltaInsertOp("\n",
                    new OpAttributes { Blockquote = true })
            };
            var act = Grouper.PairOpsWithTheirBlock(ops);
            var exp = new Group[] {
                new VideoItem(ops[0]),
                new InlineGroup(new DeltaInsertOp[] {
                    ops[1], ops[2], ops[3], ops[4] }),
                new BlockGroup(ops[6], new DeltaInsertOp[] { ops[5] })
            };
            act.Should().BeEquivalentTo(exp,
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void CombinesBlocksWithSameTypeAndStyle()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("this is code"),
                new DeltaInsertOp("\n",
                    new OpAttributes { CodeBlock = true }),
                new DeltaInsertOp("this is code TOO!"),
                new DeltaInsertOp("\n",
                    new OpAttributes { CodeBlock = true }),
                new DeltaInsertOp("\n",
                    new OpAttributes { Blockquote = true }),
                new DeltaInsertOp("\n",
                    new OpAttributes { Blockquote = true }),
                new DeltaInsertOp("\n"),
                new DeltaInsertOp("\n",
                    new OpAttributes { Header = 1 }),
                new DeltaInsertOp("\n",
                    new OpAttributes { Header = 1 }),
            };
            var pairs = Grouper.PairOpsWithTheirBlock(ops);
            var groups = Grouper.GroupConsecutiveSameStyleBlocks(pairs,
                header: true,
                codeBlocks: true,
                blockquotes: true
            );
            groups.Should().BeEquivalentTo(new Group[] {
                new GroupGroup() {
                    Groups = new Group[] {
                        new BlockGroup(ops[1], new DeltaInsertOp[] { ops[0] }),
                        new BlockGroup(ops[1], new DeltaInsertOp[] { ops[2] })
                    },
                },
                new GroupGroup() {
                    Groups = new Group[] {
                        new BlockGroup(ops[4], new DeltaInsertOp[] { }),
                        new BlockGroup(ops[4], new DeltaInsertOp[] { })
                    }
                },
                new InlineGroup(new DeltaInsertOp[] { ops[6] }),
                new GroupGroup() {
                    Groups = new Group[] {
                        new BlockGroup(ops[7], new DeltaInsertOp[] { }),
                        new BlockGroup(ops[8], new DeltaInsertOp[] { })
                    }
                }
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void OpsOfCombinedGroupsMoveTo1stGroup()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("this is code"),
                new DeltaInsertOp("\n",
                    new OpAttributes { CodeBlock = true }),
                new DeltaInsertOp("this is code TOO!"),
                new DeltaInsertOp("\n",
                    new OpAttributes { CodeBlock = true }),
                new DeltaInsertOp("\n",
                    new OpAttributes { Blockquote = true }),
                new DeltaInsertOp("\n",
                    new OpAttributes { Blockquote = true }),
                new DeltaInsertOp("\n"),
                new DeltaInsertOp("\n",
                    new OpAttributes { Header = 1 }),
            };
            var pairs = Grouper.PairOpsWithTheirBlock(ops);
            var groups = Grouper.GroupConsecutiveSameStyleBlocks(pairs);
            //console.log(groups);
            var act = Grouper.ReduceConsecutiveSameStyleBlocksToOne(groups);
            //console.log(act);
            //console.log(JSON.stringify(act));
            act.Should().BeEquivalentTo(new Group[] {
                new BlockGroup(ops[1],
                    new DeltaInsertOp[] { ops[0], ops[6], ops[2] }),
                new BlockGroup(ops[4],
                    new DeltaInsertOp[] { ops[6], ops[6] }),
                new InlineGroup(new DeltaInsertOp[] { ops[6] }),
                new BlockGroup(ops[7], new DeltaInsertOp[] { ops[6] })
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }
    }
}
