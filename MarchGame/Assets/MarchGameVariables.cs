using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MarchGameVariables", menuName = "Scriptable Objects/MarchGameVariables")]
public class MarchGameVariables : ScriptableObject
{
    [Header("Border Settings")]
    public int minX;
    public int maxX;
    public int minY;
    public int maxY;

    [Header("Resources")]
    public int wood;
    public int food;
    public int possibleUnits;
    public int currentUnits;
    public Dictionary<Durability, float> refreshables = new Dictionary<Durability, float>();
    public Dictionary<GameObject, float> plantedTrees = new Dictionary<GameObject, float>();
    public int waterTilesCount;
    public int dayCount;
}
