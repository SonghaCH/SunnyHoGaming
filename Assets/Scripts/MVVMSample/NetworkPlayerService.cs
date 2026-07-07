using UnityEngine;

public class NetworkPlayerService
{
    // Player와 관련된 전반적인 서비스를 담당
    // DB나 송수신을 통해 받은 플레이어 관련 인스턴스 데이터들을 여기에 보관
    private LocalPlayerViewModel _localPlayerViewModel;

    public LocalPlayerViewModel GetLocalPlayerViewModel()
    {
        if(_localPlayerViewModel == null)
        {
            CreateLocalPlayerViewModel();
        }

        return _localPlayerViewModel;
    }

    public LocalPlayerViewModel CreateLocalPlayerViewModel()
    {
        var localPlayerVm = new LocalPlayerViewModel();
        localPlayerVm.Name = "기본 이름";
        localPlayerVm.TotalExp = 0;
        _localPlayerViewModel = localPlayerVm; 
        return localPlayerVm;
    }

    public void RequestGiveExpToLocalPlayer(int exp)
    {
        if (_localPlayerViewModel != null)
        {
            _localPlayerViewModel.TotalExp += exp;
        }
    }

    public void RequestChangePlayerName(string newName)
    {
        if (_localPlayerViewModel != null)
        {
            _localPlayerViewModel.Name = newName;
        }
    }

}
