using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelect, PartySelect, MoveToForget, Busy }

public class InventoryUI : MonoBehaviour
{
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
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

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

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null)
    {
        this.onItemUsed = onItemUsed;

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

            if (Input.GetKeyDown(KeyCode.Z))
                StartCoroutine(ItemSelected());
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
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
        else if (state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelect(moveIndex));
            };

            moveSelect.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selected, selectedCat);
        
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

        if (selectedCat == (int)ItemCategory.MonBalls)
        {
            // Mon ball
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();

            if (item is TmItem)
                party.ShowIfTmIsUsable(item as TmItem);
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

        var usedItem = inventory.UseItem(selected, party.SelectedMon, selectedCat);
        if (usedItem != null)
        {
            if (usedItem is Recovery)
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");

            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCat == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText($"This will not have any affect!");
        }

        ClosePartyScreen();
    }

    IEnumerator HandleTmItems()
    {
        var tmItem = inventory.GetItem(selected, selectedCat) as TmItem;

        if (tmItem == null)
            yield break;

        var mon = party.SelectedMon;

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

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBasic newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose a move you want to forget", true, false);

        moveSelect.gameObject.SetActive(true);
        moveSelect.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

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

    void ResetSelect()
    {
        selected = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemImage.sprite = null;
        itemDesc.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelect;

        party.gameObject.SetActive(true);
    }
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelect;

        party.ClearMemberSlotMessages();

        party.gameObject.SetActive(false);
    }

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
