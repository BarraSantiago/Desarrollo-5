using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Instancia est�tica del SceneLoader para el singleton
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        // Aseguramos que solo haya una instancia de SceneLoader
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ya existe otra instancia, destruimos esta
            return;
        }

        Instance = this; // Establecemos la instancia �nica
        DontDestroyOnLoad(gameObject); // No destruir al cargar otra escena
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
