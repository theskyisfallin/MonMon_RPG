using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class that all items have contains name, description, icon
public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string desc;
    [SerializeField] Sprite icon;

    public virtual string Name => name;

    public virtual string Desc => desc;

    public Sprite Icon => icon;

    public virtual bool Use(Pokemon mon)
    {
        return false;
    }

    // default values for items
    public virtual bool IsReusable => false;

    public virtual bool CanUseInBattle => true;

    public virtual bool CanUseOutsideBattle => true;
}
