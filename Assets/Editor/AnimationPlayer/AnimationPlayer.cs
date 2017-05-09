using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class AnimationPlayerWindow  : EditorWindow
{
    const float StepLength = 0.05f;
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
    List<string> exposedBoneList;

    bool lockTarget = false;
    bool loop = false;

    bool refreshSelection;

    HashSet<int> persistenceSet = new HashSet<int>();

    Vector2 scrollPos;

    void OnEnable()
    {
        animationCtrl = new AnimationClipControl();
        animationCtrl.playEndCallback = OnPlayEnd;
        RegistEvent();
        InitPersistenceList();
    }


    private void OnDisable()
    {
        EditorApplication.update -= Update;
        ClearGameObjectsInHierarchy();
        Clear();
    }

    void RegistEvent()
    {
        EditorApplication.update += Update;
    }

    void UnRegistEvent()
    {
        EditorApplication.update -= Update;
    }


    void InitPersistenceList()
    {
        var objects = SceneManager.GetActiveScene().GetRootGameObjects();
        for(int i = 0; i < objects.Length; i++)
        {
            persistenceSet.Add(objects[i].GetInstanceID());
        }
    }

    //清空多余的中间gameobject
    void ClearGameObjectsInHierarchy()
    {
        var objects = SceneManager.GetActiveScene().GetRootGameObjects();
        for(int i = objects.Length - 1; i > 0; i--)
        {
            if(!persistenceSet.Contains(objects[i].GetInstanceID()))
            {
                GameObject.DestroyImmediate(objects[i]);
            }
        }
    }
    void Clear()
    {
        Selection.activeObject = null;
        currentSelectObj = null;
        realTragetGo = null;
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

        if(refreshSelection)
        {
            Selection.activeGameObject = currentSelectObj;
            refreshSelection = false;
        }

    }

    void OnSelectionChange()
    {
        if (!(Selection.activeObject is GameObject))
            return;
        TrySelectObject(Selection.activeObject as GameObject);
        //刷新界面
        Repaint();
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
        //必须为挂载animator的骨骼
        animationCtrl.SetOwnGo(animator.gameObject);
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
        ShowCheckPrefabHelpBox();
        if (realTragetGo == null)
            return;
        ShowSelectAniamtionView();
        ShowToogles();
        ShowSpeedSilder();
        ShowSelectGridBtns();
        ShowStepBtns();
        ShowSlider();
        ShowProcessBar();
        ShowAddButton();
        ShowPersistenceList();
        ShowTips();
    }

    void ShowTips()
    {
        EditorGUILayout.HelpBox("列表中的GameObject会在关闭编辑器时保留", MessageType.Info);
    }
    void ShowAddButton()
    {
        if (realTragetGo != null && !persistenceSet.Contains(realTragetGo.GetInstanceID()))
        {
            if(GUILayout.Button("Add To Persistence List", GUILayout.Width(200)))
            {
                persistenceSet.Add(realTragetGo.GetInstanceID());
            }
        }
    }
    void ShowPersistenceList()
    {
        List<int> removeList = null;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical("box");
        var iter = persistenceSet.GetEnumerator();
        while(iter.MoveNext())
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.gray;
            EditorGUILayout.LabelField(EditorUtility.InstanceIDToObject(iter.Current).name);
            if(GUILayout.Button("Del"))
            {
                if (removeList == null)
                    removeList = new List<int>();
                removeList.Add(iter.Current);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if(removeList != null && removeList.Count > 0)
        {
            for(int i = 0; i < removeList.Count; i++)
            {
                persistenceSet.Remove(removeList[i]);
            }
            removeList = null;
        }
    }

    void ShowSpeedSilder()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Speed", GUILayout.Width(150));
        float speed = EditorGUILayout.Slider(animationCtrl.speed, 0, 4, GUILayout.Width(200), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
        if(speed != animationCtrl.speed)
        {
            animationCtrl.speed = speed;
        }
    }
    void ShowCheckPrefabHelpBox()
    {
        if (!string.IsNullOrEmpty(msg))
            EditorGUILayout.HelpBox(msg, MessageType.Error);
    }

    void ShowToogles()
    {
        EditorGUILayout.BeginHorizontal();
        var needLoop = EditorGUILayout.ToggleLeft("Loop", loop);
        if(needLoop != loop)
        {
            loop = needLoop;
            animationCtrl.loop = loop;
        }
        var locked = EditorGUILayout.ToggleLeft("LockTarget", lockTarget);
        if (locked != lockTarget)
        {
            lockTarget = locked;
        }
        EditorGUILayout.EndHorizontal();
    }

    void ShowSlider()
    {
        float time = EditorGUILayout.Slider(animationCtrl.escapeTime, 0, animationCtrl.length);
        if(time != animationCtrl.escapeTime)
        {
            animationCtrl.SetTime(time);
        }
    }

    void ShowProcessBar()
    {
        if(Event.current.type == EventType.Repaint)
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.ProgressBar(new Rect(lastRect.x, lastRect.y + lastRect.height, lastRect.width, 20), animationCtrl.escapeTime / animationCtrl.length, "");
        }
        GUILayout.Space(20);
    }

    void ShowSelectView()
    {
        var selectGo = EditorGUILayout.ObjectField("选择模型Prefab", currentSelectObj, typeof(GameObject), true) as GameObject;
        TrySelectObject(selectGo);
    }

    void TrySelectObject(GameObject selectGo)
    {
        if (currentSelectObj == selectGo)
        {
            return;
        }
        if (realTragetGo != null && lockTarget)
        {
            return;
        }
        currentSelectObj = selectGo;
        if (!CheckPrefabIsValid(selectGo, out msg))
        {
            return;
        }
        else
        {
            if (!currentSelectObj.activeInHierarchy)
            {
                currentSelectObj = Instantiate(currentSelectObj);
                refreshSelection = true;
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
    void ShowStepBtns()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Step Forward"))
        {
            animationCtrl.Step(StepLength);
        }
        if (GUILayout.Button("Step Back"))
        {
            animationCtrl.Step(-StepLength);
        }
        EditorGUILayout.EndHorizontal();
    }
    void ShowSelectGridBtns()
    {
        EditorGUILayout.BeginHorizontal();
        int index = GUILayout.SelectionGrid(buttonIndex, buttonNameArr, 3);
        EditorGUILayout.EndHorizontal();
        if (index == buttonIndex)
            return;
        if(index == 0)
        {
            animationCtrl.Play();
            if(currentSelectObj != realTragetGo)
            {
                currentSelectObj = realTragetGo;
            }
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
        Repaint();
    }

}
