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
    AnimationClipControl animationCtrl;
    string[] toolBarNames = new string[] { "play", "pause", "stop" };
    int toolBarOption = 2;
    void OnEnable()
    {
        EditorApplication.update += Update;
        if(roleTarget == null)
        {
            var go = Resources.Load<GameObject>(resPath);
            roleTarget = Instantiate(go);
            InitAnimations();
            AnimatorUtility.DeoptimizeTransformHierarchy(roleTarget);
        }
    }


    private void OnDisable()
    {
        EditorApplication.update -= Update;
        if(null != roleTarget)
        {
            GameObject.DestroyImmediate(roleTarget);
        }
        AnimationMode.StopAnimationMode();
    }

    private void Update()
    {
        if(animationCtrl != null)
        {
            animationCtrl.Update();
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
        animationCtrl = new AnimationClipControl();
        animationCtrl.SetOwnGo(roleTarget);
        animationCtrl.SetClip(animationClips[0]);
    }

    void OnGUI()
    {
        if(GUILayout.Button("click"))
        {
            animationCtrl.Play();
        }
        if (GUILayout.Button("step"))
        {
            animationCtrl.Step(0.1f);
        }
        if (GUILayout.Button("back"))
        {
            animationCtrl.Step(-0.1f);
        }
    }

    void ShowBtns()
    {
        int option = GUILayout.Toolbar(toolBarOption, toolBarNames);
    }
}
