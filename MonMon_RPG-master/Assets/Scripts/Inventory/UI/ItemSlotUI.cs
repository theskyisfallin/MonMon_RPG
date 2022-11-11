using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text name;
    [SerializeField] Text count;

    RectTransform rectTrasform;

    private void Awake()
    {
        
    }

    public Text Name => name;
    public Text Count => count;

    public float Height => rectTrasform.rect.height;

    public void SetData(ItemSlot item)
    {
        rectTrasform = GetComponent<RectTransform>();
        name.text = item.Item.Name;
        count.text = $"x {item.Count}";
    }
}
