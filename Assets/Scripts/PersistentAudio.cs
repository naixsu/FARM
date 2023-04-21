using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentAudio : MonoBehaviour
{
    public static PersistentAudio Instance;
    [SerializeField] private AudioSource _musicSource;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }
}
