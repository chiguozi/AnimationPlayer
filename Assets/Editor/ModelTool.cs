using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Animations;

public class ModelTool
{
    static List<string> boneList = new List<string>() { "Bip001", "Bip001 Head", "Bip001 R Clavicle", "Bip001 L Clavicle",
        "Bip001 R Hand", "Bip001 L Hand", "Bip001 R Foot", "Bip001 L Foot", "Bip001 Prop1" };
    static List<string> loopAnimList = new List<string>() { "idle", "run", "walk" };


    //如果没有Controller 删除animator组件
    static void AddControllerToAnimator(GameObject go, AnimatorController controller)
    {
        var animator = go.GetComponent<Animator>();
        if (controller == null)
        {
            GameObject.DestroyImmediate(animator);
        }
        else
        {
            animator.runtimeAnimatorController = controller;
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }
    }

    [MenuItem("Assets/Model/生成人物模型预设")]
    public static void SetModelImportSetting()
    {
        var objs = Selection.objects;
        HashSet<string> fbxPathSet = new HashSet<string>();

        for (int i = 0; i < objs.Length; i++)
        {
            string assetPath = AssetDatabase.GetAssetPath(objs[i]);
            if (!EditorPath.CheckIsModelPath(assetPath))
                continue;
            assetPath = FormatFbxPath(assetPath);
            if (!fbxPathSet.Contains(assetPath))
                fbxPathSet.Add(assetPath);
        }

        if (fbxPathSet.Count == 0)
        {
            Debug.LogError("没有选择模型fbx");
            return;
        }

        var iter = fbxPathSet.GetEnumerator();
        while (iter.MoveNext())
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(iter.Current);
            SetSingleModelImportSetting(iter.Current);
            CopyAnimations(iter.Current);
            var controller = CreateAnimatorController(iter.Current);
            go = CreatePrefab(EditorPath.GetModelPrefabPath(iter.Current), go);
            AddControllerToAnimator(go, controller);
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log("模型生成完成");
    }

    static string FormatFbxPath(string assetPath)
    {
        if (!assetPath.Contains("@"))
            return assetPath;
        assetPath = assetPath.Split('@')[0] + EditorPath.FBXExt;
        return assetPath;
    }

    static void SetSingleModelImportSetting(string assetPath)
    {
        var assetImproter = AssetImporter.GetAtPath(assetPath);
        var modelImproter = assetImproter as ModelImporter;

        if (modelImproter == null)
            return;
        //关闭Read/Write Enable
        modelImproter.isReadable = false;
        //关闭 BlendShapes  用于模型表情
        modelImproter.importBlendShapes = false;
        //关闭lightMapUV
        modelImproter.generateSecondaryUV = false;

        if (modelImproter.animationType != ModelImporterAnimationType.Generic)
            modelImproter.animationType = ModelImporterAnimationType.Generic;

        if (modelImproter.optimizeGameObjects == false)
            modelImproter.optimizeGameObjects = true;

        //设置暴露的骨骼
        List<string> boneNameList = new List<string>();
        for (int i = 0; i < modelImproter.transformPaths.Length; i++)
        {
            var trPath = modelImproter.transformPaths[i];
            if (string.IsNullOrEmpty(trPath))
                continue;
            for (int j = 0; j < boneList.Count; j++)
            {
                if (trPath.EndsWith(boneList[j]))
                    boneNameList.Add(trPath);
            }
        }

        modelImproter.extraExposedTransformPaths = boneNameList.ToArray();
        AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        AssetDatabase.Refresh();
    }

    static void CopyAnimations(string assetPath)
    {
        string assetFolderPath = EditorPath.GetFolderPath(assetPath);
        string fullFolderPath = Application.dataPath + assetFolderPath.Replace("Assets", "");

        EditorFileUtil.FileWalker(fullFolderPath,
            (s, n) =>
            {
                if (s.EndsWith(".FBX") && s.Contains("@"))
                    return true;
                return false;
            }
            ,
            (s, n) =>
            {
                var path = s.Replace(Application.dataPath, "Assets");
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null)
                {
                    CopySingleAnimation(path, clip);
                }
            }
            ,
            FileWalkOption.File,
            false
        );

    }

    static void CopySingleAnimation(string path, AnimationClip clip)
    {
        string fileName = clip.name;
        string folderPath = EditorPath.GetFolderPath(path);
        var newClip = new AnimationClip();

        EditorUtility.CopySerialized(clip, newClip);
        if (!AssetDatabase.IsValidFolder(folderPath + EditorPath.AnimationFolderPath))
        {
            AssetDatabase.CreateFolder(folderPath, EditorPath.AnimationFolderPath.Replace("/", ""));
        }

        for (int i = 0; i < loopAnimList.Count; i++)
        {
            if (fileName.Contains(loopAnimList[i]))
            {
                SetAnimationLoopTime(newClip);
                newClip.wrapMode = WrapMode.Loop;
            }
        }

        AssetDatabase.CreateAsset(newClip, EditorPath.GetAnimationFilePath(folderPath, fileName));
    }

    static void SetAnimationLoopTime(AnimationClip clip)
    {
        var setting = AnimationUtility.GetAnimationClipSettings(clip);
        setting.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, setting);
    }

    static AnimatorController CreateAnimatorController(string assetPath)
    {
        string folder = EditorPath.GetFolderPath(assetPath);

        List<AnimationClip> clipList = new List<AnimationClip>();
        if (!AssetDatabase.IsValidFolder(folder + EditorPath.AnimationFolderPath))
            return null;
        var fullPath = EditorPath.AssetPathToFullPath(folder + EditorPath.AnimationFolderPath);
        EditorFileUtil.FileWalker(fullPath,
            (s, n) =>
            {
                return s.EndsWith(".anim");
            },
            (s, n) =>
            {
                string path = EditorPath.FullPathToAssetPath(s);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                clipList.Add(clip);
            }
            , FileWalkOption.File);
        AnimatorController controller = null;
        if (clipList.Count > 0)
            controller = AnimatorController.CreateAnimatorControllerAtPath(folder + "/controller.controller");
        for (int i = 0; i < clipList.Count; i++)
        {
            var state = controller.AddMotion(clipList[i]);
            if (clipList[i].name.Contains("idle01"))
                controller.layers[0].stateMachine.defaultState = state;
        }
        return controller;
    }

    static GameObject CreatePrefab(string path, GameObject go)
    {
        return PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.ReplaceNameBased);
    }
}
