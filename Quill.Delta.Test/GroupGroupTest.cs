using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class GroupGroupTest
    {
        class NumGroup : Group
        {
            public int Value { get; set; }
        }

        class StringGroup : Group
        {
            public string Value { get; set; }
        }

        [Test]
        public void GroupConsecutiveElementsWhileWorks()
        {
            var groups = new Group[] {
                new NumGroup() { Value = 1 },
                new StringGroup() { Value = "ha" },
                new NumGroup() { Value = 3 },
                new StringGroup() { Value = "ha" },
                new StringGroup() { Value = "ha" }
            };
            var result = GroupGroup.GroupConsecutiveElementsWhile(groups,
                (a, b) => a.GetType() == b.GetType());
            result.Should().BeEquivalentTo(new Group[] {
                new NumGroup() { Value = 1 },
                new StringGroup() { Value = "ha" },
                new NumGroup() { Value = 3 },
                new GroupGroup() {
                    Groups = new Group[] {
                        new StringGroup() { Value = "ha" },
                        new StringGroup() { Value = "ha" }
                    }
                }
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void GroupConsecutiveElementsContiguous()
        {
            var groups = new Group[] {
                new NumGroup() { Value = 1 },
                new NumGroup() { Value = 2 },
                new NumGroup() { Value = 3 },
                new NumGroup() { Value = 10 },
                new NumGroup() { Value = 11 },
                new NumGroup() { Value = 12 },
            };
            var result = GroupGroup.GroupConsecutiveElementsWhile(groups,
                (a, b) => ((NumGroup)a).Value - 1 == ((NumGroup)b).Value);
            result.Should().BeEquivalentTo(new Group[] {
                new GroupGroup() {
                    Groups = new Group[] {
                        new NumGroup { Value = 1 },
                        new NumGroup { Value = 2 },
                        new NumGroup { Value = 3 }
                    }
                },
                new GroupGroup() {
                    Groups = new Group[] {
                        new NumGroup() { Value = 10 },
                        new NumGroup() { Value = 11 },
                        new NumGroup() { Value = 12 }
                    }
                }
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }
    }
}
