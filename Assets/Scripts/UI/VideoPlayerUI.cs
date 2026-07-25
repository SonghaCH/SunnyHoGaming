using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerUI : UIBase
{
    [Header("Skip Settings")]
    [Tooltip("K 키를 꾹 누르고 있어야 하는 시간 (초)")]
    [SerializeField] private float holdDuration = 1.0f;

    private VideoPlayer videoPlayer;
    private float holdTimer = 0f;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        holdTimer = 0f;

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // 1. 영상 재생 시작 시 게임을 Pause 상태로 만들고 마우스 커서/시점 조작을 멈춥니다.
        PauseGameAndStartMapPreload();
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
        // K 키 입력 감지 (꾹 누르기)
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

    // 영상 재생 자연 종료 시 호출
    private void OnVideoFinished(VideoPlayer vp)
    {
        FinishVideoAndStartGame();
    }

    // [영상 시작 시] 퍼즈 상태 전환 + 백그라운드 맵 미리 로딩
    private void PauseGameAndStartMapPreload()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            var viewModel = NetworkManager.Inst.GameStateService.GetViewModel();
            if (viewModel != null)
            {
                // 게임 상태를 Paused로 변경 (카메라/커서 고정)
                viewModel.OnRequestingPause();

                // 동시에 백그라운드에서 맵 생성을 시작함
                viewModel.OnRequestingPlay();
            }
        }
    }

    // [영상 종료/스킵 시] UI 닫기 + 게임 Playing 상태로 전환
    private void FinishVideoAndStartGame()
    {
        gameObject.SetActive(false);

        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            var viewModel = NetworkManager.Inst.GameStateService.GetViewModel();
            if (viewModel != null)
            {
                // 영상이 끝났으므로 Resume 또는 Play 신호를 보내 상태를 Playing으로 변경
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
    }
}