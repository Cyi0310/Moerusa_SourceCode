using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NormalNPC : MonoBehaviour
{    
    private IEnumerator TextWordMove = null;
    
    public NormalTalk normalTalk;


    public Vector3 uiPosOffset;
    public ScreenDisplayUI npcDisplayUI;


    void Start()
    {       
        StartCoroutine(CheckTalk());
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (npcDisplayUI.display_Gameobject != null && npcDisplayUI.display_UI != null)
            npcDisplayUI.SetUIPosisition(uiPosOffset);
#endif
    }

    IEnumerator CheckTalk()
    {
        while (true) {
            if (normalTalk.CheckPlayer(transform))
            {
                normalTalk.displayTalk.SetActive(true);

                TextWordMove = TextAWordAWord((normalTalk.talkText));
                StartCoroutine(TextWordMove);

                yield return new WaitUntil(() => !normalTalk.CheckPlayer(transform));
                StopCoroutine(TextWordMove);
            }
            else            
                normalTalk.NotLookAtPlayer_NormalTalk();
            
            yield return null;
        }
    }
    IEnumerator TextAWordAWord(string _string)
    {
        string[] stringArray = _string.Split(new char[1] { ' ' });
        for (int i = 0; i < stringArray.Length && normalTalk.CheckPlayer(transform); i++)
        {
            normalTalk.text_displayTalk.text += stringArray[i];

            yield return new WaitForSeconds(normalTalk.talkWaitTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, normalTalk.lookAtDistance);


    }  
}

[System.Serializable]
public class NormalTalk  //For 講話用的Class
{
    public GameObject displayTalk;
    public Text text_displayTalk;

    [TextArea(5, 10)]
    public string talkText;

    public LayerMask playerLayerMask;  //PlayerLayerMask
    public float lookAtDistance = 5f, talkWaitTime = 0.5f;
    public void NotLookAtPlayer_NormalTalk()
    {
        displayTalk.SetActive(false);

        text_displayTalk.text = null;

    }
    public bool CheckPlayer(Transform _obj)
    {
        return Physics.CheckSphere(_obj.position, lookAtDistance, playerLayerMask);
    }
}