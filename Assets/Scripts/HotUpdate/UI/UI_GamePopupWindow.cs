using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamePopupWindow : UI_WindowBase
{
    [SerializeField] private Button BtnContinue;
    [SerializeField] private Button BtnSetting;
    [SerializeField] private Button BtnBack;
    [SerializeField] private Button BtnQuit;

    void Start()
    {
        BtnContinue.onClick.AddListener(() =>
        {
            UISystem.Close<UI_GamePopupWindow>();
        });
        
        BtnSetting.onClick.AddListener(() =>
        {
            UISystem.Show<UI_GameSettingsWindow>();

        });
        
        BtnBack.onClick.AddListener(() =>
        {
            //退出到菜单场景
            NetMessageManager.Instance.SendMessageToServer<C_S_Disconnet>(MessageType.C_S_Disconnet, default);
            // ClientGlobal.Instance.EnterLoginScene();
            //等待服务端回复消息后退出到登陆场景 由ClientGlobal来监听
        });
        
        BtnQuit.onClick.AddListener(() =>
        {
            //完全关闭应用 不用理会网络连接问题 因为NetCode 会自动处理
            Application.Quit();
        });
    }
}
