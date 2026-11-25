using UnityEngine;
using System.Collections;

public class CitizensReaction : MonoBehaviour
{
    private Animator anim;

    // Nombres de los estados en tu Animator Controller
    public string[] cheerAnimations = { "Clap", "Cheer1", "JumpJoy" };
    public string[] booAnimations = { "Boo", "AngryFist", "ThumbsDown" };

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(AnimationLoop());
    }

    IEnumerator AnimationLoop()
    {
        // Desfase inicial para que no empiecen todos en el frame 0
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));

        while (true)
        {
            // Supongamos que ArenaManager.Instance.charisma es un float entre 0 y 1
            float currentCharisma = ArenaManager.Instance.charisma;
            float roll = Random.value; // Número aleatorio entre 0.0 y 1.0

            string chosenAnim;
            if (roll <= currentCharisma)
            {
                // Animación buena
                chosenAnim = cheerAnimations[Random.Range(0, cheerAnimations.Length)];
            }
            else
            {
                // Animación mala / enojo
                chosenAnim = booAnimations[Random.Range(0, booAnimations.Length)];
            }

            // CrossFade hace una transición suave de 0.25s entre animaciones
            anim.CrossFade(chosenAnim, 0.25f);

            // Esperar un frame para que el Animator actualice la información del nuevo estado
            yield return null;

            // Obtener la duración de la animación actual y esperar a que termine
            float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength + Random.Range(0.1f, 0.4f));
        }
    }
}
