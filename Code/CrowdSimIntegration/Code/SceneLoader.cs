using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int evacuationBuildIndex;
    [SerializeField] private int staircaseBuildIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadEvacautionLevel()
    {
        SceneManager.LoadScene(evacuationBuildIndex);
    }

    public void LoadStaircaseLevel()
    {
        SceneManager.LoadScene(staircaseBuildIndex);
    }
}
