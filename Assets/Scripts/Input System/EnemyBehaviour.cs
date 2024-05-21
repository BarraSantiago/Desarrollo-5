using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public bool isRanged;

    private void Start()
    {
        // Determinar el tipo de enemigo basado en la etiqueta
        if (CompareTag("Ranged"))
        {
            isRanged = true;
        }
        else if (CompareTag("Melee"))
        {
            isRanged = false;
        }
    }

    public void DoRangedAction()
    {
        Debug.Log("Este es un enemigo a distancia. Realizando acción a distancia.");
        // Aquí puedes agregar la lógica específica para enemigos a distancia
    }

    public void DoMeleeAction()
    {
        Debug.Log("Este es un enemigo cuerpo a cuerpo. Realizando acción cuerpo a cuerpo.");
        // Aquí puedes agregar la lógica específica para enemigos cuerpo a cuerpo
    }
}
