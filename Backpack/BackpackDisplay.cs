using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using CyiLibrary;

public class BackpackDisplay : MonoBehaviour
{
    private int nowWhatCountSlot = -1; //目前位於哪個Slot (大於等於0是該Slot (-1為都不是(尚未Click) (-2為任何Event尚未觸發
    
    private bool checkLeftOnClick;     //確認左鍵是否有案 
    private MouseClick mouseClick;
    
    [Space(20)]
    [SerializeField]
    private Backpack playerBackpack;
    [SerializeField]
    private BackpackUIManagement backpackUIManagement;

    public GameObject slotPrefab;
    public UIIntroduction uiIntroduction; //打開背包右邊的介紹

    public MouseItem mouseItem = new MouseItem();
    #region Use Dispaly (Public) variable
    [Header("Use Dispaly")]
    public RectTransform useDispalyUI;
    public Button useButton, removeButton, cancellationButton;

    #endregion
    [Header("Container AND Introduction change")]
    public Image IntroductionContainer;
    public Image ContainerBG , ContainerOutline;
    public Sprite ItemContainerSprite, CardContainerSprite, ItemIntroductionContainer, CardIntroductionContainer, ItemContainerOutline, CardContainerOutline;
        
    public int leftClickCount;
    public bool leftClickDouble = false;

    //查看要Updata哪一個 -> Slot或是Amount
    //Dictionary<BackpackSlot, GameObject> itemDisplayed = new Dictionary<BackpackSlot, GameObject>();
    private void Awake()
    {
        if(playerBackpack == null)
            playerBackpack = GameObject.FindGameObjectWithTag("Player").GetComponent<Backpack>();
        if(backpackUIManagement == null)
            backpackUIManagement = FindObjectOfType<BackpackUIManagement>();

        useButton.onClick.AddListener(UseBackpackSlot);
        removeButton.onClick.AddListener(UseDisplay_RemoveButton);
        cancellationButton.onClick.AddListener(UseDisplay_CancellationButton);
    }
    private void OnEnable()
    {
        switch (backpackUIManagement.GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                ContainerBG.sprite = ItemContainerSprite;
                ContainerOutline.sprite = ItemContainerOutline;
                IntroductionContainer.sprite = ItemIntroductionContainer;
                InitialContainer(playerBackpack.GetItemContainer); 
                break;
            case Backpack_Type.Card_Button:
                ContainerBG.sprite = CardContainerSprite;
                ContainerOutline.sprite = CardContainerOutline;
                IntroductionContainer.sprite = CardIntroductionContainer;
                InitialContainer(playerBackpack.GetCardContainer); 
                break;
            default:
                break;
        }
    }

    void InitialContainer(BackpackSlot[] _Container)
    {
        for (int i = 0; i < _Container.Length; i++)
        {
            var _slot = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, transform);
            AddEvent(_slot, EventTriggerType.PointerEnter, delegate { OnEnter(_slot); });
            AddEvent(_slot, EventTriggerType.PointerExit, delegate { OnExit(); });
            
            if (_Container[i].itemBasic != null) //Slot裡面有ItemBasic
            {
                _slot.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _Container[i].itemBasic.UiDisplay;
                _slot.GetComponentInChildren<Text>().text = (_Container[i].amount == 1) ? "" : _Container[i].amount.ToString();
                                
                AddEvent(_slot, EventTriggerType.PointerClick, delegate { OnClick(_slot); });
                AddEvent(_slot, EventTriggerType.Drag, delegate { OnDrag(_slot); });
                AddEvent(_slot, EventTriggerType.BeginDrag, delegate { OnDragStart(_slot); });
                AddEvent(_slot, EventTriggerType.EndDrag, delegate { OnDragEnd(_slot); });
            }
        }
    }

    void Update()
    {        
        mouseClick = MouseInputCheck.MouseClickCheck();     //< -- 這邊要另外寫

        switch (backpackUIManagement.GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                UpdateContainer(playerBackpack.GetItemContainer);
                
                if (Input.GetKeyDown(KeyCode.R))////////////////////////////////////////記得要改
                    playerBackpack.ResetContainer(playerBackpack.GetItemContainer);

                if (nowWhatCountSlot == -1 || CheckContainerIsNull(playerBackpack.GetItemContainer))
                    uiIntroduction.ClearUI_Introduction();
                break;
            case Backpack_Type.Card_Button:
                UpdateContainer(playerBackpack.GetCardContainer);

                if (Input.GetKeyDown(KeyCode.R))////////////////////////////////////////記得要改
                    playerBackpack.ResetContainer(playerBackpack.GetCardContainer);

                if (nowWhatCountSlot == -1 || CheckContainerIsNull(playerBackpack.GetCardContainer))
                    uiIntroduction.ClearUI_Introduction();
                break;
        }
    }
    void UpdateContainer(BackpackSlot[] _Container)
    {
        //可以來個 按下 F or L Click or R Click 才執行
        //if (!CheckAction())
        //    return;

        for (int i = 0; i < _Container.Length; i++)
        {
            if (_Container[i].itemBasic != null) //Slot裡面有ItemBasic
            {
                Transform _slot = transform.GetChild(i);
                _slot.GetComponentInChildren<Text>().text = (_Container[i].amount == 0) ? "" : _Container[i].amount.ToString();
                Image _slotIMG = _slot.GetChild(0).GetComponentInChildren<Image>();
                _slotIMG.enabled = true;
                _slotIMG.sprite = _Container[i].itemBasic.UiDisplay;

                var _SlOT = _slot.gameObject;

                ClearEvent(_slot.gameObject);
                AddEvent(_SlOT, EventTriggerType.PointerClick, delegate { OnClick(_SlOT); });
                AddEvent(_SlOT, EventTriggerType.PointerEnter, delegate { OnEnter(_SlOT); });
                AddEvent(_SlOT, EventTriggerType.PointerExit, delegate { OnExit(); });
                AddEvent(_SlOT, EventTriggerType.Drag, delegate { OnDrag(_SlOT); });
                AddEvent(_SlOT, EventTriggerType.BeginDrag, delegate { OnDragStart(_SlOT); });
                AddEvent(_SlOT, EventTriggerType.EndDrag, delegate { OnDragEnd(_SlOT); });

            }
            else
            {
                Transform _slot = transform.GetChild(i);
                _slot.GetComponentInChildren<Text>().text = "".ToString();
                Image _slotIMG = _slot.GetChild(0).GetComponentInChildren<Image>();
                _slotIMG.enabled = false;
                _slotIMG.sprite = null;
                    
                if (_slot.gameObject.GetComponent<EventTrigger>().triggers.Count >=6)  //有點消耗效能 -> 每一個 Frames 都在GetComponemt ?
                {
                    ClearEvent(_slot.gameObject);
                    Initialization_nowWhatCountSlot_AND_useDispalyObject();
                    AddEvent(_slot.gameObject, EventTriggerType.PointerEnter, delegate { OnEnter(_slot.gameObject); });
                    AddEvent(_slot.gameObject, EventTriggerType.PointerExit, delegate { OnExit(); });
                }
            }
        }
    }

    bool CheckContainerIsNull(BackpackSlot[] _Container) //Check Container 是否為null , true -> Instuction 為空白 反之 ( 另外一個方法 -> 每次增加or刪除時在做Check
    {
        int _checkContainerIsNull = 0;
        for (int i = 0; i < _Container.Length; i++)
        {
            if (_Container[i].itemBasic == null)
                _checkContainerIsNull++;
        }
        return _checkContainerIsNull == _Container.Length ? true : false;
    }
    

    #region Event Function
    void AddEvent(GameObject _slot,EventTriggerType type,UnityAction<BaseEventData> action)
    {    
        EventTrigger trigger = _slot.GetComponent<EventTrigger>();        
        if (trigger.triggers.Count < 6)  //// <- event的數量
        {
            EventTrigger.Entry eventTrigger = new EventTrigger.Entry();
            eventTrigger.eventID = type;  //選擇型態
            eventTrigger.callback.AddListener(action);
            trigger.triggers.Add(eventTrigger);            
        }
    }
    void ClearEvent(GameObject _slot)
    {
        EventTrigger trigger = _slot.GetComponent<EventTrigger>();
        trigger.triggers.Clear();
    }

    IEnumerator IE_ClickTime;
    public void OnClick(GameObject _slot)  //Mouse Click Slot
    {
        if (!leftClickDouble)
        {
            leftClickCount++;
            if (leftClickCount == 1)
            {
                IE_ClickTime = ClickTime(0.5f);
                StartCoroutine(IE_ClickTime);
            }
            leftClickDouble = leftClickCount > 0? true : false;
        }
        else // Left Double Click
        {
            StopCoroutine(IE_ClickTime);
            UseBackpackSlot();
            leftClickDouble = false;
            leftClickCount = 0;
            return;
        }

        nowWhatCountSlot = _slot.transform.GetSiblingIndex();       
        checkLeftOnClick = (mouseClick == MouseClick.LeftClick);

        if (mouseClick == MouseClick.LeftClick || mouseClick == MouseClick.RightClick)
            RightDisplayIntroduction(_slot);

        ////右鍵出現東西 尚未完整 封印住
        //if (mouseClick == MouseClick.RightClick)        
        //    UseDisplay_SetActive(backpackUIManagement.GetBackpack_Type);

        if (mouseClick == MouseClick.LeftClick && useDispalyUI.gameObject.activeInHierarchy)        
            Initialization_nowWhatCountSlot_AND_useDispalyObject();
    }
    IEnumerator ClickTime(float _time) //時間到 如果沒有L_Click 兩下 就取消 L_Click 兩下 變成一下
    {
        Debug.Log(" CAN ");
        yield return new WaitForSecondsRealtime(_time); //Because gamescnen pause, so using Realtime.
        Debug.Log(" NOT ");
        leftClickDouble = false;
        leftClickCount = 0;
    }

    public void OnEnter(GameObject _slot)  //Mouse Enter Slot
    {
        //Display = true

        int _Container_Amount = _slot.transform.GetSiblingIndex();
        mouseItem.hoverItemObj = _slot;
        BackpackSlot[] backpackSlot = backpackUIManagement.GetBackpack_Type == Backpack_Type.Item_Button ? playerBackpack.GetItemContainer : playerBackpack.GetCardContainer;
        mouseItem.hoverItemSlot = backpackSlot[_Container_Amount];
        mouseItem.hoverIndex = _Container_Amount;

        if (nowWhatCountSlot > -1 || mouseItem.hoverItemSlot.itemBasic == null)
            return;

        nowWhatCountSlot = -2;
        RightDisplayIntroduction(_slot);
    }
    public void OnDragStart(GameObject _slot)  //Mouse Click Slot
    {
        int _Container_Amount = _slot.transform.GetSiblingIndex();

        var mouseObject = new GameObject();
        var rt = mouseObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        mouseObject.transform.SetParent(transform.parent);
        if(playerBackpack.GetItemContainer[_Container_Amount].itemBasic != null)
        {
            var img = mouseObject.AddComponent<Image>();
            img.sprite = _slot.transform.GetChild(0).GetComponentInChildren<Image>().sprite;
            img.raycastTarget = false;
        }

        mouseItem.itemObj = mouseObject;
        BackpackSlot[] backpackSlot = backpackUIManagement.GetBackpack_Type == Backpack_Type.Item_Button ? playerBackpack.GetItemContainer : playerBackpack.GetCardContainer;
        mouseItem.itemSlot = backpackSlot[_Container_Amount];
    }
    public void OnDrag(GameObject _slot)
    {
        if (mouseItem.itemObj != null)
            mouseItem.itemObj.GetComponent<RectTransform>().position = Input.mousePosition;
    }
    public void OnDragEnd(GameObject _slot)
    {
        BackpackSlot[] backpackSlot = backpackUIManagement.GetBackpack_Type == Backpack_Type.Item_Button ? playerBackpack.GetItemContainer : playerBackpack.GetCardContainer;
        Debug.Log(backpackSlot[mouseItem.hoverIndex]);
        if (mouseItem.hoverItemObj)
        {
            playerBackpack.MoveItemFromContainer(backpackSlot[_slot.transform.GetSiblingIndex()], backpackSlot[mouseItem.hoverIndex]);
        }
        else
        {
            //Destroy obj
        }
        Destroy(mouseItem.itemObj);
        mouseItem.itemSlot = null;
    }

    void RightDisplayIntroduction(GameObject _slot) //介紹 卡 or 道具 or 材料
    {
        int _Container_Amount = _slot.transform.GetSiblingIndex();
        if (playerBackpack.GetItemContainer[_Container_Amount].itemBasic == null || playerBackpack.GetCardContainer[_Container_Amount] == null)
            return;

        if (_slot == null)
            return;

        //Basic        
        string _ItemBasic_name = null;
        string _ItemBasic_introduction = null;
        string _ItemBasic_where = null;
        Sprite _ItemBasic_uiDisplay = null;

        //Item
        string _Item_type = null;

        //Card
        string _Card_health = null;
        string _Card_movespeed = null;
        string _Card_attackType = null;
        string _Card_attackValue = null;
        string _Card_skillType = null;
        string _Card_skillValue = null;

        switch (backpackUIManagement.GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                playerBackpack.GetItemContainer[_Container_Amount].itemBasic.BasicIntroduction_UIDispaly(
                    ref _ItemBasic_name,
                    ref _ItemBasic_introduction,
                    ref _ItemBasic_where,
                    ref _ItemBasic_uiDisplay);

                playerBackpack.GetItemContainer[_Container_Amount].ItemIntroduction_UIDisplay(ref _Item_type);

                uiIntroduction.UIUpdata_ItemIntroduction("Item類型: " + _Item_type);
                uiIntroduction.UIUpdata_CardIntroduction(null, 0, 0, null, null, null, null, null);
                break;
            case Backpack_Type.Card_Button:
                playerBackpack.GetCardContainer[_Container_Amount].itemBasic.BasicIntroduction_UIDispaly(
                    ref _ItemBasic_name,
                    ref _ItemBasic_introduction,
                    ref _ItemBasic_where,
                    ref _ItemBasic_uiDisplay);

                playerBackpack.GetCardContainer[_Container_Amount].CardIntroduction_UIDisplay(
                     ref _Card_health,
                     ref _Card_movespeed,
                     ref _Card_attackType,
                     ref _Card_attackValue,
                     ref _Card_skillType,
                     ref _Card_skillValue);

                uiIntroduction.UIUpdata_ItemIntroduction(null);

                
                uiIntroduction.UIUpdata_CardIntroduction(
                    "生命值: " /*+ playerBackpack.GetCardObjectData_From_CardObjectDataContainer(_Container_Amount).nowHealth +"/" + playerBackpack.GetCardObjectData_From_CardObjectDataContainer(_Container_Amount).maxHealth*/,
                    playerBackpack.GetCardObjectData_From_CardObjectDataContainer(_Container_Amount).nowHealth,
                    playerBackpack.GetCardObjectData_From_CardObjectDataContainer(_Container_Amount).maxHealth,
                    "移動速度: " + _Card_movespeed,
                    "攻擊類型: " + _Card_attackType,
                    "攻擊力道: " + _Card_attackValue,
                    "技能類型: " + _Card_skillType,
                    "技能力道: " + _Card_skillValue);

                break;
            default:
                break;
        }


        uiIntroduction.UIUpdata_BasicIntroduction(
        _ItemBasic_name,
        _ItemBasic_introduction,
        "地點: " + _ItemBasic_where,
        _ItemBasic_uiDisplay);
    }

    public void OnExit()
    {
        //Display = false
        mouseItem.hoverItemObj = null;
        mouseItem.hoverItemSlot = null;

        if (nowWhatCountSlot == -2)
            uiIntroduction.ClearUI_Introduction();
    }
    #endregion

    private void OnDisable() //Close Script // Close Backpack
    {
        switch (backpackUIManagement.GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                for (int i = 0; i < playerBackpack.GetItemContainer.Length; i++)
                {
                    GameObject _slot = transform.GetChild(i).gameObject;
                    if (_slot.gameObject.GetComponent<EventTrigger>())  //用Getcomponent 耗效能?
                        ClearEvent(_slot.gameObject);
                    Destroy(_slot.gameObject);
                }
                break;
            case Backpack_Type.Card_Button:
                for (int i = 0; i < playerBackpack.GetCardContainer.Length; i++)
                {
                    GameObject _slot = transform.GetChild(i).gameObject;
                    if (_slot.gameObject.GetComponent<EventTrigger>())  //用Getcomponent 耗效能?
                        ClearEvent(_slot.gameObject);
                    Destroy(_slot.gameObject);
                }
                break;
            default:
                break;
        }
    }

    #region Use Dispaly Function 顯示
    void UseDisplay_SetActive(Backpack_Type _backpack_Type) //出現 DisplayUse
    {
        useDispalyUI.gameObject.SetActive(true);
        Vector3 mousePos = Input.mousePosition;     //SetDisplay Position -> MousePosition
        useDispalyUI.transform.position = mousePos;
        switch (_backpack_Type)
        {
            case Backpack_Type.Item_Button:

                //useCardButton.enabled = false;
                //useCardButton.image.color = card_Item_InUseColor.Evaluate(0);

                break;
            case Backpack_Type.Card_Button:

                //useCardButton.enabled = true;
                //useCardButton.image.color = card_Item_InUseColor.Evaluate(1);
                
                break;
            default:
                break;
        }
    }
    void UseBackpackSlot() //Button and UseSlot
    {
        if (nowWhatCountSlot < 0)
        {
            Debug.LogError("nowWhatCountSlot need is != -1 or -2 !! ");
            return;
        }
        //if() 現在所在位置的ITEM 沒了 不執行後面 且 ClickItemOrCardButtonBecome() 
        //    ClickItemOrCardButtonBecome()
        switch (backpackUIManagement.GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                ////尚未想到使用道具可以幹嘛 所以先封印住
                //playerBackpack.DeleteBasicItem_InContainer_Slot(playerBackpack.GetItemContainer, nowWhatCountSlot, 1); //Delete ItemContainer 的 Slot
                break;
            case Backpack_Type.Card_Button:

                if (playerBackpack.NowHaveCardObject())
                {
                    Debug.Log("現在有，不刪除");
                }
                else
                {
                    Debug.Log("USE Card");
                    playerBackpack.UseCard(playerBackpack.GetCardContainer, nowWhatCountSlot);
                    playerBackpack.DeleteBasicItem_InContainer_Slot(playerBackpack.GetCardContainer, nowWhatCountSlot, 1); //Delete CardContainer 的 Slot
                }

                break;
            default:
                break;
        }
    }

    void UseDisplay_RemoveButton() //Button
    {
        switch (backpackUIManagement.GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                playerBackpack.DeleteBasicItem_InContainer_Slot(playerBackpack.GetItemContainer, nowWhatCountSlot, 1); //Delete ItemContainer 的 Slot
                break;
            case Backpack_Type.Card_Button:
                playerBackpack.DeleteBasicItem_InContainer_Slot(playerBackpack.GetCardContainer, nowWhatCountSlot, 1); //Delete CardContainer 的 Slot
                break;
            default:
                break;
        }
    }
    void UseDisplay_CancellationButton() //Button
    {
        useDispalyUI.gameObject.SetActive(false);
        nowWhatCountSlot = -2;
    }
    #endregion

    public void Initialization_nowWhatCountSlot_AND_useDispalyObject()//給BackpackUIManager用的Public function
    {
        nowWhatCountSlot = -1; //-1 是為了背包右邊的介紹
        useDispalyUI.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class UIIntroduction // 把 Item or Card 的 數值 丟到 背包右邊的 Introduction    -在想要不要把 Item 改成 Basic
{
    [Header("Basic")]
    public Text BasicItem_name;
    public Text BasicItem_introduction, BasicItem_where;
    public Image BasicItem_uiDisplay;

    [Header("Item")]
    public Text Item_type;

    [Header("Card")]
    public Text Card_health;
    public Text Card_movespeed, Card_attacktype, Card_attackvalue, Card_skilltype, Card_skillvalue;
    public Image Card_health_value;
    public Gradient Card_health_gradient;

    public void UIUpdata_BasicIntroduction(string _Item_name, string _Item_introduction, string _Item_where, Sprite _Item_uiDisplay)
    {
        BasicItem_name.text = _Item_name;
        BasicItem_introduction.text = _Item_introduction;
        BasicItem_where.text = _Item_where;
        BasicItem_uiDisplay.sprite= _Item_uiDisplay;

    }
    public void UIUpdata_ItemIntroduction(string _Item_type)
    {
        Item_type.text = _Item_type;
    }
    public void UIUpdata_CardIntroduction(string _Card_health,float _Card_nowHealth, float _Card_maxHealth, string _Card_movespeed, string _Card_attacktype, string _Card_attackvalue ,string _Card_skilltype, string _Card_skillvalue)
    {
        Card_health.text = _Card_health;
        Card_movespeed.text = _Card_movespeed;
        Card_attacktype.text = _Card_attacktype;
        Card_attackvalue.text = _Card_attackvalue;
        Card_skilltype.text = _Card_skilltype;
        Card_skillvalue.text = _Card_skillvalue;

        Card_health_value.enabled = _Card_health != null;
        Card_health_value.gameObject.transform.parent.GetComponent<Image>().enabled = _Card_health != null;
        if (_Card_health == null)
            return;            
        float _health = _Card_nowHealth / _Card_maxHealth;        
        Card_health_value.rectTransform.localScale = new Vector3(_health, 1, 1);
        Card_health_value.color = Card_health_gradient.Evaluate(_health);
    }

    public void ClearUI_Introduction()//把所有Introduction清掉
    {
        UIUpdata_BasicIntroduction(null, null, null, null);
        UIUpdata_ItemIntroduction(null);
        UIUpdata_CardIntroduction(null, 0, 0, null, null, null, null, null);
    }   
}

[System.Serializable]
public class MouseItem {
    public GameObject itemObj;
    public BackpackSlot itemSlot;

    public GameObject hoverItemObj;
    public BackpackSlot hoverItemSlot;
    public int hoverIndex = -1;
}
