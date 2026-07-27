using System;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField] private Renderer _outLineRenderer;
    [SerializeField] private Material _outLineMaterial;
    [SerializeField] private Transform _sleepTransform;
    [SerializeField] private Transform _wakeUpTransform;

    private Light _mainLight;

    private GameObject _player;

    private int _sleepTime = 9;

    private Material[] _originalMaterials;
    private Material[] _outlineMaterials;


    private void Awake()
    {

        if (_outLineRenderer != null)
        {
            _outlineMaterials = _outLineRenderer.sharedMaterials;

            _originalMaterials = new Material[_outlineMaterials.Length - 1];
            Array.Copy(_outlineMaterials, _originalMaterials, _originalMaterials.Length);

            _outLineRenderer.sharedMaterials = _originalMaterials;
        }

        _mainLight = RenderSettings.sun;
    }

    private void SetOutline(bool isOn)
    {
        if (_outLineRenderer == null)
        {
            return;
        }
        _outLineRenderer.sharedMaterials = isOn ? _outlineMaterials : _originalMaterials;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(NetworkManager.Inst.TimeService.GetViewModel().CurrentHour < _sleepTime)
            {
                return;
            }

            _player = other.gameObject; 

            UserInputManager.instance.OnInteractionKey += Interact;
            SetOutline(true);

            var uiBase = UIManager.Instance.OpenUI(UIRootType.ContentUI, UIType.FPopupUI);

            if (uiBase is FPopupUI fPopupUI)
            {
                fPopupUI.SetInteractName("잠에 든다");
            }

            other.GetComponent<PlayerMovementView>().SetTarget(_sleepTransform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
            SetOutline(false);
            UIManager.Instance.CloseFPopupUI();

            other.GetComponent<PlayerMovementView>().SetTarget(null);
        }
    }

    private void Interact()
    {
        if (NetworkManager.Inst.PlayerService.GetStatusViewModel().IsSleeping)
        {
            NetworkManager.Inst.PlayerService.WakeUp();
            NetworkManager.Inst.TimeService.SkipToNextDay();

            _mainLight.color = Color.white;
        }
        else
        {
            NetworkManager.Inst.PlayerService.Sleep();
            UIManager.Instance.CloseFPopupUI();

            var uiBase = UIManager.Instance.OpenUI(UIRootType.ContentUI, UIType.FPopupUI);

            if (uiBase is FPopupUI fPopupUI)
            {
                fPopupUI.SetInteractName("일어난다");
            }

            _player.GetComponent<PlayerMovementView>().SetTarget(_wakeUpTransform);
            _mainLight.color = Color.black;
        }
    }
}