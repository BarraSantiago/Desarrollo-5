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
        Debug.Log("Este es un enemigo a distancia. Realizando acci�n a distancia.");
        // Aqu� puedes agregar la l�gica espec�fica para enemigos a distancia
    }

    public void DoMeleeAction()
    {
        Debug.Log("Este es un enemigo cuerpo a cuerpo. Realizando acci�n cuerpo a cuerpo.");
        // Aqu� puedes agregar la l�gica espec�fica para enemigos cuerpo a cuerpo
    }
}
