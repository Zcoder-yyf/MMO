using System;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using HybridCLR.Editor.Commands;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BuildMenuItems
{
    public const string rootFolderPath = "Builds";
    public const string clientFolderPath = "Client";
    public const string serverFolderPath = "Server";

    // 先确认客户端上下文，避免生成 Resources 版本 DLL 后再构建 Addressables 客户端。
    private static bool CheckClientBuildContext()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Debug.LogError("请等待 Unity 完成脚本编译和资源导入，再构建客户端。");
            return false;
        }

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64 ||
            EditorUserBuildSettings.standaloneBuildSubtarget != StandaloneBuildSubtarget.Player)
        {
            Debug.LogError("客户端生成前，请先切换 Build Settings 到 Windows, Mac, Linux / Windows / Intel 64-bit，并等待编译完成；不要在 Dedicated Server 平台下生成客户端 DLL。");
            return false;
        }

#if !ENABLE_ADDRESSABLES
        Debug.LogError("客户端需要 JKFrame 的 Addressables 资源模式。请切换资源模式并等待 Unity 重新编译后再构建。");
        return false;
#else
        if (!PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone)
                .Split(';').Contains("ENABLE_ADDRESSABLES"))
        {
            Debug.LogError("ENABLE_ADDRESSABLES 配置与当前编译状态不一致，请等待重新编译后再构建。");
            return false;
        }
        return true;
#endif
    }
    
    [MenuItem("Project/Build/GenerateDllBytesFile")]
    public static void GenerateDllBytesFile()
    {
        Debug.Log("开始生成dll文本文件");

        //上次生成的 AOT DLL
        string aotUpdateDllDirPath = Path.Combine(System.Environment.CurrentDirectory,
            SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget));
        //上次生成的 HotUpdate DLL
        string hotUpdateDllDirPath = Path.Combine(System.Environment.CurrentDirectory,
            SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));

        string aotDllTextDirPath = Path.Combine(System.Environment.CurrentDirectory, "Assets/Scripts/DllBytes/Aot");
        string hotUpdateDllTextDirPath =
            Path.Combine(System.Environment.CurrentDirectory, "Assets/Scripts/DllBytes/HotUpdate");

        foreach (string dllName in SettingsUtil.AOTAssemblyNames)
        {
            string path = Path.Combine(aotUpdateDllDirPath, $"{dllName}.dll");
            if (!File.Exists(path))
            {
                // HybirdCLR -> Generate -> All 生成裁剪后的 AOT DLL
                throw new FileNotFoundException($"缺少裁剪后的 AOT DLL 请先执行 HybirdCLR -> Generate -> All:{path}", path);
            }
            //true 覆盖原有的bytes文件
            File.Copy(path, Path.Combine(aotDllTextDirPath, $"{dllName}.dll.bytes"),true);
        }

        foreach (string dllName in SettingsUtil.HotUpdateAssemblyNamesExcludePreserved)
        {
            File.Copy(Path.Combine(hotUpdateDllDirPath, $"{dllName}.dll"), Path.Combine(hotUpdateDllTextDirPath,$"{dllName}.dll.bytes"),true);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("完成生成dll文本文件");
    }
    
    // 先构建客户端，再构建服务端。
    [MenuItem("Project/Build/All")]
    public static void All()
    {
        if (!CheckClientBuildContext()) return;
        NewClient();
        Server();
    }

    [MenuItem("Project/Build/UpdateClient")]
    public static void UpdateClient()
    {
        if (!CheckClientBuildContext()) return;
        Debug.Log("开始构建客户端(更新包)");

        BuildFliterAssemblies.ServerMode = false;
        // 按旧工程的顺序：先编译热更新程序集，再更新 Addressables 中的 DLL 文本资源。
        CompileDllCommand.CompileDllActiveBuildTarget();
        GenerateDllBytesFile();

        // 使用上一次完整资源构建的记录，生成对应客户端的内容更新包。
        string path = ContentUpdateScript.GetContentStateDataPath(false);
        Debug.Log($"内容更新构建记录:{path}");
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressablesPlayerBuildResult result = ContentUpdateScript.BuildContentUpdate(settings, path);

        // 只有构建成功才打印完成，避免把失败误认为更新包已经生成。
        if (result == null || !string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"构建客户端(更新包)失败:{result?.Error ?? "未返回构建结果"}");
            return;
        }

        Debug.Log("完成构建客户端(更新包)");
    }

    [MenuItem("Project/Build/NewClient")]
    public static void NewClient()
    {
        if (!CheckClientBuildContext()) return;
        Debug.Log("开始构建客户端(新包)");
        BuildFliterAssemblies.ServerMode = false;
        //生成裁剪好的dll文件
        PrebuildCommand.GenerateAll();
        //执行搬运逻辑
        GenerateDllBytesFile();
        


        // 清理上次留下的 Addressables Catalog 缓存
        string catalogPath = $"{Application.persistentDataPath}/com.unity.addressables";

        if (Directory.Exists(catalogPath))
            Directory.Delete(catalogPath, true);

        //挑出要打进安装包的场景
        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Server"))
            {
                Debug.Log("添加场景" + scenePath);
                sceneList.Add(scenePath);
            }
        }

        string projectRootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = sceneList.ToArray(),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            locationPathName = $"{projectRootPath}/{rootFolderPath}/{clientFolderPath}/Client.exe"
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log("完成构建客户端");
        else
            Debug.LogError($"构建客户端失败:{report.summary.result}。Builds/Client 中的旧 Client.exe 不能用于验证当前代码。");
    }


    [MenuItem("Project/Build/Server")]
    public static void Server()
    {
        Debug.Log("开始构建服务端");
        //关闭华佗
        SettingsUtil.Enable = false;
        //关闭Addressables
        AddressableAssetSettingsDefaultObject.Settings.BuildAddressablesWithPlayerBuild =
            AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
        
        BuildFliterAssemblies.ServerMode = true;

        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Client"))
            {
                Debug.Log("添加场景" + scenePath);
                sceneList.Add(scenePath);
            }
        }
        
        string projectRootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = sceneList.ToArray(),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            locationPathName = $"{projectRootPath}/{rootFolderPath}/{serverFolderPath}/Server.exe"
        };

        

        try
        {
            //查看结果
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded) Debug.Log("完成构建服务端");
            else Debug.LogError($"构建服务端失败:{report.summary.result}");
        }
        finally
        {
            //恢复 HybirdCLR
            SettingsUtil.Enable = true;
            //还原Addressables
            AddressableAssetSettingsDefaultObject.Settings.BuildAddressablesWithPlayerBuild =
                AddressableAssetSettings.PlayerBuildOption.PreferencesValue;
        }

    }
}
    
    
    

