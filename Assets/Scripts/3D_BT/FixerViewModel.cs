using System;
using UnityEngine;

public class FixerViewModel : MonoBehaviour
{
    public event Action<FixerState> OnStateChanged;

    [SerializeField]
    private FixerState currentState = FixerState.Rampaging;

    public FixerState CurrentState
    {
        get
        {
            return currentState;
        }

        set
        {
            if (currentState != value)
            {
                currentState = value;

                OnStateChanged?.Invoke(currentState);

                Debug.Log($"[ViewModel] FixerState가 {currentState}로 변경되었습니다.");
            }
        }
    }
}