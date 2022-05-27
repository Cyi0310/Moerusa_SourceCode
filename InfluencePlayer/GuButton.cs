using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GuButton : MonoBehaviour, IReceivedGu
{
    public bool isToManagerManagement = false;

    public float changeTime  = 1.5f;
    //Array
    public Transform[] eventObjects;
    public Vector3[] SetFinalPosies;

    public Quaternion[] SetFinalRoties;

    protected float _time;
    protected Vector3[] basicPosies;  //當沒有按下時 回去這個位置
    protected Vector3[] finalPosies;  //當按下時 去這位置

    protected Quaternion[] basicRoties;
    protected Quaternion[] finalRoties;

    protected IEnumerator IE_checkExit, IE_backPos, IE_ScaleAnimation, IE_BackScaleAnimation;

    protected Vector3 selfBasicScale;
    protected Vector3 selfFinalScale;
    public bool isTrigger { get; private set; }
    public bool isFullTrigger { get => _time == changeTime; }

    protected float _ScaleTime = 0f;
    protected virtual void Start()
    {
        int _eventObjects_Index = eventObjects.Length;
        basicPosies = new Vector3[_eventObjects_Index];
        finalPosies = new Vector3[_eventObjects_Index];
        
        basicRoties = new Quaternion[_eventObjects_Index];
        finalRoties = new Quaternion[_eventObjects_Index];

        selfBasicScale = transform.localScale;
        selfFinalScale = transform.localScale - (Vector3.up * (transform.localScale.y / 2));

        for (int i = 0; i < _eventObjects_Index; i++)
        {
            basicPosies[i] = eventObjects[i].position;
            finalPosies[i] = eventObjects[i].position + SetFinalPosies[i];

            basicRoties[i] = eventObjects[i].rotation;
            finalRoties[i] = eventObjects[i].rotation * SetFinalRoties[i];
        }                
    }

    public virtual void ReceivedGu()
    {
        //Button move
        if (IE_ScaleAnimation == null)
        {
            IE_ScaleAnimation = ScaleAnimation(_ScaleTime < 1f, Time.deltaTime);
            StartCoroutine(IE_ScaleAnimation);
            SetTrigger(true);
        }
        if (IE_BackScaleAnimation != null)
            StopCoroutine(IE_BackScaleAnimation);

        if (_time <= changeTime)
        {
            _time += Time.deltaTime;

            _time = Mathf.Clamp(_time, 0, changeTime);
            //Move and Rot
            for (int i = 0; i < eventObjects.Length && !isToManagerManagement; i++)
            {
                eventObjects[i].position = Vector3.Lerp(basicPosies[i], finalPosies[i], _time / changeTime); //dir * Time.deltaTime;                    
                eventObjects[i].rotation = Quaternion.Lerp(basicRoties[i], finalRoties[i], _time / changeTime);
            }
        }

        //First stop exit coroutine
        if (IE_checkExit != null)
            StopCoroutine(IE_checkExit);
            
        //Check Exit
        IE_checkExit = CheckExit();
        StartCoroutine(IE_checkExit);
    }

    protected IEnumerator CheckExit()
    {
        //先 STOP
        if (IE_backPos != null)
            StopCoroutine(IE_backPos);

        float _nowTime = _time;

        yield return null;

        //Exit
        //(ReceivedGu) Because direction will change another , if not ReceivedGu not use and  direction will not change

        if (_nowTime == _time)
        {
            //Button Back BasicPos
            if(IE_ScaleAnimation != null)
                StopCoroutine(IE_ScaleAnimation);
            IE_ScaleAnimation = null;

            IE_BackScaleAnimation = ScaleAnimation(_ScaleTime > 0f, -Time.deltaTime);
            StartCoroutine(IE_BackScaleAnimation);
            SetTrigger(false);

            IE_backPos = BackBasicPos();
            StartCoroutine(IE_backPos);
        }
    }
    
    protected virtual IEnumerator BackBasicPos() //回到原本位置
    {
        while (_time > 0)
        {
            _time -= Time.deltaTime;
            _time = Mathf.Clamp(_time, 0, changeTime);            
            for (int i = 0; i < eventObjects.Length && !isToManagerManagement; i++)
            {
                eventObjects[i].position = Vector3.Lerp(basicPosies[i], finalPosies[i], _time / changeTime);// dir * Time.deltaTime;
                eventObjects[i].rotation = Quaternion.Lerp(basicRoties[i], finalRoties[i], _time / changeTime);
            }
            yield return null;
        }
    }

    public void SetTrigger(bool _isTrigger)
    {
        isTrigger = _isTrigger;
    }
    protected IEnumerator ScaleAnimation(bool _ScaleCondition, float _ScaleTme)
    {
        while (_ScaleCondition)
        {
            transform.localScale = Vector3.Lerp(selfBasicScale, selfFinalScale, _ScaleTime);
            _ScaleTime += _ScaleTme;
            _ScaleTime = Mathf.Clamp01(_ScaleTime);
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < eventObjects.Length && SetFinalPosies.Length != 0 && SetFinalRoties.Length != 0; i++)
        {
            Gizmos.color = new Color(1, 0.92f, 0.6f, 0.75f);
            Gizmos.DrawSphere(eventObjects[i].position + SetFinalPosies[i], 1f);
        }
    }
}
