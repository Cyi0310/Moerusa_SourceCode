using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlAnimation : MonoBehaviour, MatchPositionSMB.IMatchTarget
{
    private PlayerManager playerManager;

    private Animator m_Animator;
    private float _nowDirection_Y;    
    private Vector3 targetPos;

    [Space(10), Range(-6f, -1f)]    
    public float player_Y_OverSpeed_Max = -2f; //降落速度>這個float，Player就會DontMove   

    public float direction_Down_MaxValue = -1f;//因為在落地時 地板已經離主角很近了， 所以要維持 isGround

    public float notYiTouchWaterDistance = 5f;

    [Header("Effect"), Space(10)]
    //Run effect
    public ParticleSystem prefabEffect_Run;
    private ParticleSystem effect_Run;   //已經在場上了

    public TrailRenderer prefab_QuTrailRenderer;
    public Transform[] quTrailRenderPoies;
    private TrailRenderer[] quTrailRenders;

    public ParticleSystem prefabEffect_Landing;
    private ParticleSystem effect_Landing;
    public EffectObject[] effectObject;
    
    public Vector3 TargetPosition { get => targetPos; }
    public Animator SerMyAnimator {get => m_Animator; set => m_Animator = value; }    
    public float CombatLayerWeight { get; set;} //Animator 的 Layer 的 Combat Layer 的 權重(Weight)
    public bool GetAttackking { get => m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"); }
    public bool GetUltimating { get => m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Ultimate"); }
    public bool GetQuCombatSkill { get => m_Animator.GetCurrentAnimatorStateInfo(4).IsName("QuCombatSkill"); }

    private bool IsMovement { get => playerManager.MovementDirection.magnitude != 0; }

    [Header("Sound")]
    public Sound[] attackSounds;
    public Sound[] sounds;

    private void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();

        //Run effect     
        effect_Run = Instantiate(prefabEffect_Run, transform.position, transform.rotation, transform);
        effect_Landing = Instantiate(prefabEffect_Landing, Vector3.zero, Quaternion.identity);
        quTrailRenders = new TrailRenderer[quTrailRenderPoies.Length];
        for (int i = 0; i < quTrailRenderPoies.Length; i++)
        {
            GameObject _quTrailRender = Instantiate(prefab_QuTrailRenderer.gameObject, quTrailRenderPoies[i].position, Quaternion.identity, quTrailRenderPoies[i]);
            quTrailRenders[i] = _quTrailRender.GetComponent<TrailRenderer>();
        }

        m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.keepAnimatorControllerStateOnDisable = true;


        foreach (var smb in m_Animator.GetBehaviours<MatchPositionSMB>())
        {
            smb.target = this;
        }

        StartCoroutine(WaitStartFunctionFinalFrame());    
    }
    IEnumerator WaitStartFunctionFinalFrame()
    {
        yield return new WaitForEndOfFrame();
        playerManager.Audio_InitialSounds(sounds, gameObject);
        playerManager.Audio_InitialSounds(attackSounds, gameObject);
    }


    public AudioSource DraggedSound;
    IEnumerator IE_DraggedPlaySound;
    IEnumerator DraggedPlaySound()
    {
        DraggedSound.volume = 1f;
        DraggedSound.Play();
        //playerManager.Audio_PlayAudio(sounds, "Dragged");
        yield return new WaitUntil(() => playerManager.GetGuPushMoveState != GuPushMoveState.Push /*|| IsPushMovement*/);
        DraggedSound.Stop();
        IE_DraggedPlaySound = null;
    }
    private void Update() //之後還需要製作 跑步ATK -> 往前方飄移 or LOCK 跑步ATK ->往TARGET 飄移
    {
        CombatUpdate();
        ThreeTypeUpdate();

        m_Animator.SetFloat("Horizontal", playerManager.GetBasicDirection.x);
        m_Animator.SetFloat("Vertical", playerManager.GetBasicDirection.y);

        m_Animator.SetFloat("NowDirection_Y", _nowDirection_Y);     //當落地時沒有離地板太遠不會有落地Ani 的 判定

        m_Animator.SetFloat("WalkSpeed", playerManager.GetMovementCurrentSpeed);

        //bool _isRunEffect = 
        bool isRunEffect = playerManager.GetMovementCurrentSpeed > 0.9f && !playerManager.CheckWaterGround();
        if(playerManager.PlayerNowType != PlayerType.Qu)
            NormalRunEffect(playerManager.GetMovementIsGround && isRunEffect && m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Locomotion Blend Tree"));
        else
            QuRunEffect(playerManager.GetMovementCurrentSpeed > 0.6f && !playerManager.CheckWaterGround());
        
        if (!playerManager.GetMovementIsGround)
            _nowDirection_Y = playerManager.GetForAnimationControlMaxGravity();

        if (playerManager.GetQuUseSkill) //使用Qu skill 時，_nowDirection_Y會 = 0，但如果在空中時 isGround必須要 false 。所以將isGround分開用
        {
            m_Animator.SetBool("isGround", playerManager.GetMovementIsGround);
        }
        else if (!playerManager.GetQuUseSkill)
        {
            if(playerManager.GetMovementIsGround)
                m_Animator.SetBool("isGround", true);
            else if (!playerManager.GetMovementIsGround && (_nowDirection_Y < direction_Down_MaxValue || _nowDirection_Y > 0f))//因為在落地時 地板已經離主角很近了， 所以要維持 isGround
                m_Animator.SetBool("isGround", false);
        }
        //m_Animator.SetBool("isGround", );

        m_Animator.SetInteger("NowType", (int)playerManager.PlayerNowType);

        Debug.Log("(int)playerManager.PlayerNowType     " + (int)playerManager.PlayerNowType);

        m_Animator.SetBool("IsPress", playerManager.GetGuUseIsPress);
        Debug.Log(playerManager.GetGuUseIsPress + "      playerManager.GetGuUseIsPress  ");
        switch (playerManager.PlayerNowType)
        {
            case PlayerType.Gu:
                if (playerManager.GetGuPushMoveState == GuPushMoveState.NotPush)
                {
                    if (playerManager.GetHardNowTarget() != null)
                        m_Animator.SetBool("UseTypeSkill", playerManager.GetGuUseSkill);
                }
                else                
                    m_Animator.SetBool("UseTypeSkill", playerManager.GetGuUseSkill);

                //bool isPushMovement = playerManager.MovementDirection.magnitude != 0;
                m_Animator.SetBool("PushMove", IsMovement);

                if (playerManager.GetGuPushMoveState == GuPushMoveState.Push && IE_DraggedPlaySound == null && IsMovement)
                {
                    IE_DraggedPlaySound = DraggedPlaySound();
                    StartCoroutine(IE_DraggedPlaySound);                    
                }

                GuTypeSkillUpdate();

                break;
            case PlayerType.Yi:
                m_Animator.SetBool("MyselfControl", playerManager.GetMyselfControl);
                m_Animator.SetFloat("MyselfControlHorizontal", playerManager.GetYiMyselfControlDirection.x);
                m_Animator.SetFloat("MyselfControlVertical", playerManager.GetYiMyselfControlDirection.z);

                YiTypeSkillUpdate();

                break;
            case PlayerType.Qu:
                QuTypeSkillUpdate();

                break;
            default:
                break;
        }
    }

    float BasicDirection()  //For Push Direction
    {
        if (playerManager.GetBasicDirection.x != 0f)
            return playerManager.GetBasicDirection.x;
        else if(playerManager.GetBasicDirection.y != 0f)
            return playerManager.GetBasicDirection.y;
        return 0f;
    }

    #region FireWaterBall Animation
    public void Animation_EndFireWaterBall()
    {
        playerManager.ResetYiState(WaterBallState.NotHave);
    }
    #endregion

    #region Animation Public Function

    //FallEnd For Animation
    public void Animation_LandingHeavy(string _StartOrEnd)
    {
        if (_StartOrEnd == "Start")
        {
            effect_Landing.transform.position = transform.position;
            effect_Landing.Play();  //Landing - Effect Display
            playerManager.NowPlayerMoveType = _nowDirection_Y <= player_Y_OverSpeed_Max ? PlayerMoveType.MayMove : PlayerMoveType.CanMove;
        }
        else if(_StartOrEnd == "End")
        {
            playerManager.NowPlayerMoveType = PlayerMoveType.CanMove;
        }
    }

    public void Animation_StartAttack()
    {
        playerManager.SetAttackEnable = true;
        //Sound
        int attackIndex = Random.Range(0, 3);
        playerManager.Audio_PlayAudio(attackSounds, "Attack"+ attackIndex);
        //Effect?
    }
    public void Animation_EndAttack()
    {
        playerManager.SetAttackEnable = false;
    }

    public void Animation_StartQuCombatSkill()
    {
        playerManager.SetAttackEnable = true;
        m_Animator.SetTrigger("QuCombatSkill");
        int attackIndex = Random.Range(0, 3);

        m_Animator.SetLayerWeight(4, 1f);

        playerManager.Audio_PlayAudio(attackSounds, "Attack" + attackIndex);
        //playerManager.UseController.Move(Vector3.up * 5f);
        StartCoroutine(QuCombatSkill());
        IEnumerator QuCombatSkill()
        {
            playerManager.GetStateQu.OneJump = true;
            yield return null;
            playerManager.GetStateQu.OneJump = false;

            yield return new WaitUntil(() => !GetQuCombatSkill);
            m_Animator.SetLayerWeight(4, 0f);
        }


    }

    public void Animation_EffectPlay(int index)
    {
        GameObject _effectObject = Instantiate(effectObject[index].PrefabEffect, effectObject[index].effectTransform.position, effectObject[index].effectTransform.rotation, effectObject[index].effectTransform);

        if (effectObject[index].isParentNull)
            _effectObject.transform.parent = null;

        if (effectObject[index].isPosBasic)
            _effectObject.transform.position = transform.position;

        if (effectObject[index].isRotBasic)
            _effectObject.transform.rotation = transform.rotation;
        

        Destroy(_effectObject, effectObject[index].EffectDestroyTime);
    }

    public void Animation_SoundPlay(string _soundName) => playerManager.Audio_PlayAudio(sounds, _soundName);


    #endregion

    #region For PlayerThreeType Gu Yi Qu Function       
    private void ThreeTypeUpdate()
    {
        if (playerManager.GetHardNowTarget() != null)  //這邊應該要丟到 ThreeType 裡面的Gu
            targetPos = playerManager.GetHardNowTarget().GetComponent<Collider>().ClosestPoint(transform.position);
    }
    private void GuTypeSkillUpdate()
    {

        m_Animator.SetFloat("Push_Direction", playerManager.GetGuPushDirection);
    }
    public GuPushMoveState SetGuPushMoveState()
    {
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("MatchPush"))        
            return GuPushMoveState.MatchPush;        
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsTag("Push"))        
            return GuPushMoveState.Push;
        else
            return GuPushMoveState.NotPush;        
    }

    private void YiTypeSkillUpdate()
    {
        m_Animator.SetBool("UseTypeSkill", playerManager.GetYiUseSkill);
    }
    private void QuTypeSkillUpdate()
    {            
        m_Animator.SetBool("Crouch", playerManager.GetStateQu.crouch);  //蹲下        
        if (!playerManager.GetMovementIsGround)                
            m_Animator.SetBool("Fly", playerManager.GetQuUseSkill);
    }
    #endregion

    #region For PlayerCombat Function       
    private void CombatUpdate() //ATK中 (能移動(對準移動的方向攻擊) (不能移動(算好距離自己移動)
    {
        if (playerManager.GetIsSwitchArms)
            m_Animator.SetTrigger("SwitchArms");

        m_Animator.SetBool("TakeArms", playerManager.GetIsTakeArms);            
        m_Animator.SetInteger("AttackCount", playerManager.GetAttackCount);
        
        if (playerManager.GetIsTakeArms && playerManager.GetMovementIsGround)
        {                
            m_Animator.SetBool("GatherAttack", playerManager.GatherAttack); 

            if (playerManager.GetIsSkill)
                m_Animator.SetTrigger("AttackSkill");
            else if (playerManager.GetIsAttack)
            {
                m_Animator.SetTrigger("Attack");

                if (playerManager.GetNowMonsterTarget() != null)
                    targetPos = playerManager.GetNowMonsterTarget().GetComponent<Collider>().ClosestPoint(transform.position);
                else
                    targetPos = transform.position;
            }
        }
    }
    #endregion

    private void NormalRunEffect(bool isRunEffect)
    {
        if (isRunEffect)
        {
            effect_Run.transform.rotation = transform.rotation;
            effect_Run.transform.position = transform.position;
            if (!effect_Run.isPlaying) effect_Run.Play();
        }
        else
            effect_Run.Stop();

        if (!quTrailRenders[0].enabled || quTrailRenders.Length == 0)
            return;
        for (int i = 0; i < quTrailRenders.Length; i++)
            quTrailRenders[i].enabled = false;
    }
    private void QuRunEffect(bool isRunEffect)
    {
        for (int i = 0; i < quTrailRenders.Length; i++)
            quTrailRenders[i].enabled = isRunEffect;

        if (!effect_Run.isPlaying)
            return;
        effect_Run.Stop();
    }
    public void SetTrigger(string _string) => m_Animator.SetTrigger(_string);
}