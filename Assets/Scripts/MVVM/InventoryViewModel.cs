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

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemList));
    }

    public void AddItemSlotViewModel(ItemSlotViewModel slotVm)
    {
        _ItemList.Add(slotVm.ItemUniqueId, slotVm);
        OnPropertyChanged("ItemListAdded");

    }


    public void RemoveItemSlotViewModel(long uniqueId)
    {
        if(_ItemList.ContainsKey(uniqueId))
        {
            _ItemList.Remove(uniqueId);
        }

        OnPropertyChanged("ItemListRemoved");
    }




}
