using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyiLibrary;

public class Backpack : MonoBehaviour
{
    private PlayerManager playerManager;
    private KeyCode takeItemBasicKeyCode, useBackpackKeyCode;
    
    [SerializeField] 
    private BackpackDatabase backpackDatabase; 
    [SerializeField]
    private TakeTip takeTip;
    [SerializeField]
    private BackpackUIManagement backpackUIManagement;

    #region Take variable
    
    private bool isTakeUI;
    private List<Transform> _itemBasic_Transform; // 範圍內的ItemBasic丟到這 類似小Register , 方便做運算取得距離最近的

    [Header("ItemBasic"),SerializeField]
    private float canTake_radius = 2f;    //可以拿到的半徑
    
    [Range(45f, 180f) , SerializeField] 
    private float canTake_MaxAngle = 90f;   //可以拿到的視野
    
    [SerializeField] 
    private LayerMask itemBasic_Layermask; // -> LayerMask = ItemBasicObject
    
    public GameObject Prefab_CanTakeUI;
    private GameObject canTakeUI;
    [SerializeField]
    private float canTakeUIOffset = 0.8f;

    List<float> Item2Player_distances = new List<float>();
    #endregion

    #region About Card variable & Function
    CardManager cardManager;
    public CardObjectData GetCardObjectData_From_CardObjectDataContainer(int _key) => cardManager.GetCardObjectData_From_CardObjectDataContainer(_key);
    #endregion

    public KeyCode GetUseBackpackKeyCode { get => useBackpackKeyCode; }

    [Header("Audio")]
    public Sound[] sounds;


    private void Awake()
    {
        if (backpackDatabase == null)
            Debug.LogError(this + " 的 backpackDatabase 為Null");

        itemBasic_Layermask = LayerMask.GetMask("ItemBasicObject");        
    }
    
    private void Start()
    {           
        playerManager = GetComponent<PlayerManager>();
        cardManager = gameObject.GetComponent<CardManager>();

        useBackpackKeyCode = playerManager.GetPlayerKeyCode("UseBackpack").keycode;
        takeItemBasicKeyCode = playerManager.GetPlayerKeyCode("TakeItemBasic").keycode;

        GameObject worldSpaceCanvas = GameObject.Find("WorldSpaceCanvas");
        GameObject _canTakeUI = Instantiate(Prefab_CanTakeUI.gameObject, Vector3.zero, worldSpaceCanvas.transform.localRotation, worldSpaceCanvas.transform);
        canTakeUI = _canTakeUI;

        backpackUIManagement = GameObject.Find("BackpackUIManagement").GetComponent<BackpackUIManagement>();

        //Sounds Initial
        StartCoroutine(WaitStartFunctionFinalFrame());
    }
    IEnumerator WaitStartFunctionFinalFrame()
    {
        yield return new WaitForEndOfFrame();
        playerManager.Audio_InitialSounds(sounds, gameObject);
    }

    private void Update()
    {
        PlayerTake();
    }

    #region Take Public Function & Variable
    private bool isFull = false;
    public ItemBasic TakeNewItemBasic()// For "TakeTip" use
    {
        ItemBasicObject _TheTipObject = _itemBasic_Transform[0].GetComponent<ItemBasicObject>();        
        return _TheTipObject.GetItemBasic;
    }
    #endregion

    #region Take Function
    void PlayerTake()
    {
        bool _CheckItemBasicCanTack = Physics.CheckSphere(transform.position, canTake_radius, itemBasic_Layermask);

        isTakeUI = false;  //UI use
        
        if (_CheckItemBasicCanTack)
        {
            Collider[] _ItemBasic_collider = Physics.OverlapSphere(transform.position, canTake_radius, itemBasic_Layermask);
            _itemBasic_Transform = new List<Transform>();
            for (int i = 0; i < _ItemBasic_collider.Length; i++)
            {
                Vector3 _target = _ItemBasic_collider[i].transform.position - gameObject.transform.position;
                _target.Normalize();
                float angle = Vector3.Angle(transform.forward, _target);
                //在視野角度內 增加 -> _itemBasic_Transform
                if (angle <= canTake_MaxAngle)
                    _itemBasic_Transform.Add(_ItemBasic_collider[i].transform);

                //關於Outline 先把所有ItemBasic 的 Outline = false
                _ItemBasic_collider[i].GetComponent<ItemBasicObject>().SetCanTake = false;
            }
            //眼前是否有物品
            bool eyeforword_haveItemBasic = _itemBasic_Transform.Count > 0 ? true : false;
            bool click_F = Input.GetKeyDown(takeItemBasicKeyCode);

            //m_TakeTip = click_F & eyeforword_haveItemBasic;
            StartCoroutine(WaitEndFrame_SetTakeTip(click_F & eyeforword_haveItemBasic));

            isTakeUI = eyeforword_haveItemBasic; //UI use

            if (eyeforword_haveItemBasic)
            {
                EasySortCheck();

                ItemBasicObject _itemBasicObject = _itemBasic_Transform[0].GetComponent<ItemBasicObject>();

                //撿東西 出現 請按下F的UI
                _itemBasicObject.SetTakeTip(canTakeUI.transform, canTakeUIOffset);

                //關於Outline 把可以撿起的 ItemBasic 的 Outline = true
                _itemBasicObject.SetCanTake = true;
                
                if (click_F)
                {
                    //GameObject _itemBasic = _itemBasic_Transform[0].gameObject;
                    TakeItemBasicObject(_itemBasicObject, _itemBasicObject.gameObject);
                }
            }            
        }
        canTakeUI.SetActive(isTakeUI); //UI use
    }

    public void TakeItemBasicObject(ItemBasicObject _Pick_ItemBasicObject,GameObject _destroyObj) //撿起 
    {
        if (_Pick_ItemBasicObject == null)
            return;

        var _itemBasicObject = _Pick_ItemBasicObject;
        int _itemBasicObject_Amount = _itemBasicObject.itemBasicObject_Amount;

        if (_itemBasicObject_Amount <= 0)   //Debug
            Debug.LogError(_itemBasicObject + " 的 數量未調整至 「１」 以上");
        
        isFull = false;

        if (_itemBasicObject)
        {
            if (_itemBasicObject.GetItemBasic is Item)
            {
                backpackDatabase.AddItemBasic_InContainer_Slot(GetItemContainer, _itemBasicObject.GetItemBasic, _itemBasicObject_Amount);
                isFull = backpackDatabase.ContainerIsFull(GetItemContainer);
            }
            else if (_itemBasicObject.GetItemBasic is CardItem)
            {
                backpackDatabase.AddItemBasic_InContainer_Slot(GetCardContainer, _itemBasicObject.GetItemBasic, _itemBasicObject_Amount);
                isFull = backpackDatabase.ContainerIsFull(GetCardContainer);
            }

            if (!isFull)
                StartCoroutine(DelayDestroy(_destroyObj));
            else
                backpackUIManagement.BackpackButtonSkake();
            

            string takeItemSound = !isFull ? "TakeItem" : "TakeItemError";
            playerManager.Audio_PlayAudio(sounds, takeItemSound);
        }
    }
    public IEnumerator WaitEndFrame_SetTakeTip(bool _clickF_Eye) 
    {
        /// 因為提示是一案F && 在視野內 就可Get道具or卡
        /// 但是有可能背包已滿，這時無法出現提示 ///"而且Function無法在撿起後在Check"      
        /// --> 所以等到EndFrame在Check 是否 滿了(isFull) ， 之後再等1個Frame 再變回 False <--
        yield return new WaitForEndOfFrame();
        if (_clickF_Eye)
        {
            //if (!isFull)                
            takeTip.CallTip(_itemBasic_Transform[0].GetComponent<ItemBasicObject>().GetItemBasic, isFull);            
        }
    }
    public void SetTakeTip(ItemBasic _itemBasic,bool _isfull) => takeTip.CallTip(_itemBasic, _isfull);

    IEnumerator DelayDestroy(GameObject _gameObject)//延遲 (等待一偵) 刪除 -> 為了讓tip可以抓到
    {
        yield return null;
        Destroy(_gameObject);
    }
       
    void EasySortCheck()
    {
        if (_itemBasic_Transform.Count <= 1)
            return;
        Item2Player_distances.Clear();
        //將道具~玩家的距離 丟到Item2Player_distances List裡面
        for (int i = 0; i < _itemBasic_Transform.Count; i++)
        {
            Vector3 d_distance = _itemBasic_Transform[i].position - gameObject.transform.position;
            float distance = d_distance.magnitude;
            Item2Player_distances.Add(distance);
        }
        //Sort 將距離玩家最近的Item調換到0
        for (int i = 0; i < _itemBasic_Transform.Count; i++)
        {
            if (Item2Player_distances[0] > Item2Player_distances[i])
            {
                Swap.SwapList<float>(Item2Player_distances, 0, i);
                Swap.SwapList<Transform>(_itemBasic_Transform, 0, i);
            }
        }
    }
    #endregion

    #region backpackDatabase variable (Interface
    public BackpackSlot[] GetItemContainer => backpackDatabase.ItemContainer;
    //public BackpackSlot[] GetItemContainer { get => backpackDatabase.ItemContainer; }
    public BackpackSlot[] GetCardContainer => backpackDatabase.CardContainer;
    //public BackpackSlot[] GetCardContainer { get => backpackDatabase.CardContainer; }    
    #endregion

    #region BackpackDatabase Public Function ( InterFace

    public void DeleteBasicItem_InContainer_Slot(BackpackSlot[] _Container,int _index, int _amount) //減少Item or Card
    {
        backpackDatabase.Get_DeleteBasicItem_InContainer_Slot(_Container, _index, _amount);
    }

    /// <summary>    /// <summary>    /// <summary>    /// <summary>
    /// Card Function    /// Card Function    /// Card Function    /// Card Function
    /// </summary>   /// </summary>   /// </summary>  /// </summary>
    public bool NowHaveCardObject() //現在是用Card 
    {
        return cardManager.IsCallCard;
    }
    public void UseCard(BackpackSlot[] _Container,int _index) //目前只有Card能用 //將卡片叫出
    {
        if (_Container[_index].itemBasic.GetType() == typeof(CardItem))
        {
            CardItem _cardItem = _Container[_index].itemBasic as CardItem;

            //把 要叫出來的Card 丟給 -> Card Manager
            cardManager.SetNowCard(_index);  //從 CARD MANAGER 拉資訊過來                                             
        }
    }
    public GameObject RecallCard(BackpackSlot[] _Container, int _index) //目前只有Card能用
    {
        //找叫出的CARD ， 並收回到BACKPACK
        if (_Container[_index].itemBasic.GetType() == typeof(CardItem))
        {
            CardItem _cardItem = _Container[_index].itemBasic as CardItem;
            //兩種情況 1.Card 戰死 2.玩家收回 
            return _cardItem.GetCardGameObjectPrefab;
        }
        return null;
    }

    /// <summary>    /// <summary>    /// <summary>    /// <summary>
    /// Card Function    /// Card Function    /// Card Function    /// Card Function
    /// </summary>   /// </summary>   /// </summary>  /// </summary>

    public void UseItem()
    {
        //產生 or 合成 <- 材料
    }
    #endregion

    public bool NPC_CheckItem(int _amount, Item npc_Wantitem)
    {
        for (int i = 0; i < GetItemContainer.Length; i++)
        {
            if(npc_Wantitem == (Item)GetItemContainer[i].itemBasic)
            {
                if(GetItemContainer[i].amount >= _amount)
                {
                    //表示有這個item 
                    //Check amount
                    backpackDatabase.Get_DeleteBasicItem_InContainer_Slot(GetItemContainer, i, _amount);
                    //delete container's item
                    Debug.Log("have this item and amount is ok - mission manager the mission is ok");
                    return true;
                }
                else
                {
                    //Tip Shake Now item amount few mission need
                    Debug.Log("the mission need item amount fewable ");
                    return false;
                }
            }           
        }
                
        Debug.Log("now not have item need "+npc_Wantitem.name);
        return false;
    }
    public int NPC_CheckItem(Item npc_Wantitem)
    {
        for (int i = 0; i < GetItemContainer.Length; i++)
        {
            if (npc_Wantitem == (Item)GetItemContainer[i].itemBasic)
                return GetItemContainer[i].amount;
        }
        return 0;         
    }

    public void MoveItemFromContainer(BackpackSlot itemSlot1, BackpackSlot itemSlot2) => backpackDatabase.MoveItemFromContainer(itemSlot1, itemSlot2);
    public void ResetContainer(BackpackSlot[] _Container) => backpackDatabase.ResetContainer(_Container);

    //void OnApplicationQuit() =>  backpackDatebase.Container = new BackpackSlot[30];


    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, canTake_radius);
    }
}