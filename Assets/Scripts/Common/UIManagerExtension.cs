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
    GameStartUI,
    MainUI,
    FixerPopupUI,
    HiddenNotePopupUI,
    RepairPopupUI,
    WorkPopupUI,
    InventoryPopupUI,
    JobcompletedPopupUI,
    SimplePopup,
    NotePopupUI,
    TempRepairPopupUI,
    ElectricRepairPopupUI,
    ControlRepairPopupUI,
    AirRepairPopupUI,
    PasswordPopupUI,
    DialogueUI,
    FPopupUI,
    PausePopupUI,
    QuestPopupUI,
    MapPopupUI,
    DoorPopupUI,
    RepairDisplayUI,
    SettingPopupUI
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
    public static void OpenFixerPopupUI(this UIManager uiManager, FixerViewModel fixerViewModel = null)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.FixerPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
        if (fixerViewModel != null && uiBase is FixerPopupUI popupUI)
        {
            popupUI.SetFixerInfo(fixerViewModel);
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
    public static void OpenWorkPopupUI(this UIManager uiManager, FixerViewModel fixerViewModel = null)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.WorkPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
        if (uiBase is WorkPopupUI popupUI)
        {
            if (fixerViewModel != null)
            {
                popupUI.SetFixerInfo(fixerViewModel);
            }
            else
            {
                popupUI.RefreshWorkList();
            }
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

    //
    public static void OpenNotePopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.NotePopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseNotePopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.NotePopupUI);
    }


    //
    public static void OpenTempRepairPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.TempRepairPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseTempRepairPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.TempRepairPopupUI);
    }

    //
    public static void OpenElectricRepairPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.ElectricRepairPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseElectricRepairPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.ElectricRepairPopupUI);
    }

    //
    public static void OpenControlRepairPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.ControlRepairPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseControlRepairPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.ControlRepairPopupUI);
    }

    //
    public static void OpenAirRepairPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.AirRepairPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void CloseAirRepairPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.AirRepairPopupUI);
    }

    //
    public static void OpenPasswordPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.PasswordPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }
    public static void ClosePasswordPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.PasswordPopupUI);
    }

    //
    public static void OpenFPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.ContentUI, UIType.FPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseFPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.ContentUI, UIType.FPopupUI);
    }
    public static void OpenPausePopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.PausePopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void ClosePausePopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.PausePopupUI);
    }

    public static void OpenMapPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.MapPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void ClsoeMapPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.MapPopupUI);
    }

    public static void OpenQuestPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.QuestPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseQuestPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.QuestPopupUI);
    }

    public static void OpenDoorPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.DoorPopupUI);
    }
    public static void CloseDoorPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.DoorPopupUI);
    }

    public static void OpenRepairDisplayUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.RepairDisplayUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseRepairDisplayUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.RepairDisplayUI);
    }

    public static void OpenSettingPopupUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.PopupUI, UIType.SettingPopupUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseSettingPopupUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.PopupUI, UIType.SettingPopupUI);
    }
}


