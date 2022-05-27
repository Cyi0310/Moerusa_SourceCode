using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using MoerusaGameManager;
public class CardObject : Animal, IDamage
{
    [Header("Card")]
    public AnimalStateType cardStateType; //CARD
    public GameObject atkObj;        
    [SerializeField]
    protected bool isNeedTeleportToPlayerAround;
    [SerializeField]
    protected float needBackToPlayerAroundTime = 5f, forPlayerDistance;

    protected Transform player;
    protected NavMeshAgent navMeshAgent;

    [Header("Effect")]
    public EffectObject[] atkEffects;
    public EffectObjectBasic smokeEffect;
    protected GameObject smokeEffect_Obj;
    protected ParticleSystem smokeEffect_ParticleSystem;

    [Header("Sound")]
    public Sound[] sounds;
    public Sound dieSound;

    public float MaxHealth { get => maxHealth; }    //For CardDatabase , save Health 
    public float NowHealth { get => nowHealth; set => nowHealth = value; } //For CardDatabase , save Health    


    protected override void Start()
    {
        base.Start();

        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Debug.Log(" Start " + nowHealth + " max " +maxHealth);

        meshRender = gameObject.GetComponentInChildren<MeshRenderer>();
        normal_Material = meshRender.material;

        cardStateType = AnimalStateType.Active;

        //Effect & Sound
        smokeEffect_Obj = Instantiate(smokeEffect.PrefabEffect, transform.position, transform.rotation, transform);
        smokeEffect_ParticleSystem = smokeEffect_Obj.GetComponent<ParticleSystem>();
        smokeEffect_ParticleSystem.Stop();
        GameManager.Instance_GameManager.Audio_InitialSounds(sounds, gameObject);

        StartCoroutine(BackToPlayerAround());
    }

    void Update()
    {
        if (isDie)
        {
            Die();
            return;
        }

        if (isAttack)
            return;
            
        if (isNeedTeleportToPlayerAround) //需要回到玩家身邊
        {
            //smokeEffect_ParticleSystem.Play();
            MoveToTarget(player.position);
            return;
        }

        if (CheckNowTarget() != null) //Find Enemy
        {
            MoveToTarget(CheckNowTarget().position);
            Debug.Log("GO MONSTER");
            if (CheckAttackTarget())
                Attack();

            if (Vector3.Distance(player.position, transform.position) > forPlayerDistance)
            {
                MoveToTarget(player.position);                
                StartCoroutine(NeedTeleportToPlayerAround());
            }                
        }
        else
        {
            if (Vector3.Distance(player.position, transform.position) > toNearRange)
                MoveToTarget(player.position);
            if (Vector3.Distance(player.position, transform.position) > forPlayerDistance)
            {
                StartCoroutine(NeedTeleportToPlayerAround());
            }
            //else
            //{
            //    //if(IE_NeedTPToPlayerAround != null)
            //    //    StopCoroutine(IE_NeedTPToPlayerAround);
            //}
        }            

        //navMeshAgent.SetDestination(_player);


        //找怪 沒找到 -> 玩家 ， 找到 -> 怪 ;找到但離玩家太遠 ->玩家            

        switch (cardStateType)
        {
            case AnimalStateType.Active:

                break;
            case AnimalStateType.Passive:
                
                break;
            case AnimalStateType.Await:
                
                break;
            default:
                break;
        }
    }

    protected virtual bool CheckAttackTarget()
    {
        if (Vector3.Distance(CheckNowTarget().position, transform.position) < toNearRange)
        {
            Vector3 direction = CheckNowTarget().position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            return lookForTargetAngle > angle;
        }
        return false;
    }

    protected virtual void PreAttack(IEnumerator StartAttack)
    {
        isAttack = true;

        IE_StartAttack = StartAttack;
        StartCoroutine(IE_StartAttack);
    }

    protected virtual void Attack()
    {
        PreAttack(StartAttack());

        IEnumerator StartAttack()
        {
            yield return new WaitForSeconds(atkTime);
            float _time = 0f;
            while (_time < atkTime)
            {
                Collider[] targets = Physics.OverlapSphere(atkObj.transform.position, atk_Radius, targetLayer);
                foreach (var _target in targets)
                {
                    IDamage the_Target = _target.GetComponent<IDamage>();
                    if (the_Target != null)
                        the_Target.BeDamage(attackDamage, transform, Vector3.zero, null);
                }
                atkObj.SetActive(true);

                _time += Time.deltaTime;
                yield return null;
            }
            atkObj.SetActive(false);
            isAttack = false;
        }
    }

    public bool BeDamage(float _damage,Transform _target, Vector3 _hitPos, EffectObjectBasic _hitEffect)
    {
        if (!canBeHit)
            return false;

        //Sound
        int hitIndex = Random.Range(0, 3);
        GameManager.Instance_GameManager.Audio_PlayAudio(sounds, "BeHit" + hitIndex);

        Vector3 direction = transform.position - _target.position;
        direction.y = 0;
        transform.Translate(direction * _damage * Time.deltaTime);

        nowHealth -= _damage;
        print("CardObject Damage");

        StartCoroutine(WaitBeHitCD(BeHitCD)); // X 秒 之後 才會計算 被攻擊        
        IEnumerator WaitBeHitCD(float _time)
        {
            meshRender.material = hurt_Material;
            //Color Change
            StartCoroutine(ColorChange());
            IEnumerator ColorChange()
            {
                meshRender.material.SetColor("_EmissionColor", hurt1_Color);
                yield return null;
                yield return null;
                meshRender.material.SetColor("_EmissionColor", hurt2_Color);
                yield return null;

                meshRender.material.SetColor("_EmissionColor", hurt3_Color);
                meshRender.material.color = Color.black;
                yield return null;
                yield return null;

                meshRender.material.color = Color.white;
                meshRender.material.SetColor("_EmissionColor", hurt1_Color);
            }
            canBeHit = false;
            yield return new WaitForSeconds(_time);
            canBeHit = true;

            meshRender.material = normal_Material;
        }
        return true;
    }
    IEnumerator NeedTeleportToPlayerAround()
    {
        isNeedTeleportToPlayerAround = true;
        float _time = 0f;
        while(_time < needBackToPlayerAroundTime)
        {
            if (Vector3.Distance(player.position, transform.position) < forPlayerDistance)
            {
                isNeedTeleportToPlayerAround = false;
                yield break;
            }

            _time += Time.deltaTime;
            yield return null;
        }
        Debug.Log("!卡片 因為距離玩家太遠 瞬間移動");
        isNeedTeleportToPlayerAround = false;
        
        transform.position = player.transform.position + Vector3.up *2.5f;
        smokeEffect_ParticleSystem.Play();
        GameManager.Instance_GameManager.Audio_PlayAudio(sounds, "TPToPlayer");

        StartCoroutine(BackToPlayerAround());
    }
    IEnumerator BackToPlayerAround()
    {
        navMeshAgent.enabled = false;
        yield return new WaitForSeconds(1f);

        navMeshAgent.enabled = true;
    }


    public void SetHealth(float _health) {

        nowHealth = _health;
    }  

    public void Die()
    {
        smokeEffect_Obj.transform.parent = null;
        smokeEffect_ParticleSystem.Play();
        Destroy(smokeEffect_Obj, smokeEffect.EffectDestroyTime);
        GameManager.Instance_GameManager.Audio_InitialSound(dieSound, smokeEffect_Obj); //因為怪物死亡後會消失 音效播不到 所以音效放在特效上
        dieSound.source.Play();

        Destroy(gameObject);
        Debug.Log("Die");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, forPlayerDistance);

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(atkObj.transform.position, atk_Radius);
    }

}
