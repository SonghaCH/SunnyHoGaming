using UnityEngine;

public class MainController : UIBase
{
    private bool _isTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isTrigger = true;

            //TODO HUD 나오게 하는  코드 작성
            //UIManager.Instance.Open

            if (Input.GetKeyDown(KeyCode.F))
            {
                //TODO 명진님이 만들어주시면 넣을거 조향 미니게임
                //UIManager.Instance.OpenPopupUI(UIType.);
            }
        }
    }
}
