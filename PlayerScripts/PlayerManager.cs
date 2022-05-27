using CyiLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoerusaGameManager;

public class PlayerManager : MonoBehaviour
{
    public PlayerDatabase playerDatabase;

    GameManager gameManager;

    PlayerControl playerControl;
    PlayerControlAnimation playerControlAnimation;
    PlayerThreeType playerThreeType;
    PlayerMenu playerMenu;
    PlayerCombat playerCombat;
    MissionManager missionManager;
    Backpack backpack;
    CardManager cardManager;

    Outline m_Outline;

    #region From GameManager Variable    
    public Transform GetNowTargetMonster { get => gameManager.GetNowTargetMonster; }
    public bool playerInsideQuAirFlow { get; set; }
    public Vector3 QuAirFlowDirection { get; set; }
    public bool GetWaterSpaceInside { get => GetStateYi.WaterSpaceInside; } // true 表示玩家在HardSpace裡面
    #endregion

    #region From GameManager Function
    public Vector3 GetPlayerInHardSpaceWhere(float _h,float _v,Transform _player) => gameManager.GetPlayerInHardSpaceWhere(_h, _v, _player); 
    public void CameraShake(float _intensity,float _time) => gameManager.CameraShake(_intensity, _time);
    public Transform GetHardNowTarget()  // GuPushCurrentTarget
    {
        if (gameManager.NowHardSpace!= null)
            return gameManager.NowHardSpace.transform;
        else
            return null;
    }

    //ExclamationMark
    public void OnExclamationMark(bool isCondition) => gameManager.OnExclamationMark(isCondition);

    //Audio
    public void Audio_InitialSounds(Sound[] _sounds, GameObject _gameObject) => gameManager.Audio_InitialSounds(_sounds, _gameObject);
    
    public void Audio_PlayAudio(Sound[] _sounds, string _audioName) => gameManager.Audio_PlayAudio(_sounds, _audioName);
    
    #endregion

    #region From PlayerDatabase Variable
    public float GetMaxHealth { get => playerDatabase.MaxHealth; }
    public float GetMaxMagicBar { get => playerDatabase.MaxMagicBar; }
    public PlayerMoveType NowPlayerMoveType { get => playerDatabase.PlayerMoveType; set => playerDatabase.PlayerMoveType = value; }
    public PlayerType PlayerNowType { get => playerDatabase.PlayerNowType; set => playerDatabase.PlayerNowType = value; }

    public List<Mission> GetRMContainer { get => playerDatabase.GetRMContainer; }
    public List<Mission> GetSMContainer { get => playerDatabase.GetSMContainer; }

    public void SetAltarIndex(PlayerAltarPosition _playerAltarPosition) => playerDatabase.SetAltarIndex(_playerAltarPosition);
    public PlayerAltarPosition GetPlayerAltarPosition { get => playerDatabase.playerInAltarPosition; }

    public void SetPlayerPosition(Vector3 _position)
    {
        StartCoroutine(WaitFrameWaitFrameWaitFrame());
        IEnumerator WaitFrameWaitFrameWaitFrame()
        {
            UseController.enabled = false;
            yield return new WaitForSeconds(2f);
            transform.position = _position;
            yield return null; 
            UseController.enabled = true;
        }
    }
    #endregion

    #region From PlayerControl Variable
    public CharacterController UseController { get => playerControl.UseController; set => playerControl.UseController = value; }
    public float GetMovementCurrentSpeed { get => playerControl.CurrentSpeed; }
    public bool GetMovementIsGround { get => playerControl.IsGround; }
    public float GetForAnimationControlMaxGravity() => playerControl.ForAnimationControl_MaxGravity();
    public Vector3 MovementDirection { get => playerControl.UseDirection; set => playerControl.UseDirection = value; }
    public Vector2 GetBasicDirection { get => playerControl.BasicDirection; } //給Animation         
    public float GetGuPushDirection { get => playerControl.GuPushDirection; }
    public Vector3 GetYiMyselfControlDirection { get => playerControl.GetYiMyselfControlDirection; }

    public void SetGoundLayermask(LayerMask _LayerMask) => playerControl.SetGoundLayermask(_LayerMask);    
    #endregion

    #region From PlayerAnimation Variable
    public GuPushMoveState SetGuPushMoveState() => playerControlAnimation.SetGuPushMoveState();
    public Animator SetAnimator { get => playerControlAnimation.SerMyAnimator; set => playerControlAnimation.SerMyAnimator = value; }
    public float SetCombatLayerWeight {get => playerControlAnimation.CombatLayerWeight; set => playerControlAnimation.CombatLayerWeight = value; } //丟給PlayerCombat 修改
    public bool GetAttackking { get => playerControlAnimation.GetAttackking; }
    public bool GetUltimating { get => playerControlAnimation.GetUltimating; }
    public bool GetQuCombatSkill { get => playerControlAnimation.GetQuCombatSkill; }
    public IEnumerator IE_QuSkillMoveUp;
    public void SetTrigger(string _string) => playerControlAnimation.SetTrigger(_string);
    #endregion

    #region From PlayerThreeType Variable
    public KeyCode GetUseTypeKeycode { get => playerThreeType.UseTypeKeycode; }

    public StateGu GetStateGu { get => playerThreeType.stateGu; }
    public StateYi GetStateYi { get => playerThreeType.stateYi; }
    public StateQu GetStateQu { get => playerThreeType.stateQu; }

    public bool GetMyselfControl { get => playerThreeType.isMyselfControl; }
    public Vector3 GetMyselfControlPos { get => playerThreeType.GetMyselfControlPos; }
    public Transform GetMyselfControlTransform { get => playerThreeType.GetMyselfControlTransform; }

    public bool UseSkill() => GetGuUseSkill || GetGuPushMoveState != GuPushMoveState.NotPush || GetYiUseSkill || GetQuUseSkill || playerThreeType.GetIsOpenUiTypeDisplay;
    public bool UseSkillButNotFly() => GetGuUseSkill || GetGuPushMoveState != GuPushMoveState.NotPush || GetYiUseSkill || GetStateQu.crouch || playerThreeType.GetIsOpenUiTypeDisplay;
    public bool GetIsOpenUiTypeDisplay { get => playerThreeType.GetIsOpenUiTypeDisplay; } //True --> Can't open Note or Backpack or Setting or Map

    public bool GetGuUseSkill { get => playerThreeType.stateGu.useGu ;}
    public bool GetYiUseSkill { get => playerThreeType.stateYi.UseYiSkill(playerThreeType.stateYi); } ///////////////
    public bool GetQuUseSkill { get => playerThreeType.stateQu.useQu; } //現在是否有用Qu
    public bool GetGuUseIsPress { get => playerThreeType.stateGu.isPress; } //是否有壓物品
    public MeshRenderer ChangeTypeSphere_MeshRender { get => playerThreeType.ChangeTypeSphere_MeshRender; set => playerThreeType.ChangeTypeSphere_MeshRender = value; }
    public Vector2 ChangeTypeShaderLerp { get; set; }
    public IEnumerator IE_ChangeTypeTexture { get; set; }
    public IEnumerator IE_ChangeTypeTextureExit { get; set; }
    public GuPushMoveState GetGuPushMoveState { get => playerThreeType.stateGu.guPushMoveState; }
    public void ResetYiState(WaterBallState _YiWaterBallState) => playerThreeType.stateYi.ResetYiState(_YiWaterBallState);

    public bool GetCanAddMagicBar { get => playerThreeType.CanAddMagicBar(); }
    #endregion

    #region From PlayerMenu Function or Variable

    public bool MagicBarNotHaveMagic { get => playerMenu.MagicBarNotHaveMagic(); }
    public void PlayerMagicBar_SetValue(float _value)
    {
        playerMenu.PlayerMagicBar_ChangeValue(_value);
    }
    public void SetNowType(PlayerType _playerType)
    {
        playerMenu.UseUi_NowTypeIcon.sprite = playerDatabase.TypeSprite[(int)_playerType];
        PlayerNowType = _playerType;
    }
    #endregion


    #region From PlayerThreeType Function
    public float GetFlyPlayerDirection_Y(float _m_Direction_y) //飛行中(飛行Y -> 0) or 沒飛行(沒飛行Y -> 原本的m_Direction_y)
    {
        if (GetQuUseSkill)
            return 0;
        return _m_Direction_y;
    }

    // Get玩家的向量(Direction)的Y值 (在放開按鍵 -> 丟 GetJumpSpeed * GetStateQuHeavyJump   
    public float GetQuDirection_Y ()  => GetStateQu.GetHeavyJumpValue();

    //看下面是不是水地板
    public bool CheckWaterGround() => playerThreeType.CheckWaterGround();

    #endregion

    #region From PlayerCombat Function & Variable
    public CombatStatus GetCombatStatus { get => playerCombat.GetCombatStatus; }
    public bool SetAttackEnable {set => playerCombat.attackEnable = value; }
    public bool GetIsTakeArms { get => playerCombat.isTakeArms; }
    public bool GetIsSwitchArms { get => playerCombat.isSwitchArms; }
    public bool GetIsAttack { get => playerCombat.isAttack; }
    public bool GetIsSkill { get => playerCombat.isSkill; }
    public int GetAttackCount { get => playerCombat.attackCount; }

    public bool GatherAttack { get => playerCombat.GatherAttack(); }   
    public Transform GetNowMonsterTarget() => playerCombat.NowMonsterTarget != null ? playerCombat.NowMonsterTarget : null;
    #endregion

    #region From CardManager Function & Variable

    #endregion

    #region From MissionManager Function & Variable
    public MissionManager GetMissionManager { get => missionManager; }
    public bool GetCheckHaveNPC { get => missionManager.checkHaveNPC; }

    public bool GetIsTalk { get => missionManager.isTalking; }
    #endregion

    #region From Backpack Function & Variable
    public Backpack GetBackpack { get => backpack; }
    public int NPC_CheckItem(Item npc_Wantitem) => backpack.NPC_CheckItem(npc_Wantitem);
    #endregion

    private PlayerKeyCode[] playerKeyCodes = new PlayerKeyCode[11];
    private List<MonoBehaviour> list_playerAllComponents = new List<MonoBehaviour>();

    [Header(" For Interact")]
    [SerializeField] private float interactRadius = 5f;
    [SerializeField] private LayerMask interationLayer;
    List<Transform> _Interact_Transforms = new List<Transform>();
    List<float> Item2Player_distances = new List<float>();

    /// <summary>
    /// Material
    /// </summary>
    [Header("Materials")]
    public Material material_Cloth_Basic;
    public Material material_Cloth_Dissolve;
    public Material material_Face_Basic, material_Face_Dissolve;
    public Material material_Hat_Basic01, material_Hat_Basic02, material_Hat_Dissolve;

    private List<Renderer> renderer_Cloth, renderer_Face, renderer_Hat;


    [Header("Sound")]
    public Sound[] sounds;
    
    private KeyCode interactKeycode;
    private IInteractable interactable;

    private void Awake()
    {
        //一些初始化設定
        NowPlayerMoveType = PlayerMoveType.CanMove;

        playerKeyCodes = new PlayerKeyCode[11];
        playerKeyCodes[0] = new PlayerKeyCode("Run", KeyCode.LeftShift);
        playerKeyCodes[1] = new PlayerKeyCode("UseType", KeyCode.Space);
        playerKeyCodes[2] = new PlayerKeyCode("ChangeType", KeyCode.Tab);
        playerKeyCodes[3] = new PlayerKeyCode("TakeItemBasic", KeyCode.F);
        playerKeyCodes[4] = new PlayerKeyCode("UseBackpack", KeyCode.B);
        playerKeyCodes[5] = new PlayerKeyCode("SwitchArms", KeyCode.Q);
        playerKeyCodes[6] = new PlayerKeyCode("Interact", KeyCode.E);
        playerKeyCodes[7] = new PlayerKeyCode("Attack", KeyCode.Mouse0);
        playerKeyCodes[8] = new PlayerKeyCode("LClick", KeyCode.Mouse0);
        playerKeyCodes[9] = new PlayerKeyCode("RClick", KeyCode.Mouse1);
        playerKeyCodes[10] = new PlayerKeyCode("ESC", KeyCode.Escape);
        //增加的時候記得修改 New PlayerKeyCode["---> 這邊 <---"];


        playerControl = gameObject.GetComponent<PlayerControl>();
        playerControlAnimation = gameObject.GetComponent<PlayerControlAnimation>();
        playerThreeType = gameObject.GetComponent<PlayerThreeType>();
        playerMenu = gameObject.GetComponent<PlayerMenu>();
        playerCombat = gameObject.GetComponent<PlayerCombat>();
        missionManager = gameObject.GetComponent<MissionManager>();
        backpack = gameObject.GetComponent<Backpack>();
        cardManager = gameObject.GetComponent<CardManager>();
        m_Outline = gameObject.GetComponent<Outline>(); // Outline

        //Initial all component //應該有語法是 直接 Get All Component
        list_playerAllComponents.Add(playerControl);
        list_playerAllComponents.Add(playerControlAnimation);
        list_playerAllComponents.Add(playerThreeType);
        list_playerAllComponents.Add(playerMenu);
        list_playerAllComponents.Add(playerCombat);
        list_playerAllComponents.Add(missionManager);
        list_playerAllComponents.Add(backpack);
        list_playerAllComponents.Add(m_Outline); //Outline

        //Material
        Renderer[] _render_Player = gameObject.GetComponentsInChildren<Renderer>();
        renderer_Cloth = new List<Renderer>();
        renderer_Face = new List<Renderer>();
        renderer_Hat = new List<Renderer>();
        for (int i = 0; i < _render_Player.Length; i++)
        {
            if (_render_Player[i].sharedMaterials[0] == material_Cloth_Basic)
                renderer_Cloth.Add(_render_Player[i]);
            else if (_render_Player[i].sharedMaterials[0] == material_Face_Basic)
                renderer_Face.Add(_render_Player[i]);
            else if (_render_Player[i].sharedMaterials[0] == material_Hat_Basic01 || _render_Player[i].sharedMaterials[0] == material_Hat_Basic02)
                renderer_Hat.Add(_render_Player[i]);
        }
        _render_Player = null;
    }
    void Start()
    {
        gameManager = GameManager.Instance_GameManager;

        //About Interact 
        interactKeycode = GetPlayerKeyCode("Interact").keycode;

        Audio_InitialSounds(sounds, gameObject);
        //playerDatabase.StartData();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("ResetPlayerMissioon");
            playerDatabase.ResetMission();
        }

        InteractCanInteractionGameObject();
    }

    public void SetPlayerComponentEnable(bool _enable)   //除了 PlayerManager而已
    {
        //All component
        for (int i = 0; i < list_playerAllComponents.Count; i++)        
            list_playerAllComponents[i].enabled = _enable;   
        
    }
    public void SetPlayerComponentEnable(string _component ,bool _enable)
    {
        //Maybe Bad Search
        for (int i = 0; i < list_playerAllComponents.Count; i++)
        {
            if (_component == list_playerAllComponents[i].GetType().ToString())
            {
                list_playerAllComponents[i].enabled = _enable;
                //Debug.Log(list_playerAllComponents[i].ToString());
                break;
            }
        }
    }
    public PlayerKeyCode GetPlayerKeyCode(string _keycodeName)
    {
        PlayerKeyCode _playerKeycode = new PlayerKeyCode(null, KeyCode.None);
        for (int i = 0; i < playerKeyCodes.Length; i++)
        {
            if (_keycodeName == playerKeyCodes[i].name)
            {
                _playerKeycode = playerKeyCodes[i];
                break;
            }
        }
        return _playerKeycode;
    }

    public void Totem_SetPlayer(float _disappearTime)
    {
        SetPlayerComponentEnable(false);
        SetTrigger("TotemTrigger");

        playerControl.Totem_Use_PlayerControl(_disappearTime);

        for (int i = 0; i < renderer_Cloth.Count; i++)        
            renderer_Cloth[i].material = material_Cloth_Dissolve;
        for (int i = 0; i < renderer_Face.Count; i++)
            renderer_Face[i].material = material_Face_Dissolve;
        for (int i = 0; i < renderer_Hat.Count; i++)        
            renderer_Hat[i].material = material_Hat_Dissolve;
        

        StartCoroutine(SetPlayerMaterials());
        IEnumerator SetPlayerMaterials()
        {
            float amount = 0f;
            while(amount < _disappearTime)
            {
                amount += Time.deltaTime;
                float materialDisappearTime = amount / (_disappearTime / 2) - 1;
                material_Cloth_Dissolve.SetFloat("Vector1_E7F711AD", materialDisappearTime);
                material_Face_Dissolve.SetFloat("Vector1_E7F711AD", materialDisappearTime);
                material_Hat_Dissolve.SetFloat("Vector1_A2092C6B", materialDisappearTime);

                yield return null;
            }

        }
    }

    private void InteractCanInteractionGameObject()
    {
        if (!Physics.CheckSphere(transform.position, interactRadius, interationLayer))
        {
            interactable = null;
            return;
        }

        Collider[] checkCanInteractColliders = Physics.OverlapSphere(transform.position, interactRadius, interationLayer);
        _Interact_Transforms.Clear();
        for (int i = 0; i < checkCanInteractColliders.Length; i++)
        {
            _Interact_Transforms.Add(checkCanInteractColliders[i].transform);
        }
        EasySort();            
        interactable = _Interact_Transforms[0].GetComponent<IInteractable>();        
        if (interactable != null)
        {
            interactable.IsInRangeInteract = true;
            StartCoroutine(IE_CheckInteract());
        }

        if (interactable != null && Input.GetKeyDown(interactKeycode))
        {
            Audio_PlayAudio(sounds, "Interact");
            interactable.Interaction();
        }

        void EasySort()
        {
            if (_Interact_Transforms.Count <= 1)
                return;
            Item2Player_distances.Clear();

            //將道具~玩家的距離 丟到Item2Player_distances List裡面
            for (int i = 0; i < _Interact_Transforms.Count; i++)
            {
                Vector3 d_distance = _Interact_Transforms[i].position - gameObject.transform.position;
                float distance = d_distance.magnitude;
                Item2Player_distances.Add(distance);
            }
            //Sort 將距離玩家最近的Item調換到0
            for (int i = 0; i < _Interact_Transforms.Count; i++)
            {
                if (Item2Player_distances[0] > Item2Player_distances[i])
                {
                    Swap.SwapList<float>(Item2Player_distances, 0, i);
                    Swap.SwapList<Transform>(_Interact_Transforms, 0, i);
                }
            }
        }
        IEnumerator IE_CheckInteract()
        {
            if (interactable == null)
                yield break;
            IInteractable beforeInteractable = interactable;
            yield return null;
            if (interactable != beforeInteractable || interactable == null)
                beforeInteractable.IsInRangeInteract = false;
        }
    }

    

    private void OnApplicationQuit()
    {
        playerDatabase.ResetMission();
        playerDatabase.ResetPlayerValue();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}

[System.Serializable]
public struct PlayerKeyCode {
    public string name;
    public KeyCode keycode;
    public PlayerKeyCode(string _name, KeyCode _keycode)
    {
        name = _name;
        keycode = _keycode;
    }
}