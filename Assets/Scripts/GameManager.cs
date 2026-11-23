using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Prefabs")]
    public GameObject boltPrefab;
    public GameObject nutPrefab;

    [Header("Colors")]
    public Color[] colors;

    [Header("Panels")]
    public GameObject winPanel;

    [Header("Level Settings")]
    public int boltCount = 4;
    public int nutsPerBolt = 4;
    public int colorCount = 3;

    [Header("Bolt Settings")]
    public float boltSpacing = 2.0f;
    public float nutSpacing = 0.45f;

    [Header("UI")]
    public Text levelText;

    [Header("Level System")]
    public int currentLevel = 1;

    private List<Bolt> bolts = new List<Bolt>();
    private Nut selectedNut = null;
    private bool levelCompleted = false;

    private struct MoveData
    {
        public Nut nut;
        public Bolt fromBolt;
        public Bolt toBolt;
    }
    private List<MoveData> moveHistory = new List<MoveData>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        if (levelText != null)
            levelText.text = "Level " + currentLevel;

        SetDifficulty();
        GenerateLevel();
        AdjustCamera();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (selectedNut != null)
                {
                    Bolt clickedBolt = hit.collider.GetComponentInParent<Bolt>();
                    if (clickedBolt != null)
                    {
                        PlaceNut(clickedBolt);
                        return;
                    }
                }

                Nut clickedNut = hit.collider.GetComponentInParent<Nut>();
                if (clickedNut != null)
                {
                    SelectNut(clickedNut);
                    return;
                }
            }
        }
    }

    void SetDifficulty()
    {
        if (currentLevel == 1) { boltCount = 4; colorCount = 3; }
        else if (currentLevel == 2) { boltCount = 5; colorCount = 3; }
        else if (currentLevel == 3) { boltCount = 6; colorCount = 4; }
        else if (currentLevel == 4) { boltCount = 6; colorCount = 4; }
        else if (currentLevel == 5) { boltCount = 7; colorCount = 5; }
        else
        {
            boltCount = Mathf.Min(8, 4 + (currentLevel / 2));
            colorCount = Mathf.Min(6, 3 + (currentLevel / 3));
        }

        int maxColors = boltCount - 2;
        if (colorCount > maxColors) colorCount = maxColors;
        if (colorCount > colors.Length) colorCount = colors.Length;
        if (colorCount < 2) colorCount = 2;

        nutsPerBolt = 4;
    }

    void GenerateLevel()
    {
        if (colorCount > colors.Length) colorCount = colors.Length;

        List<string> nutColors = new List<string>();
        for (int c = 0; c < colorCount; c++)
            for (int n = 0; n < nutsPerBolt; n++)
                nutColors.Add("Color_" + c);

        for (int i = 0; i < nutColors.Count; i++)
        {
            string temp = nutColors[i];
            int rand = Random.Range(i, nutColors.Count);
            nutColors[i] = nutColors[rand];
            nutColors[rand] = temp;
        }

        int filledBolts = boltCount - 2;
        int maxNuts = filledBolts * nutsPerBolt;
        if (nutColors.Count > maxNuts)
            nutColors.RemoveRange(maxNuts, nutColors.Count - maxNuts);

        float startX = -(boltCount - 1) * boltSpacing / 2f;

        for (int b = 0; b < boltCount; b++)
        {
            GameObject boltObj = Instantiate(boltPrefab,
                new Vector3(startX + b * boltSpacing, 0, 0),
                Quaternion.identity);

            boltObj.name = "Bolt_" + b;
            Bolt bolt = boltObj.GetComponent<Bolt>();
            bolt.maxCapacity = nutsPerBolt;
            bolt.nutSpacing = nutSpacing;

            GameObject stackPointObj = new GameObject("StackPoint");
            stackPointObj.transform.SetParent(boltObj.transform);
            stackPointObj.transform.localPosition = new Vector3(0, -0.7f, 0);
            bolt.stackPoint = stackPointObj.transform;

            bolts.Add(bolt);

            if (b < filledBolts)
            {
                for (int n = 0; n < nutsPerBolt; n++)
                {
                    int idx = b * nutsPerBolt + n;
                    if (idx >= nutColors.Count) break;

                    GameObject nutObj = Instantiate(nutPrefab);
                    Nut nut = nutObj.GetComponent<Nut>();

                    int colorIdx = int.Parse(nutColors[idx].Split('_')[1]);
                    if (colorIdx < 0 || colorIdx >= colors.Length) colorIdx = 0;

                    nut.SetColor(nutColors[idx], colors[colorIdx]);
                    bolt.AddNut(nut);
                }
            }
        }
    }

    void AdjustCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        float totalWidth = (boltCount - 1) * boltSpacing + 3f;

        if (mainCam.orthographic)
        {
            float requiredSize = totalWidth / (2f * mainCam.aspect);
            if (requiredSize > mainCam.orthographicSize)
                mainCam.orthographicSize = requiredSize + 1f;
        }
        else
        {
            float distance = Mathf.Abs(mainCam.transform.position.z);
            float requiredFOV = 2f * Mathf.Atan(totalWidth / (2f * distance)) * Mathf.Rad2Deg;
            if (requiredFOV > mainCam.fieldOfView)
                mainCam.fieldOfView = requiredFOV + 5f;
        }
    }

    public void SelectNut(Nut nut)
    {
        if (levelCompleted) return;

        if (selectedNut != null)
        {
            if (selectedNut == nut)
            {
                selectedNut.SetSelected(false);
                selectedNut = null;
                return;
            }
            else
            {
                selectedNut.SetSelected(false);
            }
        }

        if (nut.currentBolt.GetTopNut() != nut) return;

        selectedNut = nut;
        selectedNut.SetSelected(true);

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySelectSound();
    }

    public void PlaceNut(Bolt targetBolt)
    {
        if (selectedNut == null) return;
        if (levelCompleted) return;

        Bolt sourceBolt = selectedNut.currentBolt;

        if (sourceBolt == targetBolt) return;

        int chainCount = sourceBolt.GetSameColorChainCount(selectedNut.nutColor);

        if (targetBolt.nuts.Count + chainCount > targetBolt.maxCapacity)
        {
            selectedNut.SetSelected(false);
            selectedNut = null;
            return;
        }

        if (targetBolt.nuts.Count > 0 &&
            targetBolt.nuts[targetBolt.nuts.Count - 1].nutColor != selectedNut.nutColor)
        {
            selectedNut.SetSelected(false);
            selectedNut = null;
            return;
        }

        List<Nut> nutsToMove = new List<Nut>();
        for (int i = 0; i < chainCount; i++)
        {
            Nut topNut = sourceBolt.GetTopNut();
            if (topNut == null) break;
            nutsToMove.Add(topNut);
            sourceBolt.RemoveNut(topNut);
        }

        // YAHAN SE 'Reverse' HATA DIYA HAI
        // Ab list mein Top wala nut sab se pehle hai (index 0)

        foreach (Nut n in nutsToMove)
        {
            targetBolt.AddNut(n);
        }

        MoveData move = new MoveData();
        move.nut = selectedNut;
        move.fromBolt = sourceBolt;
        move.toBolt = targetBolt;
        moveHistory.Add(move);

        selectedNut.SetSelected(false);
        selectedNut = null;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayMoveSound();

        StartCoroutine(CheckWinAfterDelay());
    }

    IEnumerator CheckWinAfterDelay()
    {
        yield return new WaitForSeconds(0.6f);
        CheckWin();
    }

    public void UndoMove()
    {
        if (levelCompleted) return;
        if (moveHistory.Count == 0) return;

        MoveData lastMove = moveHistory[moveHistory.Count - 1];
        moveHistory.RemoveAt(moveHistory.Count - 1);

        lastMove.toBolt.RemoveNut(lastMove.nut);
        lastMove.fromBolt.AddNut(lastMove.nut);

        if (selectedNut != null)
        {
            selectedNut.SetSelected(false);
            selectedNut = null;
        }
    }

    public void ShuffleNuts()
    {
        if (levelCompleted) return;

        List<Nut> allNuts = new List<Nut>();
        foreach (var bolt in bolts)
        {
            allNuts.AddRange(bolt.nuts);
            bolt.nuts.Clear();
        }

        for (int i = 0; i < allNuts.Count; i++)
        {
            Nut temp = allNuts[i];
            int rand = Random.Range(i, allNuts.Count);
            allNuts[i] = allNuts[rand];
            allNuts[rand] = temp;
        }

        int nutIndex = 0;
        int filledBolts = bolts.Count - 2;

        for (int b = 0; b < filledBolts; b++)
            for (int i = 0; i < nutsPerBolt && nutIndex < allNuts.Count; i++)
            {
                bolts[b].AddNut(allNuts[nutIndex]);
                nutIndex++;
            }

        if (selectedNut != null)
        {
            selectedNut.SetSelected(false);
            selectedNut = null;
        }

        moveHistory.Clear();
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void CheckWin()
    {
        foreach (var bolt in bolts)
        {
            if (bolt.nuts.Count == 0) continue;
            if (!bolt.IsComplete()) return;
        }

        levelCompleted = true;
        Debug.Log("🎉 Level Complete!");

        if (AudioManager.instance != null)
            AudioManager.instance.PlayWinSound();

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void NEXTBUTTON()
    {
        WinPanel.instance.OnNextLevel();
    }
}