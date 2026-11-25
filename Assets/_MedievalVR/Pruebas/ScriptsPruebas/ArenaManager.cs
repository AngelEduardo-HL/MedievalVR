using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    // Esto crea el ".Instance" para que cualquier NPC pueda leerlo
    public static ArenaManager Instance { get; private set; }

    [Range(0f, 1f)] // Crea un slider en el Inspector para probarlo fácil
    public float charisma = 0.5f;

    void Awake()
    {
        // Configura la instancia única al iniciar la escena
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
