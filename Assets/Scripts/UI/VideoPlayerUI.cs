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

        // [순서 보장 핵심] 영상 UI 우선 활성화 -> 영상 준비/재생 시작 확인 -> 맵 백그라운드 로딩
        StartVideoSequenceAsync().Forget();
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

    // [핵심] 실행 순서를 보장하는 비동기 시퀀스
    private async UniTaskVoid StartVideoSequenceAsync()
    {
        // 1. 우선 게임을 Pause 상태로 변경하여 커서/입력 정리
        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            NetworkManager.Inst.GameStateService.GetViewModel()?.OnRequestingPause();
        }

        // 2. 영상 렌더링이 시작될 때까지 1프레임 대기 (영상 UI가 화면 최상단에 완전히 뜨도록 보장)
        await UniTask.Yield(PlayerLoopTiming.Update);

        // (선택 사항) VideoPlayer가 비디오를 준비(Prepare)할 때까지 완전히 대기하고 싶다면 아래 주석 해제
        /*
        if (videoPlayer != null)
        {
            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
                await UniTask.WaitUntil(() => videoPlayer.isPrepared);
            }
            videoPlayer.Play();
        }
        */

        // 3. 영상이 먼저 화면에 나오는 것이 보장된 후, 비로소 백그라운드 맵 생성을 요청
        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            Debug.Log("[VideoPlayerUI] 영상 출력 보장 완료. 백그라운드 맵 생성을 시작합니다.");
            NetworkManager.Inst.GameStateService.GetViewModel()?.OnRequestingPlay();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        FinishVideoAndStartGame();
    }

    private void FinishVideoAndStartGame()
    {
        // UIManager를 이용하거나 gameObject.SetActive(false)로 닫기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseVideoPlayerUI();
        }
        else
        {
            gameObject.SetActive(false);
        }

        // 커서 잠금 처리
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 게임 상태 복귀
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
    }
}