using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using MoerusaGameManager;

public class NPCLookAtPlayer : MonoBehaviour
{
    /// <summary>
    /// Animation Rig
    /// </summary>
    [Header("Animation Rig")]
    [SerializeField, Range(10f, 90f)] private float isFindTargetAngle = 70f;
    [SerializeField] private float isFindTargetRange = 4f, ToTargetTime = 1.45f;
    [SerializeField] private Vector3 lookAtDirection = new Vector3(0, 0, 1);
    private Transform player;
    private RigBuilder m_RigBuilder;
    private Rig m_Head_Rig;
    private Transform rig_Target;
    private float lookAtToTargetValue = 0f;

    void Start()
    {
        player = GameManager.Instance_GameManager.GetPlayerManager.transform;

        m_RigBuilder = gameObject.GetComponent<RigBuilder>();
        if (m_RigBuilder != null)
        {
            m_Head_Rig = m_RigBuilder.layers[0].rig;
            rig_Target = m_Head_Rig.transform.GetChild(0).GetChild(0).transform;
        }

    }
    void Update()
    {
        if (m_RigBuilder != null)
        {
            //在視野角度內 增加 -> _itemBasic_Transform
            Vector3 _target = player.position - transform.position;
            float angle = Vector3.Angle(lookAtDirection, _target);
            bool isFindTarget = Physics.CheckSphere(transform.position, isFindTargetRange, LayerMask.GetMask("Player")) && angle <= isFindTargetAngle;
            if (!isFindTarget)
            {
                lookAtToTargetValue -= lookAtToTargetValue >= 0 ? Time.deltaTime : 0;
                //m_Head_Rig.weight = 0;
            }
            else
            {
                lookAtToTargetValue += lookAtToTargetValue <= ToTargetTime ? Time.deltaTime : 0;
                rig_Target.position = player.position + Vector3.up * 1.5f;
            }
            m_Head_Rig.weight = Mathf.Lerp(0, 1, lookAtToTargetValue / ToTargetTime);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, isFindTargetRange);
    }
}
