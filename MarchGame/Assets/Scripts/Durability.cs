using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Durability : MonoBehaviour
{
    [SerializeField] public int maxDurability;
    private int currentDurability;
    private bool destroyed = false;

    [Header("Building")]
    public bool isBuilding = false;
    public bool isDam = false;
    [SerializeField] private MarchGameVariables marchGameVariables;
    [SerializeField] private GameObject buildingStage1;
    [SerializeField] private GameObject buildingStage2;
    [SerializeField] private List<AudioClip> buildingSounds;

    [Header("Tree")]
    [SerializeField] private GameObject treeStage1;
    [SerializeField] private GameObject treeStage2;
    [SerializeField] private GameObject treeStage3;
    [SerializeField] private GameObject treeStage0;
    [SerializeField] private List<AudioClip> treeSounds;

    [Header("Farm")]
    [SerializeField] private GameObject farmStage1;
    [SerializeField] private GameObject farmStage2;
    [SerializeField] private GameObject farmStage3;
    [SerializeField] private List<AudioClip> farmSounds;
    [Header("Dam")]
    [SerializeField] private GameObject damStage1;
    [SerializeField] private GameObject damStage2;
    public WorkAssignScript workAssignScript;
    private UnitStatus unitStatus_;
    [SerializeField] private bool grown;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] public enum ResourceType
    {
        Wood,
        Food,
        Building
    }
    public ResourceType resourceType;

    void Start()
    {
        currentDurability = maxDurability;
    }

    public void Grow()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        treeStage0.SetActive(false);
        treeStage1.SetActive(true);
        grown = true;
        workAssignScript.activeWalkToPoints.Add(gameObject);
    }
    
    void Update()
    {
        if(!isBuilding && !isDam)
        {
            if(resourceType == ResourceType.Wood)
            {
                if(currentDurability > maxDurability/3 && grown)
                {
                    treeStage1.SetActive(true);
                    treeStage2.SetActive(false);
                    treeStage3.SetActive(false);
                }
                else if (currentDurability <= maxDurability/3 && currentDurability >0)
                {
                    treeStage1.SetActive(false);
                    treeStage2.SetActive(true);
                    treeStage3.SetActive(false);
                }
                else if(currentDurability <= 0 && !destroyed)
                {
                    treeStage1.SetActive(false);
                    treeStage2.SetActive(false);
                    treeStage3.SetActive(true);

                    destroyed = true;

                    workAssignScript.activeWalkToPoints.Remove(gameObject);
                    workAssignScript.WalkToPoints.Remove(gameObject);
                }
            }
            else if(resourceType == ResourceType.Food)
            {
                if(currentDurability > maxDurability/3)
                {
                    farmStage1.SetActive(true);
                    farmStage2.SetActive(false);
                    farmStage3.SetActive(false);
                }
                else if (currentDurability <= maxDurability/3 && currentDurability >0)
                {
                    farmStage1.SetActive(false);
                    farmStage2.SetActive(true);
                    farmStage3.SetActive(false);
                }
                else if(currentDurability <= 0 && !destroyed)
                {
                    farmStage1.SetActive(true);
                    farmStage2.SetActive(false);
                    farmStage3.SetActive(true);
                    marchGameVariables.food += 10;
                    destroyed = true;
                    workAssignScript.boxCollider2DFarm.enabled = false;
                    marchGameVariables.refreshables.Add(this,1);
                    workAssignScript.activeWalkToPoints.Remove(gameObject);

                }
            }
        }
        else if(isBuilding && !isDam)
        {
            if(currentDurability <= 0 && !destroyed)
            {
                buildingStage1.SetActive(false);
                buildingStage2.SetActive(true);
                Destroy(buildingStage1);
                destroyed = true;
                workAssignScript.DestroyScript();
                workAssignScript.UnAssignWork();
                Destroy(this);
                workAssignScript.activeWalkToPoints.Remove(gameObject);
                marchGameVariables.possibleUnits+=2;
            }
        }
        else if(isBuilding && isDam)
        {
            if (currentDurability <= 0 && !destroyed)
            {
                damStage1.SetActive(false);
                damStage2.SetActive(true);
                destroyed = true;
                workAssignScript.activeWalkToPoints.Remove(gameObject);
                TilemapPlacer tilemapPlacer = FindFirstObjectByType<TilemapPlacer>();
                tilemapPlacer.PlaceDam(this.gameObject.transform.position);
                workAssignScript.UnAssignWork();
                workAssignScript.DestroyScript();
                Destroy(this);
            }
        }
        
    }
    public void TakeDamage(int damage, ResourceType resourceType)
    {
        if(resourceType == ResourceType.Wood)
        {
            marchGameVariables.wood += damage;
        }
        else if(resourceType == ResourceType.Food)
        {
            marchGameVariables.food += damage;
        }
        playRandomSound(resourceType);
        currentDurability -= damage;
    }
    public int GetCurrentDurability()
    {
        return currentDurability;
    }
    public void RefreshResource(int durability)
    {
        Debug.Log("Refreshed");
        maxDurability = durability;
        currentDurability = durability;
        destroyed = false;
        float random = Random.Range(0.2f, .35f);
        if(this.gameObject.tag == "Tree")
        {
            this.gameObject.transform.localScale = new Vector3(random, random, random);
        }
        if(workAssignScript.activeWalkToPoints.Contains(gameObject) == false)
        {
            workAssignScript.activeWalkToPoints.Add(gameObject);
        }
        workAssignScript.boxCollider2DFarm.enabled = true;
    }
    public void DestroyResource()
    {
        Debug.Log("Destroyed Resource" + gameObject.name);
        //workAssignScript.StopAllCoroutines();
        if(workAssignScript.activeWalkToPoints.Contains(gameObject) == true)
        {
            workAssignScript.activeWalkToPoints.Remove(gameObject);
        }
        workAssignScript.WalkToPoints.Remove(gameObject);
        if(workAssignScript.WalkToPoints.Count == 0)
        {
            workAssignScript.DestroyThis();
        }
        Destroy(gameObject);
    }

    public IEnumerator WaitForDestruction()
    {
        yield return new WaitForSecondsRealtime(2f);
        Destroy(gameObject);
    }
    public void playRandomSound(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                audioSource.PlayOneShot(treeSounds[Random.Range(0, treeSounds.Count)]);
                break;
            case ResourceType.Food:
                audioSource.PlayOneShot(farmSounds[Random.Range(0, farmSounds.Count)]);
                break;
            case ResourceType.Building:
                audioSource.PlayOneShot(buildingSounds[Random.Range(0, buildingSounds.Count)]);
                break;
        }
    }


    public void StartWoodCutting(GameObject unit, GameObject workslot, WorkAssignScript workAssignScript, UnitStatus unitStatus)
    {
        StopAllCoroutines();
        unitStatus_ = unitStatus;
        this.workAssignScript = workAssignScript;
        StartCoroutine(WoodCutting(unit, workslot, workAssignScript));
    }
    public IEnumerator WoodCutting(GameObject unit, GameObject workslot, WorkAssignScript workAssignScript)
    {
        GameObject targetTree = workslot;
        Durability durability = targetTree.GetComponent<Durability>();
        float duration = 50000f;
        float damagePerSecond = 1f;
        
        for (float t = 0; t < duration; t += 1f)
        {
            int _currentDurability = durability.GetCurrentDurability();
           // Debug.Log("Current durabilityyyyy: " + _currentDurability + "activeWalkToPoints: " + activeWalkToPoints.Count + "duration: " + t);
            if( _currentDurability <= 0 && workAssignScript.activeWalkToPoints.Count > 0)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                //StartCoroutine(workAssignScript.AssignRandomWorkSlot(unit, UnitStatus.CurrentState.WoodCutting));
                workAssignScript.OnAssignRandomWork(unit, UnitStatus.CurrentState.WoodCutting);
                durability.StartCoroutine(durability.WaitForDestruction());
                yield break;
            }
            else if(_currentDurability <= 0 && workAssignScript.activeWalkToPoints.Count <= 0)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                durability.StartCoroutine(durability.WaitForDestruction());
                yield break;
            }
            durability.TakeDamage((int)damagePerSecond, Durability.ResourceType.Wood);
            unit.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 0, 0);

            yield return new WaitForSeconds(1f);
        }
    }
    public void UnAssignWork(UnitStatus unitStatus_)
    {
        Debug.Log("Work unassigned from Durability");
        StopAllCoroutines();
        unitStatus_.SetState(UnitStatus.CurrentState.Idle);
        if(unitStatus_.workSlot != null)
        {
            workAssignScript.activeWalkToPoints.Add(unitStatus_.workSlot);
        }
        unitStatus_.workSlot = null;
        unitStatus_.currentWorkAssign = null;
    }
    /*public IEnumerator WoodCutting(GameObject unit, GameObject workslot)
    {
        GameObject targetTree = workslot;
        Durability durability = targetTree.GetComponent<Durability>();
        float duration = 50000f;
        float damagePerSecond = 1f;
        
        for (float t = 0; t < duration; t += 1f)
        {
            int _currentDurability = durability.GetCurrentDurability();
           // Debug.Log("Current durabilityyyyy: " + _currentDurability + "activeWalkToPoints: " + activeWalkToPoints.Count + "duration: " + t);
            if( _currentDurability <= 0 && activeWalkToPoints.Count > 0)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                StartCoroutine(AssignRandomWorkSlot(unit, UnitStatus.CurrentState.WoodCutting));
                durability.StartCoroutine(durability.WaitForDestruction());
                yield break;
            }
            else if(_currentDurability <= 0 && activeWalkToPoints.Count <= 0)
            {
                unit.GetComponent<UnitStatus>().SetState(UnitStatus.CurrentState.Idle);
                durability.StartCoroutine(durability.WaitForDestruction());
                yield break;
            }
            durability.TakeDamage((int)damagePerSecond, Durability.ResourceType.Wood);
            unit.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 0, 0);

            yield return new WaitForSeconds(1f);
        }
    }*/
}
