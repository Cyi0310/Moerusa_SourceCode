using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MoerusaGameManager;

[RequireComponent(typeof(MonsterMenu))]
public class Monster : Animal, IDamage
{
    [Header("Monster")]
    public AnimalStateType MonsterStateType;  // <-- 主動 or 被動 or 原地 //may give

    public bool isPrepareAttack; //Give MonsterManager Set

    public GameObject atkObj;
    [Header("Monster Drops"), SerializeField]
    protected DropsItemBasicRNG[] dropsItemBasicRNG = new DropsItemBasicRNG[0];//掉落物品

    [Header("Effect"), SerializeField]
    protected EffectObjectBasic smokeEffect;

    [Header("Sound"), SerializeField]
    protected Sound[] sounds;

    protected IEnumerator IE_Retreat;   //BackToBasicPos

    protected MonsterMenu monsterMenu;

    protected Vector3 basicPos; //If Run to far, the pos let monster back to basic pos.
    protected Vector3 atkDirection;
    protected Collider m_collider;

    //float _resetTime = 0f; //如果有回血機制 <- 

    //Find Targer
    public bool isFindTarget { get => (Vector3.Distance(targetAtk.position, transform.position) < findRange) && !isAttack; }
    public bool isRetreat { get; private set; }

    protected override void Start()
    {
        base.Start();

        monsterMenu = gameObject.GetComponent<MonsterMenu>();

        meshRender = gameObject.GetComponentInChildren<Renderer>();
        normal_Material = meshRender.material;

        targetAtk = GameObject.FindGameObjectWithTag("Player").transform;
        basicPos = gameObject.transform.position;

        m_collider = gameObject.GetComponent<Collider>();

        GameManager.Instance_GameManager.Audio_InitialSounds(sounds, gameObject);
    }
    protected virtual void Update()
    {
        if (isDie)
        {
            Die();
            return;
        }
        UpdateHealth();

        //Animator

        //SetMonsterStateType();

        if (isAttack || !canBeHit)
        {
            m_Direction = Vector3.zero;
            return;
        }
        
        if (Vector3.Distance(targetAtk.position, transform.position) < findRange)
        {
            //LootAt Smooth

            Vector3 targetDirection = targetAtk.position - transform.position;
            targetDirection.y = 0;
            Quaternion lookDir = Quaternion.LookRotation(targetDirection);
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, lookDir, rotSpeed);

            monsterMenu.SetFindUI(true, isAttack);

            //Near Attack Target
            if (isPrepareAttack)
            {
                targetRot = Quaternion.Slerp(transform.rotation, lookDir, rotSpeed * 5);

                if (IE_Retreat != null)
                {
                    StopCoroutine(IE_Retreat);
                    isRetreat = false;                       
                }                    
                if (Vector3.Distance(targetAtk.position, transform.position) < toNearRange)
                {
                    Vector3 direction = targetAtk.position - transform.position;
                    float angle = Vector3.Angle(direction, transform.forward);
                    if (lookForTargetAngle > angle)
                    {
                        //atkObj.transform.position = atkBasicPos;
                        switch (attackType)
                        {
                            case Attack_OR_SkillType.Near:
                                NearAttack(); //Attack

                                break;
                            case Attack_OR_SkillType.Far:
                                //transform.rotation = targetRot;
                                FarAttack();
                                break;
                            default:
                                break;
                        }

                        monsterMenu.SetFindUI(true, isAttack);

                    }
                }
                m_Direction = Vector3.MoveTowards(transform.position, targetAtk.position, moveSpeed * Time.deltaTime);
                transform.position = m_Direction;
                m_Direction.Normalize();
            }
            else
            {
                if (!isRetreat)
                {
                    IE_Retreat = Retreat();
                    StartCoroutine(IE_Retreat);
                }
            }

            transform.rotation = targetRot;

            //MoveToTarget
            ////Near Attack Target

        }
        else
        {
            if (IE_Retreat != null)
            {
                StopCoroutine(IE_Retreat);
                isRetreat = false;
            }

            if (Vector3.Distance(transform.position, basicPos) > 3f)
            {
                MoveToTarget(basicPos);               
            }
            //亂走
            m_Direction = Vector3.zero;
            monsterMenu.SetFindUI(false, isAttack);            
        }

        switch (MonsterStateType)
        {
            case AnimalStateType.Active:
                //主動攻擊 Target -- 進到範圍內就跑過去...
                break;
            case AnimalStateType.Passive:
                //被動攻擊Target -- 被攻擊才會跑過去...

                break;
            default:
                break;
        }

        //if (health < maxHealth)
        //{
        //    _resetTime += Time.deltaTime;
        //    if (_resetTime > 2.5f)
        //        health = Mathf.Lerp(health, maxHealth, _resetTime - 2.5f);
        //}
        //else
        //    _resetTime = 0;
    }
    protected virtual void NearAttack()//Animator trigger
    {
        //Animator Trigger
        m_Direction = Vector3.zero;

        isAttack = true;
        IE_StartAttack = StartNearAttack();
        StartCoroutine(IE_StartAttack);
                
        IEnumerator StartNearAttack()
        {
            yield return new WaitForSeconds(atkTime);
            float _time = 0;

            while (_time < atkTime)
            {
                Collider[] targets = Physics.OverlapSphere(atkObj.transform.position, atk_Radius);
                foreach (var _target in targets)
                {
                    IDamage the_Target = _target.GetComponent<IDamage>();
                    if (the_Target != null && the_Target.GetType() != this.GetType())
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
    protected virtual void FarAttack()
    {
        //Animator trigger
        m_Direction = Vector3.zero;

        isAttack = true;
        IE_StartAttack = StartFarAttack();
        StartCoroutine(IE_StartAttack);

        IEnumerator StartFarAttack()
        {
            yield return new WaitForSeconds(atkTime / 2);
            float _time = 0;
            atkDirection = atkObj.transform.position;
            Vector3 targetDirection = targetAtk.position;

            atkObj.SetActive(true);
            while (_time < atkTime)
            {
                Collider[] targets = Physics.OverlapSphere(atkObj.transform.position, atk_Radius);
                foreach (var _target in targets)
                {
                    IDamage the_Target = _target.GetComponent<IDamage>();
                    if (the_Target != null && the_Target.GetType() != this.GetType())
                        the_Target.BeDamage(attackDamage, transform, Vector3.zero, null);
                }

                atkObj.transform.position = Vector3.Lerp(atkDirection, targetDirection, _time / atkTime);

                _time += Time.deltaTime;
                yield return null;
            }
            atkObj.SetActive(false);
            atkObj.transform.position = atkDirection;
            isAttack = false;
        }

    }

    private void UpdateHealth()
    {
        nowHealth = Mathf.Clamp(nowHealth, 0, maxHealth);
    }
    protected virtual void Die()
    {
        m_collider.enabled = false;
        //出現煙霧Effect -> BOOM -> 特效
        GameObject _dieEffect = Instantiate(smokeEffect.PrefabEffect, transform.position, transform.rotation);
        Destroy(_dieEffect, smokeEffect.EffectDestroyTime);
        GameManager.Instance_GameManager.Audio_InitialSounds(sounds, _dieEffect); //因為怪物死亡後會消失 音效播不到 所以音效放在特效上
        GameManager.Instance_GameManager.Audio_PlayAudio(sounds, "Die");

        Drops();
        monsterMenu.DieUiDisappear();

        bool isHaveParent = gameObject.transform.parent != null;
        if (isHaveParent)
        { 
            MonsterManager _monsterManager = gameObject.transform.parent.GetComponent<MonsterManager>();
            if (_monsterManager != null)
                _monsterManager.RemoveChildrenMonster(this);
        }

        Destroy(gameObject);
        //this.enabled = false;
        //this.gameObject.SetActive(false);
    }
    private void Drops()
    {
        if (dropsItemBasicRNG.Length == 0)        
            return;

        List<ItemBasic> _dropsItemBasicRNG = new List<ItemBasic>();
        for (int i = 0; i < dropsItemBasicRNG.Length; i++)
        {
            int index = Random.Range(1, 100);            
            if(index - dropsItemBasicRNG[i].RandomNumberGeneration < 0)            
                _dropsItemBasicRNG.Add(dropsItemBasicRNG[i].itemBasic);                        
        }
        
        //掉落
        for (int i = 0; i < _dropsItemBasicRNG.Count; i++)
        {
            float pos = Random.Range(0 * 0.5f, i * 0.5f);
            Vector3 DropsPos = transform.position + new Vector3(pos, 0, pos);
            Instantiate(_dropsItemBasicRNG[i].GetItemBasicObject_Prefab, DropsPos, Quaternion.identity);
        }
    }

    public virtual bool BeDamage(float _damage, Transform target, Vector3 _hitPos, EffectObjectBasic _hitEffect)
    {
        if (!canBeHit)
            return false;

        //Effect
        if (_hitEffect != null)
        {
            Vector3 hit_pos = m_collider.ClosestPoint(_hitPos);
            GameObject _dieEffect = Instantiate(_hitEffect.PrefabEffect, hit_pos, Quaternion.identity);
            Destroy(_dieEffect, _hitEffect.EffectDestroyTime);
        }

        //Sound
        int hitIndex = Random.Range(0, 3);
        GameManager.Instance_GameManager.Audio_PlayAudio(sounds, "BeHit"+ hitIndex);

        //if (IE_StartAttack != null) //沒有硬質的情況下 被A到 停止攻擊
        //{
        //    StopCoroutine(IE_StartAttack);
        //    atkObj.SetActive(false);
        //    isAttack = false;
    
        //    if(attackType == Attack_OR_SkillType.Far)
        //        atkObj.transform.position = atkDirection;
        //}

        Vector3 direction = transform.position - target.position;
        direction.y = 0;
        direction.Normalize();
        //Vector3 
        transform.Translate(direction * _damage * Time.deltaTime);

        nowHealth -= _damage;

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
    private IEnumerator Retreat()
    {
        if(isRetreat)
            yield return new WaitForSeconds(toNearRange);

        isRetreat = true;
        float _time = 0;
        int randomNumber = Random.Range(-1, 2); // -1 ~ 2 not include 2        

        while (_time < toNearRange)
        {
            m_Direction = Vector3.MoveTowards(transform.position, transform.position + randomNumber * transform.right * moveSpeed, moveSpeed / 2f * Time.deltaTime);
            transform.position = m_Direction;
            m_Direction.Normalize();

            _time += Time.deltaTime;
            yield return null;
        }
        isRetreat = false;
    }

    public void ResetCombatLock(float _damage, List<Transform> _Monsters)
    {
        if (_Monsters != null && nowHealth <= _damage)   //最後一擊
            _Monsters.Remove(gameObject.transform);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1, 0, 0, 0.8f);
        Gizmos.DrawSphere(atkObj.transform.position, atk_Radius);
    }
}

[System.Serializable]
public class DropsItemBasicRNG
{
    public ItemBasic itemBasic;
    [Range(0,100)]
    public int RandomNumberGeneration = 50;
}