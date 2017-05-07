using UnityEngine;
using UnityEditor;

public class AnimationClipControl
{
    AnimationClip _clip;
    float _length;
    public float length { get { return _length; } }
    public bool loop = false;

    GameObject _ownerGo;
    bool _play = false;
    double _startTime;
    double _escapeTime = 0;
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
        _play = true;
        _startTime = EditorApplication.timeSinceStartup;
    }

    public void Update()
    {
        if (!_play)
            return;
        var now = EditorApplication.timeSinceStartup;
        _escapeTime = now - _startTime;
        PlayAnimation();
        if (_escapeTime >= _length)
        {
            _play = false;
            _escapeTime = 0;
        }
    }

    public void Step(float delTime)
    {
        _play = false;
        _escapeTime += delTime;
        if (_escapeTime >= _length)
            _escapeTime = 0;
        PlayAnimation();
    }

    public void Reset()
    {
        _escapeTime = 0;
        _play = false;
    }

    void PlayAnimation()
    {
        AnimationMode.SampleAnimationClip(_ownerGo, _clip, (float)_escapeTime);
    }
}
