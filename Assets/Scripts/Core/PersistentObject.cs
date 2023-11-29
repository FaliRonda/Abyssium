using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    private void Awake()
    {
        // Verificar si ya hay una instancia de este objeto
        if (instance == null)
        {
            // Si no hay una instancia, hacer de este objeto la instancia persistente
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si ya hay una instancia, destruir el objeto duplicado
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Desuscribirse al evento de carga de escena para evitar fugas de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Verificar si ya hay una instancia en la nueva escena
        if (instance != null && instance != this)
        {
            // Si hay una instancia, destruir este objeto duplicado
            Destroy(gameObject);
        }
    }
}