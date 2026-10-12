using JKFrame;
using UnityEngine;
using UnityEngine.UI;


public class UI_MainMenuWindow : UI_WindowBase
{
    [SerializeField] private Button BtnLogin;
    [SerializeField] private Button BtnRegister;
    [SerializeField] private Button BtnQuit;
    [SerializeField] private Button BtnSettings;
    public override void Init()
    {
        BtnLogin.onClick.AddListener(() =>
        {
            UISystem.Show<UI_LoginWindow>();
        });
        BtnRegister.onClick.AddListener(() =>
        {
            UISystem.Show<UI_RegisterWindow>();
        });
        BtnQuit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        BtnSettings.onClick.AddListener(() =>
        {
            UISystem.Show<UI_GameSettingsWindow>();
        });
        
        
    }
    
    
}
