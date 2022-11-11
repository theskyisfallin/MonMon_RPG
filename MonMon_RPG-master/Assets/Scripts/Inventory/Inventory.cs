using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory { Items, MonBalls, Tms }

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> ballSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, ballSlots, tmSlots };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "Items", "Mon Ball", "TMs & HMs"
    };

    public List<ItemSlot> GetSlotsByCat(int catIndex)
    {
        return allSlots[catIndex];
    }

    public ItemBase GetItem(int itemIndex, int catIndex)
    {
        var currentSlots = GetSlotsByCat(catIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedMon, int selectedCat)
    {
        var item = GetItem(itemIndex, selectedCat);

        bool itemUsed = item.Use(selectedMon);
        if (itemUsed)
        {
            if (!item.IsReusable)
                RemoveItem(item, selectedCat);

            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count=1)
    {
        var cat = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCat(cat);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item, int cat)
    {
        var currentSlots = GetSlotsByCat(cat);

        var itemSlot = currentSlots.First(slots => slots.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
            currentSlots.Remove(itemSlot);

        OnUpdated?.Invoke();
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is Recovery)
            return ItemCategory.Items;
        else if (item is BallItem)
            return ItemCategory.MonBalls;
        else
            return ItemCategory.Tms;
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;


    public ItemBase Item
    {
        get => item;
        set => item = value;
    }

    public int Count
    {
        get => count;
        set => count = value;
    }
}
