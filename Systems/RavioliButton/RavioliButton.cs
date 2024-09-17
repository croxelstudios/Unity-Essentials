using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RavioliButton : RavioliButton_Button
{
    //TO DO: Add support for autocalculate direction of button from relative position to make dynamic selection systems
    [SizedFoldoutGroup("Group options")]
    public Transform buttonsParent = null;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    int _buttonSelectionLimit = 255;
    public int buttonSelectionLimit { get { return _buttonSelectionLimit; } set { _buttonSelectionLimit = value; } }
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    bool loopButtons = true;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    bool restartPositionOnSelection = true;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    bool selectionsDoNotWaitForMovements = true;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    bool launchRedundantSelections = false;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    int _defaultButton = 0;
    public int defaultButton { get { return _defaultButton; } set { _defaultButton = value; } }
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    DXIntEvent changedButtonID = null;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    DXIntEvent pressedButtonID = null;

    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [Header("Selector")]
    bool useSelector = true;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [ShowIf("@useSelector")]
    Transform selector = null;
    bool canUseSelector { get { return useSelector && selector; } }
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [ShowIf("@useSelector")]
    [InlineProperty]
    [HideLabel]
    MovementBehaviour selectorBehaviour = new MovementBehaviour(0f, 0.01f);

    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [Header("Buttons carousel")]
    bool useButtonsCarousel = false;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [ShowIf("@useButtonsCarousel")]
    [InlineProperty]
    [HideLabel]
    MovementBehaviour carouselBehaviour = new MovementBehaviour(0f, 0.01f);
    //TO DO: Add support for non-looping carousels
    //(Limit button selection to a range and use invisible looping buttons for the extra positions.
    //This should just be an option on the main Button Group settings that stops you from selecting
    //the higher button counts, that way you can just have virtual buttons do the looping.
    //Search every use of "buttons.Count" and rethink how it should work now.)

    [Space]
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    TimeMode timeMode = TimeMode.Update;
    //TO DO: This variable is not used anywhere, but it should be
    //[SizedFoldoutGroup("Group options")]
    //[Tooltip("Whether the button should launch the onSelect event on first selection or not")]
    //public bool launchFirstSelectedEvent = false;

    bool groupInitialized = false;
    ProgrammedSelectButtonAction programmedSelection;
    int buttonsInitCount = -1;
    List<RavioliButton_Button> buttons;
    Vector3 tmpSpd;
    RavioliButton_Button currentButton;
    RavioliButton_Button prevButton;
    Coroutine sCo;
    Coroutine cCo;

    #region Main functions
    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        groupInitialized = false;
    }

    void OnValidate()
    {
        if (buttonsParent == null)
            buttonsParent = transform;
    }

    void Reset()
    {
        buttonsParent = transform;
    }
    #endregion

    #region Internal functions
    public override void TrySelect()
    {
        base.TrySelect();
        ResetButtons();
    }

    public override void TryDeselect()
    {
        base.TryDeselect();
        FinalizeMovementBehaviors();
    }

    public override void PressButtonWith_Internal(int id)
    {
        if (currentButton == null)
            base.PressButtonWith_Internal(id);
        else currentButton.PressButtonWith_Internal(id);
    }

    public void PressButtonGroup(int id, int buttonId)
    {
        foreach (RavioliButton group in groups)
            group.PressButtonGroup(id, transform.GetSiblingIndex());
        if (id < buttonActions.Length) buttonActions[id]?.Invoke();
        pressedButtonID?.Invoke(buttonId);
    }

    void InitButtonsList()
    {
        buttons = new List<RavioliButton_Button>();
    }

    public void ResetButtons()
    {
        if ((buttons != null) && (buttons.Count > 0))
        {
            if (restartPositionOnSelection) UpdateCurrentButton(defaultButton);
            if (currentButton != null)
            {
                for (int i = 0; i < buttons.Count; i++) buttons[i].TryDeselect();
                InitializeMovementBehaviors();
                UpdateMovementBehaviours(false);
                currentButton.TrySelect();
            }
        }
    }

    /// <summary>
    /// Returns false when no movement behaviours are selected
    /// </summary>
    /// <param name="doMovement"></param>
    /// <param name="activateSelector"></param>
    /// <param name="waitTillEndOfFrame"></param>
    /// <returns></returns>
    bool UpdateMovementBehaviours(bool doMovement)
    {
        if (canUseSelector) DoSelector(doMovement);
        if (useButtonsCarousel) DoCarousel(doMovement);
        return canUseSelector || useButtonsCarousel;
    }

    void InitializeMovementBehaviors()
    {
        if (canUseSelector) selector.gameObject.SetActive(true);
    }

    void FinalizeMovementBehaviors()
    {
        if (canUseSelector)
        {
            StopSelectorAtTarget();
            selector.gameObject.SetActive(false);
        }
        if (useButtonsCarousel) StopCarouselAtTarget();
    }

    void UpdateCurrentButton(RavioliButton_Button newCurrentButton)
    {
        if (groupInitialized) prevButton = currentButton;
        currentButton = newCurrentButton;
    }

    void UpdateCurrentButton(int id)
    {
        UpdateCurrentButton(
            (id >= ButtonsLimit()) ?
                buttons[ButtonsLimit() - 1]
            :
                ((id < 0) ?
                    buttons[0]
                :
                    buttons[id]
                )
            );
    }

    MovementBehaviour SBOfButton(RavioliButton_Button button)
    {
        //Group
        if (button && button.overrideSelectorBehaviour) return button.selectorBehaviourOverride;
        else return selectorBehaviour;
    }

    MovementBehaviour CBOfButton(RavioliButton_Button button)
    {
        //Group
        if (button && button.overrideCarouselBehaviour) return button.carouselBehaviourOverride;
        else return carouselBehaviour;
    }

    int ButtonsLimit()
    {
        return Mathf.Min(buttonSelectionLimit, buttons.Count);
    }
    #endregion

    #region Actions
    public void PressAllButtonsWith(int id)
    {
        for (int i = 0; i < ButtonsLimit(); i++)
            buttons[i].PressButtonWith(id);
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
            int next = (int)(loopButtons ? Mathf.Repeat(id + 1f, ButtonsLimit()) :
                Mathf.Clamp(id + 1, 0f, ButtonsLimit() - 1f));
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
            int previous = (int)(loopButtons ? Mathf.Repeat(id - 1f, ButtonsLimit()) :
                Mathf.Clamp(id - 1, 0f, ButtonsLimit() - 1f));
            SelectButton(previous, doMovement);
        }
    }

    public void SelectButtonInstant(int id)
    {
        SelectButton(id, false);
    }

    public void SelectButton(int id)
    {
        SelectButton(id, true);
    }

    public void SelectButton(int id, bool doMovement)
    {
        if (this.IsActiveAndEnabled())
        {
            if (!groupInitialized)
                programmedSelection.ProgramSelection(this, id, doMovement);
            else SelectButton(buttons[id], doMovement);
        }
    }

    public void SelectButton(RavioliButton_Button button, bool doMovement = true)
    {
        if (this.IsActiveAndEnabled())
        {
            if (!groupInitialized)
                programmedSelection.ProgramSelection(this, button, doMovement);
            else if ((button != currentButton) || launchRedundantSelections)
            {
                currentButton.TryDeselect();
                UpdateCurrentButton(button);
                changedButtonID?.Invoke(buttons.IndexOf(currentButton));
                if ((!UpdateMovementBehaviours(doMovement)) || selectionsDoNotWaitForMovements)
                    currentButton.TrySelect();
            }
        }
    }
    #endregion

    #region Selector
    bool MoveSelector(Vector3 target, MovementBehaviour SB, float deltaTime, ref Vector3 tmpSpd)
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

    void DoSelector(bool doMovement = true)
    {
        MovementBehaviour SB = SBOfButton(currentButton);
        if (doMovement)
        {
            if (sCo == null) SB.startMoving?.Invoke();
            else StopCoroutine(sCo);
            sCo = StartCoroutine(SelectorMovement());
        }
        else StopSelectorAtTarget();
    }

    IEnumerator SelectorMovement()
    {
        MovementBehaviour SB = SBOfButton(currentButton);
        while (!MoveSelector(currentButton.transform.position, SB, timeMode.DeltaTime(), ref tmpSpd))
        {
            if (timeMode.IsFixed()) yield return new WaitForFixedUpdate();
            else yield return null;
        }
        StopSelectorAtTarget();
    }

    void StopSelectorAtTarget()
    {
        if (sCo != null)
        {
            StopCoroutine(sCo);
            sCo = null;
        }
        if (currentButton != null)
        {
            selector.position = currentButton.transform.position;
            SBOfButton(currentButton).stopMoving?.Invoke();
            currentButton.TrySelect();
        }
    }
    #endregion

    #region Carousel
    //TO DO: Accurately handle grid button layouts (Move the buttons inside sibling groups).
    //It also should preserve "currentButton" variable between siblings.
    bool MoveCarousel(MovementBehaviour CB, float deltaTime)
    {
        bool everyoneArrived = true;
        for (int i = 0; i < buttons.Count; i++)
        {
            bool didArrive = buttons[i].MoveToTarget(buttons[i].carouselTarget, CB, deltaTime);
            if (!didArrive) everyoneArrived = false;
        }
        return everyoneArrived;
    }

    void DoCarousel(bool doMovement = true)
    {
        MovementBehaviour CB = CBOfButton(currentButton);
        CalculateCarouselTargets(CalculateCarouselDirection());
        if (doMovement)
        {
            if (cCo == null) CB.startMoving?.Invoke();
            else StopCoroutine(cCo);
            cCo = StartCoroutine(CarouselMovement());
        }
        else StopCarouselAtTarget();
    }

    IEnumerator CarouselMovement()
    {
        MovementBehaviour CB = CBOfButton(currentButton);

        while (!MoveCarousel(CB, timeMode.DeltaTime()))
        {
            if (timeMode.IsFixed()) yield return new WaitForFixedUpdate();
            else yield return null;
        }
        StopCarouselAtTarget();
    }

    void StopCarouselAtTarget()
    {
        if (cCo != null)
        {
            StopCoroutine(cCo);
            cCo = null;
        }
        if (currentButton != null)
        {
            for (int i = 0; i < buttons.Count; i++)
                buttons[i].position = buttons[i].carouselTarget;
            CBOfButton(currentButton).stopMoving?.Invoke();
            if (!canUseSelector) currentButton.TrySelect();
        }
    }

    int CalculateCarouselDirection()
    {
        int cID = buttons.IndexOf(currentButton);
        int pID = buttons.IndexOf(prevButton);
        if (pID < 0) pID = cID;
        int dif = pID - cID;
        int absDif = Mathf.Abs(dif);
        if ((_buttonSelectionLimit < buttons.Count) || (absDif < (buttons.Count / 2))) return dif;
        else return (buttons.Count - absDif) * (-(int)Mathf.Sign(dif));
    }

    void CalculateCarouselTargets(int offset)
    {
        Vector3[] targets = new Vector3[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
            targets[i] = buttons[(int)Mathf.Repeat(i + offset, buttons.Count)].carouselTarget;
        for (int i = 0; i < buttons.Count; i++)
            buttons[i].carouselTarget = targets[i];
    }
    #endregion

    #region Buttons management
    public void AddButton(RavioliButton_Button button)
    {
        if (buttons == null) InitButtonsList();
        button.TryDeselect();
        buttons.Add(button);
        buttons = buttons.OrderBy(x => x.transform.GetSiblingIndex()).ToList();

        if (groupInitialized)
        {
            if (currentButton == null)
            {
                UpdateCurrentButton(button);
                InitializeMovementBehaviors();
                UpdateMovementBehaviours(false);
                currentButton.TrySelect();
            }
            else UpdateMovementBehaviours(false);
        }
        else
        //Handles spacific case where the group has just been activated and needs to preserve data from before the previous deactivation
        {
            if (buttonsInitCount <= 0)
            {
                RavioliButton_Button[] allChildren = buttonsParent.GetComponentsInChildren<RavioliButton_Button>();
                buttonsInitCount = 0;
                for (int i = 0; i < allChildren.Length; i++)
                {
                    if (allChildren[i].transform.parent == buttonsParent)
                        buttonsInitCount++;
                }
            }
            if (buttons.Count >= buttonsInitCount)
            {
                groupInitialized = true;
                ResetButtons();
                programmedSelection.Execute();
            }
        }
    }

    public void RemoveButton(RavioliButton_Button button)
    {
        if (this.IsActiveAndEnabled(false))
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
                    FinalizeMovementBehaviors();
                    UpdateCurrentButton(null);
                }
                else
                {
                    UpdateCurrentButton(previous);
                    changedButtonID?.Invoke(previous);
                    if ((!UpdateMovementBehaviours(true)) || selectionsDoNotWaitForMovements)
                        currentButton.TrySelect();
                }
            }
            else buttons.Remove(button);
        }
        else buttons.Remove(button);
    }
    #endregion

    struct ProgrammedSelectButtonAction
    {
        public bool isProgrammed;
        public bool useButtonID;
        public int buttonID;
        public RavioliButton_Button button;
        public bool doMovement;
        public RavioliButton group;

        public ProgrammedSelectButtonAction(bool isProgrammed, int buttonID,
            RavioliButton_Button button, bool doMovement, RavioliButton group)
        {
            this.isProgrammed = isProgrammed;
            this.useButtonID = button == null;
            this.buttonID = buttonID;
            this.button = button;
            this.doMovement = doMovement;
            this.group = group;
        }

        public void Execute()
        {
            if (isProgrammed)
            {
                if (useButtonID) group.SelectButton(buttonID, doMovement);
                else group.SelectButton(button, doMovement);
                isProgrammed = false;
            }
        }

        public void ProgramSelection(RavioliButton group, int id, bool doMovement)
        {
            isProgrammed = true;
            useButtonID = true;
            buttonID = id;
            this.doMovement = doMovement;
            this.group = group;
        }

        public void ProgramSelection(RavioliButton group, RavioliButton_Button button, bool doMovement)
        {
            isProgrammed = true;
            useButtonID = true;
            this.button = button;
            this.doMovement = doMovement;
            this.group = group;
        }
    }
}

public class RavioliButton_Button : MonoBehaviour
{
    [SizedFoldoutGroup("Button options", Expanded = true)]
    public bool selected = true;
    [SizedFoldoutGroup("Button options")]
    public DXEvent onSelect = null;
    [SizedFoldoutGroup("Button options")]
    public DXEvent onDeselect = null;
    [SizedFoldoutGroup("Button options")]
    public DXEvent[] buttonActions = null;
    [SizedFoldoutGroup("Button options")]
    public bool overrideSelectorBehaviour = false;
    [SizedFoldoutGroup("Button options")]
    [InlineProperty]
    [HideLabel]
    [ShowIf("@overrideSelectorBehaviour")]
    public MovementBehaviour selectorBehaviourOverride = new MovementBehaviour(0f, 0.01f);
    [SizedFoldoutGroup("Button options")]
    public bool overrideCarouselBehaviour = false;
    [SizedFoldoutGroup("Button options")]
    [InlineProperty]
    [HideLabel]
    [ShowIf("@overrideCarouselBehaviour")]
    public MovementBehaviour carouselBehaviourOverride = new MovementBehaviour(0f, 0.01f);

    protected RavioliButton[] groups;
    Vector3 cTmpSpd;
    [HideInInspector]
    public Vector3 carouselTarget;
    public Vector3 position { get { return transform.position; } set { transform.position = value; } }

    public virtual void OnEnable()
    {
        carouselTarget = position;
        AddMyselfToParentGroups();
    }

    public virtual void OnDisable()
    {
        RemoveFromAllGroups();
    }

    #region Internal functions
    void AddMyselfToParentGroups()
    {
        if ((groups == null) || (groups.Length <= 0))
        {
            RavioliButton[] grs = FindObjectsOfType<RavioliButton>();
            List<RavioliButton> lGrs = new List<RavioliButton>();
            foreach (RavioliButton gr in grs)
                if (gr.buttonsParent == transform.parent)
                    lGrs.Add(gr);

            groups = lGrs.ToArray();

            foreach (RavioliButton gr in groups)
                gr.AddButton(this);
        }
    }

    void RemoveFromAllGroups()
    {
        if (groups != null)
            foreach (RavioliButton group in groups)
                group.RemoveButton(this);
        groups = null;
    }

    public virtual void TrySelect()
    {
        if ((!selected) && (groups != null))
        {
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

    public virtual void PressButtonWith_Internal(int id)
    {
        foreach (RavioliButton group in groups)
            group.PressButtonGroup(id, transform.GetSiblingIndex());

        if (id < buttonActions.Length)
            buttonActions[id]?.Invoke();
    }
    #endregion

    #region Actions
    public void Select()
    {
        if (this.IsActiveAndEnabled())
        {
            AddMyselfToParentGroups();
            foreach (RavioliButton group in groups)
            {
                group.SelectButton(transform.GetSiblingIndex());
                group.Select();
            }
        }
    }

    public void SelectInstant()
    {
        if (this.IsActiveAndEnabled())
        {
            AddMyselfToParentGroups();
            foreach (RavioliButton group in groups)
            {
                group.SelectButtonInstant(transform.GetSiblingIndex());
                group.SelectInstant();
            }
        }
    }

    public void PressButtonWith(int id)
    {
        if (this.IsActiveAndEnabled())
        {
            AddMyselfToParentGroups();
            PressButtonWith_Internal(id);
        }
    }

    public void PressButton()
    {
        PressButtonWith(0);
    }
    #endregion

    public bool MoveToTarget(Vector3 target, MovementBehaviour CB, float deltaTime)
    {
        Vector3 movement = Vector3.SmoothDamp(transform.position, target,
            ref cTmpSpd, CB.smoothTime, Mathf.Infinity, deltaTime) - transform.position;
        transform.Translate(movement, Space.World);
        if (Vector3.Distance(transform.position, target) < CB.minDistance)
        {
            transform.position = target;
            cTmpSpd = Vector3.zero;
            return true;
        }
        else return false;
    }

    [Serializable]
    public struct MovementBehaviour // TO DO: Support for animation curves?
    {
        public float smoothTime;
        public float minDistance;
        [FoldoutGroup("Events")]
        public DXEvent startMoving;
        [FoldoutGroup("Events")]
        public DXEvent stopMoving;

        public MovementBehaviour(float smoothTime, float minDistance)
        {
            this.smoothTime = smoothTime;
            this.minDistance = minDistance;
            startMoving = null;
            stopMoving = null;
        }
    }
}
