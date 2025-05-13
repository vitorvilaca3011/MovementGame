using UnityEngine;
using UnityEngine.UI;

public class PlayerSpeedDisplay : MonoBehaviour
{
    public StrafeMovement strafeMovement;
    public Rigidbody controller; // ou Rigidbody se estiver usando
    public Text speedText;

    public Text gearsEngaged;

    void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;
        speedText.text = $"Speed: {speed:F2}";

        int gearEngaged = strafeMovement.gearEngaged; // Supondo que você tenha um método GetGearEngaged() na sua classe StrafeMovement
        gearsEngaged.text = $"Gear: {gearEngaged}";
    }
}