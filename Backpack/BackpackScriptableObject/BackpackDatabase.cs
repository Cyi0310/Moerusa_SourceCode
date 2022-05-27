using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BackpackDatabase", menuName = "BackpackData/PlayerBackpackDatabase")]
public class BackpackDatabase : ScriptableObject
{
    public BackpackSlot[] ItemContainer = new BackpackSlot[30];
    public BackpackSlot[] CardContainer = new BackpackSlot[20];

    int containerCount = 0;
    //增加ItemBasicContainer  (BackpackSlot , 哪一個ItemBasic , 要加幾個, 那一個物件(要刪除用) )
    public void AddItemBasic_InContainer_Slot(BackpackSlot[] _Container, ItemBasic _itemBasic, int _amount)
    {
        containerCount = 0;
        //Check是否有該ItemBasic 看Backpack裡面所有的Slot是否有此Item
        for (int i = 0; i < _Container.Length; i++)
        {
            //有此ItamBasic -> 增加他的amount
            if (_itemBasic is Item)//_itemBasic.GetType() == typeof(Props) || _itemBasic.GetType() == typeof(Stuff))
            {
                if (_Container[i].itemBasic == _itemBasic)
                {
                    _Container[i].SetAmount(_amount);
                    return;
                }
            }
            //當Slot為Null時
            if (_Container[i].itemBasic == null)
            {
                int _slotIsNull_index = i;
                //Check Null Slot 後面 Slot沒有相同的ItemBasic
                if (_itemBasic is Item)  //_itemBasic.GetType() == typeof(Props) || _itemBasic.GetType() == typeof(Stuff))
                {
                    for (int _slotIsNull_Afterindex = _slotIsNull_index; _slotIsNull_Afterindex < _Container.Length; _slotIsNull_Afterindex++)
                    {
                        //Check後 有 -> 增加他的amount
                        if (_Container[_slotIsNull_Afterindex].itemBasic == _itemBasic)
                        {
                            _Container[_slotIsNull_Afterindex].SetAmount(_amount);
                            return;
                        }
                    }
                }
                //Check後 沒有 -> 加入到BackpackSlot的Null裡面
                _Container[_slotIsNull_index].itemBasic = _itemBasic;
                _Container[_slotIsNull_index].SetAmount(_amount);
                break;
            }
            containerCount++;
        }
    }
    public bool ContainerIsFull(BackpackSlot[] _Container) => containerCount == _Container.Length;
        
    public void MoveItemFromContainer(BackpackSlot itemSlot1, BackpackSlot itemSlot2)
    {
        BackpackSlot temp = new BackpackSlot(itemSlot1.itemBasic, itemSlot1.amount);
        itemSlot1.UpdateSlot(itemSlot2.itemBasic, itemSlot2.amount);
        itemSlot2.UpdateSlot(temp.itemBasic, temp.amount);
    }
    public void Get_DeleteBasicItem_InContainer_Slot(BackpackSlot[] _Container, int _index, int _amount)  //減少Item
    {    
        _Container[_index].SetAmount(-_amount);
        
        if (_Container[_index].amount <= 0)
            _Container[_index].itemBasic = null;
    }

    public void ResetContainer(BackpackSlot[] _Container)
    {
        for (int i = 0; i < _Container.Length; i++)
        {
            _Container[i] = new BackpackSlot(null, 0);
        }
    }
}

[System.Serializable]
public class BackpackSlot
{
    [SerializeField] public ItemBasic itemBasic;
    [SerializeField] public int amount;
            
    public BackpackSlot(ItemBasic _itemBasic, int _amount) //初始化
    {
        itemBasic = _itemBasic;
        amount = _amount;
    }

    //設置Slot裡面的Amount
    public void SetAmount(int value)
    {     
        amount += value;
        if (amount <= 0)
            amount = 0;
    }
    public void UpdateSlot(ItemBasic _itemBasic, int _amount) //初始化
    {
        itemBasic = _itemBasic;
        amount = _amount;
    }

    #region Introduction - Item & Card Public Function
    public void ItemIntroduction_UIDisplay(ref string _itemType)
    {
        Item _item = itemBasic as Item;
        _item.ItemIntroduction_UIDispaly(ref _itemType);
    }

    public void CardIntroduction_UIDisplay(ref string _card_health,ref string _card_movespeed,ref string _card_attackType,ref string _card_attackValue,ref string _card_skillType,ref string _card_skillValue)
    {
        CardItem _card  = itemBasic as CardItem;

        _card.Introduction_UIDispaly(ref _card_health,
        ref _card_movespeed,
        ref _card_attackType,
        ref _card_attackValue,
        ref _card_skillType,
        ref _card_skillValue);
    }
    #endregion
}
