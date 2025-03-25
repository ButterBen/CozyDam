using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitStatus : MonoBehaviour
{
    public enum CurrentState
    {
        Idle,
        WoodCutting,
        Farming,
        Building
    }
    public Animator animator;
    public WorkAssignScript currentWorkAssign;
    public CurrentState currentState;
    public SimpleGoalNavigationScript simpleGoalNavigationScript;
    public GameObject unit;
    public GameObject workSlot;
    public List <Renderer> renderers;
    private MaterialPropertyBlock propBlock;
    public SortingGroup sortingGroup;
    public GameObject lamp;
    private static readonly int OutlineToggle = Shader.PropertyToID("_SwitchOutlineBool"); // Replace with your property name
    private DayNightHandler dayNightHandler;

    void Awake()
    {
        dayNightHandler = FindFirstObjectByType<DayNightHandler>();
        propBlock = new MaterialPropertyBlock();
        dayNightHandler.nightEvent.AddListener(ToggleLamp);
        dayNightHandler.dayEvent.AddListener(ToggleLamp);
    }
    void Start()
    {
        if(dayNightHandler.isNight)
        {
            ToggleLamp();
        }
        currentState = CurrentState.Idle;
    }
    void Update()
    {
        if((currentState == CurrentState.WoodCutting || currentState== CurrentState.Farming || currentState== CurrentState.Building)  && workSlot == null)
        {
            SetState(CurrentState.Idle);
        }
    }

    public void SetState(CurrentState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case CurrentState.Idle:
                animator.SetBool("walking", false);
                animator.SetBool("woodCutting", false);
                animator.SetBool("farming", false);
                animator.SetBool("building", false);
                break;
            case CurrentState.WoodCutting:
                unit.transform.rotation = Quaternion.Euler(0, 0, 0);
                animator.SetBool("woodCutting", true);
                break;
            case CurrentState.Farming:
                unit.transform.rotation = Quaternion.Euler(0, 0, 0);
                animator.SetBool("farming", true);
                break;
            case CurrentState.Building:
                animator.SetBool("building", true);
                break;
        }
    }
    public void SetWorkAssign(WorkAssignScript newWorkAssign)
    {
        if(currentWorkAssign != null)
        {
            currentWorkAssign.UnAssignWork(this);
        }
        currentWorkAssign = newWorkAssign;
    }
    public void SetSelected(bool isSelected)
    {
        foreach(Renderer renderer in renderers)
        {
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat(OutlineToggle, isSelected ? 1 : 0);
            renderer.SetPropertyBlock(propBlock);
            sortingGroup.sortingOrder = isSelected ? 3 : 2;

        }
    }
    public void ToggleLamp()
    {
        lamp.SetActive(!lamp.activeSelf);
    }
    public void Deselect()
    {
        SetSelected(false);
    }
}
