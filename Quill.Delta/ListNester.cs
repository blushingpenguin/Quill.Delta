using System.Collections.Generic;
using System.Linq;

namespace Quill.Delta
{
    public static class ListNester
    {
        public static IList<Group> Nest(IList<Group> groups)
        {
            var listBlocked = ConvertListBlocksToListGroups(groups);
            var groupedByListGroups = GroupConsecutiveListGroups(listBlocked);

            // convert grouped ones into listgroup
            var nested = groupedByListGroups.SelectMany(g => g is GroupGroup gg ?
                NestListSection(gg.Groups.Cast<ListGroup>().ToList()) :
                Enumerable.Repeat(g, 1)).ToList();

            var groupRootLists = GroupGroup.GroupConsecutiveElementsWhile(nested,
                (curr, prev) =>
            {
                if (curr is ListGroup currLg && prev is ListGroup prevLg)
                {
                    return currLg.Items[0].Item.Op.IsSameListAs(
                        prevLg.Items[0].Item.Op);
                }
                return false;
            });

            return groupRootLists.Select(v =>
            {
                if (v is GroupGroup gg)
                {
                    return new ListGroup(
                        gg.Groups.SelectMany(g => ((ListGroup)g).Items).ToList());
                }
                return v;
            }).ToList();
        }

        static IList<Group> ConvertListBlocksToListGroups(IList<Group> items)
        {
            var grouped = GroupGroup.GroupConsecutiveElementsWhile(items,
                (g, gPrev) =>
                {
                    return g is BlockGroup bg && gPrev is BlockGroup bgPrev &&
                        bg.Op.IsList() && bgPrev.Op.IsList() &&
                        bg.Op.IsSameListAs(bgPrev.Op) &&
                        bg.Op.HasSameIndentationAs(bgPrev.Op);
                });

            return grouped.Select(g =>
            {
                if (g is GroupGroup gg)
                {
                    return new ListGroup(gg.Groups.Select(
                        bg2 => new ListItem((BlockGroup)bg2)).ToList());
                }
                if (g is BlockGroup bg && bg.Op.IsList())
                {
                    var lgItems = new List<ListItem>();
                    lgItems.Add(new ListItem(bg));
                    return new ListGroup(lgItems);
                }
                return g;
            }).ToList();
        }

        static IList<Group> GroupConsecutiveListGroups(IList<Group> items)
        {
            return GroupGroup.GroupConsecutiveElementsWhile(items,
                (curr, prev) => curr is ListGroup && prev is ListGroup);
        }

        static IList<ListGroup> NestListSection(IList<ListGroup> sectionItems)
        {
            var indentGroups = GroupByIndent(sectionItems);
            foreach (var kv in indentGroups.OrderByDescending(x => x.Key))
            {
                foreach (var lg in kv.Value)
                {
                    var idx = sectionItems.IndexOf(lg);
                    if (PlaceUnderParent(lg, sectionItems, idx))
                    {
                        sectionItems.RemoveAt(idx);
                    }
                }
            }
            return sectionItems;
        }

        static IDictionary<int, IList<ListGroup>> GroupByIndent(IEnumerable<ListGroup> items)
        {
            return items.Aggregate(new Dictionary<int, IList<ListGroup>>(),
                (dic, val) =>
                {
                    int indent = val.Items[0].Item.Op.Attributes.Indent ?? 0;
                    if (indent > 0)
                    {
                        IList<ListGroup> list;
                        if (!dic.TryGetValue(indent, out list))
                        {
                            list = new List<ListGroup>();
                            dic.Add(indent, list);
                        }
                        list.Add(val);
                    }
                    return dic;
                });
        }

        static bool PlaceUnderParent(ListGroup target, IList<ListGroup> items, int length)
        {
            for (var i = length; i >= 0; i--)
            {
                var elm = items[i];
                if (target.Items[0].Item.Op.HasHigherIndentThan(elm.Items[0].Item.Op))
                {
                    var parent = elm.Items[elm.Items.Count - 1];
                    if (parent.InnerList != null)
                    {
                        foreach (var item in target.Items)
                        {
                            parent.InnerList.Items.Add(item);
                        }
                    }
                    else
                    {
                        parent.InnerList = target;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
