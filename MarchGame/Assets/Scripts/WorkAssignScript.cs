using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorkAssignScript : MonoBehaviour
{
    [SerializeField] public enum WorkType
    {
        Wood,
        Farm,
        Build
    }
    public BoxCollider2D boxCollider2DFarm;
    [SerializeField] public WorkType workType;
    public List<GameObject> WalkToPoints = new List<GameObject>();
    [SerializeField] 
    public List<GameObject> activeWalkToPoints = new List<GameObject>();
    private UnitStatus unitStatus;


    void Start()
    {
        if(workType == WorkType.Farm )
        {
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Collider2D>().enabled = true;
        }
        if(workType != WorkType.Wood)
        {
            foreach (GameObject walkTo in WalkToPoints)
            {
                activeWalkToPoints.Add(walkTo);
            }
        }
    }

    public void DestroyThis()
    {
        Destroy(this.gameObject);
    }
    public void DestroyScript()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.enabled = false;
        Destroy(this);
    }

    public WorkType GetWorkType()
    {
        return workType;
    }

    public void OnAssignWork(GameObject unit, GameObject workslot)
    {
        unitStatus = unit.GetComponent<UnitStatus>();
        Debug.Log("Work assigned to " + unit.name);
        switch (workType)
        {
            case WorkType.Wood:
                Durability durability = workslot.GetComponent<Durability>();
                durability.StartWoodCutting(unit, workslot, this, unitStatus);
                break;
            case WorkType.Farm:
                StartCoroutine(Farming(unit, workslot));
                break;
            case WorkType.Build:
                StartCoroutine(Building(unit, workslot));
                break;
        }
    }
    public void UnAssignWork(UnitStatus unitStatus)
    {
        if(unitStatus.workSlot != null)
        {
            unitStatus.workSlot.GetComponent<Durability>().UnAssignWork(unitStatus);
        }
        else
        {
            Debug.Log("Workslot is null");
            unitStatus.currentState = UnitStatus.CurrentState.Idle;
        }
    }
    public void UnAssignWork()
    {
        Debug.Log("Work unassigned");
        // StopAllCoroutines();
        unitStatus.workSlot.GetComponent<Durability>().UnAssignWork(unitStatus);
        unitStatus.SetState(UnitStatus.CurrentState.Idle);
        if(unitStatus.workSlot != null)
        {
            activeWalkToPoints.Add(unitStatus.workSlot);
        }
        unitStatus.workSlot = null;
        unitStatus.currentWorkAssign = null;
    }
    public void OnAssignRandomWork(GameObject unit, UnitStatus.CurrentState state)
    {
        StartCoroutine(AssignRandomWorkSlot(unit, state));
    }
    public IEnumerator AssignRandomWorkSlot(GameObject unit, UnitStatus.CurrentState state)
    {
        unitStatus = unit.GetComponent<UnitStatus>();
        Vector3 unitPosition = unit.transform.position;
        unitPosition.z = 0;
        SimpleGoalNavigationScript simpleGoalNavigationScript = unit.GetComponent<SimpleGoalNavigationScript>();
        if(activeWalkToPoints.Count == 0)
        {
            unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
            yield break;
        }
        GameObject randomWorkslot = activeWalkToPoints[Random.Range(0, activeWalkToPoints.Count)];
        unitStatus.workSlot = randomWorkslot;
        activeWalkToPoints.Remove(randomWorkslot);
        simpleGoalNavigationScript.SetTargetGO(randomWorkslot);
        if(randomWorkslot == null)
        {
            unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
            yield break;
        }
        while (Vector3.Distance(unitPosition, randomWorkslot.transform.position) > 0.1f && randomWorkslot != null)
        {
            if(randomWorkslot == null)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                yield break;
            }
            unitPosition = unit.transform.position;
            unitPosition.z = 0;
            yield return null;
        }
        if(randomWorkslot == null)
        {
            unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
            yield break;
        }
        unit.GetComponent<UnitStatus>().SetState(state);

        OnAssignWork(unit, randomWorkslot);
    }
    public IEnumerator AssignSetWorkslot(GameObject unit, UnitStatus.CurrentState state, GameObject workslot)
    {
        unitStatus = unit.GetComponent<UnitStatus>();
        Vector3 unitPosition = unit.transform.position;
        unitPosition.z = 0;
        SimpleGoalNavigationScript simpleGoalNavigationScript = unit.GetComponent<SimpleGoalNavigationScript>();
        if(activeWalkToPoints.Count == 0)
        {
            unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
            yield break;
        }
        unitStatus.workSlot = workslot;
        activeWalkToPoints.Remove(workslot);
        simpleGoalNavigationScript.SetTargetGO(workslot);
        if(workslot == null)
        {
            unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
            yield break;
        }
        while (Vector3.Distance(unitPosition, workslot.transform.position) > 0.1f && workslot != null)
        {
            if(workslot == null)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                yield break;
            }
            unitPosition = unit.transform.position;
            unitPosition.z = 0;
            yield return null;
        }
        unit.GetComponent<UnitStatus>().SetState(state);
        OnAssignWork(unit, workslot);
    }
    public IEnumerator AssignRandomWorkSlotDam(GameObject unit, UnitStatus.CurrentState state)
    {
        unitStatus = unit.GetComponent<UnitStatus>();
        Vector3 unitPosition = unit.transform.position;
        unitPosition.z = 0;
        SimpleGoalNavigationScript simpleGoalNavigationScript = unit.GetComponent<SimpleGoalNavigationScript>();
        NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();

        if (activeWalkToPoints.Count == 0)
        {
            unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
            unitStatus.currentWorkAssign = null;
            yield break;
        }

        GameObject randomWorkslot = activeWalkToPoints[Random.Range(0, activeWalkToPoints.Count)];
        unitStatus.workSlot = randomWorkslot;
        activeWalkToPoints.Remove(randomWorkslot);
        simpleGoalNavigationScript.SetTargetGO(randomWorkslot);
        agent.isStopped = false;  // Make sure agent is moving
        agent.SetDestination(randomWorkslot.transform.position);

        while (agent.remainingDistance > 1.5f || agent.pathPending)
        {
            yield return null;
        }
        agent.isStopped = true;  // Stop movement precisely when within range
        agent.SetDestination(unit.transform.position);  // Reset destination to unit positio
        agent.isStopped = false;  // Make sure agent is moving
        unit.GetComponent<UnitStatus>().SetState(state);
        OnAssignWork(unit, randomWorkslot);
    }



    private IEnumerator Farming(GameObject unit, GameObject workslot)
    {
        GameObject targetFarm = workslot;
        Durability durability = targetFarm.GetComponent<Durability>();

        float duration = 500f;
        float damagePerSecond = 1f;
        
        for (float t = 0; t < duration; t += 1f)
        {

            int _currentDurability = durability.GetCurrentDurability();
            if(_currentDurability <= 0 && activeWalkToPoints.Count > 0)
            {
                targetFarm = activeWalkToPoints[Random.Range(0, activeWalkToPoints.Count)];
                durability = targetFarm.GetComponent<Durability>();
                unit.GetComponent<SimpleGoalNavigationScript>().SetTargetGO(targetFarm);
            }
            else if(_currentDurability <= 0)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                yield break;
            }
            durability.TakeDamage((int)damagePerSecond, Durability.ResourceType.Food);
            unit.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator Building(GameObject unit, GameObject workslot)
    {
        GameObject targetBuilding = workslot;
        Durability durability = targetBuilding.GetComponent<Durability>();

        float duration = 500f;
        float damagePerSecond = 1f;
        
        for (float t = 0; t < duration; t += 1f)
        {
            int _currentDurability = durability.GetCurrentDurability();
            if(_currentDurability <= 0 && activeWalkToPoints.Count > 0)
            {
                targetBuilding = activeWalkToPoints[Random.Range(0, activeWalkToPoints.Count)];
                durability = targetBuilding.GetComponent<Durability>();
                unit.GetComponent<SimpleGoalNavigationScript>().SetTargetGO(targetBuilding);
            }
            else if(_currentDurability <= 0)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                yield break;
            }
            durability.TakeDamage((int)damagePerSecond, Durability.ResourceType.Building);
            yield return new WaitForSeconds(1f);
        }
    }
}
