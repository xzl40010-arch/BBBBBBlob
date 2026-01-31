using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneJump : MonoBehaviour
{
    public string targetScene;
    public float delayTime=0f;
    public void JumpToTargetScene()
    {
        if(string.IsNullOrEmpty(targetScene)) return;
        Invoke("LoadTargetScene", delayTime);
    }
    private void LoadTargetScene()
    {
        SceneManager.LoadScene(targetScene);
    }
    
}
