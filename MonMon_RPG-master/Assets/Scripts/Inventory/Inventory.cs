using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// holds the inventory
public enum ItemCategory { Items, MonBalls, Tms }

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> ballSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    // 2D list one is first is the category and the second is the item in that category
    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    // get a 2D list of all slots on awake
    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, ballSlots, tmSlots };
    }

    // set item categories, can be expanded
    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "Items", "Mon Ball", "TMs & HMs"
    };

    // gets the slots by what category you are in
    public List<ItemSlot> GetSlotsByCat(int catIndex)
    {
        return allSlots[catIndex];
    }

    // gets the item you are currently on
    public ItemBase GetItem(int itemIndex, int catIndex)
    {
        var currentSlots = GetSlotsByCat(catIndex);
        return currentSlots[itemIndex].Item;
    }

    // if you choose to use an item this code runs it
    public ItemBase UseItem(int itemIndex, Pokemon selectedMon, int selectedCat)
    {
        // gets item first
        var item = GetItem(itemIndex, selectedCat);

        // tries to use it on the selected monmon
        // more on this in the Use
        bool itemUsed = item.Use(selectedMon);
        // if the item is not reusable remove an item from the count
        if (itemUsed)
        {
            if (!item.IsReusable)
                RemoveItem(item);

            return item;
        }

        return null;
    }

    // if you gain an item find if you have one, if you do add one to the preexisting count
    // if not add it to the end of the right category list
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

    // if you use an item or complete a quest where you give one an item is removed from the list
    public void RemoveItem(ItemBase item)
    {
        var cat = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCat(cat);

        var itemSlot = currentSlots.First(slots => slots.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
            currentSlots.Remove(itemSlot);

        OnUpdated?.Invoke();
    }

    // fetches the inventory from the player controller
    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    // chekcs if you have a specific item
    public bool HasItem(ItemBase item)
    {
        var cat = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCat(cat);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    // gets the category from the item
    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is Recovery)
            return ItemCategory.Items;
        else if (item is BallItem)
            return ItemCategory.MonBalls;
        else
            return ItemCategory.Tms;
    }

    // needs to be able to be saved so we capture all the slots in each category
    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            monball = ballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = tmSlots.Select(i => i.GetSaveData()).ToList()
        };

        return saveData;
    }

    // be able to restore the inventory items with the captured data
    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        ballSlots = saveData.monball.Select(i => new ItemSlot(i)).ToList();
        tmSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>>() { slots, ballSlots, tmSlots };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetItemViaName(saveData.name);
        count = saveData.count;
    }

    // gets the items by their names and count and saves them as saveData
    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.Name,
            count = count
        };

        return saveData;
    }

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

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> monball;
    public List<ItemSaveData> tms;
}