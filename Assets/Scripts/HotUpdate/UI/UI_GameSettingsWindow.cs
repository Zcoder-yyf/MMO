using JKFrame;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameSettingsWindow : UI_WindowBase
{
    [SerializeField] private Button BtnClose;
    [SerializeField] private Button BtnLanguagePrev;
    [SerializeField] private Button BtnLanguageNext;
    [SerializeField] private Image ImageLanguage;
    [SerializeField] private Sprite EnglishIcon;
    [SerializeField] private Sprite ChineseIcon;
    [SerializeField] private Slider MusicVolumeSlider;
    [SerializeField] private Slider SoundEffectSlider;


    public override void Init()
    {
        BtnClose.onClick.AddListener(() =>
        {
            UISystem.Close<UI_GameSettingsWindow>();
        });
        
        BtnLanguagePrev.onClick.AddListener(() =>
        {
            LocalizationSystem.LanguageType = LocalizationSystem.LanguageType == LanguageType.SimplifiedChinese
                ? LanguageType.English
                : LanguageType.SimplifiedChinese;
            SetLanguageIcon(LocalizationSystem.LanguageType);
        });
        
        BtnLanguageNext.onClick.AddListener(() =>
        {
            LocalizationSystem.LanguageType = LocalizationSystem.LanguageType == LanguageType.SimplifiedChinese
                ? LanguageType.English
                : LanguageType.SimplifiedChinese;
            SetLanguageIcon(LocalizationSystem.LanguageType);
        });
        
        MusicVolumeSlider.onValueChanged.AddListener(MusicVolumeSliderValueChanged);
        SoundEffectSlider.onValueChanged.AddListener(SoundEffectSliderValueChanged);
        
    }

    public override void OnShow()
    {
        SetLanguageIcon(ClientGlobal.Instance.basicSetting.languageType);
        MusicVolumeSlider.SetValueWithoutNotify(ClientGlobal.Instance.gameSetting.musicVolume);
        SoundEffectSlider.SetValueWithoutNotify(ClientGlobal.Instance.gameSetting.soundEffectVolume);
    }

    private void SetLanguageIcon(LanguageType languageType)
    {
        ImageLanguage.sprite = languageType == LanguageType.SimplifiedChinese ? ChineseIcon : EnglishIcon;
    }

    private void SoundEffectSliderValueChanged(float arg0)
    {
        ClientGlobal.Instance.gameSetting.musicVolume = arg0;
        AudioSystem.BGVolume = arg0;
        EventSystem.TypeEventTrigger<MusicVolumeChangeEvent>(default);
    }

    private void MusicVolumeSliderValueChanged(float arg0)
    {
        ClientGlobal.Instance.gameSetting.soundEffectVolume = arg0;
        AudioSystem.EffectVolume = arg0;
    }

    public override void OnClose()
    {
        ClientGlobal.Instance.SaveGameSetting();
        ClientGlobal.Instance.SaveGameBasicSetting();
    }
}

