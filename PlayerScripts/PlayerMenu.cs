using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoerusaGameManager;

public class PlayerMenu : MonoBehaviour,IDamage
{
    private PlayerManager playerManager;

    [SerializeField, Range(0, 100)] 
    private float health, magicBar;


    public Image ui_NowTypeIcon, ui_Health, ui_MagicBar;
    public Image UseUi_NowTypeIcon {get => ui_NowTypeIcon; set => ui_NowTypeIcon = value; }
    public float GetHealth { get => health; }
    public float GetMagicBar { get => magicBar; }

    [Header("//AddMagicBar//")]
    [Range(0.5f, 4f)] public float howLong_AddMagicBar = 1.5f;
    [Range(1f, 10f)] public float howToFast_AddMagicBar = 6f;

    [Space(10)]
    public float BeHitCD = 1f;
    private bool canBeHit = true; //讓 hit 不要連續 hit

    private float _riseMagicTime = 0;

    public Sound[] sounds;
    private void Start()
    {
        playerManager = gameObject.GetComponent<PlayerManager>();

        //將數值從Scriptable傳到這邊
        playerManager.playerDatabase.SetPlayerMenu(ref health,ref magicBar);
        playerManager.SetNowType(playerManager.PlayerNowType);

        GameManager.Instance_GameManager.Audio_InitialSounds(sounds, gameObject);
    }
    void Update()
    {
        //playerManager.SetPlayerComponentEnable("PlayerThreeType", magicBar > 0f);

        PlayerMenuUpdata(ui_Health, health, playerManager.GetMaxHealth);
        PlayerMenuUpdata(ui_MagicBar, magicBar, playerManager.GetMaxMagicBar);

        if (CanAddMagicBar(howLong_AddMagicBar))    //如果沒有在轉換型態 回魔
            PlayerMagicBar_ChangeValue(howToFast_AddMagicBar * Time.deltaTime);
    }
    void PlayerMenuUpdata(Image _UI,float _value,float _maxValue)
    {
        float _playerMenu = _value / _maxValue;
        _UI.transform.localScale = new Vector3 (_playerMenu , 1 , 1);
    }
    bool PlayerDie()
    {
        if (health <= 0f)
        {
            ui_Health.transform.localScale = new Vector3(0, 1, 1);
            GameManager.Instance_GameManager.DieDisplay(true);
            //主角死亡動作
            //主角GameOver UI
            return true;
        }
        return false;
    }
    public void PlayerHealth_ChangeValue(float _ChangeVlaue)
    {
        health += _ChangeVlaue;
        health = Mathf.Clamp(health, 0, playerManager.GetMaxHealth);
    }
    public void PlayerMagicBar_ChangeValue(float _ChangeVlaue)
    {
        //要沒魔 且 為負值
        if (magicBar <= 1f && _ChangeVlaue < 0 ) //方案二 當 Reduce 過後 小於 0 則不扣 Return                    
            return;
        
        magicBar += _ChangeVlaue;
        magicBar = Mathf.Clamp(magicBar, 0, playerManager.GetMaxMagicBar);
    }


    bool CanAddMagicBar(float _howLong) //為True才可回魔 
    {   
        if (magicBar >= 100)
            return false;
        if (playerManager.GetCanAddMagicBar && !playerManager.UseSkill())
        {
            _riseMagicTime += Time.deltaTime;
            if (_riseMagicTime >= _howLong)
                return true;
        }
        else
            _riseMagicTime = 0;
        
        return false;
    }
    public bool MagicBarNotHaveMagic()
    {
        //MB Shake or red

        return magicBar <= 1f;
    }

    public bool BeDamage(float _damage,Transform _target, Vector3 _hitPos, EffectObjectBasic _hitEffect)
    {
        if (!canBeHit)
            return false;
            
        Animator m_ani = gameObject.GetComponent<Animator>();
        
        m_ani.enabled = false;
                    
        //Vector3 direction = transform.position - _target.position;
        //direction.y = 0;
        //playerManager.MovementDirection = direction.normalized * _damage * Time.deltaTime;        
        //Debug.DrawLine(transform.position, direction, Color.red, 1f);

        m_ani.enabled = true;
        
        PlayerHealth_ChangeValue(-_damage);

        //ANIMATION BeHit            
        playerManager.SetTrigger("BeHit");

        //Sound
        playerManager.Audio_PlayAudio(sounds, "BeHit");

        if (PlayerDie())
        {
            playerManager.SetTrigger("Die");
            
            playerManager.SetPlayerComponentEnable("PlayerMenu", false);
            playerManager.SetPlayerComponentEnable("PlayerControl", false);
            playerManager.SetPlayerComponentEnable("PlayerControlAnimation", false);
            playerManager.SetPlayerComponentEnable("PlayerThreeType", false);
            playerManager.SetPlayerComponentEnable("Backpack", false);
            playerManager.SetPlayerComponentEnable("MissionManager", false);
            playerManager.SetPlayerComponentEnable("PlayerCombat", false);

            playerManager.UseController.enabled = false;
        }
        else
            StartCoroutine(WaitBeHitCD(BeHitCD)); // X 秒 之後 才會計算 被攻擊

        IEnumerator WaitBeHitCD(float _time)
        {
            //Material Chacnge
            canBeHit = false;
            yield return new WaitForSeconds(_time);
            //Material Chacnge to basic
            canBeHit = true;
        }

        return true;
    }

    private void OnApplicationQuit()
    {
        //將數值傳給Scriptable
        playerManager.playerDatabase.GetPlayerMenu(health, magicBar);
    }
}
