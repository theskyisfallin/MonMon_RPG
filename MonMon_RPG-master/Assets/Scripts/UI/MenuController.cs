using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelect;
    public event Action onBack;

    List<Text> menuItems;

    int selected = 0;

    // shows the list of our menu
    public void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    // when opening menu set it as active and update the item select
    public void OpenMenu()
    {
        menu.SetActive(true);
        UpdateItemSelect();
    }

    // on close set the menu to inactive
    public void CloseMenu()
    {
        menu.SetActive(false);
    }

    // code what the user has selected and Z to chose and X to back out
    public void HandleUpdate()
    {
        int prevSelected = selected;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            selected++;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selected--;

        selected = Mathf.Clamp(selected, 0, menuItems.Count - 1);

        if (prevSelected != selected)
            UpdateItemSelect();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelect?.Invoke(selected);
            CloseMenu();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    // show what the user has selected with the highlighted color
    void UpdateItemSelect()
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selected)
                menuItems[i].color = GlobalSettings.i.Hightlight;
            else
                menuItems[i].color = Color.black;
        }
    }
}
