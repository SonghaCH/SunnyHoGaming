using System.ComponentModel;
using UnityEngine;

public class LocalPlayerView : ViewBase
{
    // 플레이어 3D 모델, 애니메이션, 물리에 관한 뷰
    [SerializeField] TextMesh TextMesh_Name;
    [SerializeField] TextMesh TextMesh_Level;
    [SerializeField] Entity Entity_Self;

    // 뷰에서 절대 new로 VewModel을 하지 않고, 네트워크 매니저를 통해
    // 실제 생성된 뷰 모델을 받아와야한다!
    private LocalPlayerViewModel _vm;

    public void BindViewModel(LocalPlayerViewModel vm)
    {
        _vm = vm;
        _vm.PropertyChanged += OnPropChagned_View;
        _vm.InvokeOnceOnInit();
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropChagned_View;
        }
    }

    private void OnPropChagned_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(LocalPlayerViewModel.Name):
            {
                TextMesh_Name.text = _vm.Name;
            }
            break;
            case nameof(LocalPlayerViewModel.CurrentLevel):
            {
                TextMesh_Level.text = $"Lv.{_vm.CurrentLevel}";
                ChangeAnimationOnSuccessLevelUp();
            }
            break;
        }
    }

    private void ChangeAnimationOnSuccessLevelUp()
    {
        var animator = Entity_Self.GetEntityAnimator();
        if(animator != null)
        {
            animator.SetTrigger("IsPositive");
        }
    }
}
