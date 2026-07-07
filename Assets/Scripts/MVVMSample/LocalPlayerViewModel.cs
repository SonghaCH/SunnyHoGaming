using System.ComponentModel;
using UnityEngine;

public class LocalPlayerViewModel : ViewModelBase
{
    // 전통적인 MVVM은 모델이 따로 있는 것이 맞으나
    // 사실 모델은 저장된 원본 데이터를 의미하기 때문에, 서버의 데이터 등 해석 범위가 넓다
    // 뷰모델이 모델의 역할을 어느정도 수렴한다고 타협하고 짠 것임을 꼭 인지하자 (Model을 따로 둬도 되지만
    // 파일과 클래스가 불필요하게 많아지기에 데이터 관리와 뷰를 분리하고, 데이터의 갱신에 뷰가 자동으로 갱신된다는 의의만 가져간다!

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(TotalExp));
        OnPropertyChanged(nameof(CurrentLevel));
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    private int _totalExp;
    public int TotalExp
    {
        get => _totalExp;
        set
        {
            if (_totalExp != value)
            {
                _totalExp = value;
                CurrentLevel = (int)_totalExp / 100;
                OnPropertyChanged(nameof(TotalExp));
            }
        }
    }

    private int _currentLevel;
    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            if (_currentLevel != value)
            {
                _currentLevel = value;
                OnPropertyChanged(nameof(CurrentLevel));
            }
        }
    }

}
