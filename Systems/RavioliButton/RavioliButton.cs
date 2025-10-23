using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

[DefaultExecutionOrder(-1)]
public class RavioliButton : RavioliButton_Button
{
    //TO DO: Add support for autocalculating direction of button from relative position to make dynamic selection systems
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
    float directionSelectMaxDistance = Mathf.Infinity;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    float directionSelectMaxAngle = 40f;
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
    ReachLimitEvents reachLimitEvents = new ReachLimitEvents();

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
    bool handleSelectorActiveState = true;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [ShowIf("@useSelector")]
    MovementBehaviour selectorBehaviour = new MovementBehaviour(0f, 0.01f);

    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [Header("Buttons carousel")]
    bool useButtonsCarousel = false;
    [SerializeField]
    [SizedFoldoutGroup("Group options")]
    [ShowIf("@useButtonsCarousel")]
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
    static List<RavioliButton_Button> auxButtonList;
    Vector3 tmpSpd;
    RavioliButton_Button currentButton;
    RavioliButton_Button prevButton;
    Coroutine sCo;
    Coroutine cCo;

    const float Vector45value = 0.7071f;

    #region Main functions
    public override void OnEnable()
    {
        base.OnEnable();

        if (!auxButtonList.IsNullOrEmpty())
            foreach (RavioliButton_Button button in auxButtonList)
                if (button.IsInGroup(this))
                    AddButton(button);

        StartCoroutine(MBResetAfterOneFrame());
    }

    public override void OnDisable()
    {
        if (!buttons.IsNullOrEmpty())
        {
            auxButtonList = auxButtonList.ClearOrCreate();
            auxButtonList.AddRange(buttons);
            foreach (RavioliButton_Button button in auxButtonList)
                RemoveButton(button);
        }

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
        if (groups != null)
            foreach (RavioliButton group in groups)
                group.PressButtonGroup(id, transform.GetSiblingIndex());
        if (id < buttonActions.Length) buttonActions[id]?.Invoke();
        pressedButtonID?.Invoke(buttonId);
    }

    public void ResetButtons()
    {
        if ((buttons != null) && (buttons.Count > 0))
        {
            if (restartPositionOnSelection || (currentButton == null)) UpdateCurrentButton(defaultButton);
            if (currentButton != null)
            {
                for (int i = 0; i < buttons.Count; i++) buttons[i].TryDeselect();
                InitializeMovementBehaviors();
                TrySelectCurrent();
            }
        }
    }

    void TrySelectCurrent()
    {
        if ((!UpdateMovementBehaviours(false)) && (currentButton != null))
            currentButton.TrySelect();
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
        if (canUseSelector && handleSelectorActiveState)
            selector.gameObject.SetActive(true);
    }

    void FinalizeMovementBehaviors()
    {
        if (canUseSelector && handleSelectorActiveState)
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
            CheckLimits(id, 1);
            int next = loopButtons ? (int)Mathf.Repeat(id + 1f, ButtonsLimit()) :
                Mathf.Clamp(id + 1, 0, ButtonsLimit() - 1);
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
            CheckLimits(id, -1);
            int previous = loopButtons ? (int)Mathf.Repeat(id - 1f, ButtonsLimit()) :
                Mathf.Clamp(id - 1, 0, ButtonsLimit() - 1);
            SelectButton(previous, doMovement);
        }
    }

    void CheckLimits(int current, int movement)
    {
        int prev = (int)Mathf.Repeat(current - movement, ButtonsLimit());
        int next = (int)Mathf.Repeat(current + movement, ButtonsLimit());
        Vector3 prevDir = buttons[current].position - buttons[prev].position;
        Vector3 nextDir = (buttons[next].position - buttons[current].position).normalized;
        //TO DO: 3D Directional movement limits
        float signX = Mathf.Sign(prevDir.x);
        if ((Mathf.Abs(nextDir.x) > Vector45value) && (Mathf.Sign(nextDir.x) != signX))
        {
            if (signX > 0f) reachLimitEvents.reachedRight?.Invoke();
            else reachLimitEvents.reachedLeft?.Invoke();
        }
        float signY = Mathf.Sign(prevDir.y);
        if ((Mathf.Abs(nextDir.y) > Vector45value) && (Mathf.Sign(nextDir.y) != signY))
        {
            if (signY > 0f) reachLimitEvents.reachedTop?.Invoke();
            else reachLimitEvents.reachedBottom?.Invoke();
        }
    }

    public void DirectionSelect_Left()
    {
        DirectionSelect(Vector3.left);
    }

    public void DirectionSelect_Up()
    {
        DirectionSelect(Vector3.up);
    }

    public void DirectionSelect_Right()
    {
        DirectionSelect(Vector3.right);
    }

    public void DirectionSelect_Down()
    {
        DirectionSelect(Vector3.down);
    }

    public void DirectionSelect_Forward()
    {
        DirectionSelect(Vector3.forward);
    }

    public void DirectionSelect_Back()
    {
        DirectionSelect(Vector3.back);
    }

    public void DirectionSelect(Vector2 direction)
    {
        DirectionSelect((Vector3)direction);
    }

    public void DirectionSelect(Vector3 direction)
    {
        if (this.IsActiveAndEnabled()) DirectionSelect(direction, true);
    }

    void DirectionSelect(Vector3 direction, bool doMovement)
    {
        if (currentButton != null)
        {
            float dist = directionSelectMaxDistance;
            float sqrDist = dist * dist;
            int id = -1;
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] == currentButton)
                    continue;

                Vector3 dir = buttons[i].transform.position - currentButton.transform.position;
                if ((Vector3.Angle(direction, dir) < directionSelectMaxAngle) &&
                    (dir.sqrMagnitude < sqrDist))
                {
                    sqrDist = dir.sqrMagnitude;
                    id = i;
                }
            }
            if (id < 0)
            {
                //TO DO: 3D Directional movement limits
                if (Vector3.Angle(direction, Vector3.right) < directionSelectMaxAngle)
                    reachLimitEvents.reachedRight?.Invoke();
                else if (Vector3.Angle(direction, Vector3.left) < directionSelectMaxAngle)
                    reachLimitEvents.reachedLeft?.Invoke();
                if (Vector3.Angle(direction, Vector3.up) < directionSelectMaxAngle)
                    reachLimitEvents.reachedTop?.Invoke();
                else if (Vector3.Angle(direction, Vector3.down) < directionSelectMaxAngle)
                    reachLimitEvents.reachedBottom?.Invoke();

                if (loopButtons)
                {
                    auxButtonList = auxButtonList.ClearOrCreate(); //Opposite buttons
                    auxButtonList.AddRange(buttons);
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        if (buttons[i] == currentButton)
                            auxButtonList.Remove(buttons[i]);
                        else
                            for (int j = 0; j < buttons.Count; j++)
                            {
                                if (i == j)
                                    continue;

                                Vector3 dir = buttons[j].position - buttons[i].position;
                                if (Vector3.Angle(-direction, dir) < directionSelectMaxAngle)
                                {
                                    auxButtonList.Remove(buttons[i]);
                                    break;
                                }
                            }
                    }

                    float ang = 180f;
                    int opId = -1;
                    for (int i = 0; i < auxButtonList.Count; i++)
                    {
                        Vector3 dir = auxButtonList[i].position - currentButton.position;
                        float angle = Vector3.Angle(-direction, dir);
                        if (angle < ang)
                        {
                            ang = angle;
                            opId = i;
                        }
                    }
                    if (opId >= 0) id = buttons.IndexOf(auxButtonList[opId]);
                }
            }

            if (id < 0) id = buttons.IndexOf(currentButton);
            SelectButton(id, doMovement);
        }
    }

    public void SelectButtonInstant(int id)
    {
        SelectButton(id, false);
    }

    public void SelectButton(RavioliButton_Button button)
    {
        SelectButton(buttons.IndexOf(button));
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
        selector.SetVirtualParent(null);
        MovementBehaviour SB = SBOfButton(currentButton);
        while (!MoveSelector(currentButton.transform.position, SB, timeMode.DeltaTime(), ref tmpSpd))
            yield return timeMode.WaitFor();
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
        selector.SetVirtualParent(currentButton.transform);
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
            yield return timeMode.WaitFor();
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
        buttons = buttons.CreateIfNull();
        if (!buttons.Contains(button))
        {
            button.TryDeselect();
            buttons.Add(button);
            buttons = buttons.OrderBy(x => x.transform.GetSiblingIndex()).ToList();

            if (groupInitialized)
            {
                if (currentButton == null)
                {
                    UpdateCurrentButton(button);
                    InitializeMovementBehaviors(); 
                    TrySelectCurrent();
                }
                else TrySelectCurrent();
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

                int previous = (int)Mathf.Clamp(currentId - 1, 0f, buttons.Count - 1f);
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
            StartCoroutine(MBResetAfterOneFrame());
        }
        else
        {
            FinalizeMovementBehaviors();
            buttons.Remove(button);
        }
    }
    #endregion

    IEnumerator MBResetAfterOneFrame()
    {
        yield return WaitFor.Frames(2);
        TrySelectCurrent();
    }

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

    [Serializable]
    [InlineProperty]
    [HideLabel]
    struct ReachLimitEvents
    {
        [FoldoutGroup("ReachLimitEvents")]
        public DXEvent reachedRight;
        [FoldoutGroup("ReachLimitEvents")]
        public DXEvent reachedLeft;
        [FoldoutGroup("ReachLimitEvents")]
        public DXEvent reachedTop;
        [FoldoutGroup("ReachLimitEvents")]
        public DXEvent reachedBottom;

        public ReachLimitEvents(DXEvent reachedRight, DXEvent reachedLeft,
            DXEvent reachedTop, DXEvent reachedBottom)
        {
            this.reachedRight = reachedRight;
            this.reachedLeft = reachedLeft;
            this.reachedTop = reachedTop;
            this.reachedBottom = reachedBottom;
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
    bool imAddedToGroups;
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
    protected void AddMyselfToParentGroups()
    {
        if (groups == null)
        {
            RavioliButton[] grs = FindObjectsByType<RavioliButton>(FindObjectsSortMode.None);
            List<RavioliButton> lGrs = new List<RavioliButton>();
            foreach (RavioliButton gr in grs)
                if (gr.buttonsParent == transform.parent)
                    lGrs.Add(gr);

            groups = lGrs.ToArray();
        }

        if (!imAddedToGroups)
        {
            imAddedToGroups = true;
            if (!groups.IsNullOrEmpty())
                foreach (RavioliButton gr in groups)
                    gr.AddButton(this);
        }
    }

    void RemoveFromAllGroups()
    {
        if (!groups.IsNullOrEmpty())
            foreach (RavioliButton group in groups)
                group.RemoveButton(this);
        imAddedToGroups = false;
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
        if (!groups.IsNullOrEmpty())
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

            if (!groups.IsNullOrEmpty())
                foreach (RavioliButton group in groups)
                {
                    group.SelectButton(this);
                    group.Select();
                }
        }
    }

    public void SelectInstant()
    {
        if (this.IsActiveAndEnabled())
        {
            AddMyselfToParentGroups();

            if (!groups.IsNullOrEmpty())
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
    [InlineProperty]
    [HideLabel]
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

    public bool IsInGroup(RavioliButton group)
    {
        return groups.Contains(group);
    }
}
