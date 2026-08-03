using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class EndingVideoPlayerUI : UIBase
{
    private VideoPlayer videoPlayer;
    private bool isFinished = false;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        isFinished = false;

        SetPlayerCanMove(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        StartEndingVideoSequenceAsync().Forget();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }
    }

    private async UniTaskVoid StartEndingVideoSequenceAsync()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            NetworkManager.Inst.GameStateService.GetViewModel()?.OnRequestingPause();
        }

        SetPlayerCanMove(false);

        if (videoPlayer != null)
        {
            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
                await UniTask.WaitUntil(() => videoPlayer.isPrepared, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            videoPlayer.Play();
        }
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

            try
            {
                UIManager.Instance.OpenEndingDialogueUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EndingVideo] EndingDialogUI 오픈 실패: {ex.Message}");
            }
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