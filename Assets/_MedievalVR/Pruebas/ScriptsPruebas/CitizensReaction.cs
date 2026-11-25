using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CitizensReaction : MonoBehaviour
{
    public string[] cheerAnimations = { "Clap 01", "Clap 02", "Clap 03", "Clap 04", "Cheer 01", "Cheer 02" };
    public string[] booAnimations = { "Disbelief 01", "Disbelief 02", "Disbelief 03", "Angry", "Yell 01", "Yell 02" };

    IEnumerator Start()
    {
        var anim = GetComponent<Animator>();
        if (anim.runtimeAnimatorController == null) yield break;
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));

        while (true)
        {
            float charisma = ArenaManager.Instance != null ? ArenaManager.Instance.charisma : 0.5f;
            string[] animations = Random.value <= charisma ? cheerAnimations : booAnimations;
            if (animations.Length == 0) yield break;

            string chosen = animations[Random.Range(0, animations.Length)];
            anim.CrossFadeInFixedTime(anim.GetLayerName(0) + "." + chosen, .25f, 0);
            yield return new WaitForSeconds(.25f);

            float length = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(length + Random.Range(.1f, .4f));
        }
    }
}
