using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class RavioliButton_Group : RavioliButton
{
    [Header("Group options")]
    public Transform buttonsParent = null;
    [SerializeField]
    bool loopButtons = true;
    [SerializeField]
    bool restartPositionOnSelection = true;
    [SerializeField]
    bool reselectWhenScrollingOneButton = false;
    [SerializeField]
    int _defaultButton = 0;
    public int defaultButton { get { return _defaultButton; } set { _defaultButton = value; } }
    [SerializeField]
    DXIntEvent changedButtonID = null;
    [SerializeField]
    DXIntEvent pressedButtonID = null;
    [Header("Selector object")]
    [SerializeField]
    Transform selector = null;
    [SerializeField]
    SelectorBehaviour selectorBehaviour = new SelectorBehaviour(0f, 0.01f);
    [SerializeField]
    TimeMode timeMode = TimeMode.Update;

    List<RavioliButton> buttons;
    RavioliButton currentButton;
    Vector3 tmpSpd;
    Coroutine co;

    [Tooltip("Whether the button should launch the onSelect event on first selection or not")]
    public bool launchFirstSelectedEvent = false;

    #region Main functions
    void Start()
    {
        if (buttons == null) Init();
        if (selector) selector.gameObject.SetActive(false);
        StartCoroutine(WaitForResetButtons());
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (selector && currentButton) UpdateSelector(false);
    }

    void OnValidate()
    {
        if (buttonsParent == null)
            buttonsParent = transform;
    }

    public override void TrySelect()
    {
        base.TrySelect();
        if (selector && (currentButton != null)) selector.gameObject.SetActive(true);
        ResetButtons();
    }

    public override void TryDeselect()
    {
        base.TryDeselect();
        if (selector)
        {
            StopSelectorAtTarget();
            selector.gameObject.SetActive(false);
        }
    }

    void Init()
    {
        buttons = new List<RavioliButton>();
    }

    void ResetButtons()
    {
        if ((buttons != null) && (buttons.Count > 0))
        {
            if (restartPositionOnSelection)
            {
                if (defaultButton >= buttons.Count) currentButton = buttons[buttons.Count - 1];
                else currentButton = buttons[defaultButton];
            }
            if (currentButton != null)
            {
                for (int i = 0; i < buttons.Count; i++) buttons[i].TryDeselect();
                currentButton.TrySelect();
                if (selector)
                {
                    selector.gameObject.SetActive(true);
                    UpdateSelector(false);
                }
            }
        }
    }

    public void InitializeAllButtons()
    {
        foreach (RavioliButton rb in buttons)
            rb.initialized = true;
    }

    public void DeinitializeAllButtons()
    {
        foreach (RavioliButton rb in buttons)
            rb.initialized = false;
    }

    IEnumerator WaitForResetButtons()
    {
        yield return new WaitForEndOfFrame();
        if (!launchFirstSelectedEvent)
            DeinitializeAllButtons();
        ResetButtons();
    }

    IEnumerator WaitForUpdateSelector()
    {
        yield return new WaitForEndOfFrame();
        UpdateSelector(false);
    }
    #endregion

    #region Actions
    public override void PressButtonWith_Internal(int id)
    {
        base.PressButtonWith_Internal(id);
        if (currentButton != null)
        {
            currentButton.PressButtonWith_Internal(id);
            pressedButtonID?.Invoke(buttons.IndexOf(currentButton));
        }
    }

    public void PressAllButtonsWith(int id)
    {
        for (int i = 0; i < buttons.Count; i++)
            buttons[i].PressButtonWith_Internal(id);
    }

    public void PressAllButtons()
    {
        PressAllButtonsWith(0);
    }

    public void Next()
    {
        if (this.IsActiveAndEnabled()) Next(true);
    }

    void Next(bool doMovement)
    {
        if (currentButton != null)
        {
            int id = buttons.IndexOf(currentButton);
            int next = (int)(loopButtons ? Mathf.Repeat(id + 1f, buttons.Count) :
                Mathf.Clamp(id + 1, 0f, buttons.Count - 1f));
            SelectButton(next, doMovement);
        }
    }

    public void Previous()
    {
        if (this.IsActiveAndEnabled()) Previous(true);
    }

    void Previous(bool doMovement)
    {
        if (currentButton != null)
        {
            int id = buttons.IndexOf(currentButton);
            int previous = (int)(loopButtons ? Mathf.Repeat(id - 1f, buttons.Count) :
                Mathf.Clamp(id - 1, 0f, buttons.Count - 1f));
            SelectButton(previous, doMovement);
        }
    }

    public void SelectButton(int id)
    {
        if (this.IsActiveAndEnabled()) SelectButton(buttons[id], true);
    }

    public void SelectButton(int id, bool doMovement)
    {
        if (this.IsActiveAndEnabled()) SelectButton(buttons[id], doMovement);
    }

    public void SelectButton(RavioliButton button, bool doMovement = true)
    {
        if (this.IsActiveAndEnabled())
        {
            if ((buttons.Count > 1) || reselectWhenScrollingOneButton)
            {
                currentButton.TryDeselect();
                currentButton = button;
                changedButtonID?.Invoke(buttons.IndexOf(currentButton));
                if (selector) UpdateSelector(doMovement);
                else currentButton.TrySelect();

            }
        }
    }
    #endregion

    # region Selector
    bool MoveSelector(Vector3 target, SelectorBehaviour SB, float deltaTime, ref Vector3 tmpSpd)
    {
        Vector3 movement = Vector3.SmoothDamp(selector.position, target,
            ref tmpSpd, SB.smoothTime, Mathf.Infinity, deltaTime) - selector.position;
        selector.Translate(movement, Space.World);
        if (Vector3.Distance(selector.position, target) < SB.minDistance)
        {
            selector.position = target;
            tmpSpd = Vector3.zero;
            return true;
        }
        else return false;
    }

    void UpdateSelector(bool doMovement = true)
    {
        SelectorBehaviour SB = SBOfButton(currentButton);
        if (doMovement)
        {
            if (co == null) SB.startMoving?.Invoke();
            else StopCoroutine(co);
            co = StartCoroutine(SelectorMovement());
        }
        else
        {
            if (co != null) UpdateSelector();
            else StopSelectorAtTarget();
        }
    }

    IEnumerator SelectorMovement()
    {
        SelectorBehaviour SB = SBOfButton(currentButton);
        while (!MoveSelector(currentButton.transform.position, SB, timeMode.DeltaTime(), ref tmpSpd))
        {
            if (timeMode.IsFixed()) yield return new WaitForFixedUpdate();
            else yield return null;
        }
        StopSelectorAtTarget();
    }

    void StopSelectorAtTarget()
    {
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }
        if (currentButton != null)
        {
            selector.position = currentButton.transform.position;
            SBOfButton(currentButton).stopMoving?.Invoke();
            currentButton.TrySelect();
            //InitializeAllButtons();
        }
    }
    #endregion

    #region Buttons management
    public void AddButton(RavioliButton button)
    {
        if (buttons == null) Init();
        button.TryDeselect();
        if (currentButton == null)
        {
            buttons.Add(button);
            currentButton = button;
            button.TrySelect();
            if (selector)
            {
                selector.gameObject.SetActive(true);
                if (gameObject.activeInHierarchy) StartCoroutine(WaitForUpdateSelector());
            }
        }
        else
        {
            buttons.Add(button);
            buttons = buttons.OrderBy(x => x.transform.GetSiblingIndex()).ToList();
            if (selector && gameObject.activeInHierarchy) StartCoroutine(WaitForUpdateSelector());
        }
    }

    public void RemoveButton(RavioliButton button)
    {
        int id = buttons.IndexOf(button);
        int currentId = buttons.IndexOf(currentButton);
        if (id <= currentId)
        {
            currentButton.TryDeselect();
            buttons.Remove(button);
            int previous = (int)(loopButtons ? Mathf.Repeat(currentId - 1f, buttons.Count) :
                Mathf.Clamp(currentId - 1, 0f, buttons.Count - 1f));
            if (buttons.Count <= 0)
            {
                if (selector) selector.gameObject.SetActive(false);
                currentButton = null;
            }
            else
            {
                currentButton = buttons[previous];
                changedButtonID?.Invoke(previous);
                if (selector)
                {
                    if (gameObject.activeInHierarchy)
                        StartCoroutine(WaitForUpdateSelector());
                }
                else currentButton.TrySelect();
            }
        }
        else
        {
            buttons.Remove(button);
            if (selector)
            {
                if (gameObject.activeInHierarchy)
                    StartCoroutine(WaitForUpdateSelector());
            }
            else currentButton.TrySelect();
        }
    }
    #endregion

    #region Helper methods
    SelectorBehaviour SBOfButton(RavioliButton button)
    {
        if (button && button.overrideSelectorBehaviour) return button.selectorBehaviourOverride;
        else return selectorBehaviour;
    }
    #endregion
}
