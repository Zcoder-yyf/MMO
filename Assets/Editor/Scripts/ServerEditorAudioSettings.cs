using UnityEditor;
using UnityEngine;

// 只处理编辑器中的服务端运行；退出 Play 后恢复原来的音频设置。
[InitializeOnLoad]
public static class ServerEditorAudioSettings
{
    private const string ChangedKey = "MMORPG.ServerEditorAudioSettings.Changed";
    private const string PreviousKey = "MMORPG.ServerEditorAudioSettings.Previous";
    private const string AudioSettingsPath = "ProjectSettings/AudioManager.asset";

    static ServerEditorAudioSettings()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += RestoreAudio;
        // 脚本重编译后也能继续处理当前的服务端运行，或恢复退出时的设置。
        EditorApplication.delayCall += SyncCurrentState;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && IsServerSession())
            DisableAudio();
        else if (state == PlayModeStateChange.EnteredEditMode)
            RestoreAudio();
    }

    private static void SyncCurrentState()
    {
        if (EditorApplication.isPlaying && IsServerSession())
            DisableAudio();
        else if (!EditorApplication.isPlayingOrWillChangePlaymode)
            RestoreAudio();
    }

    private static bool IsServerSession()
    {
        // 开始运行前识别服务端入口，切换到 GameScene 后根据网络角色识别。
        if (Object.FindObjectOfType<ServerLaunch>() != null)
            return true;

        NetManager manager = NetManager.Instance;
        return manager != null && manager.IsServer && !manager.IsClient;
    }

    private static SerializedObject GetAudioSettings()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AudioSettingsPath);
        return assets.Length > 0 ? new SerializedObject(assets[0]) : null;
    }

    private static void DisableAudio()
    {
        SerializedObject settings = GetAudioSettings();
        SerializedProperty disabled = settings?.FindProperty("m_DisableAudio");
        if (disabled == null)
            return;

        // SessionState 跨脚本重载保存最初的设置，避免将临时关闭状态当成原值。
        if (!SessionState.GetBool(ChangedKey, false))
        {
            SessionState.SetBool(PreviousKey, disabled.boolValue);
            SessionState.SetBool(ChangedKey, true);
        }

        disabled.boolValue = true;
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RestoreAudio()
    {
        if (!SessionState.GetBool(ChangedKey, false))
            return;

        SerializedObject settings = GetAudioSettings();
        SerializedProperty disabled = settings?.FindProperty("m_DisableAudio");
        if (disabled == null)
            return;

        disabled.boolValue = SessionState.GetBool(PreviousKey, false);
        settings.ApplyModifiedPropertiesWithoutUndo();
        SessionState.EraseBool(ChangedKey);
        SessionState.EraseBool(PreviousKey);
    }
}
