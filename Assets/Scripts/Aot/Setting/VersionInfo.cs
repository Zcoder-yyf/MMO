using System.Collections.Generic;
using JKFrame;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/VersionInfo")]
public class VersionInfo : ConfigBase
{
    public class VersionData
    {
        [Multiline] 
        public string info;
    }
    
    // 一种语言，对应一份版本说明
    public Dictionary<LanguageType, VersionData> versionInfoDic = new Dictionary<LanguageType, VersionData>();
    public VersionData GetVersionData(LanguageType languageType)
    {
        return versionInfoDic[languageType];
    }
}