using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// handles the items in the players inventory
public class ItemDB
{
    static Dictionary<string, ItemBase> item;

    // inits the items in inventomry
    public static void Init()
    {
        item = new Dictionary<string, ItemBase>();

        var itemArray = Resources.LoadAll<ItemBase>("");

        // makes sure that you don't have duplicate items in the scriptable objects
        foreach (var item in itemArray)
        {
            if (ItemDB.item.ContainsKey(item.Name))
            {
                Debug.LogError($"There are two items with the name {item.Name}");
                continue;
            }

            ItemDB.item[item.Name] = item;
        }
    }

    // gets the items by name
    // all other "DB"s use a generic class, this one had an error so it doesn't
    // error has been fixed but was caused from not initing the items in GameControl
    // TODO: change to generic class
    public static ItemBase GetItemViaName(string name)
    {
        if (!item.ContainsKey(name))
        {
            Debug.LogError($"Object \"{name}\" not found");
            return null;
        }

        return item[name];
    }
}
