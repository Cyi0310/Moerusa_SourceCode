using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private PlayerManager playerManager;

    private Vector3 m_Direction = Vector3.zero;
    private float m_CurrnetSpeed = 0f;


    private Transform main_Camera; //Main 攝影機

    [Header("Player Basic Value")]
    [SerializeField] 
    private CharacterController m_Controller;
    [SerializeField]
    private float moveSpeed = 4f;
    [SerializeField ,Range(.1f, 10f)]
    private float howFast2MaxSpeed = 5f;
    [SerializeField ,Range(-15, -5)]
    private float maxGravity = -5.5f;
    [SerializeField , Range(0.1f, 1f)]
    private float rotSmooth = 0.25f;

    //public const float gravity = 9.8f;
    public BasicMovement basicMovement;
    public GuMovement guMovement;
    public YiMovement yiMovement;
    public QuMovement quMovement;
    public CombatLockMovement combatLockMovement;

    [Header("For Check Ground")]
    [SerializeField] private float checkGroundCenter = -0.35f;
    [SerializeField] private float checkGroundRadius = 0.5f;
    [SerializeField] private LayerMask goundLayermask; //Layer Name => Ground
   
    private Vector3 checkGround { get => transform.position + Vector3.down * checkGroundCenter; }
    private float horizontal { get => Input.GetAxis("Horizontal"); }
    private float vertical { get => Input.GetAxis("Vertical"); }

    #region Public For PlayerManager Variable and Function
    public CharacterController UseController { get => m_Controller; set => m_Controller = value; }
    public Vector2 BasicDirection { get => new Vector2(horizontal, vertical); } // ForAnimation
    public float GuPushDirection { get; private set; }  //Gu Push Direction. 1 = Forword , -1 = Back
    public Vector3 GetYiMyselfControlDirection { get; private set; }    //Yi Have WaterBall and Use myselfcontrol
    public Vector3 UseDirection { get => m_Direction; set => m_Direction = value; }
    public float CurrentSpeed { get => m_CurrnetSpeed; }
    public bool IsGround { get => CheckGround(); } 
    public float ForAnimationControl_MaxGravity() => CheckGround() ? 0 : m_Direction.y;//在空中時 為 0 
    public void SetGoundLayermask(LayerMask _Layermask) => goundLayermask = _Layermask;

    public void Totem_Use_PlayerControl(float _disappearTime) // 給圖騰使用 玩家往上飄 逐漸消失
    {
        playerManager.UseController.enabled = false;
        Vector3 basicPos = transform.position;
        Vector3 aboveBasicPos = transform.position + Vector3.up * _disappearTime;
        StartCoroutine(AboveThePlayer());
        IEnumerator AboveThePlayer()
        {
            float amount = 0;
            while (amount < _disappearTime)
            {
                amount += Time.deltaTime;
                gameObject.transform.position = Vector3.Lerp(basicPos, aboveBasicPos, amount / _disappearTime);
                yield return null;
            }
        } 
    }
    #endregion   
    public bool isInsidePlatformMove() // For Platform Move , true = isMove => gameobject of parent is not Platform
    {
        if (playerManager.GetStateQu.crouch)
            return false;
        else if (playerManager.GetStateQu.OneJump)
            return true;

        return BasicDirection.magnitude != 0f;
    }

    void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();

        m_Controller = gameObject.GetComponent<CharacterController>();
        main_Camera = Camera.main.transform;
    }

    //[Space(10)]
    //public float viewRadius = 10f; // 視野距離
    //public float viewAngleStep = 20; // 射線數量

    void Update()
    {
        if (!m_Controller.enabled)
            m_Controller.enabled = isInsidePlatformMove();

        Vector3 _m_Direction = Vector3.zero;
        BasicMovement _basicMovement = basicMovement;
        float _moveSpeed = moveSpeed;

        switch (playerManager.GetCombatStatus)
        {
            case CombatStatus.NotHasArms:

                switch (playerManager.PlayerNowType)
                {
                    case PlayerType.Gu:

                        //if in water --> don't move
                        if (playerManager.GetWaterSpaceInside)
                        {
                            PlayerBasicMovement(basicMovement);
                            _m_Direction = Vector3.down * moveSpeed * Time.deltaTime;
                            break;
                        }

                        switch (playerManager.GetGuPushMoveState)
                        {
                            case GuPushMoveState.NotPush:
                                if (playerManager.GetHardNowTarget() != null)
                                    playerManager.GetHardNowTarget().parent = null;

                                //isPress                                   
                                //playerManager.NowPlayerMoveType = playerManager.GetGuUseIsPress? PlayerMoveType.MayMove: PlayerMoveType.CanMove;
                                //if (!playerManager.GetGuUseIsPress)
                                //{
                                //}
                                    
                                PlayerBasicMovement(basicMovement);
                                
                                _m_Direction = m_Direction * moveSpeed * Time.deltaTime;

                                break;
                            case GuPushMoveState.MatchPush:

                                if (!playerManager.GetGuUseSkill)
                                    playerManager.GetHardNowTarget().parent = null;

                                break;
                            case GuPushMoveState.Push:

                                if (!playerManager.GetGuUseSkill)
                                    playerManager.GetHardNowTarget().parent = null;
                                else if (playerManager.GetHardNowTarget() != null)
                                    playerManager.GetHardNowTarget().parent = gameObject.transform;


                                //Gu Movement
                                PlayerGuMovement(guMovement);

                                _m_Direction = m_Direction * guMovement.PushMoveSpeed * Time.deltaTime;

                                break;
                            default:
                                break;
                        }
                        break;
                    case PlayerType.Yi:

                        //持球案右鍵 //轉身到滑鼠位置
                        
                        //使用水球 //轉身到滑鼠位置
                        if (playerManager.GetYiUseSkill)
                        {
                            PlayerYiMyselfControlMovement(yiMovement);
                            _moveSpeed = playerManager.GetMyselfControl? (moveSpeed / 2) : moveSpeed; //R Click will slowly
                        }
                        else //在水中移動速度 -> 較慢
                        {
                            _basicMovement = playerManager.GetWaterSpaceInside ? yiMovement : basicMovement;
                            PlayerBasicMovement(_basicMovement);
                            _moveSpeed = playerManager.GetWaterSpaceInside ? (moveSpeed) : moveSpeed;
                        }

                        _m_Direction = m_Direction * _moveSpeed * Time.deltaTime;
                        break;
                    case PlayerType.Qu:

                        PlayerQuMovement(quMovement.maxMoveSpeed, quMovement.minMoveSpeed, quMovement.gravity);
                        _m_Direction = m_Direction * moveSpeed * Time.deltaTime;

                        break;
                    default:
                        break;
                }

                break;
            case CombatStatus.HasArms:

                //在水中移動較慢
                _moveSpeed = playerManager.GetWaterSpaceInside ? (moveSpeed / 2) : moveSpeed;
                
                if(playerManager.PlayerNowType == PlayerType.Qu)
                    PlayerQuMovement(quMovement.maxMoveSpeed, quMovement.minMoveSpeed, quMovement.gravity);
                else
                    PlayerBasicMovement(basicMovement);

                _m_Direction = m_Direction * _moveSpeed * Time.deltaTime;
                
                break;
            case CombatStatus.Attackking:

                Vector3 _basicDirection = new Vector3(horizontal, 0, vertical);
                m_Direction = _basicDirection;
                float m_magnitude = m_Direction.magnitude;
                
                m_magnitude = Mathf.Clamp(m_magnitude, 0, 1);
                
                m_CurrnetSpeed = m_magnitude * moveSpeed / guMovement.maxMoveSpeed;
                
                //m_CurrnetSpeed = 0f;

                //Vector3 _basicDirection = new Vector3(horizontal, 0, vertical);
                //m_Direction = _basicDirection;
                //m_Controller.Move(m_Direction * 1 * Time.deltaTime);

                break;
            case CombatStatus.LockMonster:

                if (playerManager.PlayerNowType == PlayerType.Qu)
                    PlayerQuMovement(quMovement.maxMoveSpeed, quMovement.minMoveSpeed, quMovement.gravity);
                else
                {
                    PlayerBasicMove(combatLockMovement);
                    PlayerRotation(main_Camera, playerManager.GetNowTargetMonster.forward);
                }                

                _m_Direction = m_Direction * moveSpeed* Time.deltaTime;
                break;

            case CombatStatus.Ultimating:

                Debug.Log("ULT");
                switch (playerManager.PlayerNowType)
                {
                    case PlayerType.Gu:
                        break;
                    case PlayerType.Yi:
                        break;
                    case PlayerType.Qu:
                        if (/*playerManager.GetMovementIsGround && */playerManager.GetQuCombatSkill)
                        {                            
                            PlayerQuMovement(quMovement.maxMoveSpeed, quMovement.minMoveSpeed, quMovement.gravity);
                            _m_Direction = m_Direction * moveSpeed * 2 * Time.deltaTime;
                        }

                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        if (playerManager.NowPlayerMoveType != PlayerMoveType.CanMove)                    
            return;

        m_Controller.Move(_m_Direction);
    }
    private void PlayerBasicMovement(BasicMovement _movement)
    {
        PlayerBasicMove(_movement);

        if (playerManager.NowPlayerMoveType == PlayerMoveType.MayMove) 
            return;

        PlayerRotation(main_Camera, transform.forward);
    }
    private void PlayerBasicMove(BasicMovement _movement)//float _maxMoveSpeed, float _minMoveSpeed, float _gravity)
    {
        float _maxMoveSpeed = _movement.maxMoveSpeed;
        float _minMoveSpeed = _movement.minMoveSpeed;
        float _gravity = _movement.gravity;        

        Vector3 _basicDirection = new Vector3(horizontal, 0, vertical);
        if (IsGround)
        {
            m_Direction = _basicDirection;

            float m_magnitude = m_Direction.magnitude;
            m_magnitude = Mathf.Clamp(m_magnitude, 0, 1);
            m_CurrnetSpeed = m_magnitude * moveSpeed / _maxMoveSpeed;

            Run(_maxMoveSpeed, _minMoveSpeed);
        }
        else //在空中
        {
            m_Direction = _basicDirection + Vector3.up * m_Direction.y;

            if (m_Direction.y >= maxGravity) ////讓重力不要太重            
                m_Direction.y -= Gravity(_gravity);
            LowerSpeed(Time.deltaTime * howFast2MaxSpeed, _minMoveSpeed);
        }
    }
    private void PlayerGuMovement(GuMovement _movement)
    {
        float _maxMoveSpeed = _movement.PushMoveSpeed;

        Vector3 _basicDirection = new Vector3(horizontal, 0, vertical);
        if (IsGround)
        {
            float targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + -transform.eulerAngles.y;
           
            if (_basicDirection.magnitude >= 0.1f)
            {
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                GuPushDirection = moveDir.normalized.z; // 大於1 = 向前 , 小於1 = 往後
            }         
            m_Direction = _basicDirection - playerManager.GetPlayerInHardSpaceWhere(horizontal, vertical, transform);

            float m_magnitude = m_Direction.magnitude;
            m_magnitude = Mathf.Clamp(m_magnitude, 0, 1);
            m_CurrnetSpeed = m_magnitude * guMovement.PushMoveSpeed/ _maxMoveSpeed;

            //Run(_maxMoveSpeed*2, _maxMoveSpeed);
        }
    }
    private void PlayerYiMyselfControlMovement(BasicMovement _movement)
    {
        float _maxMoveSpeed = _movement.maxMoveSpeed;
        float _minMoveSpeed = _movement.minMoveSpeed;
        float _gravity = _movement.gravity;

        Vector3 waterBallTargerDir = playerManager.GetMyselfControlPos - transform.position;
        waterBallTargerDir.y = 0;

        Vector3 _basicDirection = new Vector3(horizontal, 0, vertical);
        if (IsGround)
        {
            ////Calculate Myself Control WaterBall Direction
            Vector3 wbNormalized = _basicDirection;
            float targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + -playerManager.GetMyselfControlTransform.eulerAngles.y;

            if (wbNormalized.magnitude >= 0.1f)
            {
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                GetYiMyselfControlDirection = moveDir.normalized * (moveSpeed / _maxMoveSpeed);
            }
            else
                GetYiMyselfControlDirection = Vector3.zero;

            ////// Yi Camera
            main_Camera = Camera.main.transform;
            //m_Direction = _basicDirection + main_Camera.forward.normalized;
            //float transform_y = main_Camera.rotation.eulerAngles.y;

            float transform_y = main_Camera.rotation.eulerAngles.y;
            m_Direction = Quaternion.Euler(0, transform_y, 0) * m_Direction;
            Vector3 _moveDir = main_Camera.forward * vertical;

            _moveDir += main_Camera.right * horizontal;
            _moveDir.Normalize();
            Vector3 moveDirection = _moveDir;
            Vector3 targetDir = moveDirection;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = Vector3.zero;
            m_Direction = /*Quaternion.Euler(0, transform_y, 0) * m_Direction*/ /*moveDirection*/targetDir;
            //////

            Run(_maxMoveSpeed, _minMoveSpeed);
        }
        else
        {
            m_Direction = _basicDirection + Vector3.up * m_Direction.y;

            if (m_Direction.y >= maxGravity) ////讓重力不要太重            
                m_Direction.y -= Gravity(_gravity);
        }
        
        //// Rotation
        Quaternion lookDir = Quaternion.LookRotation(waterBallTargerDir);
        Quaternion SmoothRot = Quaternion.Slerp(transform.rotation, lookDir, rotSmooth);
        transform.rotation = SmoothRot;
    }

    private void PlayerQuMovement(float _maxMoveSpeed,float _minMoveSpeed,float _gravity)
    {
        Vector3 _basicDirection = new Vector3(horizontal, 0, vertical);

        float m_magnitude = m_Direction.magnitude;
        if (IsGround)
        {
            m_Direction = _basicDirection + Vector3.up * playerManager.GetQuDirection_Y();
            m_magnitude = Mathf.Clamp(m_magnitude, 0, 1);
            Run(_maxMoveSpeed, _minMoveSpeed);
        }
        else //在空中
        {
            m_Direction = _basicDirection + Vector3.up * playerManager.GetFlyPlayerDirection_Y(m_Direction.y);

            if (!playerManager.GetQuUseSkill) //沒有使用Qu時
            {
                if (m_Direction.y >= maxGravity) ////讓重力不要太重            
                    m_Direction.y -= Gravity(_gravity);
                
                LowerSpeed(Time.deltaTime * howFast2MaxSpeed, _minMoveSpeed);
            }
            else 
            {
                Run(_maxMoveSpeed, _minMoveSpeed);
            }
        }

        //Speed calculate ,need to keep 0~1 of speed
        Vector3 m_Direction_NotHaveVectorUp = m_Direction - Vector3.up * m_Direction.y;
        float m_DirectionMagnitude = m_Direction_NotHaveVectorUp.magnitude;
        m_DirectionMagnitude = Mathf.Clamp01(m_DirectionMagnitude);
        m_CurrnetSpeed = m_DirectionMagnitude * moveSpeed / _maxMoveSpeed;

        //Airflow
        if (playerManager.playerInsideQuAirFlow)
            m_Direction = _basicDirection + playerManager.QuAirFlowDirection;
        else if(playerManager.GetWaterSpaceInside)
            m_Direction = _basicDirection + Vector3.up;

        PlayerRotation(main_Camera, transform.forward);
    }

    void PlayerRotation(Transform _transform,Vector3 _notMoveForward)     //Rotation
    {
        //利用四元數左乘向量來得到目標的方向    
        float transform_y = _transform.rotation.eulerAngles.y;
        m_Direction = Quaternion.Euler(0, transform_y, 0) * m_Direction;
        Vector3 moveDir = _transform.forward * vertical;

        moveDir += _transform.right * horizontal;
        moveDir.Normalize();
        Vector3 moveDirection = moveDir;

        //處理旋轉。  運算處理該向量,使物體旋轉朝向該向量      
        Vector3 targetDir = moveDirection;
        targetDir.y = 0;
        if (targetDir == Vector3.zero)
            targetDir = _notMoveForward;

        //Smooth
        Quaternion lookDir = Quaternion.LookRotation(targetDir);
        Quaternion targetRot = Quaternion.Slerp(transform.rotation, lookDir, rotSmooth); 

        transform.rotation = targetRot;
    }

    bool CheckGround()// -> ( 若是沒有踩地板 檢查Layer  ////////////////// 應該還會再改 (尤其是走路 往下走時 判定會為False
    {
        if (m_Direction.y < 0.1f)
        {
            //Debug.Log("Physics.CheckSphere(transform.position + Vector3.down * checkGroundCenter, checkGroundRadius, goundLayermask)    " + Physics.CheckSphere(transform.position + Vector3.down * checkGroundCenter, checkGroundRadius, goundLayermask));
            return Physics.CheckSphere(checkGround, checkGroundRadius, goundLayermask);            
        }
            
        //Debug.Log("m_Controller.isGrounded  " + m_Controller.isGrounded);
        return m_Controller.isGrounded;               
    }

    float Gravity(float _gravity) => _gravity * Time.deltaTime;

    #region 跑步 About Run Function
    void Run(float _maxMoveSpeed , float _minMoveSpeed)
    {
        if (m_Direction.magnitude > 0.1f && Input.GetKey(playerManager.GetPlayerKeyCode("Run").keycode)) //如果當前速度 != 0 時 且案Shift  <- KeyCode                
            AddSpeed(Time.deltaTime * howFast2MaxSpeed, _maxMoveSpeed);        
        else        
            LowerSpeed(Time.deltaTime * howFast2MaxSpeed, _minMoveSpeed);
        
    }

    void AddSpeed(float _speed, float _maxMoveSpeed)
    {
        moveSpeed += _speed;
        if (moveSpeed > _maxMoveSpeed)
            moveSpeed = _maxMoveSpeed;
    }
    void LowerSpeed(float _speed, float _minMoveSpeed)
    {
        moveSpeed -= _speed;
        if (moveSpeed < _minMoveSpeed)
            moveSpeed = _minMoveSpeed;
    }
    #endregion
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 0, 0.7f); //Black
        Gizmos.DrawSphere(checkGround, checkGroundRadius);
    }
}

[System.Serializable]
public class GuMovement : BasicMovement
{
    public float PushMoveSpeed;
}
[System.Serializable]
public class YiMovement : BasicMovement
{

}
[System.Serializable]
public class QuMovement : BasicMovement
{

}

[System.Serializable]
public class CombatLockMovement : BasicMovement
{

}
