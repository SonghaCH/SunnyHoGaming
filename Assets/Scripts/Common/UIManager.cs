using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas Canvas_BgRoot;
    [SerializeField] Canvas Canvas_MainRoot;
    [SerializeField] Canvas Canvas_ContentRoot;
    [SerializeField] Canvas Canvas_PopupRoot;
    [SerializeField] Canvas Canvas_VeryFrontRoot;

    public static UIManager Instance { get; set; }

    private Dictionary<UIType, UIBase> _createdUIDic = new Dictionary<UIType, UIBase>();
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>();

    private Stack<UIType> _openedPopupStack = new Stack<UIType>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        this.ShowStartupUIOnGameStart();

        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInventoryKey += HandleInventoryInput;
            UserInputManager.instance.OnQuestKey += HandleQuestInput;
            UserInputManager.instance.OnMapKey += HandleMapInput;
            UserInputManager.instance.OnEscapeKey += HandleEscapeInput;
        }
    }

    private void OnDestroy()
    {
        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInventoryKey -= HandleInventoryInput;
            UserInputManager.instance.OnQuestKey -= HandleQuestInput;
            UserInputManager.instance.OnMapKey -= HandleMapInput;
            UserInputManager.instance.OnEscapeKey -= HandleEscapeInput;
        }
    }

    private void HandleInventoryInput()
    {
        if (_openedUIDic.Contains(UIType.InventoryPopupUI))
        {
            CloseUI(UIRootType.PopupUI, UIType.InventoryPopupUI);
        }
        else
        {
            this.OpenInventoryPopupUI();
        }
    }

    private void HandleQuestInput()
    {
        ToggleUI(UIRootType.PopupUI, UIType.QuestPopupUI);
    }

    private void HandleMapInput()
    {
        ToggleUI(UIRootType.PopupUI, UIType.MapPopupUI);
    }
    public void ToggleUI(UIRootType rootType, UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            CloseUI(rootType, uiType);
        }
        else
        {
            OpenUI(rootType, uiType);
        }
    }
    private void HandleEscapeInput()
    {
        if (_openedPopupStack.Count > 0)
        {
            UIType topPopup = _openedPopupStack.Peek();

            CloseUI(UIRootType.PopupUI, topPopup);

            if(topPopup == UIType.PausePopupUI)
            {
                NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingResume();
            }
        }
        else
        {
            this.OpenPausePopupUI();
            NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingPause();
        }
    }


    private void RemovePopupFromStack(UIType uiType)
    {
        if (_openedPopupStack.Contains(uiType))
        {
            List<UIType> temp = new List<UIType>(_openedPopupStack);
            temp.Remove(uiType);

            _openedPopupStack.Clear();
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                _openedPopupStack.Push(temp[i]);
            }
        }
    }



    public UIBase OpenUI(UIRootType uiRootType, UIType uiType, bool isInitialHide = false)
    {
        var openedUI = GetCreatedUI(uiRootType, uiType);

        bool isSetActiveOnOpen = (isInitialHide == false);
        if (_openedUIDic.Contains(uiType) == false)
        {
            openedUI.gameObject.SetActive(isSetActiveOnOpen);
            _openedUIDic.Add(uiType);

            if (uiRootType == UIRootType.PopupUI && isSetActiveOnOpen)
            {
                if (_openedPopupStack.Contains(uiType) == false)
                {
                    _openedPopupStack.Push(uiType);
                }
            }
        }

        return openedUI;
    }

    public void CloseUI(UIRootType uiRootType, UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var openedUi = _createdUIDic[uiType];
            openedUi.gameObject.SetActive(false);
            _openedUIDic.Remove(uiType);

            if (uiRootType == UIRootType.PopupUI)
            {
                RemovePopupFromStack(uiType);
            }
        }
    }

    
    private Transform GetRootTransform(UIRootType uiRootType)
    {
        Transform root = null;
        switch (uiRootType)
        {
            case UIRootType.BackgroundUI: root = Canvas_BgRoot.transform; break;
            case UIRootType.MainUI: root = Canvas_MainRoot.transform; break;
            case UIRootType.ContentUI: root = Canvas_ContentRoot.transform; break;
            case UIRootType.PopupUI: root = Canvas_PopupRoot.transform; break;
            case UIRootType.VeryFrontUI: root = Canvas_VeryFrontRoot.transform; break;
        }
        return root;
    }

    private void CreateUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = this.GetUIPath(uiRootType, uiType);
            GameObject loadedObj = (GameObject)Resources.Load(path);
            Transform root = GetRootTransform(uiRootType);
            GameObject gObj = Instantiate(loadedObj, root);
            if (gObj != null)
            {
                var uiBase = gObj.GetComponent<UIBase>();
                _createdUIDic.Add(uiType, uiBase);
            }
        }
    }

    public UIBase GetOpenedUI(UIRootType uiRootType, UIType uiType)
    {
        return GetCreatedUI(uiRootType, uiType);
    }

    private UIBase GetCreatedUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiRootType, uiType);
        }
        return _createdUIDic[uiType];
    }
}