using MemoryLeakExampleDatabase;

namespace MemoryLeakExample.ViewModels
{
    public partial class GroupedStoreSelectionItemList : List<StoreSelection>
    {
        public string GroupName { get; set; }

        public GroupedStoreSelectionItemList(string groupName, List<StoreSelection> storeItem) : base(storeItem)
        {
            GroupName = groupName;
        }
    }
}
