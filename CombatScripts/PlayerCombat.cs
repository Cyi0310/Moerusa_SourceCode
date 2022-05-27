using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoerusaGameManager;
using UnityEngine.UI;

public enum CombatStatus
{
    NotHasArms,
    HasArms,
    Attackking,
    Ultimating,
    LockMonster
}

//掛在Player身上
public class PlayerCombat : MonoBehaviour
{
    private GameManager _GameManager;    
    private PlayerManager playerManager;

    [SerializeField]
    private GameObject arms;
    [SerializeField]
    private Transform armsPos;

    #region Fighting Variable
    private bool isSwitchLockEnemy;
    private CombatStatus m_CombatStatus;
    private List<Transform> nowMonsterTargets;
    private Transform nowMonsterTarget;   //Now 鎖定 用
    public GameObject PrefabLockMonsterUI;
    private Image lockMonsterUI;

    //Set 重製 attackCount  的 時間 ( 時間到 重製
    //Set isAttack  時間到 重製
    [Header("About Attack"), SerializeField]
    private float mainAttackRadius;
    [SerializeField]
    private float gatherAttackMaxTime = 0.5f;
    [SerializeField]
    private Vector3 mainAttackPositionOffset, subAttackPositionOffset, subAttackSize;
    [SerializeField]
    private Transform attackTransform; //是有加 TrailRenderer的那個GameObject
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("About Skill Attack")]
    [SerializeField]
    private Transform skillTransform;
    [SerializeField]
    private Vector3 skillSize;
    [SerializeField]
    private float moveDistance = 4f;
    IEnumerator IE_Skill;

    [Header("Attack FeedBack"),SerializeField]
    public float checkEnemyRadius;
    [SerializeField]
    private float combatShakeIntensity, combatShakeTime, animationStopTime;  
    private TrailRenderer armsTrailRenderer; //Gameobject放在武器那邊

    [Header("Attack Detail"), SerializeField]
    private float attackValue = 40f;

    [SerializeField] 
    private int maxAttackCount, attackAnimationCount; //上限攻擊次數 , 攻擊次數    

    [Header("Audio")]
    public Sound[] sounds;

    [Header("CombatEffect")]
    public Transform gatherEffect_Transform;
    public ParticleSystem gatherEffect_Prefab;
    private ParticleSystem gatherEffect;

    public EffectObjectBasic hitEffect;

    public float GatherAttackTime { get; private set; }
    public CombatStatus GetCombatStatus { get => m_CombatStatus; }
    public Transform NowMonsterTarget{ get ; private set; }
    public bool attackEnable { get; set; }
    public bool isAttack { get; private set; }
    public bool isSkill { get; private set; }
    public int attackCount { get; set; } //攻擊次數
    public bool isLockMonster { get; private set; } // true -> Lock 中

    #endregion
    
    ////Display Arms Variable
    private  Material[] armsMaterials;
    private IEnumerator IECreatArms, IEDestoryArns;
    private float _switchTime = 1;  // 1 -> 消失 ， -1 -> 出現    

    private KeyCode switchArmsKeycode, attackKeycode, switchLockEnemyKeyCode, skillKeycode;

    public bool isSwitchArms { get; private set; }
    public bool isTakeArms { get; private set; }

    void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();
        _GameManager = GameManager.Instance_GameManager;

        _GameManager.Audio_InitialSounds(sounds, gameObject);

        switchArmsKeycode = playerManager.GetPlayerKeyCode("SwitchArms").keycode;
        attackKeycode = playerManager.GetPlayerKeyCode("Attack").keycode;
        switchLockEnemyKeyCode = playerManager.GetPlayerKeyCode("RClick").keycode;
        skillKeycode = playerManager.GetPlayerKeyCode("UseType").keycode;

        GameObject _lockMonsterUI = Instantiate(PrefabLockMonsterUI, _GameManager.WorldSpaceCanvas.transform);
        lockMonsterUI = _lockMonsterUI.GetComponent<Image>();
        lockMonsterUI.enabled = false;

        ArmsDisplayStart();

        armsTrailRenderer = attackTransform.GetComponent<TrailRenderer>();

        //Effect
        GameObject _gatherEffect = Instantiate(gatherEffect_Prefab.gameObject, gatherEffect_Transform.position, Quaternion.identity, gatherEffect_Transform);
        gatherEffect = _gatherEffect.GetComponent<ParticleSystem>();
    }
    private void ArmsDisplayStart()
    {
        Renderer[] _armsRenders = arms.GetComponentsInChildren<Renderer>();
        armsMaterials = new Material[_armsRenders.Length];
        for (int i = 0; i < _armsRenders.Length; i++)
        {
            armsMaterials[i] = _armsRenders[i].material;
            armsMaterials[i].SetFloat("Vector1_E7F711AD", 1);
        }
    }
    void Update()
    {
        m_CombatStatus = CombatStatusUpdate();

        //沒使用技能 且 沒有開UI 可招喚武器 且 沒有對話
        isSwitchArms = Input.GetKeyDown(switchArmsKeycode) && !playerManager.UseSkill() && _GameManager.gameMainCanvas == GameManager.GameMainCanvas.None && !playerManager.GetIsTalk;
        isAttack = Input.GetKeyDown(attackKeycode);
        isSkill = Input.GetKeyDown(skillKeycode);
        isSwitchLockEnemy = Input.GetKeyDown(switchLockEnemyKeyCode);
            
        if (isSwitchArms && !GatherAttack())
            TakeArms();

        if (isTakeArms) //有拿武器
        {
            if (CheckMonsterInRange()) //範圍內有Monster
            {
                LockMonsterUpdate();

                NowMonsterTarget = isLockMonster ? nowMonsterTarget: null;
            }
            else
            {
                if (nowMonsterTarget == null || nowMonsterTargets.Count <= 0)
                    isLockMonster = false;

                Debug.Log(" NOT HAVE MONSTER");
                _GameManager.CombatUnLockInMonsterTarget(transform, nowMonsterTargets, lockMonsterUI);
                nowMonsterTarget = null;
            }


            if (isAttack)
            {
                Fighting();
                //特效
            }
            else if (m_CombatStatus == CombatStatus.Attackking && Input.GetKey(attackKeycode))
            {
                if (GatherAttack())
                    gatherEffect.Play();
                else
                    gatherEffect.Stop();


                GatherAttackTime += Time.deltaTime;
            }
            else if (Input.GetKeyUp(attackKeycode))
            {
                gatherEffect.Stop();

                GatherAttackTime = 0f;
            }

            if (MainAttack(attackEnable))
            {
                Collider[] _enemies = Physics.OverlapSphere(attackTransform.position + mainAttackPositionOffset, mainAttackRadius, enemyLayer);
                AttackColliderJudge(_enemies, attackValue, attackTransform.position + mainAttackPositionOffset);

                playerManager.CameraShake(combatShakeIntensity, combatShakeTime);
                ActionStop(animationStopTime);
            }

            //playerManager.MagicBarNotHaveMagic 
            //return
            if (SkillAttack())
            {
                bool isTriggerEnemy = Physics.CheckBox(skillTransform.position, skillSize / 2, skillTransform.rotation, enemyLayer);
                if (isTriggerEnemy)
                {
                    Collider[] _enemies = Physics.OverlapBox(skillTransform.position, skillSize / 2, skillTransform.rotation, enemyLayer);
                    AttackColliderJudge(_enemies, attackValue, skillTransform.position);

                    ActionStop(animationStopTime);
                }

                if (IE_Skill == null)
                {
                    IE_Skill = SkillColliderMove();
                    StartCoroutine(IE_Skill);
                }

                IEnumerator SkillColliderMove() //Gu
                {
                    playerManager.PlayerMagicBar_SetValue(-25f);
                    Vector3 startPos = skillTransform.position;
                    Vector3 finalPos = skillTransform.position + skillTransform.forward * moveDistance;
                    float time = 0f;
                    while (m_CombatStatus == CombatStatus.Ultimating)
                    {
                        skillTransform.position = Vector3.Lerp(startPos, finalPos, time);
                        time += Time.deltaTime;
                        playerManager.CameraShake(combatShakeIntensity, combatShakeTime);
                        yield return null;
                    }
                    skillTransform.position = startPos;
                    IE_Skill = null;
                }

                playerManager.CameraShake(combatShakeIntensity * 2, combatShakeTime * 2);
            }
        }
        else //沒拿武器 
        {                
            _GameManager.CombatUnLockInMonsterTarget(transform, nowMonsterTargets, lockMonsterUI);
        }

        //Debug.Log(SubAttack(attackEnable) + " sub "); // <---  之後讓 攻擊範圍增加 -> 讓玩家更容易打到怪物   ////////////
        
        ArmsEffect();
    }

    private CombatStatus CombatStatusUpdate()
    {
        if (isTakeArms)
        {
            if (playerManager.GetUltimating)
                return CombatStatus.Ultimating;
            else if (!playerManager.GetAttackking && !isLockMonster)
                return CombatStatus.HasArms;
            else if (playerManager.GetAttackking)
                return CombatStatus.Attackking;
            else if (!playerManager.GetAttackking && isLockMonster)
                return CombatStatus.LockMonster;
        }

        return CombatStatus.NotHasArms;
    }

    void Fighting()
    {
        if (attackCount >= 0)
        {
            attackCount++;
            if (attackCount > maxAttackCount)
                attackCount = 0;
        }
    }
    bool MainAttack(bool _attackEnable)
    {
        if (!_attackEnable)
            return false;
        if (isTakeArms && m_CombatStatus == CombatStatus.Attackking)
            return Physics.CheckSphere(attackTransform.position + mainAttackPositionOffset, mainAttackRadius, enemyLayer);

        return false;
    }
    bool SubAttack(bool _attackEnable)  //  可讓攻擊範圍增加 -> 讓玩家更容易打到怪物 
    {
        if (!_attackEnable)
            return false;

        if (isTakeArms && m_CombatStatus == CombatStatus.Attackking)
            return Physics.CheckBox(transform.position + subAttackPositionOffset, subAttackSize / 2, transform.rotation, enemyLayer);

        return false;
    }
    bool SkillAttack()
    {
        return isTakeArms && m_CombatStatus == CombatStatus.Ultimating;
    }

    void AttackColliderJudge(Collider[] _enemies, float _attackValue, Vector3 _hitPos)
    {
        foreach (Collider _enemy in _enemies)
        {
            IDamage _idamage = _enemy.GetComponent<IDamage>();

            if (_idamage == null)
                return;

            _enemy.GetComponent<Monster>().ResetCombatLock(_attackValue, nowMonsterTargets);
            _enemy.GetComponent<IDamage>().BeDamage(_attackValue, gameObject.transform, _hitPos, hitEffect);
        }
    }

    public bool GatherAttack() => GatherAttackTime >= gatherAttackMaxTime;

    void ActionStop(float _time)
    {
        playerManager.SetAnimator.enabled = false;
        StartCoroutine(WaitResetAction(_time));
    }
    IEnumerator WaitResetAction(float _time)
    {
        yield return new WaitForSeconds(_time);
        playerManager.SetAnimator.enabled = true;
    }
    void ArmsEffect()
    {
        armsTrailRenderer.enabled = m_CombatStatus == CombatStatus.Attackking || m_CombatStatus == CombatStatus.Ultimating;
    }

    #region Display Arms Function        
    void TakeArms()
    {
        isTakeArms = !isTakeArms;

        if (isTakeArms)        
            AppearArms();        
        else        
            DisappearArms();
    }

    void AppearArms()   //(出現)Take Arms  
    {
        //如果沒有使用技能中(不含飛) 且 沒開UI介面 且 沒有對話 才可拿起武器
        if (playerManager.UseSkillButNotFly() && _GameManager.gameMainCanvas == GameManager.GameMainCanvas.None && !playerManager.GetIsTalk)
            return;

        if (IEDestoryArns != null)
            StopCoroutine(IEDestoryArns);
        IECreatArms = SwitchArmsSetShader(_switchTime > -1f, -Time.deltaTime, true);
        StartCoroutine(IECreatArms);

        //紀錄時間 時間到把武器收起
    }
    void DisappearArms() //(消失)Back Arms
    {        
        if (IECreatArms != null)
            StopCoroutine(IECreatArms);
        IEDestoryArns = SwitchArmsSetShader(_switchTime < 1f, Time.deltaTime, false);
        StartCoroutine(IEDestoryArns);
    }

    IEnumerator SwitchArmsSetShader(bool condition,float _index,bool _armsDisplay)
    {
        if (_armsDisplay)
        {
            arms.SetActive(_armsDisplay);
            //_GameManager.SetCursor(true);          //鼠標改變
        }
        while (condition) 
        {
            _switchTime += _index;
            _switchTime = Mathf.Clamp(_switchTime, -1f, 1f); 
            foreach (var armsMT in armsMaterials)            
                armsMT.SetFloat("Vector1_E7F711AD", _switchTime);

            if (_armsDisplay ? _switchTime == -1f : _switchTime == 1f)
            {
                if (!_armsDisplay)
                {
                    //_GameManager.SetCursor(false); //鼠標改變
                    arms.SetActive(_armsDisplay);
                }                    
                yield break;
            }
            yield return null;
        }
    }
    #endregion 
    private Transform LockMonster()  //如果 Lock中 且 取消拿武器 -> LOCK = false
    {      
        Collider[] _enemies = Physics.OverlapSphere(transform.position, checkEnemyRadius, enemyLayer);

        if (_enemies == null || _enemies.Length <= 0)
            return null;

        nowMonsterTargets = new List<Transform>(_enemies.Length);

        List<float> Distances = new List<float>();
        for (int i = 0; i < _enemies.Length; i++)
        {
            nowMonsterTargets.Add(_enemies[i].transform);
            Vector3 direction = nowMonsterTargets[i].position - transform.position;
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
                    CyiLibrary.Swap.SwapList<Transform>(nowMonsterTargets, 0, i);
                }
            }
        }

        return nowMonsterTargets[0].transform;
    }
    bool CheckMonsterInRange()
    {
        if(nowMonsterTarget != null)        
            return Vector3.Distance(transform.position, nowMonsterTarget.position) < checkEnemyRadius;        

        return Physics.CheckSphere(transform.position, checkEnemyRadius, enemyLayer);
    }

    void LockMonsterUpdate()
    {
        if (isSwitchLockEnemy) //切換LockEnemy
        {
            isLockMonster = !isLockMonster;
            nowMonsterTarget = LockMonster();  //抓取 Target

            //鎖定時，出現兩個聲音
            playerManager.Audio_PlayAudio(sounds, isLockMonster ? "LockMonster" : "UnLockMonster");
            if (isLockMonster) //Secone sound
            {
                StartCoroutine(WaitSecondSound());
                IEnumerator WaitSecondSound()
                {
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    yield return null;
                    playerManager.Audio_PlayAudio(sounds, "LockMonster2");
                }
            }
        }

        if (isLockMonster)
        {
            if (nowMonsterTarget != null)            
                _GameManager.CombatLockInMonsterTarget(transform, nowMonsterTarget, lockMonsterUI);            
            else            
                nowMonsterTarget = LockMonster();  //抓取 Target
        }
        else
        {                         
            _GameManager.CombatUnLockInMonsterTarget(transform, nowMonsterTargets, lockMonsterUI);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!isTakeArms)
            return;

        Gizmos.color = new Color(1, 0, 0, 0.6f);
        Gizmos.DrawSphere(attackTransform.position + mainAttackPositionOffset, mainAttackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkEnemyRadius);
        
        Gizmos.matrix = skillTransform.localToWorldMatrix;
        Gizmos.color = new Color(1, 0.5f, 0, 0.6f); //Orange
        Gizmos.DrawCube(Vector3.zero, skillSize);
    }
}
