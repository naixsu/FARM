using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    private AudioSource audioSource;
    public void StartGame()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();

        StartCoroutine(Wait());
        
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.1f);
        Loader.Load(Loader.Scene.GameScene_1);
    }
}
