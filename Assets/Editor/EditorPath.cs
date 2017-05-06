using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EditorPath
{
    public static string AssetPath = "Assets/";
    public static string ResourcePath = AssetPath + "Resources/";
    public static string PrebfabPath = ResourcePath + "Game/Prefab/";
    public static string UIPrefabPath = PrebfabPath + "UI/";
    public static string ModelPath = ResourcePath + "Model/";
    public static string EffectPath = ResourcePath + "Effect/";
    public static string ScenePath = ResourcePath + "Scene/";
    public static string ShaderPath = ResourcePath + "Shader/";
    public static string CompletePath = ResourcePath + "Complete/";
    public static string BundleByFolderPath = ResourcePath + "BundleByFolder/";
    public static string AudioPath = ResourcePath + "Audio/";
    public static string UIShaderPath = ResourcePath + "Shader/CustomUI";

    public static string UIResPath = AssetPath + "UI/";

    static string ABExt = ".ab";
    public static string FBXExt = ".FBX";
    public static string AnimExt = ".anim";
    public static string PrefabExt = ".prefab";

    public static string AnimationFolderPath = "/Animation";

    public static string GetFullBundleName(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return string.Empty;
        }
        if (bundleName.EndsWith(ABExt))
            return bundleName;
        return bundleName + ABExt;
    }

    public static string GetBundleRelativePath(string assetPath)
    {
        if (assetPath.StartsWith(ResourcePath))
        {
            assetPath = assetPath.Replace(ResourcePath, string.Empty);
        }
        else
        {
            assetPath = assetPath.Replace(AssetPath, string.Empty);
        }
        return assetPath;
    }

    public static string ToLower(string path)
    {
        return path.ToLower();
    }

    public static string GetFolderPath(string assetPath)
    {
        return Path.GetDirectoryName(assetPath);
    }

    public static string RemoveExtension(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        var index = path.LastIndexOf(".");
        if (index == -1)
            return path;
        path = path.Substring(0, index);
        return path;
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }
    public static string GetPathExt(string path)
    {
        return Path.GetExtension(path);
    }

    //@todo  判断路径是否合法（中文，空格等检测）
    public static bool CheckValidPath(string path)
    {
        return true;
    }

    public static string AssetPathToFullPath(string unityPath)
    {
        return Application.dataPath + unityPath.Replace("Assets", "");
    }

    public static string FullPathToAssetPath(string fullPath)
    {
        return fullPath.Replace(Application.dataPath, "Assets");
    }

    //model相关
    public static bool CheckIsModelPath(string assetPath)
    {
        return assetPath.StartsWith(ModelPath);
    }

    public static string GetAnimationFilePath(string path, string fileName)
    {
        return path + AnimationFolderPath + "/" + fileName + AnimExt;
    }

    public static string GetModelPrefabPath(string assetPath)
    {
        string fileName = GetFileName(assetPath);
        return PrebfabPath + "Model/" + fileName + PrefabExt;
    }

    public static bool IsUIResPath(string assetPath)
    {
        return assetPath.StartsWith(UIResPath);
    }
}

