using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationPlayerWindow  : EditorWindow
{
    [MenuItem("Tool/AnimationPlayer")]
    public static void ShowWindow()
    {
        GetWindow<AnimationPlayerWindow>();
    }

    static string resPath = "Prefab/Alpha";
    GameObject roleTarget;
    string[] animationClipNames;
    AnimationClip[] animationClips;

    string[] toolBarNames = new string[] { "play", "pause", "stop" };
    int toolBarOption = 2;
    void OnEnable()
    {
        if(roleTarget == null)
        {
            var go = Resources.Load<GameObject>(resPath);
            roleTarget = Instantiate(go);
            InitAnimations();
        }
    }

    void InitAnimations()
    {
        var animator = roleTarget.GetComponent<Animator>();
        animationClips = animator.runtimeAnimatorController.animationClips;
        animationClipNames = new string[animationClips.Length];
        for (int i = 0; i < animationClips.Length; i++)
        {
            animationClipNames[i] = animationClips[i].name;
        }
    }

    void OnGUI()
    {

    }

    void ShowBtns()
    {
        int option = GUILayout.Toolbar(toolBarOption, toolBarNames);
    }


}
