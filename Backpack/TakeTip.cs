using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeTip : MonoBehaviour
{    
    private Queue<GameObject> queue_ObjectTip = new Queue<GameObject>();
    private Queue<GameObject> queue_ObjectTipRegister = new Queue<GameObject>();  //把最後一個強制取消顯示 拉回來 
    private Queue<ItemBasic> queue_ItemTip = new Queue<ItemBasic>();
    private Queue<IEnumerator> queue_WaitTip = new Queue<IEnumerator>();

    [Range(1,10)] 
    public int tipAmount = 5;
    [Range(0.1f, 5f)] 
    public float howLongDestroyTip = 3f; // != 負數

    public GameObject prefab_Tip;    

    void Start()
    {
        for (int i = 0; i < tipAmount; i++)
        {
            GameObject _tip = Instantiate(prefab_Tip, transform.position, Quaternion.identity , transform);
            _tip.SetActive(false);
            queue_ObjectTip.Enqueue(_tip);
        }
    }

    public void CallTip(ItemBasic _itemBasic,bool _isFull)
    {
        GameObject _Tip = CallTipQueueCalculate();
        _Tip.transform.SetSiblingIndex(tipAmount);

        Debug.Log(_itemBasic.GetType());

        queue_ItemTip.Enqueue(_itemBasic);
        ItemBasic _itemTip = queue_ItemTip.Dequeue();

        _Tip.transform.GetChild(0).GetComponentInChildren<Image>().sprite = _itemTip.UiDisplay;
        
        string _tipText = _itemTip.MYName.ToString();
        if (_isFull)
        {
            if (_itemBasic is CardItem)            
                _tipText = "卡片欄位已滿";            
            else if(_itemBasic is Item)            
                _tipText = "道具欄位已滿";            
        }
        _Tip.GetComponentInChildren<Text>().text = _tipText;

        _Tip.transform.GetComponentInChildren<Image>().color = !_isFull ? Color.white : Color.red;
        //for (int i = 0; i < _Tip.transform.childCount; i++)
        _Tip.transform.GetComponent<Image>().color = !_isFull ? Color.white : Color.red;


        IEnumerator _waitTip = QueueWaitDestroy();
        StartCoroutine(_waitTip);
        queue_WaitTip.Enqueue(_waitTip);
    }

    GameObject CallTipQueueCalculate()
    {
        if(queue_ObjectTip.Count <= 0)
        {
            StopCoroutine(queue_WaitTip.Dequeue());

            GameObject _TheQueueObject = queue_ObjectTipRegister.Dequeue();
            queue_ObjectTip.Enqueue(_TheQueueObject);
            _TheQueueObject.SetActive(false);
        }

        GameObject _queueObject = queue_ObjectTip.Dequeue();

        queue_ObjectTipRegister.Enqueue(_queueObject);

        _queueObject.transform.position = transform.position;
        _queueObject.transform.rotation = transform.rotation;                     
        _queueObject.SetActive(true);

        return _queueObject;
    }

    IEnumerator QueueWaitDestroy()
    {
        yield return new WaitForSeconds(howLongDestroyTip);

        queue_WaitTip.Dequeue();
        GameObject _queueObject = queue_ObjectTipRegister.Dequeue();
        queue_ObjectTip.Enqueue(_queueObject);
        _queueObject.SetActive(false);
    }
}