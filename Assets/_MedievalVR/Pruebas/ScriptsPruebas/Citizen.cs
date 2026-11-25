using UnityEngine;

public class Citizen : MonoBehaviour
{
    public GameObject[] citizenPrefab;
    public Material[] citizenMaterial; //Instanciar el material para que cada ciudadano tenga un color random
    public Transform arenaCenter;

    public int rows = 5;
    public int columns = 10;
    public float spacing = 1.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateCitizens()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 pos = transform.position + new Vector3(j * spacing, i * 0.5f, i * spacing);

                GameObject randomPrefab = citizenPrefab[Random.Range(0, citizenPrefab.Length)];
                GameObject npc = Instantiate(randomPrefab, pos, Quaternion.identity, transform);

                Vector3 lookTarget = new Vector3(arenaCenter.position.x, npc.transform.position.y, arenaCenter.position.z);
                npc.transform.LookAt(lookTarget);

                Material chosenMat = citizenMaterial[Random.Range(0, citizenMaterial.Length)];
                Renderer[] childRenderers = npc.GetComponentsInChildren<Renderer>();
                foreach (Renderer rend in childRenderers)
                {
                    rend.sharedMaterial = chosenMat;
                }
            }
        }
    }

    void DressCitizen()
    {

    }

    void AnimateCitizen()
    {
    }




}
