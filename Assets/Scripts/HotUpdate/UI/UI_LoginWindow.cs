using JKFrame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginWindow : UI_WindowBase
{
    [SerializeField] private Button BtnClose;
    [SerializeField] private Button BtnSubmit;
    [SerializeField] private Button BtnRegister;

    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Toggle RememberAccountToggle;
    public override void Init()
    {
        
        // 点击关闭按钮，关闭登录窗口
        BtnClose.onClick.AddListener(() =>
        {
            UISystem.Close<UI_LoginWindow>();
        });

// 点击注册按钮，从登录窗口切换到注册窗口
        BtnRegister.onClick.AddListener(() =>
        {
            UISystem.Close<UI_LoginWindow>();
            UISystem.Show<UI_RegisterWindow>();
        });
        // 登记登录按钮的点击事件
        BtnSubmit.onClick.AddListener(() =>
        {
            // 勾选“记住账号”时，保存输入的账号和密码
            if (RememberAccountToggle.isOn)
            {
                ClientGlobal.Instance.RememberAccount(
                    nameInputField.text, passwordInputField.text);
            }

            // 将账号、密码放入登录申请，发送给服务端
            NetMessageManager.Instance.SendMessageToServer(
                MessageType.C_S_Login,
                new C_S_Login
                {
                    accountInfo = new AccountInfo
                    {
                        playerName = nameInputField.text,
                        password = passwordInputField.text
                    }
                });
        });

        // 账号变化时，重新检查输入格式
        nameInputField.onValueChanged.AddListener(OnInputFieldValueChanged);

        // 密码变化时，也重新检查输入格式
        passwordInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }
    
    private void OnS_C_Login(ulong clientId, INetworkSerializable serializable)
    {
        BtnSubmit.interactable = true;
        // 把收到的消息转成具体的登录结果类型
        S_C_Login netMessage = (S_C_Login)serializable;

        // 没有错误：显示绿色成功提示
        if (netMessage.errorCode == ErrorCode.None)
        {
            ClientGlobal.Instance.EnterGameScene();
        }
        else
        {
            // 有错误：用错误码名称查找本地化文本，显示红色提示
            UISystem.Show<UI_MessagePopupWindow>()
                .ShowMessageByLocalzaionKey(
                    netMessage.errorCode.ToString(), Color.red);
        }

        // 收到结果后，把登录按钮设置为可以点击
       
    }
    
    private void OnInputFieldValueChanged(string arg0)
    {
        // 账号和密码格式都合格，才允许点击登录按钮
        BtnSubmit.interactable =
            AccountFormatUtility.CheckName(nameInputField.text) &&
            AccountFormatUtility.CheckPassword(passwordInputField.text);
    }
    
    public override void OnShow()
    {
        // 窗口显示时，先禁止点击登录按钮
        BtnSubmit.interactable = false;

        // 取出客户端启动时已经加载好的设置
        GameSetting gameSetting = ClientGlobal.Instance.gameSetting;

        // 有记住的账号就填入，否则填入空字符串
        nameInputField.text =
            (gameSetting.rememberPlayerName != null
                ? gameSetting.rememberPlayerName
                : "");

        // 有记住的密码就填入，否则填入空字符串
        passwordInputField.text =
            (gameSetting.rememberPassword != null
                ? gameSetting.rememberPassword
                : "");

        // 收到服务端的登录结果时，执行 OnS_C_Login
        NetMessageManager.Instance.RegisterMessageCallback(
            MessageType.S_C_Login, OnS_C_Login);
    }
    
    public override void OnClose()
    {
        // 窗口关闭时，取消本窗口的登录结果处理方法
        NetMessageManager.Instance.UnRegisterMessageCallback(
            MessageType.S_C_Login, OnS_C_Login);
    }
}
