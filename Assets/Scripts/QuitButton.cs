using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    private AudioSource audioSource;
    public void Quit()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        StartCoroutine(GoQuit());
    }

    IEnumerator GoQuit()
    {
        yield return new WaitForSeconds(0.1f);
        Application.Quit();
    }
}
