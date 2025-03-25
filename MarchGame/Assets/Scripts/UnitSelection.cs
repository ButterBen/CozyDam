using UnityEngine;
using System.Collections.Generic;

public class UnitSelection : MonoBehaviour
{
    private Vector2 startMousePos;
    private Vector2 currentMousePos;
    private Vector2 _startMousePos;
    private Vector2 _currentMousePos;
    private bool isSelecting = false;
    private List<GameObject> selectedUnits = new List<GameObject>();
    public bool isBuildingUnit = false;
    public Texture2D cursorTextureWood;
    public Texture2D cursorTextureBuilding;
    public Texture2D cursorTextureFarm;
    public RectTransform selectionBox;
    public bool inMenu = false;

    void Update()
    {
        if(isBuildingUnit || inMenu)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (GameObject unit in selectedUnits)
            {
                unit.GetComponent<UnitStatus>().SetSelected(false);
            }
            selectedUnits.Clear();
            startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _startMousePos = Input.mousePosition;
            isSelecting = true;
            selectionBox.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(1))
        {
            currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _currentMousePos = Input.mousePosition;
            UpdateSelectionBox();
        }

        if (Input.GetMouseButtonUp(1))
        {
            isSelecting = false;
            selectionBox.gameObject.SetActive(false);
            SelectUnits();
        }

        //color mouse cursor when hovering over wood resource

        RaycastHit2D hit = Physics2D.Raycast(
        Camera.main.ScreenToWorldPoint(Input.mousePosition), 
        Vector2.zero,
        Mathf.Infinity,
        LayerMask.GetMask("UI")
        );
        if(selectedUnits.Count > 0)
        {
            switch (hit.collider)
            {
                case null:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
                case { } _ when hit.collider.CompareTag("Tree"):
                    if (cursorTextureWood != null)
                    { 
                        Vector2 hotspot = new Vector2(cursorTextureWood.width / 2, cursorTextureWood.height / 2);
                        Cursor.SetCursor(cursorTextureWood, hotspot, CursorMode.Auto);
                    }
                    break;
                case { } _ when hit.collider.CompareTag("FoodResource"):
                    if (cursorTextureFarm != null)
                    {
                        Vector2 hotspot = new Vector2(cursorTextureFarm.width / 2, cursorTextureFarm.height / 2);
                        Cursor.SetCursor(cursorTextureFarm, hotspot, CursorMode.Auto);
                    }
                    break;
                case { } _ when hit.collider.CompareTag("Building"):
                    if (cursorTextureBuilding != null)
                    {
                        Vector2 hotspot = new Vector2(cursorTextureBuilding.width / 2, cursorTextureBuilding.height / 2);
                        Cursor.SetCursor(cursorTextureBuilding, hotspot, CursorMode.Auto);
                    }
                    break;
                case { } _ when hit.collider.CompareTag("DamBuilding"):
                    if (cursorTextureBuilding != null)
                    {
                        Vector2 hotspot = new Vector2(cursorTextureBuilding.width / 2, cursorTextureBuilding.height / 2);
                        Cursor.SetCursor(cursorTextureBuilding, hotspot, CursorMode.Auto);
                    }
                    break;
                default:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
            }
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        



        // Handle right-click interactions
        if (Input.GetMouseButtonDown(0) && selectedUnits.Count > 0)
        {
            Vector3 worldClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldClickPosition.z = 0; // Ensure correct 2D positioning

            //TO DO show different mouse cursor when hovering over wood resource
           // RaycastHit2D hit = Physics2D.Raycast(worldClickPosition, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Tree"))
            {
                int selectionCount = 0;
               // WorkAssignScript workAssign = hit.collider.GetComponent<WorkAssignScript>();
                Durability durability = hit.collider.GetComponent<Durability>();
                WorkAssignScript workAssign = durability.workAssignScript;
                foreach(GameObject unit in selectedUnits)
                {

                    UnitStatus _unitStatus = unit.GetComponent<UnitStatus>();
                    if(_unitStatus.currentState == UnitStatus.CurrentState.WoodCutting)
                    {
                        return;
                    }
                    else
                    {
                        _unitStatus.SetWorkAssign(workAssign);
                        if(selectionCount == 0)
                        {
                            StartCoroutine(workAssign.AssignSetWorkslot(unit, UnitStatus.CurrentState.WoodCutting, hit.collider.gameObject));
                            selectionCount++;
                        }
                        else
                        {
                            StartCoroutine(workAssign.AssignRandomWorkSlot(unit, UnitStatus.CurrentState.WoodCutting));
                        }
                    }
                    unit.GetComponent<UnitStatus>().SetSelected(false);
                }
                selectedUnits.Clear();
            }
            else if(hit.collider != null && hit.collider.CompareTag("Building"))
            {
                WorkAssignScript workAssign = hit.collider.GetComponent<WorkAssignScript>();
                foreach(GameObject unit in selectedUnits)
                {
                    
                    UnitStatus _unitStatus = unit.GetComponent<UnitStatus>();
                    if(_unitStatus.currentState == UnitStatus.CurrentState.Building)
                    {
                        return;
                    }
                    else
                    {
                        _unitStatus.SetWorkAssign(workAssign);
                        StartCoroutine(workAssign.AssignRandomWorkSlot(unit, UnitStatus.CurrentState.Building));
                    }
                    unit.GetComponent<UnitStatus>().SetSelected(false);
                }
                selectedUnits.Clear();
            }
            else if(hit.collider != null && hit.collider.CompareTag("FoodResource"))
            {
                WorkAssignScript workAssign = hit.collider.GetComponent<WorkAssignScript>();
                foreach(GameObject unit in selectedUnits)
                {
                    UnitStatus _unitStatus = unit.GetComponent<UnitStatus>();
                    if(_unitStatus.currentState == UnitStatus.CurrentState.Farming)
                    {
                        return;
                    }
                    else
                    {
                        _unitStatus.SetWorkAssign(workAssign);
                        StartCoroutine(workAssign.AssignRandomWorkSlot(unit, UnitStatus.CurrentState.Farming));
                    }
                    unit.GetComponent<UnitStatus>().SetSelected(false);
                }
                selectedUnits.Clear();
            }
            else if(hit.collider != null && hit.collider.CompareTag("DamBuilding"))
            {
                WorkAssignScript workAssign = hit.collider.GetComponent<WorkAssignScript>();
                foreach(GameObject unit in selectedUnits)
                {
                    UnitStatus _unitStatus = unit.GetComponent<UnitStatus>();
                    if(_unitStatus.currentState == UnitStatus.CurrentState.Building)
                    {
                        return;
                    }
                    else
                    {
                        _unitStatus.SetWorkAssign(workAssign);
                        StartCoroutine(workAssign.AssignRandomWorkSlotDam(unit, UnitStatus.CurrentState.Building));
                    }
                    unit.GetComponent<UnitStatus>().SetSelected(false);
                }
                selectedUnits.Clear();
                
            }
            else
            {

                float spreadRadius = 1.5f;

                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    if(selectedUnits[i].GetComponent<UnitStatus>().currentWorkAssign != null)
                    {

                        selectedUnits[i].GetComponent<UnitStatus>().currentWorkAssign.UnAssignWork(selectedUnits[i].GetComponent<UnitStatus>());
                        
                    }
                    spreadRadius += 0.2f;
                    Vector3 randomOffset = Random.insideUnitCircle * spreadRadius;
                    if (selectedUnits.Count == 1)
                    {
                        randomOffset = Vector3.zero;
                    }

                    Vector3 finalPosition = worldClickPosition + randomOffset;
                    selectedUnits[i].GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                    selectedUnits[i].GetComponent<SimpleGoalNavigationScript>().SetTargetTransform(finalPosition);
                    
                }
            }
        }
    }




    void SelectUnits()
    {
        Vector2 center = (startMousePos + currentMousePos) / 2f;
        Vector2 size = new Vector2(Mathf.Abs(currentMousePos.x - startMousePos.x), Mathf.Abs(currentMousePos.y - startMousePos.y));

        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f);

        selectedUnits.Clear();
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("WoodCutter"))
            {
                selectedUnits.Add(collider.gameObject);
                Debug.Log("Selected: " + collider.name);
                collider.gameObject.GetComponent<UnitStatus>().SetSelected(true);
            }
        }
    }

    void UpdateSelectionBox()
    {
        Vector2 boxStart = _startMousePos;
        Vector2 boxEnd = _currentMousePos;
        
        Vector2 min = Vector2.Min(boxStart, boxEnd);
        Vector2 max = Vector2.Max(boxStart, boxEnd);
        Vector2 size = max - min;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            selectionBox.parent as RectTransform,
            min + size/2f,
            null, 
            out Vector2 localPos
        );
        
        selectionBox.localPosition = localPos;
        selectionBox.sizeDelta = size;
    }
}