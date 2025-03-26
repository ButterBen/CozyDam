using UnityEngine;
using NavMeshPlus.Components;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public TilemapPlacer tilemapPlacer;
    public GameObject buildOptions;
    public MarchGameVariables marchGameVariables;
    public Texture2D defaultCursor;
    public Image waterCoveredImage;
    public UnitSelection unitSelection;
    [Header("Preview Objects")]
    public GameObject closeBuildMenuButton;
    public GameObject openBuildMenuButton;
    public NavMeshSurface navMeshSurface;
    public GameObject roadPreviewObject;
    public GameObject damPreviewObject;
    public GameObject buildingPreviewObject;
    public GameObject unitPreviewObject;
    public GameObject farmPreviewObject;
    public GameObject TreePreviewObject;
    public GameObject groundPreviewObject;
    private AudioSource audioSource;
    [Header("Audio")]
    public AudioClip buildButtonSound;
    public AudioClip selectButtonSound;
    public AudioClip CancelButtonSound;
    [Header("MenuItems")]
    public GameObject menuItems;
    public GameObject settingsMenu;
    public CameraNavigation CameraNavigation;
    private bool settingsOpen = false;
    public bool menuScene = false;
    public GameObject loseScreen;
    public TMPro.TextMeshProUGUI loseText;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        marchGameVariables.waterTilesCount = 0;
        marchGameVariables.currentUnits = 0;
        marchGameVariables.possibleUnits = 3;
        marchGameVariables.food = 0;
        marchGameVariables.wood = 0;
    }
    void Update()
    {
        float waterPercentage = (float)marchGameVariables.waterTilesCount / 1258f; // Ensure floating-point division
        waterPercentage = waterPercentage * 100;

        // Normalize fill amount (0% = 0, 60% = 1)
        float fillAmount = Mathf.Clamp01(waterPercentage / 100f);
        waterCoveredImage.fillAmount = fillAmount;
        
        // Interpolate color from white to red based on percentage (0% = white, 60% = red)
        waterCoveredImage.color = Color.Lerp(Color.white, Color.red, fillAmount);
        if(waterCoveredImage.fillAmount >= .99f)
        {
            loseScreen.SetActive(true);
            loseText.text = "You survived " + marchGameVariables.dayCount + " days!";
            Time.timeScale = 0;
            unitSelection.inMenu = true;
            CameraNavigation.menuOpen = true;
        }
        if(Keyboard.current.escapeKey.wasPressedThisFrame && !menuScene)
        {
            if(buildOptions.activeSelf)
            {
                CloseBuildMenu();
            }
            else
            {
                if(settingsOpen)
                {
                    CloseSettingsMenu();
                }
                else
                {
                    OpenSettingsMenu();
                }
            }
        }
        if(menuScene)
        {
            return;
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2;
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 3;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = 0;
        }
    }


    public void OpenBuildMenu()
    {
        unitSelection.isBuildingUnit = true;
        closeBuildMenuButton.SetActive(true);
        openBuildMenuButton.SetActive(false);
        buildOptions.SetActive(true);
        audioSource.PlayOneShot(buildButtonSound);
        Time.timeScale = 0;
        //set cursor to default
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);

    }
    public void CloseBuildMenu()
    {
        tilemapPlacer.isBuilding = false;
        unitSelection.isBuildingUnit = false;
        closeBuildMenuButton.SetActive(false);
        openBuildMenuButton.SetActive(true);
        roadPreviewObject.SetActive(false);
        damPreviewObject.SetActive(false);
        buildOptions.SetActive(false);
        navMeshSurface.BuildNavMeshAsync();
        farmPreviewObject.SetActive(false);
        audioSource.PlayOneShot(buildButtonSound);
        Time.timeScale = 1;
    }

    public void BuildRoad()
    {
        if(marchGameVariables.wood < 5)
        {
            audioSource.PlayOneShot(CancelButtonSound);
            Debug.Log("Not enough wood");
            return;
        }
        audioSource.PlayOneShot(selectButtonSound);
        tilemapPlacer.isBuilding = true;
        roadPreviewObject.SetActive(true);
        tilemapPlacer.previewObject = roadPreviewObject;
        tilemapPlacer.tileType = TilemapPlacer.TileType.Road;
    }
    public void BuildDam()
    {
        if(marchGameVariables.wood < 50)
        {
            audioSource.PlayOneShot(CancelButtonSound);
            Debug.Log("Not enough wood");
            return;
        }
        else
        {
            audioSource.PlayOneShot(selectButtonSound);
            tilemapPlacer.isBuilding = true;
            damPreviewObject.SetActive(true);
            tilemapPlacer.previewObject = damPreviewObject;
            tilemapPlacer.tileType = TilemapPlacer.TileType.Dam;
        }

    }

    public void BuildBuidling()
    {
        if(marchGameVariables.wood < 20 || marchGameVariables.food < 10)
        {
            audioSource.PlayOneShot(CancelButtonSound);
            Debug.Log("Not enough wood");
            return;
        }
        else
        {
            audioSource.PlayOneShot(selectButtonSound);
            tilemapPlacer.isBuilding = true;
            buildingPreviewObject.SetActive(true);
            tilemapPlacer.previewObject = buildingPreviewObject;
            tilemapPlacer.tileType = TilemapPlacer.TileType.Building;
        }
    }
    public void BuildUnit()
    {
        if(marchGameVariables.food < 20 || marchGameVariables.currentUnits >= marchGameVariables.possibleUnits)
        {
            audioSource.PlayOneShot(CancelButtonSound);
            Debug.Log("Not enough food");
            return;
        }
        else if(marchGameVariables.food >= 0 && marchGameVariables.currentUnits < marchGameVariables.possibleUnits)
        {
            audioSource.PlayOneShot(selectButtonSound);
            tilemapPlacer.isBuilding = true;
            unitPreviewObject.SetActive(true);
            tilemapPlacer.previewObject = unitPreviewObject;
            tilemapPlacer.tileType = TilemapPlacer.TileType.Unit;
        }
    }
    public void BuildFarm()
    {
        if(marchGameVariables.wood < 20)
        {
            audioSource.PlayOneShot(CancelButtonSound);
            Debug.Log("Not enough wood");
            return;
        }
        else
        {
            audioSource.PlayOneShot(selectButtonSound);
            tilemapPlacer.isBuilding = true;
            farmPreviewObject.SetActive(true);
            tilemapPlacer.previewObject = farmPreviewObject;
            tilemapPlacer.tileType = TilemapPlacer.TileType.Farm;
        }
    }
    public void BuildGround()
    {
        audioSource.PlayOneShot(selectButtonSound);
        tilemapPlacer.isBuilding = true;
        groundPreviewObject.SetActive(true);
        tilemapPlacer.previewObject = groundPreviewObject;
        tilemapPlacer.tileType = TilemapPlacer.TileType.Ground;
    }
    public void BuildTree()
    {
        audioSource.PlayOneShot(selectButtonSound);
        tilemapPlacer.isBuilding = true;
        TreePreviewObject.SetActive(true);
        tilemapPlacer.previewObject = TreePreviewObject;
        tilemapPlacer.tileType = TilemapPlacer.TileType.Tree;
    }
    public void ExitGame()
    {
        audioSource.PlayOneShot(selectButtonSound);
        Application.Quit();
    }
    public void StartGame()
    {
        audioSource.PlayOneShot(selectButtonSound);
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    public void OpenSettingsMenu()
    {
        unitSelection.inMenu = true;
        closeBuildMenuButton.SetActive(false);
        openBuildMenuButton.SetActive(false);
        buildOptions.SetActive(false);
        CameraNavigation.menuOpen = true;
        audioSource.PlayOneShot(selectButtonSound);
        menuItems.SetActive(true);
        settingsMenu.SetActive(false);
        settingsOpen = true;
        Time.timeScale = 0;
        CameraNavigation.CenterCamera();
    }
    public void CloseSettingsMenu()
    {
        unitSelection.inMenu = false;
        CameraNavigation.menuOpen = false;
        settingsOpen = false;
        openBuildMenuButton.SetActive(true);
        closeBuildMenuButton.SetActive(false);
        buildOptions.SetActive(false);
        menuItems.SetActive(false);
        settingsMenu.SetActive(false);
        audioSource.PlayOneShot(selectButtonSound);
        Time.timeScale = 1;
    }
    public void OpenAudioSettings()
    {
        audioSource.PlayOneShot(selectButtonSound);
        menuItems.SetActive(false);
        settingsMenu.SetActive(true);  
    }
    public void CloseAudioSettings()
    {
        audioSource.PlayOneShot(selectButtonSound);
        menuItems.SetActive(true);
        settingsMenu.SetActive(false);
        CameraNavigation.UpdatePanSpeed();
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1;
    }
}
