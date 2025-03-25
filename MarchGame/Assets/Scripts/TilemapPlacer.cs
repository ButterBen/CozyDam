using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NavMeshPlus.Components;

public class TilemapPlacer : MonoBehaviour
{
    [Header("Road Tiles")]
    public Tilemap roadTilemap;  // Assign your Tilemap in the Inspector
    public TileBase roadTile; // Assign the road tile in the Inspector
    public AudioClip roadPlacementSound;
    [Header("Border Tiles")]
    public Tilemap borderTileMap; // Assign the border tilemap in the Inspector
    public TileBase borderTile; // Assign the border tile in the Inspector
    [Header("Ground Tiles")]
    public Tilemap groundTilemap; // Assign the ground tilemap in the Inspector
    public TileBase groundTile; // Assign the ground tile in the Inspector
    [Header("Dam Tiles")]
    public Tilemap damTilemap; // Assign the dam tilemap in the Inspector
    public TileBase damTile; // Assign the dam tile in the Inspector
    public AudioClip damHousePlacementSound;
    public GameObject dam;
    public Tilemap waterTilemap;
    [Header("Building Tiles")]
    public Tilemap buildingTilemap; // Assign the building tilemap in the Inspector
    public TileBase buildingTile1; // Assign the building tile in the Inspector
    public TileBase buildingTile2; // Assign the building tile in the Inspector
    public TileBase buildingTile3; // Assign the building tile in the Inspector
    public TileBase buildingTile4; // Assign the building tile in the Inspector
    public Tilemap decoTileMap;
    public GameObject farm;
    public AudioClip farmPlacementSound;
    public GameObject Unit;
    public List<GameObject> Trees;
    public AudioClip treePlacementSound;
    public bool isBuilding = false;
    public GameObject previewObject;
    public MarchGameVariables marchGameVariables;
    [SerializeField] GameObject buildingObject;
    public NavMeshSurface navMeshSurface;
    public WorkAssignScript treeWorkAssign;
    private int minX;
    private int maxX;
    private int minY;
    private int maxY;
    public enum TileType
    {
        Road,
        Building,
        Dam,
        Unit,
        Farm,
        Ground,
        Tree
    }
    [SerializeField]
    public TileType tileType;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        minX = marchGameVariables.minX;
        maxX = marchGameVariables.maxX;
        minY = marchGameVariables.minY;
        maxY = marchGameVariables.maxY;
        //Debug.Log("Ground tiles: " + CountGroundTiles());

    }
    private int CountGroundTiles()
    {
        int count = 0;
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (var position in bounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(position))
            {
                count++;
            }
        }

        return count;
    }
    
    void Update()
    {
        if (isBuilding && previewObject!=null)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0;
            Vector3Int cellPosition = groundTilemap.WorldToCell(worldPosition);
            Vector3 snappedWorldPosition = groundTilemap.CellToWorld(cellPosition);

            previewObject.transform.position = snappedWorldPosition;
            //switch case for tileType
            switch (tileType)
            {
                case TileType.Road:
                    HandleTileType(cellPosition, TileType.Road, roadTilemap);
                    break;
                case TileType.Building:
                    HandleTileType(cellPosition, TileType.Building, buildingTilemap);
                    break;
                case TileType.Dam:
                    HandleDamPlacement(cellPosition);
                    break;
                case TileType.Unit:
                    HandleUnitPlacement(cellPosition);
                    break;
                case TileType.Farm:
                    HandleFarmPlacement(cellPosition);
                    break;
                case TileType.Ground:
                    HandleGroundPlacement(cellPosition);
                    break;
                case TileType.Tree:
                    HandleTreePlacement(cellPosition);
                    break;
            }
        }
    }
    void HandleUnitPlacement(Vector3Int cellPosition)
    {
        if (Input.GetMouseButtonDown(0) && isWithinBounds(cellPosition) && marchGameVariables.currentUnits < marchGameVariables.possibleUnits && marchGameVariables.food >= 20) // Left mouse button
        {
            if (isWithinBounds(cellPosition))
            {
                // Place the unit
                marchGameVariables.food -= 20;
                Vector3 snappedWorldPosition = groundTilemap.CellToWorld(cellPosition);
                Instantiate(Unit, snappedWorldPosition, Quaternion.identity);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            isBuilding = false;
        }
    }

    void HandleTreePlacement(Vector3Int cellPosition)
    {
        if (Input.GetMouseButtonDown(0) && isWithinBounds(cellPosition)) // Left mouse button
        {
            if (isWithinBounds(cellPosition))
            {
                // Place the tree
                Vector3 snappedWorldPosition = groundTilemap.CellToWorld(cellPosition);
                snappedWorldPosition += new Vector3(-0.871f, 0, 0); // Center the tree
                int randomIndex = Random.Range(0, Trees.Count);
                GameObject tempTree = Instantiate(Trees[randomIndex], snappedWorldPosition, Quaternion.identity);
                tempTree.GetComponent<Durability>().workAssignScript = treeWorkAssign;
                float randomGrowChance = Random.Range(0.3f, 0.7f);
                marchGameVariables.plantedTrees.Add(tempTree,randomGrowChance);
                audioSource.PlayOneShot(treePlacementSound);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            isBuilding = false;
        }
    }


    void HandleGroundPlacement(Vector3Int cellPosition)
    {
        if (Input.GetMouseButtonDown(0) && isWithinBounds(cellPosition)) // Left mouse button
        {
            if (isWithinBounds(cellPosition))
            {
                // Place the ground
                roadTilemap.SetTile(cellPosition, null); // Clear the road tile
                groundTilemap.SetTile(cellPosition, groundTile);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            isBuilding = false;
        }
    }
    void HandleFarmPlacement(Vector3Int position)
    {
        if (Input.GetMouseButtonDown(0) && isWithinBounds(position) && !isCollidingWithOtherBuilding(position) && marchGameVariables.wood >= 20) // Left mouse button
        {
            if (isWithinBounds(position))
            {
                // Place the farm
                
                Vector3 snappedWorldPosition = groundTilemap.CellToWorld(position);
                Vector3Int tilePos = groundTilemap.WorldToCell(snappedWorldPosition);
                Vector3Int tilePosXNeighbor = new Vector3Int(tilePos.x + 1, tilePos.y, 0);
                Vector3Int tilePosYNeighbor = new Vector3Int(tilePos.x, tilePos.y + 1, 0);
                Vector3Int tilePosXNegNeighbor = new Vector3Int(tilePos.x - 1, tilePos.y, 0);
                Vector3Int tilePosYNegNeighbor = new Vector3Int(tilePos.x, tilePos.y - 1, 0);

                if(roadTilemap.HasTile(tilePos) || roadTilemap.HasTile(tilePosXNeighbor) || roadTilemap.HasTile(tilePosYNeighbor) || roadTilemap.HasTile(tilePosXNegNeighbor) || roadTilemap.HasTile(tilePosYNegNeighbor))
                {
                    return;
                }
                decoTileMap.SetTile(tilePos, null);
                decoTileMap.SetTile(tilePosXNeighbor, null);
                decoTileMap.SetTile(tilePosYNeighbor, null);
                decoTileMap.SetTile(tilePosXNegNeighbor, null);
                decoTileMap.SetTile(tilePosYNegNeighbor, null);
                Instantiate(farm, snappedWorldPosition, Quaternion.identity);
                marchGameVariables.wood -= 20;
                audioSource.PlayOneShot(farmPlacementSound);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            isBuilding = false;
        }
    }
    public void PlaceDam(Vector3 position)
    {
        Debug.Log("Placing dam at " + position);
        Vector3Int snappedWorldPosition = groundTilemap.WorldToCell(position);
        if (true)
        {
            Debug.Log("Placingggg dam at " + snappedWorldPosition);
            // Place the dam
            groundTilemap.SetTile(snappedWorldPosition, null); //
            //  Clear the ground tile
            damTilemap.SetTile(snappedWorldPosition, groundTile); 
            navMeshSurface.BuildNavMesh();
        }
    }
    void HandleDamPlacement(Vector3Int position)
    {
        Vector3Int collidingPosition = new Vector3Int(position.x, position.y, position.z);
        if (Input.GetMouseButtonDown(0) && isWithinBounds(position) && !isCollidingWithOtherBuilding(collidingPosition) && !waterTilemap.HasTile(position) && marchGameVariables.wood >=50) // Left mouse button
        {
            if (isWithinBounds(position))
            {

                Vector3 snappedWorldPosition = groundTilemap.CellToWorld(position);
                Debug.Log("Placing dam at " + snappedWorldPosition);
                Vector3Int tilePos = groundTilemap.WorldToCell(snappedWorldPosition);
                decoTileMap.SetTile(tilePos, null);
                roadTilemap.SetTile(tilePos, null);
                Instantiate(dam, snappedWorldPosition, Quaternion.identity);
                marchGameVariables.wood -= 50;
                audioSource.PlayOneShot(damHousePlacementSound);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
            isBuilding = false;
        }
    }
    
    void HandleTileType(Vector3Int cellPosition, TileType tileType, Tilemap tilemap)
    {
        if (Input.GetMouseButtonDown(0) && isWithinBounds(cellPosition)) // Left mouse button
        {
            if(tileType == TileType.Road && marchGameVariables.wood >= 5)
            {
                tilemap.SetTile(cellPosition, null);
                groundTilemap.SetTile(cellPosition, null);
                decoTileMap.SetTile(cellPosition, null);
                PlaceRoadTile(cellPosition, tilemap); 
                marchGameVariables.wood -= 5;
                audioSource.PlayOneShot(roadPlacementSound);
            }
            else if(tileType == TileType.Building && marchGameVariables.wood >= 20 && marchGameVariables.food >= 10)
            {
                // Check if we have enough space to place all 4 building tiles
                if (isWithinBounds(cellPosition) && 
                    isWithinBounds(new Vector3Int(cellPosition.x + 1, cellPosition.y, 0)) &&
                    isWithinBounds(new Vector3Int(cellPosition.x, cellPosition.y + 1, 0)) &&
                    isWithinBounds(new Vector3Int(cellPosition.x + 1, cellPosition.y + 1, 0))
                    && !isCollidingWithOtherBuilding(cellPosition) && !isCollidingWithOtherBuilding(new Vector3Int(cellPosition.x + 1, cellPosition.y, 0)) && !isCollidingWithOtherBuilding(new Vector3Int(cellPosition.x, cellPosition.y + 1, 0)) && !isCollidingWithOtherBuilding(new Vector3Int(cellPosition.x + 1, cellPosition.y + 1, 0)))
                {
                    // Clear any existing tiles in the building area
                    for (int xOffset = 0; xOffset <= 1; xOffset++)
                    {
                        for (int yOffset = 0; yOffset <= 1; yOffset++)
                        {
                            Vector3Int tilePos = new Vector3Int(cellPosition.x + xOffset, cellPosition.y + yOffset, 0);
                            roadTilemap.SetTile(tilePos, null);
                            groundTilemap.SetTile(tilePos, null);
                            decoTileMap.SetTile(tilePos, null);
                            buildingTilemap.SetTile(tilePos, null);
                        }
                    }
                    marchGameVariables.wood -= 20;
                    marchGameVariables.food -= 10;
                    // Place the 4 building tiles in a 2x2 formation
                    audioSource.PlayOneShot(damHousePlacementSound);
                    PlaceBuilding(cellPosition);
                }
            }
        }

        if (Input.GetMouseButtonDown(1) ) // Right mouse button
        {
            if(previewObject!=null)
            {
                previewObject.SetActive(false);
            }
            isBuilding = false;
        }
    }

    bool isWithinBounds(Vector3Int cellPosition)
    {
        return cellPosition.x >= minX && cellPosition.x <= maxX && cellPosition.y >= minY && cellPosition.y <= maxY;
    }

    void PlaceRoadTile(Vector3Int cellPosition, Tilemap tilemap)
    {
        tilemap.SetTile(cellPosition, roadTile);
    }
    bool isCollidingWithOtherBuilding(Vector3Int cellPosition)
    {
        //cast a box around the building to check for collisions
        Vector3 overlapPosistion = groundTilemap.CellToWorld(cellPosition);
        overlapPosistion.y += 1f;
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(overlapPosistion, new Vector2(1, 1), 0);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Building")|| hitCollider.CompareTag("FieldFood")|| hitCollider.CompareTag("Tree"))
            {
                Vector3 worldPosition = groundTilemap.CellToWorld(cellPosition);
                Debug.Log("Colliding with other building" + hitCollider.name + " at " + hitCollider.transform.position + " and " + overlapPosistion);

                return true;
            }
        }
        return false;
    }
    
    void PlaceBuilding(Vector3Int cellPosition)
    {
        Vector3 snappedWorldPosition = groundTilemap.CellToWorld(cellPosition);
        Instantiate(buildingObject, snappedWorldPosition, Quaternion.identity);
        // Place the 4 building tiles in a 2x2 grid formation
        groundTilemap.SetTile(cellPosition, null); // Clear the ground tile
        groundTilemap.SetTile(new Vector3Int(cellPosition.x + 1, cellPosition.y, 0), null); // Clear the ground tile
        groundTilemap.SetTile(new Vector3Int(cellPosition.x, cellPosition.y + 1, 0), null); // Clear the ground tile
        groundTilemap.SetTile(new Vector3Int(cellPosition.x + 1, cellPosition.y + 1, 0), null); // Clear the ground tile

        buildingTilemap.SetTile(cellPosition, buildingTile1); // Bottom-left
        buildingTilemap.SetTile(new Vector3Int(cellPosition.x + 1, cellPosition.y, 0), buildingTile2); // Bottom-right
        buildingTilemap.SetTile(new Vector3Int(cellPosition.x, cellPosition.y + 1, 0), buildingTile3); // Top-left
        buildingTilemap.SetTile(new Vector3Int(cellPosition.x + 1, cellPosition.y + 1, 0), buildingTile4); // Top-right
    }
}