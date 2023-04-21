using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnStart : MonoBehaviour
{
    [SerializeField] private List<AudioClip> bgmList = new List<AudioClip>();
    void Start()
    {
        int randomIndex = Random.Range(0, bgmList.Count);
        AudioClip randomClip = bgmList[randomIndex];
        PersistentAudio.Instance.PlaySound(randomClip);
    }
}
