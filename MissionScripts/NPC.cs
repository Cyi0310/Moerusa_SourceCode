using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoerusaGameManager;
using UnityEngine.UI;
public class NPC : MonoBehaviour
{   
    [SerializeField] protected NPCDatabase npcDatabase; //NPC的Database
    public Touch[] touchWhats;
    public bool isOutlineEnable = false;

    private Outline NPCOutline;

    protected GameManager gameManager;
    protected Dialogue dialogue; 
    //Need to database
    protected DialogueVariable[] dialoguesVariable = new DialogueVariable[0];

    // Use Database remaber this. Be solution mission type differ .Maybe can use ArrayList or List<object>  
    protected Mission[] missions = new Mission[0];                              

    [Header("CanTouchNPCOffset")]
    public Vector3 CanTouchNPCOffset = Vector3.zero;
    protected Vector3 CanTouchNPCPos { get => transform.position + CanTouchNPCOffset; }


    public bool IsCheckNowMissionHaveReady
    {
        get
        {
            for (int i = 0; i < missions.Length; i++)
            {
                if (missions[i].missionState == MissionState.Ready)
                    return true;
                else if(missions[i].missionState == MissionState.Running)
                    return false;
                Debug.Log(missions[i].missionState + "      " + MissionState.Ready);
            }
            return false;
        }
    }

    protected virtual void Awake()
    {
        Outline _Outline = gameObject.GetComponent<Outline>();
        if (_Outline != null)
            NPCOutline = _Outline;

        dialogue = FindObjectOfType<Dialogue>();
        isOutlineEnable = false;

        //把兩種 Mission 丟到 "missions"
        npcDatabase.MissionTypeTO_Missions(ref missions, ref dialoguesVariable, touchWhats, this);
    }

    protected virtual void Start()
    {
        gameManager = GameManager.Instance_GameManager;

        //Set Touch of inside NPC
        for (int i = 0; i < touchWhats.Length; i++)
            touchWhats[i].SetNPC(this);

    }
    protected virtual void Update()
    {
        if (NPCOutline != null)
            NPCOutline.enabled = isOutlineEnable;
    }

    public void SetNPCIsCanTouchTip(bool _condition,GameObject _E_Tip)
    {
        isOutlineEnable = _condition;
        NPCOutline.enabled = isOutlineEnable;

        if(_E_Tip != null)            
            _E_Tip.SetActive(isOutlineEnable);

        if (!isOutlineEnable)
            return;            
        
        _E_Tip.transform.position = CanTouchNPCPos;
    }

    public TouchType GetTouchTypeInTheScriptobject(Touch _touch) => npcDatabase.GetTouchTypeInTheScriptobject(_touch);

    public void UpdateNPCDatabaseMission(Touch _touch)
    {
        npcDatabase.UpdateNPCDatabaseMission(_touch);
    }

    #region Mission Function
    public void TouchNPC(MissionManager _missionManager, Backpack playerBackpack)
    {
        int checkMissionAmount = 0;
        //Everyone Missions Check ID 0 ~ x  if is Ready -> trigger to running...
        for (int i = 0; i < missions.Length ; i++)
        {
            checkMissionAmount++;

            if (missions[i].missionState == MissionState.Ready)
            {                                
                missions[i].missionState = MissionState.Running;
                
                _missionManager.SetPlayerDatabase_MContainer(missions[i]);

                //Set The Object Can Touch
                if (missions[i].GetType() == typeof(TouchMission))
                {
                    TouchMission _touchMission = (TouchMission)missions[i];
                    for (int j = 0; j < _touchMission.touchWhatValues.Length; j++)
                    {
                        if(_touchMission.whereScene == WhereScene.Altar || _touchMission.whereScene == WhereScene.Color)
                            _touchMission.touchWhatValues[j].touchWhat.SetTouchType(TouchType.CanTouch);
                        npcDatabase.UpdateNPCDatabaseMission(touchWhats);
                    }
                }

                ExecutionDialogue(_missionManager, missions[i], MissionState.Ready);

                //"紀錄" 出現資料
                //

                //右下角出現「!」
                _missionManager.OnExclamationMark(true);

                Debug.Log("The mission is Ready --> Running");
                break;
            }
            else if (missions[i].missionState == MissionState.Running)
            {               
                //Check is the mission condition
                if(CheckMissionIsSuccess(i, playerBackpack))
                {
                    missions[i].missionState = MissionState.Success;

                    _missionManager.SetPlayerDatabase_MContainer(missions[i]);

                    //Get item OR card OR MainLineText

                    ExecutionDialogue(_missionManager, missions[i], MissionState.Success);

                    Debug.Log("The mission is Running --> Success ");
                }
                else
                {
                    ExecutionDialogue(_missionManager, missions[i], MissionState.Running);
                    Debug.Log("The mission is Running ");
                    //little talk display;
                }                
                break;
            }
            else if (missions[i].missionState == MissionState.Success)
            {
                Debug.Log("mission is success   :" + missions[i].missionName);
                //see Next Missions
            }
        }

        if (checkMissionAmount == missions.Length)
        {
            Debug.Log("This NPC's missions all success or NPC not mission " + checkMissionAmount + " = " +missions.Length);
            return;
        }

    }
    protected bool CheckMissionIsSuccess(int _MissionIndex, Backpack _playerBackpack)
    {
        if (missions[_MissionIndex].GetType() == typeof(TouchMission))// check this human is touch;
        {
            TouchMission _touchMission = (TouchMission)missions[_MissionIndex];
            for (int j = 0; j < _touchMission.touchWhatValues.Length; j++)
            {
                bool isTouch = _touchMission.touchWhatValues[j].touchType == TouchType.IsTouch;
                if (!isTouch) //有一個 或以上 沒碰到
                {
                    Debug.Log(_touchMission.touchWhatValues[j].touchType + "  is touch(true) ? can't run to success  ");            
                    return false;
                }
            }

            //全碰到
            return true;
        }
        else if (missions[_MissionIndex].GetType() == typeof(TakeItemMission))
        {
            // check this item is get
            TakeItemMission _takeItemMission = (TakeItemMission)missions[_MissionIndex];
            Debug.Log("check backpack item     ,_takeItemMission and amount 484 == backpackitem");
            for (int i = 0; i < _takeItemMission.missionItem.Length; i++)
            {
                if (!_playerBackpack.NPC_CheckItem(_takeItemMission.missionItem[i].itemAmount, _takeItemMission.missionItem[i].needItem))
                    return false;                
            }
            return true;
        }
        return false;
    }
    protected bool CheckMissionIsSuccess(int _i)  //Now for AltarNPC
    {
        if (missions[_i].GetType() == typeof(TouchMission))// check this human is touch;
        {
            TouchMission _touchMission = (TouchMission)missions[_i];
            for (int j = 0; j < _touchMission.touchWhatValues.Length; j++)
            {
                bool isTouch = _touchMission.touchWhatValues[j].touchType == TouchType.IsTouch;//  touchWhat.CheckThisObjectIsTouch;
                if (!isTouch) //有一個 或以上 沒碰到
                {
                    Debug.Log(_touchMission.touchWhatValues[j].touchType + "  is touch(true) ? can't run to success  ");
                    return false;
                }
            }
            return true; //全碰到
        }
        return false;
    }
    public bool CheckTheMissionIsSuccess(int _MissionIndex) => missions[_MissionIndex].missionState == MissionState.Success;

    protected virtual void ExecutionDialogue(MissionManager _missionManager ,Mission _mission, MissionState _missionState)
    {
        int _missionID = _mission.missionID;
        string _missionName = _mission.missionName;

        _missionManager.SetIsTalk(false);
        for (int j = 0; j < dialoguesVariable.Length; j++)
        {
            if (!dialoguesVariable[j].isDialogued && dialoguesVariable[j].DialogueID == _missionID && dialoguesVariable[j].MissionState == _missionState)
            {
                _missionManager.SetIsTalk(true);

                //是Running則為False 可以繼續Dialogue ,其他則為True,因為對話不能夠重複
                dialoguesVariable[j].isDialogued = _missionState != MissionState.Running;

                //Wait dialogue
                IEnumerator _WaitTimeDialogue = null;
                _WaitTimeDialogue = WaitTimeDialogue(_missionManager, 2f, dialoguesVariable[j], _missionState, _missionName);
                StartCoroutine(_WaitTimeDialogue);
                //Skip dialogue
                IEnumerator _SkipDialogue = null;
                _SkipDialogue = SkipDialogue();
                StartCoroutine(_SkipDialogue);

                IEnumerator SkipDialogue()
                {
                    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F12) || _WaitTimeDialogue == null);
                    StopCoroutine(_WaitTimeDialogue);
                    dialogue.SetDialogue(false, null, null, null, null, null, DialogueTalker.None);
                    MissionTitleAppear(_missionState, _missionManager, _missionName);
                }

                break;
            }
        }

        if (dialoguesVariable.Length <= 0) //當沒有對話時
            MissionTitleAppear(_missionState, _missionManager, _missionName);
    }
    IEnumerator WaitTimeDialogue(MissionManager _missionManager, float _waitTime, DialogueVariable _dialogueVariable, MissionState _missionState, string _missionName)
    {
        //for loop when dialogue result --> for loop end
        for (int k = 0; k < _dialogueVariable.DialogueValues.Length; k++)
        {
            //Talk
            DialogueValue _DialogueValue = _dialogueVariable.DialogueValues[k];
            dialogue.SetDialogue(true, _dialogueVariable.L_Talker, _dialogueVariable.R_Talker,
                _DialogueValue.MainText, _DialogueValue.L_Sprite, _DialogueValue.R_Sprite,
                _DialogueValue.dialogueTalker);

            _missionManager.SetMoveType(PlayerMoveType.DontMove);

            //wait
            yield return new WaitForSeconds(_waitTime / 2);

            yield return new WaitUntil(() => !dialogue.isTalking);
            IEnumerator dialogue_LMouse = dialogue.LMouse_display(true);
            StartCoroutine(dialogue_LMouse);
            Debug.Log("出現 點擊....");
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));
            StopCoroutine(dialogue_LMouse);
            dialogue_LMouse = null;
            Debug.Log("消失 點擊....");
        }
        dialogue.SetDialogue(false, null, null, null, null, null, DialogueTalker.None);
        MissionTitleAppear(_missionState, _missionManager, _missionName);       
    }

    protected void MissionTitleAppear(MissionState _missionState, MissionManager _missionManager, string _missionName)
    {
        dialogue.MissionDisplayTitle(_missionState, _missionName);

        _missionManager.PlayMissionAudio(_missionState);
        _missionManager.SetMoveType(PlayerMoveType.CanMove);
        _missionManager.SetIsTalk(false);
    }

    #endregion

    protected virtual void OnDrawGizmosSelected()
    { 
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(CanTouchNPCPos, 0.25f);
    }
}