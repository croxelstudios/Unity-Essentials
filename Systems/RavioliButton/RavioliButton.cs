using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;

public class RavioliButton : MonoBehaviour
{
    public bool selected = true;
    public DXEvent onSelect = null;
    public DXEvent onDeselect = null;
    public DXEvent[] buttonActions = null;
    public bool overrideSelectorBehaviour = false;
    public SelectorBehaviour selectorBehaviourOverride;
    [HideInInspector]
    public bool initialized = false;

    RavioliButton_Group[] groups;

    protected virtual void OnEnable()
    {
        RavioliButton_Group[] grs = FindObjectsOfType<RavioliButton_Group>();
        List<RavioliButton_Group> lGrs = new List<RavioliButton_Group>();
        foreach (RavioliButton_Group gr in grs)
            if (gr.buttonsParent == transform.parent)
                lGrs.Add(gr);                

        groups = lGrs.ToArray();

        foreach (RavioliButton_Group gr in groups)
            gr.AddButton(this);
    }

    protected virtual void OnDisable()
    {
        if (groups != null)
            foreach (RavioliButton_Group group in groups)
                group.RemoveButton(this);
        groups = null;
    }

    public virtual void TrySelect()
    {
        if ((!selected) && (groups != null))
        {
            if (!initialized)
            {
                foreach (RavioliButton_Group group in groups)
                    group.InitializeAllButtons();
            }
            //else
                onSelect?.Invoke();
            selected = true;
        }
    }

    public virtual void TryDeselect()
    {
        if (selected)
        {
            onDeselect?.Invoke();
            selected = false;
        }
    }

    public void Select()
    {
        foreach (RavioliButton_Group group in groups)
        {
            group.SelectButton(transform.GetSiblingIndex());
            group.Select();
        }
    }

    public virtual void PressButtonWith_Internal(int id)
    {
        if (this.IsActiveAndEnabled() && (id < buttonActions.Length))
            buttonActions[id]?.Invoke();
    }

    public void PressButtonWith(int id)
    {
        if ((groups == null) || (groups.Length == 0))
            PressButtonWith_Internal(id);
        else
            foreach (RavioliButton_Group group in groups)
                group.PressButtonWith(id);
    }

    public void PressButton()
    {
        PressButtonWith(0);
    }

    [Serializable]
    public struct SelectorBehaviour // TO DO: Support for animation curves?
    {
        public float smoothTime;
        public float minDistance;
        public DXEvent startMoving;
        public DXEvent stopMoving;

        public SelectorBehaviour(float smoothTime, float minDistance)
        {
            this.smoothTime = smoothTime;
            this.minDistance = minDistance;
            startMoving = null;
            stopMoving = null;
        }
    }
}
