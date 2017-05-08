using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationPlayerWindow  : EditorWindow
{
    [MenuItem("Tool/AnimationPlayer")]
    public static void ShowWindow()
    {
        var window = GetWindow<AnimationPlayerWindow>();
        window.Focus();
        window.Show();
    }

    GameObject currentSelectObj;
    GameObject realTragetGo;
    string[] animationClipNames;
    AnimationClip[] animationClips;
    AnimationClipControl animationCtrl;
    int animationIndex = 0;
    Animator animator;
    string[] buttonNameArr = new string[] { "play", "pause", "stop" };
    int buttonIndex = 2;
    string msg = "请选择模型";
    bool deoptimize = false;

    void OnEnable()
    {
        animationCtrl = new AnimationClipControl();
        animationCtrl.playEndCallback = OnPlayEnd;
        EditorApplication.update += Update;
    }


    private void OnDisable()
    {
        EditorApplication.update -= Update;
        //if (deoptimize)
            //AnimatorUtility.OptimizeTransformHierarchy(animator.gameObject, animator.velocity);
    }


    void Clear()
    {
        if (null != currentSelectObj)
        {
            GameObject.DestroyImmediate(currentSelectObj);
        }
        animationClipNames = null;
        animationClips = null;
        animationCtrl = null;
    }

    private void Update()
    {
        if(animationCtrl != null)
        {
            //刷新Silder
            if (animationCtrl.isPlaying)
                Repaint();
            animationCtrl.Update();
        }

    }
    void InitAnimations()
    {
        animator = realTragetGo.GetComponentInChildren<Animator>();
        animationClips = animator.runtimeAnimatorController.animationClips;
        animationClipNames = new string[animationClips.Length];
        for (int i = 0; i < animationClips.Length; i++)
        {
            animationClipNames[i] = animationClips[i].name;
        }
      
        animationCtrl.SetOwnGo(realTragetGo);
        animationCtrl.SetClip(animationClips[0]);

        if(animator.isOptimizable)
        {
            AnimatorUtility.DeoptimizeTransformHierarchy(animator.gameObject);
            deoptimize = true;
        }
    }

    void OnGUI()
    {
        ShowSelectView();
        if (realTragetGo == null)
            return;
        ShowSelectAniamtionView();
        ShowBtns();
        ShowSlider();
    }

    void ShowSlider()
    {
        float time = EditorGUILayout.Slider(animationCtrl.escapeTime, 0, animationCtrl.length);
        if(time != animationCtrl.escapeTime)
        {
            animationCtrl.SetTime(time);
        }
    }


    void ShowSelectView()
    {
        var selectGo = EditorGUILayout.ObjectField("选择模型Prefab", currentSelectObj, typeof(GameObject), true) as GameObject;
        if (currentSelectObj == selectGo)
        {
            if(!string.IsNullOrEmpty(msg))
                EditorGUILayout.HelpBox(msg, MessageType.Error);
            return;
        }
        currentSelectObj = selectGo;

        if (!CheckPrefabIsValid(selectGo, out msg))
        {
            EditorGUILayout.HelpBox(msg, MessageType.Error);
            return;
        }
        else
        {
            if (!currentSelectObj.activeInHierarchy)
            {
                currentSelectObj = Instantiate(currentSelectObj);
            }
            realTragetGo = currentSelectObj;
            InitAnimations();
        }
    }

    bool CheckPrefabIsValid(GameObject selectGo, out string msg)
    {
        if (selectGo == null)
        {
            msg = "请选择模型";
            return false;
        }

        if (selectGo == realTragetGo)
        {
            msg = "";
            return true;
        }

        var animator = selectGo.GetComponentInChildren<Animator>();
        if(animator == null)
        {
            msg = "找不到animator";
            return false;
        }
        if(animator.runtimeAnimatorController == null)
        {
            msg = "找不到animationController";
            return false;
        }
        if(animator.runtimeAnimatorController.animationClips.Length == 0)
        {
            msg = "没有动作";
            return false;
        }
       
        msg = "";
        return true;
    }

    void ShowSelectAniamtionView()
    {
        int index = EditorGUILayout.Popup("选择动作:",animationIndex, animationClipNames);
        if(index != animationIndex)
        {
            animationIndex = index;
            animationCtrl.SetClip(animationClips[index]);
        }
    }

    void ShowBtns()
    {
        int index = GUILayout.SelectionGrid(buttonIndex, buttonNameArr, 3);
        if (index == buttonIndex)
            return;
        if(index == 0)
        {
            animationCtrl.Play();
        }
        if(index == 1)
        {
            animationCtrl.Pause();
        }
        if(index == 2)
        {
            animationCtrl.Stop();
        }
        buttonIndex = index;
    }

    void OnPlayEnd()
    {
        buttonIndex = 2;
    }

}
