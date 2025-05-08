using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingPad : MonoBehaviour
{
    public float jumpForceAdded = 10f; // Força aplicada
    public Vector3 jumpDirection = Vector3.up; // Direção da força (padrão: para cima)

    public Color gizmoColor = Color.red; // Cor do gizmo

    private void OnCollisionEnter(Collision other)
    {
        // Verifica se o objeto que colidiu tem a tag "Player"
        if (other.gameObject.CompareTag("Player"))
        {
            // Obtém o Rigidbody do jogador
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Aplica a força na direção especificada
                rb.AddForce(jumpDirection.normalized * jumpForceAdded, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Desenha uma linha no editor para mostrar a direção do Jumping Pad
        Gizmos.color = gizmoColor;
        Gizmos.DrawLine(transform.position, transform.position + jumpDirection.normalized);

        // Desenha um cone para indicar a direção
        Vector3 arrowHead = transform.position + jumpDirection.normalized;
        Vector3 right = Quaternion.LookRotation(jumpDirection) * Quaternion.Euler(0, 150, 0) * Vector3.forward * 0.2f;
        Vector3 left = Quaternion.LookRotation(jumpDirection) * Quaternion.Euler(0, -150, 0) * Vector3.forward * 0.2f;

        Gizmos.DrawLine(arrowHead, arrowHead - right);
        Gizmos.DrawLine(arrowHead, arrowHead - left);
    }
}
