using Codice.CM.Common;
using JKFrame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ClientGlobal : SingletonMono<ClientGlobal>
{
    public GameSetting gameSetting { get; private set; }
    public GameBasicSetting basicSetting { get; private set; }


    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);
        
        LoadGameSetting();
        

        NetworkVaribaleSerializationBinder.Init();
        InitWindowData();
        LocalizationSystem.GlobalConfig = ResSystem.LoadAsset<LocalizationConfig>("GlobalLocalizationConfig");
        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);
        EventSystem.AddTypeEventListener<GameSceneLaunchEvent>(OnGameSceneLaunchEvent);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_Disconnet, OnDisconnet);
        JKLog.Succeed("InitClient");
        EnterLoginScene();


    }

    

    private void InitWindowData()
    {
        UISystem.AddUIWindowData<UI_MainMenuWindow>(new UIWindowData(false, nameof(UI_MainMenuWindow), 0));
        UISystem.AddUIWindowData<UI_MessagePopupWindow>(new UIWindowData(true, nameof(UI_MessagePopupWindow), 4));
        UISystem.AddUIWindowData<UI_RegisterWindow>(new UIWindowData(false, nameof(UI_RegisterWindow), 1));
        UISystem.AddUIWindowData<UI_LoginWindow>(new UIWindowData(false, nameof(UI_LoginWindow), 1));
        UISystem.AddUIWindowData<UI_GamePopupWindow>(new UIWindowData(false, nameof(UI_GamePopupWindow), 3));
        UISystem.AddUIWindowData<UI_GameSettingsWindow>(new UIWindowData(false, nameof(UI_GameSettingsWindow), 3));
    }

    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ResSystem.InstantiateGameObject("ClientOnGameScene");
    }
    
    public void SaveGameSetting()
    {
        // 把当前的游戏设置保存到本机文件
        SaveSystem.SaveSetting(gameSetting);
    }
    
    private void LoadGameSetting()
    {
        // 从本机设置文件中，读取 GameSetting 类型的数据
        gameSetting = SaveSystem.LoadSetting<GameSetting>();
        basicSetting = SaveSystem.LoadSetting<GameBasicSetting>();

        // 没有读到设置时，创建一份新的设置
        if (gameSetting == null)
        {
            gameSetting = new GameSetting();

            // 把新设置保存下来
            SaveGameSetting();
        }

        if (basicSetting == null)
        {
            basicSetting = new GameBasicSetting();
            basicSetting.languageType = Application.systemLanguage == SystemLanguage.ChineseSimplified
                ? LanguageType.SimplifiedChinese
                : LanguageType.English;
        }

        LocalizationSystem.LanguageType = basicSetting.languageType;
        ClientGlobal.Instance.basicSetting.languageType = LocalizationSystem.LanguageType;
        AudioSystem.BGVolume = gameSetting.musicVolume;
        AudioSystem.EffectVolume = gameSetting.soundEffectVolume;
        SaveGameSetting();
    }

    public void SaveGameBasicSetting()
    {
        SaveSystem.SaveSetting(basicSetting);
    }
    
    
    public void RememberAccount(string name, string password)
    {
        // 将传进来的账号和密码，放入当前设置对象
        gameSetting.rememberPlayerName = name;
        gameSetting.rememberPassword = password;

        // 将更新后的设置保存到本机文件
        SaveGameSetting();
    }

    public void EnterLoginScene()
    {
        UISystem.CloseAllWindow();
        Addressables.LoadSceneAsync("LoginScene").WaitForCompletion();
    }

    public void EnterGameScene()
    {
        UISystem.CloseAllWindow();
        SceneSystem.LoadScene("GameScene");
    }
    
    private void OnDisconnet(ulong clientId, INetworkSerializable serializable)
    {
        S_C_Disconnet message = (S_C_Disconnet)serializable;
        if (message.errorCode == ErrorCode.None)
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzaionKey(message.errorCode.ToString(), Color.red);
            if (message.errorCode == ErrorCode.AccountRepeatLogin)
            {
                Invoke(nameof(EnterLoginScene), 1);
            }
        }
        else
        {
            EnterLoginScene();
        }
        
        
    }






}
