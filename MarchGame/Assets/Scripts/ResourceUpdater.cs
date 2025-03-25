using UnityEngine;

public class ResourceUpdater : MonoBehaviour
{
    public MarchGameVariables marchGameVariables;
    public enum ResourceType
    {
        Wood,
        Food,
        Unit
    }
    public ResourceType resourceType;
    public TMPro.TextMeshProUGUI resourceText;

    void Update()
    {
        if(resourceType == ResourceType.Wood)
        {
            resourceText.text = marchGameVariables.wood.ToString();
        }
        else if(resourceType == ResourceType.Food)
        {
            resourceText.text = marchGameVariables.food.ToString();
        }
        else if(resourceType == ResourceType.Unit)
        {
            resourceText.text = marchGameVariables.currentUnits.ToString() +" / " + marchGameVariables.possibleUnits.ToString();
        }
    }
}
