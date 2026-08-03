using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EndingDialogUI : UIBase
{
    [Header("Dialogue Settings")]
    [Tooltip("시작할 엔딩 대화의 ID를 입력하세요")]
    [SerializeField] private string _startDialogueId = "Dialogue_Ending_001";

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float waitTimePerDialogue = 2.0f;

    [Header("Audio Settings")]
    [Tooltip("타이핑 시 재생될 SFX Sound Data ID (Addressables Key)")]
    [SerializeField] private string typingSfxAddressKey = "Sound/TypingKey";

    private CancellationTokenSource _cts;
    private AudioClip _cachedTypingClip;

    private void OnEnable()
    {
        ShowCursor();

        if (AudioController.Instance != null)
        {
            AudioController.Instance.StopBGM();
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        PreloadTypingSfxAsync().Forget();

        StartDialogue(_startDialogueId);
    }

    private void OnDisable()
    {
        CancelActiveDialogue();

        if (_cachedTypingClip != null)
        {
            Addressables.Release(_cachedTypingClip);
            _cachedTypingClip = null;
        }
    }

    private async UniTaskVoid PreloadTypingSfxAsync()
    {
        if (string.IsNullOrEmpty(typingSfxAddressKey) || _cachedTypingClip != null) return;

        try
        {
            _cachedTypingClip = await Addressables.LoadAssetAsync<AudioClip>(typingSfxAddressKey).ToUniTask();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[EndingDialogUI] 어드레서블 사운드 로드 실패 ({typingSfxAddressKey}): {ex.Message}");
        }
    }

    public void StartDialogue(string startDialogueId)
    {
        CancelActiveDialogue();
        _cts = new CancellationTokenSource();

        PlayDialogueChain(startDialogueId, _cts.Token).Forget();
    }

    private async UniTaskVoid PlayDialogueChain(string startId, CancellationToken token)
    {
        string currentId = startId;

        while (!string.IsNullOrEmpty(currentId))
        {
            DialogueData data = GameDataManager.Instance.GetDialogueData(currentId);
            if (data == null)
            {
                Debug.LogWarning($"[EndingDialogUI] 데이터 로드 실패. Id를 찾을 수 없습니다: {currentId}");
                break;
            }

            await PlayTypingEffect(data.Description, token);

            await UniTask.Delay(System.TimeSpan.FromSeconds(waitTimePerDialogue), cancellationToken: token);

            currentId = data.NextId;
        }

        if (Text_Description != null)
        {
            Text_Description.text = string.Empty;
        }

        FinishEndingDialog();
    }

    private async UniTask PlayTypingEffect(string fullText, CancellationToken token)
    {
        if (Text_Description == null) return;

        Text_Description.text = string.Empty;

        for (int i = 0; i <= fullText.Length; i++)
        {
            Text_Description.text = fullText.Substring(0, i);

            if (i > 0 && !string.IsNullOrEmpty(typingSfxAddressKey))
            {
                char lastChar = fullText[i - 1];
                if (!char.IsWhiteSpace(lastChar))
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(typingSfxAddressKey);
                    }
                    else if (AudioController.Instance != null)
                    {
                        AudioController.Instance.PlaySFX(typingSfxAddressKey);
                    }
                }
            }

            await UniTask.Delay(System.TimeSpan.FromSeconds(typingSpeed), cancellationToken: token);
        }
    }

    private void CancelActiveDialogue()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void FinishEndingDialog()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseUI(UIRootType.ContentUI, UIType.EndingDialogueUI);
            UIManager.Instance.OpenGameStartUI();
        }
        else
        {
            gameObject.SetActive(false);
        }

        ShowCursor();
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}