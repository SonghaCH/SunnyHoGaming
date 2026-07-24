using System;
using TMPro;
using UnityEngine;

public class DoorPopupUI : UIBase
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI passwordDisplayText;
    [SerializeField] private UIButton Btn_Back;

    [Header("Game Settings")]
    [SerializeField] private string correctPassword = "0917";
    [SerializeField] private int maxPasswordLength = 4;

    private string _currentInput = "";
    private bool _isGameActive = false;

    public static event Action OnPasswordSuccess;

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
        UIManager.Instance.CloseDoorPopupUI();
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

        UIManager.Instance.OpenSimplePopup("패스워드 입력 성공! 문 잠금 해제");
        Debug.Log("패스워드 일치! 미션 성공");
        OnPasswordSuccess?.Invoke();
        UIManager.Instance.CloseDoorPopupUI();
    }

    private void FailGame()
    {
        if (passwordDisplayText != null)
        {
            passwordDisplayText.color = Color.red;
            passwordDisplayText.text = "패스워드 불일치";
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

        Debug.Log("문 패스워드 맞추기가 초기화되었습니다. (키보드로 입력하세요)");
    }
}
