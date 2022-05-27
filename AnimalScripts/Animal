using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour
{
    protected NavMeshAgent m_Agent;

    protected IEnumerator IE_StartAttack;

    [SerializeField] protected Material hurt_Material;

    [SerializeField, ColorUsage(true, true)] protected Color hurt1_Color, hurt2_Color, hurt3_Color;
    protected Material normal_Material;
    protected Renderer meshRender;

    [Header("Animal Setting")]
    [SerializeField] protected Transform targetAtk;  //目前要攻擊哪一隻
    [SerializeField] protected float nowHealth = 100f, maxHealth = 100f;
    [SerializeField] protected float lookForTargetAngle = 30f, moveSpeed = 5f, rotSpeed = 0.02f;
    [SerializeField] protected LayerMask targetLayer;
    [Header("Range")]
    [SerializeField] protected float atk_Radius = 1.5f;
    [SerializeField] protected float toNearRange = 2f, findRange = 7f; 

    [Header("Attack Type")]
    [SerializeField] protected Attack_OR_SkillType attackType;
    [SerializeField] protected float attackDamage = 5f;

    [Header("Skill Type")]
    [SerializeField] protected Attack_OR_SkillType skillType;
    [SerializeField] protected float skillDamage = 10f;
    
    [Space(10)]
    [SerializeField] protected float BeHitCD = 0.5f;
    [SerializeField] protected float atkTime = 2.5f;

    protected Vector3 m_Direction = Vector3.zero; //For animator MoveSpeed...
    protected bool canBeHit = true;               //讓 hit 不要連續 hit
    public bool isAttack { get; protected set; }

    public float GetNowHealth { get => nowHealth / maxHealth; }
    public bool isDie { get => nowHealth <= 0f; }

    protected virtual void Start()
    {
        m_Agent = gameObject.GetComponent<NavMeshAgent>();
    }

    protected virtual void MoveToTarget(Vector3 _target)
    {
        _target.y = 0;

        Vector3 targetDirection = _target - transform.position;
        targetDirection.y = 0;
        Quaternion lookDir = Quaternion.LookRotation(targetDirection);
        Quaternion targetRot = Quaternion.Slerp(transform.rotation, lookDir, rotSpeed * 5f);
        transform.rotation = targetRot;

        if (Vector3.Distance(transform.position, _target) < toNearRange) //ToNear        
            return;

        m_Direction = Vector3.MoveTowards(transform.position, _target, moveSpeed * Time.deltaTime);
        transform.position = m_Direction;
        m_Direction.Normalize();
    }

    protected virtual Transform CheckNowTarget()
    {
        Collider[] _target = Physics.OverlapSphere(transform.position, findRange, targetLayer);
        if (_target == null || _target.Length <= 0)
            return null;

        List<Transform> nowTargets = new List<Transform>(_target.Length);
        List<float> Distances = new List<float>();
        for (int i = 0; i < _target.Length; i++)
        {
            nowTargets.Add(_target[i].transform);
            Vector3 direction = nowTargets[i].position - transform.position;
            float Distance = direction.magnitude;
            Distances.Add(Distance);
        }

        if (Distances.Count > 1)
        {
            for (int i = 1; i < Distances.Count; i++)
            {
                if (Distances[0] > Distances[i]) //把離玩家最近的 丟給Target[0]
                {
                    CyiLibrary.Swap.SwapList<float>(Distances, 0, i);
                    CyiLibrary.Swap.SwapList<Transform>(nowTargets, 0, i);
                }
            }
        }
        return nowTargets[0].transform;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, findRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, toNearRange);
    }
}
