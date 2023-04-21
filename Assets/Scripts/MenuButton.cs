using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    private AudioSource audioSource;
    public void MainMenu()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        StartCoroutine(GoToMainMenu());
    }

    IEnumerator GoToMainMenu()
    {
        yield return new WaitForSeconds(0.1f);
        Loader.Load(Loader.Scene.Start_Scene);
    }
}
