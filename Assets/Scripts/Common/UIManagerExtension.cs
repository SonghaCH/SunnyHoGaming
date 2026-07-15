using System;
using UnityEngine;

public enum UIRootType
{
    None = 0,
    BackgroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI
}

public enum UIType
{
    MyProfilePopup, 
    Inventory,
    LoadingUI,
    DialogueUI,
    InfoBookUI,
    RobbyUI,
    GameBookUI,
    HudUI,
    LocalPlayerProfileUI,
    MVVMTestUI,



    GameStartUI,
    MainUI,
    FixerPopupUI,
    HiddenNotePopupUI,
    RepairPopupUI,
    WorkPopupUI,
    InventoryPopupUI,
    JobcompletedPopupUI,
    SimplePopup









}

public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty

        
        path = $"Prefab/UI/{uiRootType}/{uiType}";
        return path;
    }

    public static void ShowStartupUIOnGameStart(this UIManager uiManager)
    {
        uiManager.OpenGameStartUI();
    }

   

    
    

    public static void OpenLoadingUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }





    //
    public static void OpenGameStartUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.ContentUI, UIType.GameStartUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseGameStartUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.GameStartUI);
    }


    //
    public static void OpenMainUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.MainUI, UIType.MainUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseMainUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.MainUI, UIType.MainUI);
    }

    //
    public static void OpenFixerPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.FixerPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseFixerPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.FixerPopupUI);
    }

    //
    public static void OpenHiddenNotePopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.HiddenNotePopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseHiddenNotePopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.HiddenNotePopupUI);
    }

    //
    public static void OpenRepairPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.RepairPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseRepairPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.RepairPopupUI);
    }

    //
    public static void OpenWorkPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.WorkPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseWorkPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.WorkPopupUI);
    }

    //
    public static void OpenInventoryPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.InventoryPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseInventoryPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.InventoryPopupUI);
    }

    //
    public static void OpenJobcompletedPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.JobcompletedPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseJobcompletedPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.JobcompletedPopupUI);
    }

    //
    public static void OpenSimplePopup(this UIManager uiManager, string msg)
    {
        var uiBase = uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.SimplePopup);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }

        if (uiBase is SimplePopup simplePopup)
        {
            simplePopup.SetUI(msg);
        }
    }
    public static void CloseSimplePopup(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.SimplePopup);
    }




    







}

