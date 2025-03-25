using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomizeWorldGeneration : MonoBehaviour
{
    [Header("World Generation Tiles")]
    [SerializeField] Tilemap DecoTilemap;
    [SerializeField] TileBase GrassTile;
    [SerializeField] float GrassWeight;
    [SerializeField] TileBase StickTile;
    [SerializeField] float StickWeight;
    [SerializeField] TileBase RockTile;
    [SerializeField] float RockWeight;
    [SerializeField] TileBase BushTile;
    [SerializeField] float BushWeight;
    [SerializeField] TileBase FlowerTile;
    [SerializeField] float FlowerWeight;
    [SerializeField] Tilemap GroundTilemap;
    [SerializeField] TileBase GroundTile;
    [SerializeField] TileBase PuddleTile;
    [SerializeField] float PuddleWeight;
    [SerializeField] Tilemap borderTilemap;
    
    [Header("Decoration Chance")]
    [SerializeField] [Range(0, 1)] float DecorationChance = 0.8f;

    [Header("World Generation Variables Resources")]
    [SerializeField] WorkAssignScript workAssignScriptTree;
    [SerializeField] List<GameObject> TreePrefabs;
    [SerializeField] int TreeAmount;

    [Header("World Generation Variables")]
    public MarchGameVariables marchGameVariables;
    [SerializeField] int borderMinX;
    [SerializeField] int borderMaxX;
    [SerializeField] int borderMinY;
    [SerializeField] int borderMaxY;

    private List<GameObject> instantiatedTrees = new List<GameObject>();
    
    // Store the total weight for decoration selection
    private float totalDecoWeight;
    // Store decorations in a struct for easier selection
    private struct DecorationOption
    {
        public TileBase tile;
        public float weight;
        public float cumulativeWeight; // Used for selection
    }
    private List<DecorationOption> decorationOptions = new List<DecorationOption>();
    
    void Start()
    {
        instantiatedTrees.Clear();
        borderMaxX = marchGameVariables.maxX;
        borderMinX = marchGameVariables.minX;
        borderMaxY = marchGameVariables.maxY;
        borderMinY = marchGameVariables.minY;
        
        // Initialize decoration options
        InitializeDecorationOptions();
        
        RandomizeWorld();
    }

    void InitializeDecorationOptions()
    {
        decorationOptions.Clear();
        totalDecoWeight = 0;
        
        // Add decoration options with weights
        AddDecorationOption(GrassTile, GrassWeight);
        AddDecorationOption(StickTile, StickWeight);
        AddDecorationOption(RockTile, RockWeight);
        AddDecorationOption(BushTile, BushWeight);
        AddDecorationOption(FlowerTile, FlowerWeight);
        AddDecorationOption(PuddleTile, PuddleWeight);
    }
    
    void AddDecorationOption(TileBase tile, float weight)
    {
        if (tile != null && weight > 0)
        {
            totalDecoWeight += weight;
            
            DecorationOption option = new DecorationOption
            {
                tile = tile,
                weight = weight,
                cumulativeWeight = totalDecoWeight
            };
            
            decorationOptions.Add(option);
        }
    }

    TileBase SelectRandomDecoration()
    {
        // Draw a random value between 0 and total weight
        float randomValue = Random.Range(0, totalDecoWeight);
        
        // Find the decoration that corresponds to this random value
        foreach (DecorationOption option in decorationOptions)
        {
            if (randomValue <= option.cumulativeWeight)
            {
                return option.tile;
            }
        }
        
        // Fallback
        return null;
    }

    void RandomizeWorld()
    {
        // Clear existing decorations
        DecoTilemap.ClearAllTiles();
        
        // Generate new decorations
        for (int x = borderMinX; x < borderMaxX; x++)
        {
            for (int y = borderMinY; y < borderMaxY; y++)
            {
                // First check if this tile should have a decoration
                if (Random.value < DecorationChance)
                {
                    // Then select which decoration to place
                    if(borderTilemap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        continue;
                    }
                    TileBase selectedTile = SelectRandomDecoration();
                    DecoTilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
                }
                else
                {
                    // No decoration for this tile
                    DecoTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
        
        // Clear existing trees
        foreach (GameObject tree in instantiatedTrees)
        {
            if (tree != null)
            {
                Destroy(tree);
            }
        }
        instantiatedTrees.Clear();
        workAssignScriptTree.activeWalkToPoints.Clear();
        workAssignScriptTree.WalkToPoints.Clear();
        
        // Generate new trees
        for (int i = 0; i < TreeAmount; i++)
        {
            // Get a random tile position within tilemap bounds
            Vector3Int treeCellPosition = new Vector3Int(
                Random.Range(borderMinX+2, borderMaxX-2),
                Random.Range(borderMinY+2, borderMaxY-2),
                0
            );

            // Convert the tile position to a world position
            Vector3 treeWorldPosition = GroundTilemap.CellToWorld(treeCellPosition);

            // Instantiate tree at world position
            GameObject tempTree = Instantiate(
                TreePrefabs[Random.Range(0, TreePrefabs.Count)],
                treeWorldPosition + new Vector3(0.5f, 0.5f, 0), // Offset to center the tree
                Quaternion.identity
            );

            tempTree.GetComponent<Durability>().workAssignScript = workAssignScriptTree;
            instantiatedTrees.Add(tempTree);
        }
        
        workAssignScriptTree.activeWalkToPoints.AddRange(instantiatedTrees);
        workAssignScriptTree.WalkToPoints.AddRange(instantiatedTrees);
    }
    
    // Public method to regenerate the world (can be called from UI or other scripts)
    public void RegenerateWorld()
    {
        RandomizeWorld();
    }
}