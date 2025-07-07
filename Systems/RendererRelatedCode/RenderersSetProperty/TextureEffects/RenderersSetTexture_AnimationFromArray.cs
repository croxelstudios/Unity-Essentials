using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class RenderersSetTexture_AnimationFromArray : RenderersSetTexture
{
    [SerializeField]
    string[] extraProperties = null;
    [SerializeField]
    [OnValueChanged("UpdateFrames")]
    Texture[] _animationFrames = null;
    public Texture[] animationFrames
    {
        get { return _animationFrames; }
        set
        {
            _animationFrames = value;
            currentFrame = Mathf.Clamp(currentFrame, 0, value.Length - 1);
            UpdateGraphic(currentFrame);
        }
    }
    [OnValueChanged("UpdateFrames")]
    public int startFrame = 0;
    public bool randomStartFrame = false;
    [Min(0)]
    public float frameTime = 0.1f;
    [SerializeField]
    float _speed = 1f;
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }
    public bool restartOnEnable = true;
    public bool loop = true;
    public bool useGlobalTime = false;
    [FoldoutGroup("End Event")]
    public DXEvent endEvent = null;
    [FoldoutGroup("Every Frame Event")]
    public DXEvent everyFrameEvent = null;
    public EventOnFrame[] frameEvents = null;
    [SerializeField]
    Renderer[] renderers = null;

    float currentTime = 0;
    int currentFrame = -1;
    bool isFinalFrame = false;

    void Awake()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (AnimationIsValid())
            {
                if (randomStartFrame)
                    startFrame = Random.Range(0, animationFrames.Length);
                SetFrame(startFrame);
            }
    }

    bool AnimationIsValid()
    {
        return (animationFrames != null) && (animationFrames.Length > 0) && (animationFrames[startFrame] != null);
    }

    protected override void Init()
    {
        base.Init();
#if UNITY_EDITOR
        if (!Application.isPlaying) UpdateFrames();
        else
#endif
            if (restartOnEnable && (currentFrame != startFrame)) SetFrame(startFrame);
    }

    protected override void UpdateRenderersInternal()
    {
        if ((renderers == null) || (renderers.Length <= 0))
            renderers = GetComponentsInParent<Renderer>();
        /*if (rend == null)*/ rend = renderers;
        //else rend = FuseRendererArrays(rend, renderers);
        if (affectsChildren)
            rend = FuseRendererArrays(rend, GetComponentsInChildren<Renderer>(true));
    }

    Renderer[] FuseRendererArrays(Renderer[] first, Renderer[] second)
    {
        List<Renderer> tmpList = new List<Renderer>();
        tmpList.AddRange(first);
        for (int i = 0; i < second.Length; i++)
            if (!tmpList.Contains(second[i]))
                tmpList.Add(second[i]);
        return tmpList.ToArray();
    }

    void SetFrame(int frame)
    {
        currentFrame = frame;
        foreach (EventOnFrame ef in frameEvents)
            if (ef.frame == currentFrame) ef.frameEvent?.Invoke();
        everyFrameEvent?.Invoke();
        UpdateGraphic(currentFrame);
    }

    void UpdateGraphic(int frame)
    {
        if (animationFrames != null)
            texture = animationFrames[frame];
        base.UpdateBehaviour();
    }

#if UNITY_EDITOR
    public void UpdateFrames()
    {
        if (animationFrames == null) startFrame = 0;
        else startFrame = Mathf.Clamp(startFrame, 0, animationFrames.Length - 1);
        SetFrame(startFrame);
    }
#endif

    protected override void UpdateBehaviour()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
            if (currentFrame < animationFrames.Length)
            {
                int frame = currentFrame;
                if (useGlobalTime)
                    frame = frameTime == 0f ? 0 : (int)(timeMode.Time() / frameTime);
                else
                {
                    if (currentTime >= frameTime)
                    {
                        frame += (int)Mathf.Sign(speed);
                        currentTime = 0;
                    }

                    currentTime += timeMode.DeltaTime() * Mathf.Abs(speed);
                }

                if (frame != currentFrame)
                {
                    if ((frame >= animationFrames.Length) || (frame < 0))
                    {
                        if (loop) frame %= animationFrames.Length;
                        else frame = Mathf.Clamp(frame, 0, animationFrames.Length - 1);
                        if (!isFinalFrame)
                        {
                            endEvent?.Invoke();
                            if (animationFrames.Length > 1)
                                isFinalFrame = true;
                        }
                    }
                    else isFinalFrame = false;

                    if (frame != currentFrame) SetFrame(frame);
                }
            }
    }

    protected override void BlSetProperty(MaterialPropertyBlock block, Renderer rend, int mat)
    {
        if (texture != null)
        {
            base.BlSetProperty(block, rend, mat);
            if (extraProperties != null)
                foreach (string name in extraProperties)
                    block.SetTexture(name, texture);
        }
    }

    protected override void VSetProperty(Renderer rend, int mat)
    {
        if (texture != null)
        {
            base.VSetProperty(rend, mat);
            if (extraProperties != null)
                foreach (string name in extraProperties)
                    rend.materials[mat].SetTexture(name, texture);
        }
    }

    [Serializable]
    public struct EventOnFrame
    {
        public int frame;
        public DXEvent frameEvent;

        public EventOnFrame(int frame, DXEvent frameEvent)
        {
            this.frame = frame;
            this.frameEvent = frameEvent;
        }
    }
}
