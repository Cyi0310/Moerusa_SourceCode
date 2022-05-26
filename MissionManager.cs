using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoerusaGameManager;

public class MissionManager : MonoBehaviour
{
    //MissionDatabase missionDatabase;

    private PlayerManager playerManager;
    private KeyCode useTouchNPCKeycode;

    public float talkRadius = 3.5f;
    public string NPCTag = "NPC";

    public LayerMask npc_LayerMask;
    
    public bool isTalk{ get; private set; }   //目前正在說話 ->不能動
    public bool checkHaveNPC { get; private set; }

    [Header("Audio")]
    public Sound[] sounds;

    void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();
        useTouchNPCKeycode = playerManager.GetPlayerKeyCode("Interact").keycode;
        //Sounds
        GameManager.Instance_GameManager.Audio_InitialSounds(sounds, gameObject);
    }

    void Update()
    {
        Collider[] NPCs = Physics.OverlapSphere(transform.position, talkRadius, npc_LayerMask);
        checkHaveNPC = false;
        for (int i = 0; i < NPCs.Length; i++)
        {
            if(NPCs[i].CompareTag(NPCTag))
            {
                checkHaveNPC = true;
                break;
            }
        }
        if (isTalk)
            return;

        if (checkHaveNPC) //In range NPC's outline well light
        {
            Transform nowTalkNPC = DecideBestDistance(NPCs).transform;
            NPC nowNPC = nowTalkNPC.GetComponent<NPC>();
            nowNPC.isOutlineEnable = true;

            if (Input.GetKeyDown(useTouchNPCKeycode))
            {
                //Sound
                playerManager.Audio_PlayAudio(sounds, "ClickNPC");

                nowNPC.TouchNPC(this, playerManager.GetBackpack);
            }
        }
        else   //all npc outline = false
            GameManager.Instance_GameManager.PlayerInsideHaveNPC();

        //playerManager.PlayerMoveNowType = isTalk ?  PlayerMoveType.DontMove: PlayerMoveType.CanMove;

        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, Mathf.Infinity) && hit.collider.CompareTag(NPCTag))
        //{
        //    //NPC npc = hit.collider.GetComponent<NPC>();
        //    //if (npc != null)
        //    //{
        //    //    npc.isOutlineEnable = true;
        //    //    ////另一個模式是碰到就觸發
        //    //    //if (Input.GetKeyDown(KeyCode.Mouse0))
        //    //    //{
        //    //    //    //Sound
        //    //    //    playerManager.Audio_PlayAudio(sounds, "ClickNPC");
        //    //    //    npc.TouchNPC(this, playerManager.GetBackpack);
        //    //    //}
        //    //}
        //}
        //else
        //    GameManager.Instance_GameManager.PlayerInsideHaveNPC();
    }

    Collider DecideBestDistance(Collider[] _index)
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
     
    public void SetIsTalk(bool _isTalk) => isTalk = _isTalk; //Because ienumerator dont use "="

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

    //右下角的「!」
    public void OnExclamationMark(bool isCondition) => playerManager.OnExclamationMark(isCondition);


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, talkRadius);
    }
}