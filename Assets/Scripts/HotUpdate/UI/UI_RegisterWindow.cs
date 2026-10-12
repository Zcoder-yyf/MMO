using System.Collections;
using System.Collections.Generic;
using JKFrame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_RegisterWindow : UI_WindowBase
{
   [SerializeField] private Button BtnClose;
   [SerializeField] private Button BtnSubmit;
   [SerializeField] private Button BtnLogin;

   [SerializeField] private InputField nameInputField;
   [SerializeField] private InputField passwordInputField;
   [SerializeField] private InputField rePasswordInputField;


   public override void Init()
   {
       // 关闭按钮：点击后关闭注册窗口
       BtnClose.onClick.AddListener(() =>
       {
           UISystem.Close<UI_RegisterWindow>();
       });

       // 注册按钮：读取输入内容，向服务端发送注册申请
       BtnSubmit.onClick.AddListener(() =>
       {
           BtnSubmit.interactable = false;

           NetMessageManager.Instance.SendMessageToServer(
               MessageType.C_S_Register,
               new C_S_Register
               {
                   accountInfo = new AccountInfo
                   {
                       playerName = nameInputField.text,
                       password = passwordInputField.text
                   }
               });
       });

       // 登录按钮：关闭注册窗口，打开登录窗口
       BtnLogin.onClick.AddListener(() =>
       {
           UISystem.Close<UI_RegisterWindow>();
           UISystem.Show<UI_LoginWindow>();
       });

       // 输入内容变化时，重新检查注册按钮是否可以点击
       nameInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
       passwordInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
       rePasswordInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
   }

   private void OnInputFieldValueChanged(string arg0)
   {
       // 三项条件全部满足，注册按钮才允许点击
      BtnSubmit.interactable = AccountFormatUtility.CheckName(nameInputField.text) &&
                               AccountFormatUtility.CheckPassword(passwordInputField.text) &&
                               passwordInputField.text == rePasswordInputField.text;
   }
   
   

   private void OnS_C_Register(ulong clientId, INetworkSerializable serializable)
   {
       // 取出服务端发来的注册结果
       S_C_Register netMessage = (S_C_Register)serializable;

       // 没有错误，显示绿色成功提示
       if (netMessage.errorCode == ErrorCode.None)
       {
           UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzaionKey("注册已成功", Color.green);
       }
       else
       {
           // 用错误码的名字查找本地化提示，显示为红色
           UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalzaionKey(netMessage.errorCode.ToString(), Color.red);
       }

       // 收到结果后，重新允许点击注册
       BtnSubmit.interactable = true;
   }

   public override void OnShow()
   {
       // 窗口显示时，先禁用注册按钮
       BtnSubmit.interactable = false;

       // 清空密码和确认密码
       passwordInputField.text = "";
       rePasswordInputField.text = "";

       // 登记服务端注册结果的处理方法
       NetMessageManager.Instance.RegisterMessageCallback(
           MessageType.S_C_Register, OnS_C_Register);
       
       
   }
   
   public override void OnClose()
   {
       // 关闭窗口时，取消本窗口的注册结果回调
       NetMessageManager.Instance.UnRegisterMessageCallback(
           MessageType.S_C_Register, OnS_C_Register);
   }
   
   
}
