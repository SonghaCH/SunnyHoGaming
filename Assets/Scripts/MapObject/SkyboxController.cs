using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f; // 초당 회전 각도 (도)

    private void Update()
    {
        if (RenderSettings.skybox == null)
        {
            return;
        }

        float currentRotation = RenderSettings.skybox.GetFloat("_Rotation");
        currentRotation += _rotationSpeed * Time.deltaTime;

        if (currentRotation > 360f)
        {
            currentRotation -= 360f;
        }

        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
    }
}