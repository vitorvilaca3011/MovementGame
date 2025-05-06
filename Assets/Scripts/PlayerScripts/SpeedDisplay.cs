using UnityEngine;
using UnityEngine.UI;

public class PlayerSpeedDisplay : MonoBehaviour
{
    public Rigidbody controller; // ou Rigidbody se estiver usando
    public Text speedText;

    void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;
        speedText.text = $"Speed: {speed:F2}";
    }
}