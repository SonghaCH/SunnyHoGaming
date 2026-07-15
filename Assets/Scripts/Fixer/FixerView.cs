using UnityEngine;

[RequireComponent(typeof(Animator), typeof(FixerViewModel))]
public class FixerView : MonoBehaviour
{
    private Animator _animator;
    private FixerViewModel _viewModel;

    private void Awake()
    {
        bool hasAnimator = TryGetComponent(out _animator);
        if (hasAnimator == false)
        {
            Debug.LogError($"[FixerView] Animator 컴포넌트가 없습니다.");
        }

        bool hasViewModel = TryGetComponent(out _viewModel);
        if (hasViewModel == false)
        {
            Debug.LogError($"[FixerView] FixerViewModel 컴포넌트가 없습니다.");
        }
    }

    private void OnEnable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnAnimationStateChanged += PlayStateAnimation;
        }
    }

    private void OnDisable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnAnimationStateChanged -= PlayStateAnimation;
        }
    }

    private void PlayStateAnimation(FixerState state)
    {
        if (_animator == null)
        {
            return;
        }

        switch (state)
        {
            case FixerState.Idle:
                _animator.CrossFade("Idle", 0.1f);
                break;
            case FixerState.Rampaging:
                _animator.CrossFade("CrashRun", 0.1f);
                break;
            case FixerState.Executing:
                _animator.CrossFade("Fix", 0.1f);
                break;
            case FixerState.Returning:
                _animator.Play("Run");
                break;
            case FixerState.Wandering:
                _animator.CrossFade("Walk", 0.1f);
                break;
            case FixerState.MoveToTarget:
                _animator.CrossFade("Run", 0.1f);
                break;
        }
    }
}