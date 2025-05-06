using UnityEngine;

[ExecuteInEditMode]
public class ChangeColor : MonoBehaviour
{
    [Header("Cor personalizada")]
    public Color novaCor = Color.red;

    private Color corAnterior;

    void Update()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null || renderer.sharedMaterial == null)
            return;

        // Só atualiza se a cor mudar (pra evitar chamadas desnecessárias)
        if (novaCor != corAnterior)
        {
            // Cria uma instância do material se ainda não tiver feito
            Material novoMaterial = new Material(renderer.sharedMaterial);
            renderer.sharedMaterial = novoMaterial;

            if (novoMaterial.HasProperty("_BaseColor"))
            {
                novoMaterial.SetColor("_BaseColor", novaCor);
            }

            corAnterior = novaCor;
        }
    }
}
