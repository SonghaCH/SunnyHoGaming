using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class DayVideoPlayerUI : UIBase
{
    private VideoPlayer _videoPlayer;
    private bool _isFinished = false;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        _isFinished = false;

        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached += OnVideoFinished;
        }

        SetPlayerCanMove(false);

        StartVideoSequenceAsync().Forget();
    }

    private void OnDisable()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private async UniTaskVoid StartVideoSequenceAsync()
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
        FinishVideoAndReturnToGame();
    }

    private void FinishVideoAndReturnToGame()
    {
        if (_isFinished) return;
        _isFinished = true;

        gameObject.SetActive(false);

        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            var viewModel = NetworkManager.Inst.GameStateService.GetViewModel();
            if (viewModel != null)
            {
                if (NetworkManager.Inst.GameStateService.GetCurrentState() == GameState.Paused)
                {
                    viewModel.OnRequestingResume();
                }
                else
                {
                    viewModel.OnRequestingPlay();
                }
            }
        }

        SetPlayerCanMove(true);
    }

    private void SetPlayerCanMove(bool canMove)
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.SetCanMove(canMove);
        }
    }
}