using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// the inventory states it can have
public enum InventoryUIState { ItemSelect, PartySelect, MoveToForget, Busy }

public class InventoryUI : MonoBehaviour
{
    // set by user in unity
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI item;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemImage;
    [SerializeField] Text itemDesc;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen party;
    [SerializeField] MoveSelectUI moveSelect;

    Action<ItemBase> onItemUsed;

    int selected = 0;
    int selectedCat = 0;
    MoveBasic moveToLearn;

    InventoryUIState state;

    const int itemsInView = 8;

    List<ItemSlotUI> slotUIList;

    Inventory inventory;
    RectTransform itemListRect;

    // on awake fetch inventory and get player's item list
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    // on start update the item list and subscribe to UpdateItemList action
    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

    // when UpdateItemList is called get rid of the old list
    // possibly from last run
    // and create the new item list
    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);


        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCat(selectedCat))
        {
            var slotUIObj = Instantiate(item, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelect();
    }

    // handles updates and what you have selected in the inventory
    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

        // if you first go into your inventory/bag
        if (state == InventoryUIState.ItemSelect)
        {
            int prevSelected = selected;
            int prevCat = selectedCat;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                selected++;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                selected--;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                selectedCat++;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                selectedCat--;

            // allows for cycling through three categories by just pressing one button
            // and then loops
            if (selectedCat > Inventory.ItemCategories.Count - 1)
                selectedCat = 0;
            else if (selectedCat < 0)
                selectedCat = Inventory.ItemCategories.Count - 1;

            selected = Mathf.Clamp(selected, 0, inventory.GetSlotsByCat(selectedCat).Count - 1);

            if (prevCat != selectedCat)
            {
                ResetSelect();
                categoryText.text = Inventory.ItemCategories[selectedCat];
                UpdateItemList();
            }

            else if (prevSelected != selected)
            {
                UpdateItemSelect();
            }

            // if you selcted an item or if you back out of the inventory
            if (Input.GetKeyDown(KeyCode.Z))
                StartCoroutine(ItemSelected());
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        // player opens up their party screen by using an item
        else if (state == InventoryUIState.PartySelect)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };
            Action onBackParty = () =>
            {
                ClosePartyScreen();
            };

            party.HandleUpdate(onSelected, onBackParty);
        }
        // user tries to learn a new move using TM/HM
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelect(moveIndex));
            };

            moveSelect.HandleMoveSelection(onMoveSelected);
        }
    }

    // if an item is selected run this functin
    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selected, selectedCat);

        // checks if item can be used inside or outside of battle and tells user
        if (GameControl.Instance.State == GameState.Battle)
        {
            // in battle
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle");
                state = InventoryUIState.ItemSelect;
                yield break;
            }
        }
        else
        {
            // outside battle
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used outside battle");
                state = InventoryUIState.ItemSelect;
                yield break;
            }
        }
        // mon ball selection
        if (selectedCat == (int)ItemCategory.MonBalls)
        {
            // Mon ball
            StartCoroutine(UseItem());
        }
        // if you select a TM/HM show if the item is usable or not
        else
        {
            OpenPartyScreen();

            if (item is TmItem)
                party.ShowIfTmIsUsable(item as TmItem);
        }
    }

    // when the player tries to use an item
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

        var usedItem = inventory.UseItem(selected, party.SelectedMon, selectedCat);
        // if using a recovery tiem check if it will do anything and use it/show player
        if (usedItem != null)
        {
            if (usedItem is Recovery)
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");

            onItemUsed?.Invoke(usedItem);
        }
        // if you can't use the item then show dialog saying it won't have an affect
        else
        {
            if (selectedCat == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText($"This will not have any affect!");
        }

        // close party screen
        ClosePartyScreen();
    }

    // handles using the TM/HM item
    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selected, selectedCat) as TmItem;

        // make sure you have a TM first
        if (tmItem == null)
            yield break;

        var mon = party.SelectedMon;

        // checks if the monmon already knows the move and can be caught the move
        if (mon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{mon.Basic.Name} already knows {tmItem.Move.Name}");
            yield break;
        }

        if (!tmItem.CanBeTaught(mon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{mon.Basic.Name} cannot learn {tmItem.Move.Name}");
            yield break;
        }

        // if the mon that is being taught has < 4 moves then just learn
        // if not how the choose to forget, same as battle
        if (mon.Moves.Count < PokemonBasic.MaxNumOfMoves)
        {
            mon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{mon.Basic.Name} learned {tmItem.Move.Name}!");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{mon.Basic.Name} is trying to learn {tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {PokemonBasic.MaxNumOfMoves} moves");
            yield return ChooseMoveToForget(mon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }
    }

    // Shows list a moves and pick one you want to forget
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBasic newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move you want to forget", true, false);

        moveSelect.gameObject.SetActive(true);
        moveSelect.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    // when the item list is being used by the player and they move around using the arrow keys
    // show what they have selected and clamp so they don't go out of bounds
    void UpdateItemSelect()
    {
        var slots = inventory.GetSlotsByCat(selectedCat);

        selected = Mathf.Clamp(selected, 0, slots.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selected)
                slotUIList[i].Name.color = GlobalSettings.i.Hightlight;
            else
                slotUIList[i].Name.color = Color.black;
        }

        if (slots.Count > 0)
        {
            var slot = slots[selected];
            itemImage.sprite = slot.Item.Icon;
            itemDesc.text = slot.Item.Desc;
        }

        HandleScrolling();
    }

    // handles the scrolling function and makes sure the items selected stay on the screen
    // also controls if the player can see the up or down arrow in the UI
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInView) return;

        float scrollPos = Mathf.Clamp(selected - (itemsInView / 2), 0, selected) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selected > itemsInView / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selected + itemsInView / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    // resets what is selected on the list to the first element when switching categories
    void ResetSelect()
    {
        selected = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemImage.sprite = null;
        itemDesc.text = "";
    }

    // opens party screen
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelect;

        party.gameObject.SetActive(true);
    }

    // closes party scrren
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelect;

        party.ClearMemberSlotMessages();

        party.gameObject.SetActive(false);
    }

    // when you select a move to forget try to learn a new move
    // or if you select the new move you did not learn a new move
    IEnumerator OnMoveToForgetSelect(int moveIndex)
    {
        var mon = party.SelectedMon;

        DialogManager.Instance.CloseDialog();

        moveSelect.gameObject.SetActive(false);
        if (moveIndex == PokemonBasic.MaxNumOfMoves)
        {
            yield return DialogManager.Instance.ShowDialogText($"{mon.Basic.Name} did not learn {moveToLearn.Name}");
        }
        else
        {
            var selected = mon.Moves[moveIndex].Base;

            yield return DialogManager.Instance.ShowDialogText($"{mon.Basic.Name} forgot {selected.Name} and learned {moveToLearn.Name}");

            mon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelect;
    }
}
