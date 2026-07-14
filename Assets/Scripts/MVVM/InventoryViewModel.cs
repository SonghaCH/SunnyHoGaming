using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class InventoryViewModel :ViewModelBase
{
    private Dictionary<long, ItemSlotViewModel > _ItemList = new Dictionary<long, ItemSlotViewModel>(); 
    public Dictionary<long, ItemSlotViewModel> ItemList
    {
        get => _ItemList;
        set
        {
            if(_ItemList != value)
            {
                _ItemList = value;
                OnPropertyChanged(nameof(ItemList));

            }
        }
    }

    private ItemSlotViewModel _selectedItem;
    public ItemSlotViewModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemList));
        if (_ItemList.Count > 0)
        {
            using (var enumerator = _ItemList.Values.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    SelectedItem = enumerator.Current;
                }
            }
        }
    }

    public void SelectItem(long uniqueId)
    {
        if (_ItemList.TryGetValue(uniqueId, out var targetSlotVm))
        {
            SelectedItem = targetSlotVm;
        }
    }

    public void AddItemSlotViewModel(ItemSlotViewModel slotVm)
    {
        _ItemList.Add(slotVm.ItemUniqueId, slotVm);
        OnPropertyChanged("ItemListAdded");

    }
    public void RefreshItemList()
    {
        OnPropertyChanged(nameof(ItemList));
    }

    public void RemoveItemSlotViewModel(long uniqueId)
    {
        if(_ItemList.ContainsKey(uniqueId))
        {
            _ItemList.Remove(uniqueId);

            if (SelectedItem != null && SelectedItem.ItemUniqueId == uniqueId)
            {
                SelectedItem = null; 
            }
        }

        OnPropertyChanged("ItemListRemoved");
    }




}
