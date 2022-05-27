using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoerusaGameManager;

//此Script在Player身上
public class MissionManager : MonoBehaviour
{
    private PlayerManager playerManager;
    private KeyCode useTouchNPCKeycode;

    public GameObject E_Tip_Prefabs;
    private GameObject e_Tip;

    [Range(1f,5f)]
    public float talkRadius = 3.5f;                  //可以跟npc對話的距離
    
    public string NPCTag = "NPC";                    //Tag
    public LayerMask npc_LayerMask;

    
    public bool isTalking{ get; private set; }          //目前正在說話
    public bool checkHaveNPC {
        get 
        {
            Collider[] NPCs = Physics.OverlapSphere(transform.position, talkRadius, npc_LayerMask);
            for (int i = 0; i < NPCs.Length; i++)
            {
                if (NPCs[i].CompareTag(NPCTag))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Header("Audio")]
    public Sound[] sounds;

    void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();
        useTouchNPCKeycode = playerManager.GetPlayerKeyCode("Interact").keycode;
        //Sounds
        GameManager.Instance_GameManager.Audio_InitialSounds(sounds, gameObject);
        e_Tip = Instantiate(E_Tip_Prefabs, GameManager.Instance_GameManager.WorldSpaceCanvas.transform);       
    }

    void Update()
    {
        Collider[] NPCs = Physics.OverlapSphere(transform.position, talkRadius, npc_LayerMask);
        if (isTalking)
            return;

        if (checkHaveNPC) //In range NPC's outline well light
        {
            Transform nowTalkNPC = DecideBestDistance(NPCs).transform;
            NPC nowNPC = nowTalkNPC.GetComponent<NPC>();
            nowNPC.SetNPCIsCanTouchTip(true, e_Tip);
            if (Input.GetKeyDown(useTouchNPCKeycode))
            {
                //Sound
                playerManager.Audio_PlayAudio(sounds, "ClickNPC");

                nowNPC.TouchNPC(this, playerManager.GetBackpack);
            }
        }
        else   //all npc outline = false       
            GameManager.Instance_GameManager.PlayerInsideHaveNPC();
        
        e_Tip.SetActive(checkHaveNPC);
    }


    private void MouseCheckNPC() //使用滑鼠確認NPC與NPC互動
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.CompareTag(NPCTag))
        {
            NPC npc = hit.collider.GetComponent<NPC>();
            if (npc != null)
            {
                npc.isOutlineEnable = true;
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    //Sound
                    playerManager.Audio_PlayAudio(sounds, "ClickNPC");
                    npc.TouchNPC(this, playerManager.GetBackpack);
                }
            }
        }
        else
            GameManager.Instance_GameManager.PlayerInsideHaveNPC();
    }

    Collider DecideBestDistance(Collider[] _index) //決定離玩家最近的NPC
    {
        if (_index.Length <= 0)
            return _index[0];

        List<float> npc_Distance = new List<float>();
        for (int i = 0; i < _index.Length; i++)
        {
            Vector3 diration = _index[i].transform.position - transform.position;
            float distance = diration.magnitude;

            _index[i].GetComponent<NPC>().isOutlineEnable = false;
            npc_Distance.Add(distance);
        }

        for (int i = 0; i < npc_Distance.Count; i++)
        {
            if(npc_Distance[0] > npc_Distance[i])
            {
                CyiLibrary.Swap.SwapArray(_index, 0, i);
                CyiLibrary.Swap.SwapList(npc_Distance, 0, i);
            }
        }
        return _index[0];
    }

    public void SetMoveType(PlayerMoveType _playermovetype)
    {
        playerManager.NowPlayerMoveType = _playermovetype;
    }
     
    public void SetIsTalk(bool _isTalk) => isTalking = _isTalk; //Because ienumerator dont use "="

    public void SetPlayerDatabase_MContainer(Mission _mission)
    {        
        playerManager.playerDatabase.MContainerAdd(_mission);
    }

    public void PlayMissionAudio(MissionState _missionState)
    {
        if (_missionState == MissionState.Ready)
            playerManager.Audio_PlayAudio(sounds, "GetMission");
        else if(_missionState == MissionState.Success)
            playerManager.Audio_PlayAudio(sounds, "SuccessMission");
    }

    //右下角的「!」提示玩家現在有新任務
    public void OnExclamationMark(bool isCondition) => playerManager.OnExclamationMark(isCondition);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, talkRadius);
    }
}
