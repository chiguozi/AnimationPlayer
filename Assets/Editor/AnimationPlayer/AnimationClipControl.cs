using UnityEngine;
using UnityEditor;
using System;

public class AnimationClipControl
{
    AnimationClip _clip;
    float _length;
   
    public bool loop = false;

    public Action playEndCallback;

    GameObject _ownerGo;
    bool _isPlaying = false;
    float _lastTime;
    float _escapeTime = 0;
    float _speed = 1f;

    public float escapeTime { get { return _escapeTime; } }
    public bool isPlaying { get { return _isPlaying; } }
    public float length
    {
        get
        {
            return _length;
        }
    }

    public float speed
    {
        set
        {
            if (_speed == value)
                return;
            if (value <= 0)
                _speed = 0;
            else
                _speed = value;
        }
        get { return _speed; }
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



    public void Update()
    {
        if (!_isPlaying)
            return;
        var now = EditorApplication.timeSinceStartup;
        float delTime = (float)now - _lastTime;
        _lastTime = (float)now;
        _escapeTime += delTime * speed;
        PlayAnimation();
        if (_escapeTime >= _length)
        {
            if (!loop)
            {
                _isPlaying = false;
                if (null != playEndCallback)
                {
                    playEndCallback();
                }
            }
            else
            {
                _lastTime = (float)now;
            }
            _escapeTime = 0;
        }
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Play()
    {
        if (_isPlaying)
            return;
        _isPlaying = true;
        _lastTime = (float)EditorApplication.timeSinceStartup;
        //if(_escapeTime > 0)
        //{
        //    _lastTime -= _escapeTime;
        //}
    }
    public void Step(float delTime)
    {
        _isPlaying = false;
        _escapeTime += delTime;
        if (_escapeTime >= _length)
            _escapeTime = 0;
        PlayAnimation();
    }

    public void Stop()
    {
        _escapeTime = 0;
        _isPlaying = false;
    }

    public void SetTime(float time)
    {
        _isPlaying = false;
        _escapeTime = time;
        PlayAnimation();
    }

    void PlayAnimation()
    {
        AnimationMode.SampleAnimationClip(_ownerGo, _clip, _escapeTime);
    }
}
