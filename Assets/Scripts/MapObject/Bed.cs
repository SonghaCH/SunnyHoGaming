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

    private int _sleepTime = 23;

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
            if (NetworkManager.Inst.TimeService.GetViewModel().CurrentHour < _sleepTime)
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
            // [일어난다] 눌렀을 때
            NetworkManager.Inst.PlayerService.WakeUp();

            if (_mainLight != null)
            {
                _mainLight.color = Color.white;
            }

            if (_player != null)
            {
                _player.GetComponent<PlayerMovementView>().SetTarget(_wakeUpTransform);
            }

            UIManager.Instance.CloseFPopupUI();
        }
        else
        {
            // [잠에 든다] 눌렀을 때
            // 1. 수면 상태 전환
            NetworkManager.Inst.PlayerService.Sleep();

            // 2. 날짜 넘기기 (TimeService 기존 메서드 사용)
            NetworkManager.Inst.TimeService.SkipToNextDay();

            // 3. UI 처리
            UIManager.Instance.CloseFPopupUI();

            var uiBase = UIManager.Instance.OpenUI(UIRootType.ContentUI, UIType.FPopupUI);

            if (uiBase is FPopupUI fPopupUI)
            {
                fPopupUI.SetInteractName("일어난다");
            }

            // 4. 위치 이동 및 조명 처리
            if (_player != null)
            {
                _player.GetComponent<PlayerMovementView>().SetTarget(_sleepTransform);
            }

            if (_mainLight != null)
            {
                _mainLight.color = Color.black;
            }
        }
    }
}