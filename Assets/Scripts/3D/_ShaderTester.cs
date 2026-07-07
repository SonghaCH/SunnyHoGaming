using System.Collections;
using UnityEngine;

public class _ShaderTester : MonoBehaviour
{
    [SerializeField] private Renderer MyRenderer;

    [SerializeField] private float duration = 2.0f; // 0에서 1까지 도달하는 데 걸리는 시간 (초)
    [SerializeField] private float checkInterval = 0.1f; // 코루틴이 갱신되는 주기 (0.1초)

    [SerializeField] private bool IsTestDrawcall;


    private int freezeAmountID;
    private Material _testMaterial;

    private void OnEnable()
    {
        _testMaterial = MyRenderer.material;
        freezeAmountID = Shader.PropertyToID("_FrozenAmount");

        if(IsTestDrawcall == true)
        {
            // SRP배처가 확실히 강력해져서 쉐이더 코드 비슷하면 배칭도 아예 같이 그려버리는듯
            // 원래 아래 코드는 가상 머테리얼 추가로 복제하는 안좋은 코드였는데...
            // 유니티 6에서는 배치가 1개로 묶여지는 듯 함 
            MyRenderer.material.color = Color.dimGray;
        }
        else
        {
            StartCoroutine(CoFreezePingPong());
        }
    }

    private IEnumerator CoFreezePingPong()
    {
        float currentFreeze = 0f;
        bool increasing = true; // 값이 증가하는 중인지 체크하는 플래그

        // 0.1초 주기로 동작할 WaitForSeconds 캐싱 (가비지 컬렉션 방지)
        WaitForSeconds waitTime = new WaitForSeconds(checkInterval);

        while (true)
        {
            // 0.1초마다 변화할 단계별 수치 계산
            // (0.1초 / 목표시간) 만큼 매 턴마다 더하거나 뺍니다.
            float step = checkInterval / duration;

            if (increasing)
            {
                currentFreeze += step;
                if (currentFreeze >= 1.0f)
                {
                    currentFreeze = 1.0f;
                    increasing = false; // 1에 도달하면 감소 모드로 전환
                }
            }
            else
            {
                currentFreeze -= step;
                if (currentFreeze <= 0.0f)
                {
                    currentFreeze = 0.0f;
                    increasing = true; // 0에 도달하면 증가 모드로 전환
                }
            }

            // 쉐이더 변수 값 수정
            _testMaterial.SetFloat(freezeAmountID, currentFreeze);

            // 0.1초 대기
            yield return waitTime;
        }
    }
}
