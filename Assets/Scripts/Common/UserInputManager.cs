using System;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{
    public static UserInputManager instance { get; private set; }

    public event Action OnInteractionKey;
    public event Action OnInventoryKey;
    public event Action OnEscapeKey;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnInteractionKey?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OnInventoryKey?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapeKey?.Invoke(); 
        }
    }
}