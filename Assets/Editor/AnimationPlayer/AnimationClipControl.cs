using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationClipControl
{
    AnimationClip _clip;
    float _length;
    public float length { get { return _length; } }
    public bool loop = false;

    GameObject _ownerGo;
    bool _play = false;
    float _startTime;
    float _escapeTime;
    public static void Init()
    {
        AnimationMode.StartAnimationMode();
    }

    public void SetOwnGo(GameObject go)
    {
        _ownerGo = go;
    }

    public void SetClip(AnimationClip clip)
    {
        _clip = clip;
        if (clip == null)
        {
            _length = 0;
        }
        else
        {
            _length = clip.length;
        }
    }

    public void Play()
    {

    }

    public void Update()
    {

    }

    public void Reset()
    {

    }

}
