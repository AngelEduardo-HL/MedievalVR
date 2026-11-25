using UnityEngine;

public class Victory : MonoBehaviour
{
    public ParticleSystem[] fireworkParticles;
    public AudioSource[] victorySound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayFireworks()
    {
        foreach (var firework in fireworkParticles)
        {
            firework.Play();
        }

        foreach (var sound in victorySound)
        {
            sound.Play();
        }
    }

}
