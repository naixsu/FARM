using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    public void Retry()
    {
        StartCoroutine(RetryLevel());  
    }

    public void Quit()
    {
        StartCoroutine(QuitLevel()); 
    }

    public void NextLevel()
    {
        StartCoroutine(GoNextLevel());
    }

    public void PreviousLevel()
    {
        StartCoroutine(GoPreviousLevel());
    }

    IEnumerator GoPreviousLevel()
    {
        yield return new WaitForSeconds(0.1f);
        int prevSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        Debug.Log(prevSceneIndex);
        if (prevSceneIndex != 0)
        {
            SceneManager.LoadScene(prevSceneIndex);  
        }
        else
        {
            int lastSceneIndex = 3;
            SceneManager.LoadScene(lastSceneIndex);
        }
    }

    IEnumerator GoNextLevel()
    {
        yield return new WaitForSeconds(0.1f);
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        Debug.Log(nextSceneIndex);
        if (nextSceneIndex > 3)
        {
            int firstSceneIndex = 1;
            SceneManager.LoadScene(firstSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    IEnumerator QuitLevel()
    {
        yield return new WaitForSeconds(0.1f);
        Loader.Load(Loader.Scene.End_Scene);
    }
    IEnumerator RetryLevel()
    {
        yield return new WaitForSeconds(0.1f);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
