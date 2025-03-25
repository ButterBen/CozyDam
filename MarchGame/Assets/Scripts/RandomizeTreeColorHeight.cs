using System.Collections.Generic;
using UnityEngine;

public class RandomizeTreeColorHeight : MonoBehaviour
{
    [SerializeField] float minTreeHeight = 0.2f;
    [SerializeField] float maxTreeHeight = 0.4f;
    [SerializeField] Color[] treeColors;
    [SerializeField] List<SpriteRenderer> treeSprites;
    [SerializeField] Transform treeParent;
    public Durability durability;

    void Start()
    {
        Color randomColor = treeColors[Random.Range(0, treeColors.Length)];
        float randomScale = Random.Range(minTreeHeight, maxTreeHeight);
        treeParent.localScale = new Vector3(randomScale, randomScale, randomScale);
        if (durability != null)
        {
            durability.maxDurability = (int)(randomScale * 100);
        }
        foreach (SpriteRenderer treeSprite in treeSprites)
        {
           treeSprite.color = randomColor;
        }

    }
}
