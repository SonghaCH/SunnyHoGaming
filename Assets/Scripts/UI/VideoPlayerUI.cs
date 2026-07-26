using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerUI : UIBase
{
    [Header("Skip Settings")]
    [Tooltip("K 키를 꾹 누르고 있어야 하는 시간 (초)")]
    [SerializeField] private float holdDuration = 1.0f;

    private VideoPlayer videoPlayer;
    private float holdTimer = 0f;
    private bool isFinished = false; // 스킵 연타 및 중복 종료 방지 플래그

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

        SetPlayerCanMove(false);

        StartVideoSequenceAsync().Forget();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        SetPlayerCanMove(true);

    }

    private void Update()
    {
        if (isFinished) return;

        if (Input.GetKey(KeyCode.K))
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
            {
                Debug.Log("[VideoPlayerUI] K 꾹 누르기로 영상 스킵.");
                FinishVideoAndStartGame();
            }
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            holdTimer = 0f;
        }
    }

    private async UniTaskVoid StartVideoSequenceAsync()
    {
        await UniTask.Yield(PlayerLoopTiming.Update);

        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            var viewModel = NetworkManager.Inst.GameStateService.GetViewModel();
            if (viewModel != null)
            {
                Debug.Log("[VideoPlayerUI] 맵 생성을 트리거하고 바로 시간을 정지합니다.");

                viewModel.OnRequestingPlay();
                
                viewModel.OnRequestingPause();
            }
        }


        SetPlayerCanMove(false);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        FinishVideoAndStartGame();
    }


    private void FinishVideoAndStartGame()
    {
        if (isFinished) return;
        isFinished = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseVideoPlayerUI();
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            var viewModel = NetworkManager.Inst.GameStateService.GetViewModel();
            if (viewModel != null)
            {
                viewModel.OnRequestingResume();
            }
        }

        AudioManager.Instance.PlayBGM("Sound/InGameBGM");
    }


    private void SetPlayerCanMove(bool canMove)
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.SetCanMove(canMove);
        }
    }
}