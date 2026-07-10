using UnityEngine;

public class ItemSlotViewModel : ViewModelBase
{
    private long _ItemUniqueId;
    public long ItemUniqueId
    { 
        get => _ItemUniqueId; 
        set
        {
            if (_ItemUniqueId != value) 
            {
                _ItemUniqueId = value;
                OnPropertyChanged(nameof(ItemUniqueId));
            }
        }
    }

    private string _itemDataId;
    public string ItemDataId
    {
        get => _itemDataId;
        set
        {
            if (_itemDataId != value)
            {
                _itemDataId = value;
                OnPropertyChanged(nameof(ItemDataId));
            }
        }
    }

    private int _itemMaxStackCount;
    public int itemMaxStackCount
    {
        get => _itemMaxStackCount;
        set
        {
            if (_itemMaxStackCount != value)
            {
                _itemMaxStackCount = value;
                OnPropertyChanged(nameof(itemMaxStackCount));
            }
        }
    }
}
