using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoerusaGameManager;

public enum Backpack_Type
{
    Item_Button,
    Card_Button
}
public class BackpackUIManagement : MonoBehaviour
{
    private GameManager gameManager;    

    [SerializeField]
    private Backpack playerBackpack;
    [SerializeField] 
    private BackpackDisplay backpackDisplay;

    public Gradient backpackButton_Error;
    private IEnumerator IE_GradientBackpackButtonShake;
    private Vector3 basicPos;

    //記得把子物件.SetActive == False
    private GameObject[] Backpack_ChildGameObjects; // 看這個物件有幾個子物件就多少
    private SwitchUI_OpenOrClose backpack_switch = SwitchUI_OpenOrClose.Close;

    public Button BackpackButton; // -> Backpack Button (右下角的
    public Button closeBackpackButton;    // (開啟之後中間下面的關閉按鈕

    #region Item or Card Button ( Open backpack after
    [Header("Item or Card Button")]
    public Gradient backpack_Type_Gradient;
    public Button itemButton, cardButton;
    public Gradient backpackButtonText_Gradient;
    public Text itemButton_Text, cardButton_Text;
    #endregion


    public Backpack_Type GetBackpack_Type { get => m_backpack_Type; }   // 這個要給其他Script check
    private Backpack_Type m_backpack_Type;

    private KeyCode useBackpackKeyCode;

    //Sound
    public Sound[] sounds;
    private void Awake()
    {
        Backpack_Initial();

        itemButton.onClick.AddListener(OnItemButtonClick);
        cardButton.onClick.AddListener(OnCardButtonClick);
        closeBackpackButton.onClick.AddListener(OnSwitchBackpack);
    }
    

    void Backpack_Initial()
    {
        Backpack_ChildGameObjects = new GameObject[gameObject.transform.childCount];
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Backpack_ChildGameObjects[i] = gameObject.transform.GetChild(i).gameObject;
            Backpack_ChildGameObjects[i].SetActive(false);
        }
        BackpackButton = GameObject.Find("Backpack Button").GetComponent<Button>();
        BackpackButton.onClick.AddListener(OnSwitchBackpack); //點擊觸發OpenBackPack
    }
    private void Start()
    {
        if (playerBackpack == null)
            playerBackpack = GameManager.Instance_GameManager.GetPlayerBackpack;

        gameManager = GameManager.Instance_GameManager;
        gameManager.Audio_InitialSounds(sounds, gameObject);

        StartCoroutine(WaitEndFrameCheckUseBackpackKeyCode());

        basicPos = BackpackButton.transform.position;
    }
    void Update()
    {
        if (Input.GetKeyDown(useBackpackKeyCode) || (backpack_switch == SwitchUI_OpenOrClose.Open && Input.GetKeyDown(KeyCode.Escape)))
            OnSwitchBackpack();

        switch (GetBackpack_Type)
        {
            case Backpack_Type.Item_Button:
                itemButton.image.color = backpack_Type_Gradient.Evaluate(1);
                cardButton.image.color = backpack_Type_Gradient.Evaluate(0);

                itemButton_Text.color = backpackButtonText_Gradient.Evaluate(1);
                cardButton_Text.color = backpackButtonText_Gradient.Evaluate(0);
                break;
            case Backpack_Type.Card_Button:
                itemButton.image.color = backpack_Type_Gradient.Evaluate(0);
                cardButton.image.color = backpack_Type_Gradient.Evaluate(1);

                itemButton_Text.color = backpackButtonText_Gradient.Evaluate(0);
                cardButton_Text.color = backpackButtonText_Gradient.Evaluate(1);
                break;
            default:
                break;
        }        
    }
    IEnumerator WaitEndFrameCheckUseBackpackKeyCode()
    {
        yield return new WaitForEndOfFrame();
        useBackpackKeyCode = playerBackpack.GetUseBackpackKeyCode;
    }


    #region Item and Card Button (Click)Function 
    void OnItemButtonClick()    //按下Item 的 按鈕
     {
        if (GetBackpack_Type == Backpack_Type.Card_Button)
        {
            ///false ->先刪除原本的Container的Slot
            ///轉換成另一個Container
            ///true  ->在創造轉過的Container的Slot
            backpackDisplay.enabled = false;
            m_backpack_Type = Backpack_Type.Item_Button;               
            backpackDisplay.enabled = true;
            backpackDisplay.Initialization_nowWhatCountSlot_AND_useDispalyObject();

            //itemButton 暗 - cardButton亮
            itemButton.image.color = backpack_Type_Gradient.Evaluate(0);
            cardButton.image.color = backpack_Type_Gradient.Evaluate(1);

            itemButton_Text.color = backpackButtonText_Gradient.Evaluate(0);
            cardButton_Text.color = backpackButtonText_Gradient.Evaluate(1);
        }
    }
    void OnCardButtonClick()    //按下Card 的 按鈕
    {        
        if (GetBackpack_Type == Backpack_Type.Item_Button)
        {
            ///false ->先刪除原本的Container的Slot
            ///轉換成另一個Container
            ///true  ->在創造轉過的Container的Slot
            backpackDisplay.enabled = false;
            m_backpack_Type = Backpack_Type.Card_Button;
            backpackDisplay.enabled = true;
            backpackDisplay.Initialization_nowWhatCountSlot_AND_useDispalyObject();

            //itemButton 亮 - cardButton暗
            itemButton.image.color = backpack_Type_Gradient.Evaluate(1);
            cardButton.image.color = backpack_Type_Gradient.Evaluate(0);

            itemButton_Text.color = backpackButtonText_Gradient.Evaluate(1);
            cardButton_Text.color = backpackButtonText_Gradient.Evaluate(0);
        }
    }

    //開啟或關閉背包
    void OnSwitchBackpack() // now is Open Click -> Close,  now is Close Click -> Open 
    {
        if (GameManager.Instance_GameManager.GetPlayerManager.GetIsOpenUiTypeDisplay)
            return;

        switch (backpack_switch)
        {
            case SwitchUI_OpenOrClose.Open:
                backpackDisplay.Initialization_nowWhatCountSlot_AND_useDispalyObject();
                for (int i = 0; i < gameObject.transform.childCount; i++) 
                {
                    Backpack_ChildGameObjects[i].SetActive(false);
                }
                backpack_switch = SwitchUI_OpenOrClose.Close;

                gameManager.MainScreen_Switch(true, GameManager.GameMainCanvas.Backpack);
                gameManager.Audio_PlayAudio(sounds, "CloseBackpack");

                break;
            case SwitchUI_OpenOrClose.Close:
                for (int i = 0; i < gameObject.transform.childCount - 1; i++)// 因為Use Display 要等Onclick才出現 所以-1
                {
                    Backpack_ChildGameObjects[i].SetActive(true);
                }
                backpack_switch = SwitchUI_OpenOrClose.Open;

                gameManager.Audio_PlayAudio(sounds, "OpenBackpack");
                gameManager.MainScreen_Switch(false, GameManager.GameMainCanvas.Backpack);//時間暫停 不能使用型態 

                break;
            default:
                break;
        }
    }
    public void OnSwitchBackpack(SwitchUI_OpenOrClose condition) // now is Open Click -> Close,  now is Close Click -> Open 
    {
        if (condition == SwitchUI_OpenOrClose.Close)
        {
            backpackDisplay.Initialization_nowWhatCountSlot_AND_useDispalyObject();
            for (int i = 0; i < gameObject.transform.childCount - 1; i++)// 因為Use Display 要等Onclick才出現
            {
                Backpack_ChildGameObjects[i].SetActive(false);
            }
            backpack_switch = SwitchUI_OpenOrClose.Close;
            //gameManager.MainScreen_Switch(true);//時間暫停 不能使用型態 

            if (backpack_switch == SwitchUI_OpenOrClose.Open)
                gameManager.Audio_PlayAudio(sounds, "CloseBackpack");
        }
         
    }
    #endregion
    public void BackpackButtonSkake()
    {
        if (IE_GradientBackpackButtonShake != null)
        {
            StopCoroutine(IE_GradientBackpackButtonShake);
            BackpackButton.transform.position = basicPos;
        }

        IE_GradientBackpackButtonShake = GradientBackpackButtonShake();        
        StartCoroutine(IE_GradientBackpackButtonShake); 
    }
    IEnumerator GradientBackpackButtonShake()
    {
        float _time = 0f; 
        while (_time < 1f)
        {
            BackpackButton.image.color = backpackButton_Error.Evaluate(_time / 1f);

            float index = Random.Range(-1f, 1f);
            BackpackButton.transform.position += new Vector3(index, index, index);
            _time += Time.deltaTime;
            yield return null;
        }
        BackpackButton.transform.position = basicPos;
    }
}