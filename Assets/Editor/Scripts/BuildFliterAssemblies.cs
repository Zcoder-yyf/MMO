using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class BuildFliterAssemblies : IFilterBuildAssemblies
{
    public int callbackOrder => 1; //一个顺序 哪个程序集先处理
    public static bool ServerMode = false;
    public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
    {
        
        if (ServerMode)
        {
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = !ass.Contains("HotUpdate");
                if (!reserved)
                {
                    Debug.Log($"ServerBuildFliterAssemblies过滤了{assName}程序集");
                }

                return reserved;
            }).ToArray();
        }
        else
        {
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = !ass.Contains("Server");
                if (!reserved)
                {
                    Debug.Log($"ServerBuildFliterAssemblies过滤了{assName}程序集");
                }

                return reserved;
            }).ToArray();
        }
        
    }
}
