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

        // 🌟 1. 비디오 UI 켜짐: 플레이어 이동 중지 & 마우스 커서 표시
        SetPlayerCanMove(false);

        // [순서 보장 핵심] 영상 UI 우선 활성화 -> 영상 준비/재생 시작 확인 -> 맵 백그라운드 로딩
        StartVideoSequenceAsync().Forget();
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        // 🌟 2. 비디오 UI 닫힘 (스킵/종료): 플레이어 이동 복구 & 마우스 커서 잠금
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

        // 3. 영상 출력이 보장된 후, 백그라운드 맵 생성 및 게임 재생 요청
        if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
        {
            Debug.Log("[VideoPlayerUI] 영상 출력 보장 완료. 백그라운드 맵 생성을 시작합니다.");
            NetworkManager.Inst.GameStateService.GetViewModel()?.OnRequestingPlay();
        }

        // 🌟 [핵심] OnRequestingPlay() 호출 시 게임 상태가 풀리며 CanMove가 true로 오버라이드되는 것을 차단!
        // 비디오 재생 중임을 재보장하기 위해 플레이어 이동 제어를 다시 한 번 걸어줍니다.
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

        // UIManager를 이용하거나 gameObject.SetActive(false)로 닫기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseVideoPlayerUI();
        }
        else
        {
            gameObject.SetActive(false);
        }

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

    /// <summary>
    /// PlayerService를 참조하여 캐릭터의 이동/시점 제어를 토글하는 보조 함수
    /// </summary>
    private void SetPlayerCanMove(bool canMove)
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.SetCanMove(canMove);
        }
    }
}