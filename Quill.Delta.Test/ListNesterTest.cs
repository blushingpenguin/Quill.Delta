using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace Quill.Delta.Test
{
    public class ListNesterTest
    {
        [Test]
        public void ShouldNotNestDifferentListTypes()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("ordered list 1 item 1"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered }),
                new DeltaInsertOp("bullet list 1 item 1"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Bullet }),
                new DeltaInsertOp("bullet list 1 item 2"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Bullet }),
                new DeltaInsertOp("haha"),
                new DeltaInsertOp("\n"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Bullet }),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Checked }),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Unchecked }),
            };

            var groups = Grouper.PairOpsWithTheirBlock(ops);
            var act = ListNester.Nest(groups);
            //console.log(JSON.stringify(act, null, 3));
            act.Should().BeEquivalentTo(new Group[] {
                new ListGroup(new ListItem[] {
                    new ListItem((BlockGroup)groups[0])
                }),
                new ListGroup(new ListItem[] {
                    new ListItem((BlockGroup)groups[1]),
                    new ListItem((BlockGroup)groups[2])
                }),
                new InlineGroup(new DeltaInsertOp[] { ops[6], ops[7] }),
                new ListGroup(new ListItem[] {
                    new ListItem(new BlockGroup(ops[8], new DeltaInsertOp[] { } ))
                }),
                new ListGroup(new ListItem[] {
                    new ListItem(new BlockGroup(ops[9], new DeltaInsertOp[] { })),
                    new ListItem(new BlockGroup(ops[10], new DeltaInsertOp[] { }))
                })
            }, opts => opts.RespectingRuntimeTypes()
                           .WithStrictOrdering()
                           .AllowingInfiniteRecursion());
        }

        [Test]
        public void NestsIfListsAreSameAndLaterOnesHaveHigherIndent()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("item 1"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered }),
                new DeltaInsertOp("item 1a"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered, Indent = 1 }),
                new DeltaInsertOp("item 1a-i"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered, Indent = 3 }),
                new DeltaInsertOp("item 1b"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered, Indent = 1 }),
                new DeltaInsertOp("item 2"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered}),
                new DeltaInsertOp("haha"),
                new DeltaInsertOp("\n"),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Ordered, Indent = 5 }),
                new DeltaInsertOp("\n",
                    new OpAttributes { List = ListType.Bullet, Indent = 4 }),
            };
            var pairs = Grouper.PairOpsWithTheirBlock(ops);
            var act = ListNester.Nest(pairs);
            //console.log(JSON.stringify( act, null, 4));


            var l1b = new ListItem((BlockGroup)pairs[3]);
            var lai = new ListGroup(new ListItem[] {
                new ListItem((BlockGroup)pairs[2])
            });
            var l1a = new ListGroup(new ListItem[] {
                new ListItem((BlockGroup)pairs[1], lai)
            });
            var li1 = new ListGroup(new ListItem[] {
                new ListItem((BlockGroup)pairs[0])
            });
            li1.Items[0].InnerList = new ListGroup(
                l1a.Items.Concat(Enumerable.Repeat(l1b, 1)).ToArray());
            var li2 = new ListGroup(new ListItem[] {
                new ListItem((BlockGroup)pairs[4])
            });
            //console.log(JSON.stringify(act, null, 3));
            act.Should().BeEquivalentTo(new Group[] {
                new ListGroup(li1.Items.Concat(li2.Items).ToArray()),
                new InlineGroup(new DeltaInsertOp[] { ops[10], ops[11] }),
                new ListGroup(new ListItem[] {
                    new ListItem(new BlockGroup(ops[12], new DeltaInsertOp [] { }))
                }),
                new ListGroup(new ListItem[] {
                    new ListItem(new BlockGroup(ops[13], new DeltaInsertOp [] { }))
                })
            }, opts => opts.RespectingRuntimeTypes()
                           .WithStrictOrdering()
                           .AllowingInfiniteRecursion());
        }
    }
}
