using TMPro;
using UnityEngine;


public class PasswordMiniGame : UIBase
    {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI passwordDisplayText;
    [SerializeField] private UIButton Btn_Back;

    [Header("Game Settings")]
    [SerializeField] private string correctPassword = "2026";     
    [SerializeField] private int maxPasswordLength = 4;

    private string _currentInput = "";                         
    private bool _isGameActive = false;                           



    private void OnEnable()
    {
        Btn_Back.BindOnClickButtonEvent(OnClick_Back);
        InitializeGame();

    }


    private void Update()
    {
        if (!_isGameActive)
        {
            return;
        } 

        HandleKeyboardInput();
    }

    private void OnClick_Back()
    {
        UIManager.Instance.ClosePasswordPopupUI();
    }

    private void HandleKeyboardInput()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
            {
                AddNumber(i.ToString());
                return; 
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            BackspaceInput();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearInput();
        }
    }

    private void AddNumber(string number)
    {
        if (_currentInput.Length >= maxPasswordLength)
        {
            return;
        } 

        _currentInput += number;
        UpdateDisplay();

        if (_currentInput.Length == maxPasswordLength)
        {
            CheckPassword();
        }
    }

    private void ClearInput()
    {
        _currentInput = "";
        UpdateDisplay();
    }


    private void BackspaceInput()
    {
        if (_currentInput.Length > 0)
        {
            _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (passwordDisplayText == null) return;

        passwordDisplayText.color = Color.white;

        if (string.IsNullOrEmpty(_currentInput))
        {
            passwordDisplayText.text = "<u> </u>  <u> </u>  <u> </u>  <u> </u>";
            return;
        }

        System.Text.StringBuilder formattedText = new System.Text.StringBuilder();

        for (int i = 0; i < maxPasswordLength; i++)
        {
            if (i < _currentInput.Length)
            {
                formattedText.Append(_currentInput[i]);
            }
            else
            {
                formattedText.Append("<u> </u>");
            }

            if (i < maxPasswordLength - 1)
            {
                formattedText.Append("  ");
            }
        }

        passwordDisplayText.text = formattedText.ToString();
    }

    private void CheckPassword()
    {
        if (_currentInput == correctPassword)
        {
            SuccessGame();
        }
        else
        {
            FailGame();
        }
    }

    private void SuccessGame()
    {
        _isGameActive = false;

        if (passwordDisplayText != null)
        {
            passwordDisplayText.color = Color.green;
            passwordDisplayText.text = "ACCESS GRANTED";
        }

        UIManager.Instance.OpenSimplePopup("위성전화 연결 성공! 데이터 복구 완료.");
        Debug.Log("비밀번호 일치! 미션 성공");

        QuestManager.Instance.CheckTaskProgress("SatellitePhone");

        UIManager.Instance.ClosePasswordPopupUI();
    }

    private void FailGame()
    {
        if (passwordDisplayText != null)
        {
            passwordDisplayText.color = Color.red;
            passwordDisplayText.text = "주파수 불일치";
        }

        Debug.Log("비밀번호 불일치! 다시 시도하세요.");
        StartCoroutine(ResetAfterDelay(1f));
    }

    private System.Collections.IEnumerator ResetAfterDelay(float delay)
    {
        _isGameActive = false;
        yield return new WaitForSeconds(delay);
        InitializeGame();
    }

    public void InitializeGame()
    {
        _currentInput = "";
        _isGameActive = true;

        UpdateDisplay();

        Debug.Log("위성전화 비밀번호 미니게임이 초기화되었습니다. (키보드로 입력하세요)");
    }
}
