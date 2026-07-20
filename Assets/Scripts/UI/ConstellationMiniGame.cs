using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConstellationMiniGame : UIBase
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI[] rowTextTemplates; 
    [SerializeField] private RectTransform highlightBar;       
    [SerializeField] private Slider timerSlider;                

    [Header("Game Settings")]
    [SerializeField] private float limitTime = 15f;             

    private int totalRows = 6;
    private int colsPerRow = 10;

    private string[] gridData;        
    private List<int>[] redIndicesPerLine; 

    private int currentRow = 0;         
    private int currentCorrectCount = 0; 
    private float currentTime;
    private bool isGameActive = true;

    private ActiveTaskType _taskType = ActiveTaskType.RouteControl;

    private void Update()
    {
        GameStart();
    }

    private void OnEnable()
    {
        InitGame();
    }

    private void GameStart()
    {
        if (!isGameActive)
        { 
           return;
        }

        currentTime -= Time.deltaTime;
        timerSlider.value = currentTime;
        if (currentTime <= 0)
        {
            GameOver(false);
        }

        if (Input.anyKeyDown)
        {
            string inputStr = Input.inputString.ToUpper();
            if (!string.IsNullOrEmpty(inputStr))
            {
                char inputChar = inputStr[0];
                CheckInput(inputChar);
            }
        }
    }


    private void GenerateGrid()
    {
        gridData = new string[totalRows];
        redIndicesPerLine = new List<int>[totalRows];

        for (int r = 0; r < totalRows; r++)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int c = 0; c < colsPerRow; c++)
            {
                char randomChar = (char)Random.Range(65, 91);
                sb.Append(randomChar);
            }
            gridData[r] = sb.ToString();

            redIndicesPerLine[r] = new List<int>();
            int redCount = Random.Range(1, 4);
            while (redIndicesPerLine[r].Count < redCount)
            {
                int randIndex = Random.Range(0, colsPerRow);
                if (!redIndicesPerLine[r].Contains(randIndex))
                {
                    redIndicesPerLine[r].Add(randIndex);
                }
            }
            redIndicesPerLine[r].Sort(); 
        }
    }

    private void ResetAllRowTexts()
    {
        for (int i = 0; i < totalRows; i++)
        {
            UpdateRowText(i);
        }
    }

    private void UpdateRowText(int rowIndex)
    {
        string originalStr = gridData[rowIndex];
        System.Text.StringBuilder formattedText = new System.Text.StringBuilder();

        for (int i = 0; i < colsPerRow; i++)
        {
            int redListIndex = redIndicesPerLine[rowIndex].IndexOf(i);

            if (redListIndex != -1) 
            {
                if (rowIndex == currentRow && redListIndex < currentCorrectCount)
                {
                    formattedText.Append($"<color=green>{originalStr[i]}</color>   ");
                }
                else
                {
                    formattedText.Append($"<color=red>{originalStr[i]}</color>   ");
                }
            }
            else 
            {
                formattedText.Append($"<color=green>{originalStr[i]}</color>   ");
            }
        }

        rowTextTemplates[rowIndex].text = formattedText.ToString();
    }

    private void SetActiveRow(int rowIndex)
    {
        if (rowIndex >= totalRows)
        {
            GameOver(true);
            return;
        }

        currentRow = rowIndex;
        currentCorrectCount = 0; 

        Vector3 targetPos = rowTextTemplates[rowIndex].transform.position;
        highlightBar.position = new Vector3(highlightBar.position.x, targetPos.y, highlightBar.position.z);

        UpdateRowText(currentRow);
    }

    private void CheckInput(char pressedChar)
    {
        List<int> currentRedIndices = redIndicesPerLine[currentRow];
        if (currentRedIndices.Count == 0) return;

        int targetCharIndex = currentRedIndices[currentCorrectCount];
        char targetChar = gridData[currentRow][targetCharIndex];

        if (pressedChar == targetChar)
        {
            Debug.Log("정답!");
            currentCorrectCount++;

            UpdateRowText(currentRow);

            if (currentCorrectCount >= currentRedIndices.Count)
            {
                SetActiveRow(currentRow + 1);
            }
        }
        else
        {
            Debug.Log("오답! 처음부터 다시 시작합니다.");

            currentRow = 0;
            currentCorrectCount = 0;

            Vector3 targetPos = rowTextTemplates[0].transform.position;
            highlightBar.position = new Vector3(highlightBar.position.x, targetPos.y, highlightBar.position.z);

            ResetAllRowTexts();
        }
    }

    private void GameOver(bool isSuccess)
    {
        isGameActive = false;

        ActiveManager.Instance.OnMiniGameResult(_taskType, isSuccess);

        if (isSuccess)
        {
            UIManager.Instance.OpenSimplePopup("항로 제어 완료");
            Debug.Log("제어 성공!");
            UIManager.Instance.CloseControlRepairPopupUI();

        }
        else
        {
            UIManager.Instance.OpenSimplePopup("항로 제어 실패");
            Debug.Log("제어 실패!");
            UIManager.Instance.CloseControlRepairPopupUI();
        }
    }

    public void InitGame()
    {
        if (!ActiveManager.Instance.CanPlayMiniGame(_taskType))
        {
            Debug.LogWarning("오늘 이미 클리어한 항로 제어 미니게임입니다!");
            UIManager.Instance.CloseControlRepairPopupUI();
            return;
        }

        currentTime = limitTime;
        currentRow = 0;
        currentCorrectCount = 0;

        if (timerSlider != null)
        {
            timerSlider.maxValue = limitTime;
            timerSlider.value = limitTime;
        }

        GenerateGrid();

        ResetAllRowTexts();

        if (rowTextTemplates != null && rowTextTemplates.Length > 0)
        {
            SetActiveRow(0);
        }

        isGameActive = true;
        GameStart();
        Debug.Log("항로 제어가 초기화되었습니다.");
    }
}