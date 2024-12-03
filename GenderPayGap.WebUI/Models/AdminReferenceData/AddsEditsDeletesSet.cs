namespace GenderPayGap.WebUI.Models.AdminReferenceData
{
    public class OldAndNew<T>
    {
        public T Old { get; set; }
        public T New { get; set; }
    }

    public class AddsEditsDeletesSet<TItem>
    {

        public List<TItem> OldItems { get; }
        public List<TItem> NewItems { get; }

        public List<TItem> ItemsToAdd { get; } = new List<TItem>();
        public List<OldAndNew<TItem>> ItemsToChange { get; } = new List<OldAndNew<TItem>>();
        public List<OldAndNew<TItem>> ItemsWithNoChanges { get; } = new List<OldAndNew<TItem>>();
        public List<TItem> ItemsToDelete { get; } = new List<TItem>();

        public List<TItem> ItemsThatCannotBeDeleted { get; }
        public bool AnyItemsThatCannotBeDeleted => ItemsThatCannotBeDeleted != null && ItemsThatCannotBeDeleted.Count > 0;

        public bool AnyChanges => ItemsToAdd.Count > 0 || ItemsToChange.Count > 0 || ItemsToDelete.Count > 0;


        public AddsEditsDeletesSet(
            List<TItem> oldItems,
            List<TItem> newItems,
            Func<TItem, object> keySelector,
            Func<TItem, TItem, bool> itemsAreEqualComparer,
            Func<TItem, bool> itemCannotBeDeletedSelector = null)
        {
            OldItems = oldItems;
            NewItems = newItems;

            foreach (TItem newItem in newItems)
            {
                object newItemKey = keySelector(newItem);
                TItem matchingOldItem = oldItems.FirstOrDefault(oldItem => keySelector(oldItem).Equals(newItemKey));

                if (matchingOldItem == null)
                {
                    // No matching old item - this is a new item
                    ItemsToAdd.Add(newItem);
                }
                else
                {
                    // Found a matching old item
                    if (itemsAreEqualComparer(newItem, matchingOldItem))
                    {
                        // Items match - the item has not been changed
                        ItemsWithNoChanges.Add(new OldAndNew<TItem>{Old = matchingOldItem, New = newItem});
                    }
                    else
                    {
                        // Items don't match - there are changes
                        ItemsToChange.Add(new OldAndNew<TItem> { Old = matchingOldItem, New = newItem });
                    }
                }
            }

            foreach (TItem oldItem in oldItems)
            {
                object oldItemKey = keySelector(oldItem);
                TItem matchingNewItem = newItems.FirstOrDefault(newItem => keySelector(newItem).Equals(oldItemKey));

                if (matchingNewItem == null)
                {
                    // No matching new item - this item has been deleted
                    ItemsToDelete.Add(oldItem);
                }
            }

            if (itemCannotBeDeletedSelector != null)
            {
                ItemsThatCannotBeDeleted = ItemsToDelete.Where(itemCannotBeDeletedSelector).ToList();
            }
        }

    }
}
