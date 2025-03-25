using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NavMeshPlus.Components;
public class WaterHandler : MonoBehaviour
{
    public MarchGameVariables marchGameVariables;
    public Tilemap waterTilemap;
    public Tilemap waterAniTilemap;
    public TileBase waterTile;
    public List<TileBase> waterAniTiles;
    public List<Tilemap> environmentTilemaps;
    public Tilemap DamTileMap;
    public NavMeshSurface navMeshSurface;
    
    private List<Vector3Int> waterTiles = new List<Vector3Int>();
    public float floodInterval = 1.6f; 
    private int maxWaterTiles = 800;
    private bool floodStarted = false;
    public AudioClip waterSound;

    private int minX;
    private int maxX;
    private int minY;
    private int maxY;
    public GameObject navMeshObstaclePrefab; 
    private Dictionary<Vector3Int, GameObject> waterObstacles = new Dictionary<Vector3Int, GameObject>();
    
    // Audio-related variables
    private GameObject firstWaterAudioSource;
    private GameObject lastWaterAudioSource;
    private Vector3Int lastWaterAudioSourcePosition;
    
    void Start()
    {
        UpdateBounds();
        PlaceBorder();
        //StartFlood();
    }
    
    public void UpdateBounds()
    {
        minX = marchGameVariables.minX;
        maxX = marchGameVariables.maxX;
        minY = marchGameVariables.minY;
        maxY = marchGameVariables.maxY;
    }
    
    void PlaceBorder()
    {
        int thickness = 20; // Border thickness

        for (int x = minX - thickness; x <= maxX + thickness; x++)
        {
            for (int y = minY - thickness; y <= maxY + thickness; y++)
            {
                // Check if the current tile is within the border thickness
                bool isBorderTile = 
                    (x >= minX - thickness && x < minX) || // Left border
                    (x > maxX && x <= maxX + thickness) || // Right border
                    (y >= minY - thickness && y < minY) || // Bottom border
                    (y > maxY && y <= maxY + thickness);   // Top border

                if (isBorderTile)
                {
                    waterTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                    int random = Random.Range(0, 2);
                    if(random == 1)
                    {
                        Vector3Int tilePosition = new Vector3Int(x, y, 0);
                        TileBase selectedTile = waterAniTiles[Random.Range(0, waterAniTiles.Count)];
                        waterAniTilemap.SetTile(tilePosition, selectedTile);

                        // 50% chance to flip the tile horizontally
                        if (Random.Range(0, 2) == 0) 
                        {
                            Matrix4x4 flipMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
                            waterAniTilemap.SetTransformMatrix(tilePosition, flipMatrix);
                        }

                        waterAniTilemap.SetTile(new Vector3Int(x, y, 0), waterAniTiles[Random.Range(0, waterAniTiles.Count)]);
                    }
                }
            }
        }

        // Place extra inner corner tiles
        waterTilemap.SetTile(new Vector3Int(minX, minY, 0), waterTile); // Bottom-left
        waterTilemap.SetTile(new Vector3Int(minX , maxY, 0), waterTile); // Top-left
        waterTilemap.SetTile(new Vector3Int(maxX , minY , 0), waterTile); // Bottom-right
        waterTilemap.SetTile(new Vector3Int(maxX , maxY , 0), waterTile); // Top-right
    }
    
    public void StartFlood()
    {
        if (floodStarted)
            return;
            
        floodStarted = true;
        
        // Start with a random border tile
        Vector3Int initialTile = GetRandomBorderTile();
        StartCoroutine(PlaceWaterTileAsync(initialTile));
        
        // Create the first water audio source
        CreateWaterAudioSource(initialTile, true);
        if(lastWaterAudioSource != null)
        {
            Destroy(lastWaterAudioSource);
        }
        
        StartCoroutine(FloodRoutine());
    }
    
    private Vector3Int GetRandomBorderTile()
    {
        List<Vector3Int> borderPositions = new List<Vector3Int>();
        
        // Top and bottom borders
        for (int x = minX; x <= maxX; x++)
        {
            borderPositions.Add(new Vector3Int(x, minY, 0));
            borderPositions.Add(new Vector3Int(x, maxY, 0));
        }
        
        // Left and right borders (excluding corners which are already added)
        for (int y = minY + 1; y < maxY; y++)
        {
            borderPositions.Add(new Vector3Int(minX, y, 0));
            borderPositions.Add(new Vector3Int(maxX, y, 0));
        }
        
        // Return a random position from the border
        return borderPositions[Random.Range(0, borderPositions.Count)];
    }
    
    private IEnumerator PlaceWaterTileAsync(Vector3Int position)
    {
        CheckForResources(position);

        // Remove existing tiles
        foreach (Tilemap tilemap in environmentTilemaps)
        {
            if (tilemap.HasTile(position))
            {
                tilemap.SetTile(position, null);
            }
        }

        // Place water tile
        waterTilemap.SetTile(position, waterTile);
        int random = Random.Range(0, 2);
        if(random == 1)
        {
            waterAniTilemap.SetTile(position, waterAniTiles[Random.Range(0, waterAniTiles.Count)]);
        }

        // Add to list of water tiles
        if (!waterTiles.Contains(position))
        {
            waterTiles.Add(position);

            Vector3 worldPos = waterTilemap.CellToWorld(position);
            worldPos.y += 0.5f;

            GameObject obstacle = Instantiate(navMeshObstaclePrefab, worldPos, Quaternion.identity);
            obstacle.transform.parent = transform; // Parent to WaterHandler for organization

            // Store reference to the obstacle
            waterObstacles[position] = obstacle;

            // Yield to spread workload across frames
            yield return null;
        }
    }

    // Call this method instead of PlaceWaterTile
    public void StartPlaceWaterTile(Vector3Int position)
    {
        StartCoroutine(PlaceWaterTileAsync(position));
    }

    void CheckForResources(Vector3Int position)
    {
        Vector2 worldPosition = waterTilemap.CellToWorld(position);
        worldPosition.y += 1f;
        worldPosition.x += 1f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPosition, 1f, LayerMask.GetMask("UI")); 

        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider.CompareTag("Tree") || hitCollider.CompareTag("FieldFood") || hitCollider.CompareTag("Building"))
            {
                hitCollider.gameObject.GetComponent<Durability>()?.DestroyResource();
            }
            if(hitCollider.CompareTag("DamBuilding"))
            {
                hitCollider.gameObject.GetComponentInChildren<Durability>().DestroyResource();
            }
        }
    }
    
    private bool CanFloodToPosition(Vector3Int position)
    {
        if (waterTilemap.HasTile(position))
            return false;
            
        if (DamTileMap.HasTile(position))
            return false;
                   
        return position.x >= minX && position.x <= maxX && 
               position.y >= minY && position.y <= maxY;
    }
    
    private List<Vector3Int> GetAdjacentTiles(Vector3Int position)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),   // Right
            new Vector3Int(-1, 0, 0),  // Left
            new Vector3Int(0, 1, 0),   // Up
            new Vector3Int(0, -1, 0)   // Down
        };
        
        List<Vector3Int> adjacentTiles = new List<Vector3Int>();
        
        foreach (Vector3Int dir in directions)
        {
            Vector3Int newPos = position + dir;
            if (CanFloodToPosition(newPos))
            {
                adjacentTiles.Add(newPos);
            }
        }
        
        return adjacentTiles;
    }
    
    private IEnumerator FloodRoutine()
    {
        while (waterTiles.Count < maxWaterTiles)
        {
            yield return new WaitForSeconds(floodInterval);
            
            List<Vector3Int> currentWaterTiles = new List<Vector3Int>(waterTiles);
            
            // Find all possible expansion positions
            List<Vector3Int> expansionPositions = new List<Vector3Int>();
            
            foreach (Vector3Int waterPos in currentWaterTiles)
            {
                expansionPositions.AddRange(GetAdjacentTiles(waterPos));
            }
            
            if (expansionPositions.Count == 0)
                break;

            Vector3Int nextFloodPos = expansionPositions[Random.Range(0, expansionPositions.Count)];
            StartCoroutine(PlaceWaterTileAsync(nextFloodPos));
            lastWaterAudioSourcePosition = nextFloodPos;
            
            // Update the last water audio source position
            
            //Debug.Log($"Flooding to position: {nextFloodPos}, Water tile count: {waterTiles.Count}");
            marchGameVariables.waterTilesCount = waterTiles.Count;
        }
        
        // Once flooding is complete, destroy the first audio source
        if (firstWaterAudioSource != null)
        {
            // Fade out the first audio source
            StartCoroutine(FadeOutAndDestroy(firstWaterAudioSource.GetComponent<AudioSource>()));
        }
    }
    
    private void CreateWaterAudioSource(Vector3Int tilePosition, bool isFirstTile)
    {
        // Convert tile position to world position
        Vector3 worldPosition = waterTilemap.CellToWorld(tilePosition);
        worldPosition.y += 0.5f;
        
        // Create a new GameObject for the audio source
        GameObject audioSourceObj = new GameObject(isFirstTile ? "FirstWaterAudioSource" : "LastWaterAudioSource");
        audioSourceObj.transform.position = worldPosition;
        audioSourceObj.transform.parent = transform; // Parent to the WaterHandler
        
        // Add AudioSource component
        AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
        audioSource.clip = waterSound;
        audioSource.loop = true;
        audioSource.spatialBlend = 1.0f; // Full 3D sound
        audioSource.minDistance = .7f;
        audioSource.maxDistance = 9.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.Play();
        
        // Store reference to the audio source
        if (isFirstTile)
        {
            firstWaterAudioSource = audioSourceObj;
        }
        else
        {
            lastWaterAudioSource = audioSourceObj;
        }
    }
    
    private IEnumerator FadeOutAndDestroy(AudioSource audioSource)
    {
        float startVolume = audioSource.volume;
        float fadeTime = 2.0f;
        float elapsedTime = 0;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeTime);
            yield return null;
        }
        
        // Destroy the GameObject after fade out
        Destroy(audioSource.gameObject);
    }
    
    public void StopFlood()
    {
        StopAllCoroutines();
        floodStarted = false;
        CreateWaterAudioSource(lastWaterAudioSourcePosition, false);
        // Destroy audio sources if they exist
        if (firstWaterAudioSource != null)
        {
            Destroy(firstWaterAudioSource);
        }
    }
}