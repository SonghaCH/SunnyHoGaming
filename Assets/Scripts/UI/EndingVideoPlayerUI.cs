using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class EndingVideoPlayerUI : UIBase
{
    

    private VideoPlayer videoPlayer;
    private float holdTimer = 0f;
    private bool isFinished = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        holdTimer = 0f;
        isFinished = false;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // 🌟 1. 엔딩 영상 시작 시 플레이어 이동 및 시점 제어 차단
        SetPlayerCanMove(false);

        // 🌟 2. 백그라운드 게임 루프 일시정지 및 플레이어 차단 재보장
        StartEndingVideoSequenceAsync().Forget();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void Update()
    {
        if (isFinished) return;
    }

    private async UniTaskVoid StartEndingVideoSequenceAsync()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            NetworkManager.Inst.GameStateService.GetViewModel()?.OnRequestingPause();
        }

        await UniTask.Yield(PlayerLoopTiming.Update);

        SetPlayerCanMove(false);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        FinishEndingVideoAndOpenDialog();
    }

    
    private void FinishEndingVideoAndOpenDialog()
    {
        if (isFinished) return;
        isFinished = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseMainUI();
            UIManager.Instance.CloseAllPopups();

            UIManager.Instance.CloseEndingVideoPlayerUI();

            UIManager.Instance.OpenEndingDialogUI();
        }
        else
        {
            gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    
    private void SetPlayerCanMove(bool canMove)
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.SetCanMove(canMove);
        }
    }
}