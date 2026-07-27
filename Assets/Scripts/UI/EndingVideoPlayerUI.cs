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
        AudioManager.Instance.StopBGM();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.loopPointReached += OnVideoFinished;

            videoPlayer.Play();
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

        await UniTask.Yield(PlayerLoopTiming.Update);

        SetPlayerCanMove(false);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        FinishEndingVideoAndOpenDialog();
        AudioManager.Instance.PlayBGM("Sound/InGameBGM");

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
            UIManager.Instance.OpenEndingDialogUI();
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