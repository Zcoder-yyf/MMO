using JKFrame;
using UnityEditor;
using UnityEngine;

public class JKMenuItem
{
    [MenuItem("JKFrame/打开存档路径")]
    public static void OpenArchivedDirPath()
    {
        // 使用 Unity 官方的跨平台 API
        // 在 Mac 下会自动打开 Finder，在 Windows 下会自动打开 Explorer
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
    
    [MenuItem("JKFrame/打开框架文档")]
    public static void OpenDoc()
    {
        Application.OpenURL("http://www.yfjoker.com/JKFrame/index.html");
    }
    
    [MenuItem("JKFrame/清空存档")]
    public static void CleanSave()
    {
        SaveSystem.DeleteAll();
    }

#if ENABLE_ADDRESSABLES
    [MenuItem("JKFrame/生成资源引用代码")]
    public static void GenerateResReferenceCode()
    {
        GenerateResReferenceCodeTool.GenerateResReferenceCode();
    }
    [MenuItem("JKFrame/清理资源引用代码")]
    public static void CleanResReferenceCode()
    {
        GenerateResReferenceCodeTool.CleanResReferenceCode();
    }
#endif
}