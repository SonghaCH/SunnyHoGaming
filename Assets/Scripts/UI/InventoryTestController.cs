using UnityEngine;

public class InventoryTestController : MonoBehaviour
{
    private void Update()
    {
        // [테스트 1] 숫자 키 1번 누르면: '숨겨진 노트' 1개 획득
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[Test] '숨겨진 노트' 1개 획득 요청");

            // 데이터 드리븐 ID "Item_Note_01"로 아이템을 1개 추가합니다.
            NetworkManager.Inst.InventoryService.AddItem("Item_Note_01", 1);
        }

        // [테스트 2] 숫자 키 2번 누르면: '우주 식량' 5개 획득
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("[Test] '우주 식량' 5개 획득 요청");

            // 데이터 드리븐 ID "Item_Food_01"로 아이템을 5개 추가합니다.
            NetworkManager.Inst.InventoryService.AddItem("Item_Food_01", 5);
        }

        // [테스트 3] 숫자 키 3번 누르면: '얼음 결정석' 10개 획득
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[Test] '얼음 결정석' 10개 획득 요청");

            // 데이터 드리븐 ID "Item_Resources_01"로 아이템을 10개 추가합니다.
            NetworkManager.Inst.InventoryService.AddItem("Item_Resources_01", 10);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("[Test] 가족 사진 획득 요청");

            // 데이터 드리븐 ID "Item_Resources_01"로 아이템을 10개 추가합니다.
            NetworkManager.Inst.InventoryService.AddItem("Item_Photo_01", 1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("[Test] 위성전화기 획득 요청");

            // 데이터 드리븐 ID "Item_Resources_01"로 아이템을 10개 추가합니다.
            NetworkManager.Inst.InventoryService.AddItem("Item_Phone_01", 1);
        }
    }
}