using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GuButtonOneClick : GuButton
{
    public UnityEvent m_Event;

    protected override void Start()
    {
        base.Start();

        if(m_Event != null)
            StartCoroutine(WaitEvent());
        IEnumerator WaitEvent()
        {
            yield return new WaitUntil(() => isFullTrigger);
            m_Event.Invoke();
        }
    }
    public override void ReceivedGu()
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
    }
}
