using System.Collections.Generic;
using UnityEngine;

public class RandomizeGroundSpots : MonoBehaviour
{
    public List<GameObject> groundSpots;
    public List<Sprite> groundSprites;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (GameObject groundSpot in groundSpots)
        {
            SpriteRenderer spriteRenderer = groundSpot.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = groundSprites[Random.Range(0, groundSprites.Count)];
            float randomSize = Random.Range(minSize, maxSize);
            groundSpot.transform.localScale = new Vector3(randomSize, randomSize, 1);
        }
    }

}
