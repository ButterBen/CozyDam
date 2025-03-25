using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class DayNightHandler : MonoBehaviour
{
    [SerializeField] private float dayDuration = 60f;
    [SerializeField] private float nightDuration = 30f;
    [SerializeField] private Light2D sun;
    [SerializeField] private Light2D moon;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;

    [SerializeField] Color daystartEndColor;
    [SerializeField] Color dayMidColor;
    [SerializeField] Color nightStartEndColor;
    [SerializeField] Color nightMidColor;

    [SerializeField] Light2D globalLight;

    public MarchGameVariables marchGameVariables;
    private bool isNewDay = false;
    
    private float maxSunIntensity;
    private float maxMoonIntensity;
    public WaterHandler waterHandler;
    [SerializeField] private List<GameObject> insectsDay;
    [SerializeField] private List<GameObject> insectsNight;
    [SerializeField] private Tilemap groundTiles;
    [SerializeField] private Tilemap waterTiles;
    [SerializeField] private int insectSpawnAmmount;
    private List<GameObject> spawnedInsects = new List<GameObject>();
    [SerializeField] private UnityEngine.UI.Image sunIcon;
    [SerializeField] private UnityEngine.UI.Image moonIcon;
    public UnityEvent nightEvent;
    public UnityEvent dayEvent;
    public bool isNight = false;
    public GameObject newDayPanel;
    public GameObject newDayText;
    public AudioClip dayMusic;
    public AudioClip nightMusic;
    public AudioSource audioSource;
    public float fadeDuration = 2f;


    void Start()
    {
        marchGameVariables.refreshables.Clear();
        maxSunIntensity = sun.intensity;
        maxMoonIntensity = moon.intensity;
        SpawnRandomInsects(true);
        marchGameVariables.dayCount = 0;
        showNewDay();
        // Reset intensities to 0 at start
        sun.intensity = 0f;
        moon.intensity = 0f;
        
        StartCoroutine(UpdateDayNightCycle());
    }
    void Update()
    {
        if(isNewDay)
        {
            UpdateResources();
            SpawnRandomInsects(true);
            isNewDay = false;
        }
    }
    
    IEnumerator UpdateDayNightCycle()
    {
        float currentTime = 0f;
        bool isDay = true;
        float timeStep = 1f / dayDuration;

        while (true)
        {
            currentTime += Time.deltaTime;
            float halfDuration = isDay ? dayDuration / 2f : nightDuration / 2f;
            float intensityFactor;

            if (isDay)
            {
                moon.gameObject.SetActive(false);
                sun.gameObject.SetActive(true);
                // Day cycle
                if (currentTime <= halfDuration)
                {
                    intensityFactor = currentTime / halfDuration;
                    sun.intensity = maxSunIntensity * intensityFactor * (2 - intensityFactor);
                    globalLight.color = Color.Lerp(daystartEndColor, dayMidColor, intensityFactor);
                }
                else
                {
                    intensityFactor = (currentTime - halfDuration) / halfDuration;
                    sun.intensity = maxSunIntensity * (1 - intensityFactor) * (1 + intensityFactor);
                    globalLight.color = Color.Lerp(dayMidColor, daystartEndColor, intensityFactor);
                }
            }
            else
            {
                moon.gameObject.SetActive(true);
                sun.gameObject.SetActive(false);
                // Night cycle
                if (currentTime <= halfDuration)
                {
                    
                    intensityFactor = currentTime / halfDuration;
                    moon.intensity = maxMoonIntensity * intensityFactor * (2 - intensityFactor);
                    globalLight.color = Color.Lerp(nightStartEndColor, nightMidColor, intensityFactor);
                }
                else
                {
                    intensityFactor = (currentTime - halfDuration) / halfDuration;
                    moon.intensity = maxMoonIntensity * (1 - intensityFactor) * (1 + intensityFactor);
                    globalLight.color = Color.Lerp(nightMidColor, nightStartEndColor, intensityFactor);
                }
            }
             SunMoonIconRotator(currentTime, isDay);
            // Check for day/night transition
            if (currentTime >= (isDay ? dayDuration : nightDuration))
            {
                currentTime = 0f;
                
                // Check if we're transitioning from night to day
                if (!isDay)
                {
                    showNewDay();
                    isNight = false;
                    waterHandler.StopFlood();
                    isNewDay = true; // Flag that a new day has started
                    Debug.Log("New day flag set");
                    SwitchBackGroundMusic();
                    nightEvent.Invoke();
                }
                else
                {
                    isNight = true;
                    dayEvent.Invoke();
                    SpawnRandomInsects(false);
                    waterHandler.StartFlood();
                    SwitchBackGroundMusic();
                    Debug.Log("New night flag set");
                }
                
                isDay = !isDay;
                sun.intensity = 0f;
                moon.intensity = 0f;
                timeStep = 1f / (isDay ? dayDuration : nightDuration);
            }
            
            // Update time text
            timeText.text = isDay ? "Day" : "Night";
            yield return null;
        }
    }
    void showNewDay()
    {
        if(waterHandler.floodInterval > 0.31f)
        {
            waterHandler.floodInterval -= 0.3f;
        }
        marchGameVariables.dayCount++;
        newDayText.GetComponent<TMPro.TextMeshProUGUI>().text = "Day " + marchGameVariables.dayCount;
        newDayPanel.SetActive(true);
        StartCoroutine(HideNewDay());
    }
    IEnumerator HideNewDay()
    {
        yield return new WaitForSecondsRealtime(3.6f);
        newDayPanel.SetActive(false);
    }
    void SunMoonIconRotator(float currentTime, bool isDay)
    {
        float halfDuration = isDay ? dayDuration / 2f : nightDuration / 2f;
        float cycleDuration = isDay ? dayDuration : nightDuration;
        float normalizedTime = currentTime / cycleDuration; // 0 to 1 across entire cycle

        if (isDay)
        {
            // Day cycle
            if (currentTime <= halfDuration)
            {
                // First half of day (morning): Sun filling clockwise from 0.5 to 1
                sunIcon.fillClockwise = true;
                sunIcon.fillAmount = Mathf.Lerp(0.5f, 1f, currentTime / halfDuration);
                
                // Moon reducing counter-clockwise from 0.5 to 0
                moonIcon.fillClockwise = false;
                moonIcon.fillAmount = Mathf.Lerp(0.5f, 0f, currentTime / halfDuration);
            }
            else
            {
                // Second half of day (afternoon): Sun reducing counter-clockwise from 1 to 0.5
                sunIcon.fillClockwise = false;
                sunIcon.fillAmount = Mathf.Lerp(1f, 0.5f, (currentTime - halfDuration) / halfDuration);
                
                // Moon stays at 0
                moonIcon.fillClockwise = true;
                moonIcon.fillAmount = Mathf.Lerp(0f, 0.5f, (currentTime - halfDuration) / halfDuration);
            }
        }
        else
        {
            // Night cycle
            if (currentTime <= halfDuration)
            {
                // First half of night: Moon filling clockwise from 0.5 to 1
                moonIcon.fillClockwise = true;
                moonIcon.fillAmount = Mathf.Lerp(0.5f, 1f, currentTime / halfDuration);
                
                // Sun reducing counter-clockwise from 0.5 to 0
                sunIcon.fillClockwise = false;
                sunIcon.fillAmount = Mathf.Lerp(0.5f, 0f, currentTime / halfDuration);
            }
            else
            {
                // Second half of night: Moon reducing counter-clockwise from 1 to 0.5
                moonIcon.fillClockwise = false;
                moonIcon.fillAmount = Mathf.Lerp(1f, 0.5f, (currentTime - halfDuration) / halfDuration);
                
                // Sun filling clockwise from 0 to 0.5
                sunIcon.fillClockwise = true;
                sunIcon.fillAmount = Mathf.Lerp(0f, 0.5f, (currentTime - halfDuration) / halfDuration);
            }
        }
    }

    void UpdateResources()
    {
        Debug.Log("Updating resources");

        // Create a separate list to avoid modifying the dictionary while iterating
        List<Durability> keys = new List<Durability>(marchGameVariables.refreshables.Keys);

        foreach (Durability durability in keys)
        {
            float random = Random.Range(0f, 1f);
            if (random <= marchGameVariables.refreshables[durability])
            {
                Debug.Log("Random value: " + random + " is less than or equal to " + marchGameVariables.refreshables[durability]);
                durability.RefreshResource(Random.Range(5, 20));
                marchGameVariables.refreshables.Remove(durability);
            }
        }
        List<GameObject> keys2 = new List<GameObject>(marchGameVariables.plantedTrees.Keys);
        foreach(GameObject tree in keys2)
        {
            float randomGrowChance = Random.Range(0f, .6f);
            if(randomGrowChance <= marchGameVariables.plantedTrees[tree])
            {
                tree.GetComponent<Durability>().Grow();
                marchGameVariables.plantedTrees.Remove(tree);
            }
        }
    }

    void SpawnRandomInsects(bool isDay)
    {
        foreach(GameObject insect in spawnedInsects)
        {
            Destroy(insect);
        }
        spawnedInsects.Clear();

        float landTilePercentage = 1f - ((float)marchGameVariables.waterTilesCount / 755f);
        int adjustedSpawnAmount = Mathf.RoundToInt(insectSpawnAmmount * landTilePercentage);

        for(float i = 0; i < adjustedSpawnAmount; i++)
        {
            Vector3Int randomTile = GetRandomGroundTile();
            GameObject insect = Instantiate(isDay ? insectsDay[Random.Range(0, insectsDay.Count)] : insectsNight[Random.Range(0, insectsNight.Count)], groundTiles.CellToWorld(randomTile), Quaternion.identity);
            insect.transform.position = new Vector3(insect.transform.position.x, insect.transform.position.y, 0);
            spawnedInsects.Add(insect);
        }
    }

    Vector3Int GetRandomGroundTile()
    {
        int minX = marchGameVariables.minX + 1;
        int maxX = marchGameVariables.maxX - 1;
        int minY = marchGameVariables.minY + 1;
        int maxY = marchGameVariables.maxY - 1;

        Vector3Int randomTile = new Vector3Int(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);

        while (!groundTiles.HasTile(randomTile) || waterTiles.HasTile(randomTile))
        {
            randomTile = new Vector3Int(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
        }

        return randomTile;
    }
    public void SwitchBackGroundMusic()
    {
        StartCoroutine(FadeMusic());
    }

    private IEnumerator FadeMusic()
    {
        float startVolume = audioSource.volume;

        // Fade Out
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.unscaledTime / fadeDuration;
            yield return null;
        }

        // Change the music track
        audioSource.clip = isNight ? nightMusic : dayMusic;
        audioSource.Play();

        // Fade In
        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.unscaledTime / fadeDuration;
            yield return null;
        }

        audioSource.volume = startVolume; // Ensure volume is set back to original
    }
}
