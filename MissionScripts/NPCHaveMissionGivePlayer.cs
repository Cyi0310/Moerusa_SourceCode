using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoerusaGameManager;
public class NPCHaveMissionGivePlayer : MonoBehaviour
{
    [Header("NPC HaveMissionGivePlayer")]
    public Image npcHaveMissionGivPlayer_Prefab;
    public Vector3 npcHaveMissionGivPlayer_Offset;
    private Vector3 Pos_NPCHaveMissionGivPlayer_Offset { get => transform.position + npcHaveMissionGivPlayer_Offset; }
    private Image npcHaveMissionGivPlayer;

    private GameManager gameManager;

    private NPC m_NPC;
    void Start()
    {
        gameManager = GameManager.Instance_GameManager;
        m_NPC = gameObject.GetComponent<NPC>();

        GameObject _npcHaveMissionGivPlayer = Instantiate(npcHaveMissionGivPlayer_Prefab.gameObject, gameManager.WorldSpaceCanvas.transform);
        npcHaveMissionGivPlayer = _npcHaveMissionGivPlayer.GetComponent<Image>();

        npcHaveMissionGivPlayer.transform.position = Pos_NPCHaveMissionGivPlayer_Offset;
    }

    void Update()
    {        
        npcHaveMissionGivPlayer.enabled = m_NPC.IsCheckNowMissionHaveReady;
        npcHaveMissionGivPlayer.transform.position = Pos_NPCHaveMissionGivPlayer_Offset;
    }
    protected void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawSphere(Pos_NPCHaveMissionGivPlayer_Offset, 0.5f);
    }

}
