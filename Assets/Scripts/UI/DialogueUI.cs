using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets; // 🌟 Addressables 패키지

public class DialogueUI : UIBase
{
    [SerializeField] private TextMeshProUGUI Text_Description;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float waitTimePerDialogue = 2.0f;

    [SerializeField] private string typingSfxAddressKey = "Sound/Typing";

    private CancellationTokenSource _cts;
    private AudioClip _cachedTypingClip; 

    private void OnEnable()
    {
        PreloadTypingSfxAsync().Forget();
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
            Debug.LogWarning($"[DialogueUI] 어드레서블 사운드 로드 실패 ({typingSfxAddressKey}): {ex.Message}");
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
                Debug.LogWarning($"[DialogueUI] 데이터 로드 실패. Id를 찾을 수 없습니다: {currentId}");
                break;
            }

            await PlayTypingEffect(data.Description, token);

            await UniTask.Delay(System.TimeSpan.FromSeconds(waitTimePerDialogue), cancellationToken: token);

            currentId = data.NextId;
        }

        Text_Description.text = string.Empty;
        UIManager.Instance.CloseUI(UIRootType.ContentUI, UIType.DialogueUI);
    }

    private async UniTask PlayTypingEffect(string fullText, CancellationToken token)
    {
        Text_Description.text = string.Empty;

        for (int i = 0; i <= fullText.Length; i++)
        {
            Text_Description.text = fullText.Substring(0, i);

            if (i > 0)
            {
                char lastChar = fullText[i - 1];
                if (!char.IsWhiteSpace(lastChar))
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(typingSfxAddressKey);
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
}