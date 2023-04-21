using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource _musicSource;
    public AudioSource villagerSource;
    public AudioSource tileSource;
    public AudioSource restart;
    public AudioSource end;

    private List<AudioClip> villagerSoundsList = new List<AudioClip>();
    private List<AudioClip> tillList = new List<AudioClip>();
    private List<AudioClip> untillList = new List<AudioClip>();
    private List<AudioClip> placeList = new List<AudioClip>();
    private List<AudioClip> breakList = new List<AudioClip>();
    private List<AudioClip> plantList = new List<AudioClip>();
    private List<AudioClip> steps = new List<AudioClip>();
    private List<AudioClip> _dSteps = new List<AudioClip>();

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        villagerSoundsList = villagerSource.GetComponent<VillagerSounds>().idleSounds;
        steps = villagerSource.GetComponent<VillagerSounds>().steps;
        _dSteps = villagerSource.GetComponent<VillagerSounds>().DSteps;
        tillList = tileSource.GetComponent<TileSounds>().tillTiles;
        untillList = tileSource.GetComponent<TileSounds>().untillTiles;
        placeList = tileSource.GetComponent<TileSounds>().placeBlocks;
        breakList = tileSource.GetComponent<TileSounds>().breakBlocks;
        plantList = tileSource.GetComponent<TileSounds>().plantSeeds;
        
    }

    public void PlaySound(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }


    public void PlayVillagerDeath()
    {
        AudioClip clip = villagerSource.GetComponent<VillagerSounds>().death;
        villagerSource.PlayOneShot(clip);
    }
    public void PlayRandomVillagerIdleOnSpawn()
    {
        int randomIndex = Random.Range(0, villagerSoundsList.Count);
        AudioClip randomClip = villagerSoundsList[randomIndex];
        villagerSource.clip = randomClip;
        villagerSource.Play();
    }
    public void PlayRandomVillagerIdle()
    {
        int randomIndex = Random.Range(0, villagerSoundsList.Count);
        AudioClip randomClip = villagerSoundsList[randomIndex];
        villagerSource.PlayOneShot(randomClip);
    }

    public void PlayRandomStep()
    {
        int randomIndex = Random.Range(0, steps.Count);
        AudioClip randomClip = steps[randomIndex];
        villagerSource.PlayOneShot(randomClip);
    }

    public void PlayRandomDStep()
    {
        int randomIndex = Random.Range(0, _dSteps.Count);
        AudioClip randomClip = _dSteps[randomIndex];
        villagerSource.PlayOneShot(randomClip);
    }

    public void PlayRandomTillTile()
    {
        int randomIndex = Random.Range(0, tillList.Count);
        AudioClip randomClip = tillList[randomIndex];
        tileSource.PlayOneShot(randomClip);
    }

    public void PlayRandomUnTillTile()
    {
        int randomIndex = Random.Range(0, untillList.Count);
        AudioClip randomClip = untillList[randomIndex];
        tileSource.PlayOneShot(randomClip);
    }
    public void PlayRandomPlaceBlock()
    {
        int randomIndex = Random.Range(0, placeList.Count);
        AudioClip randomClip = placeList[randomIndex];
        tileSource.PlayOneShot(randomClip);
    }
    public void PlayRandomBreakBlock()
    {
        int randomIndex = Random.Range(0, breakList.Count);
        AudioClip randomClip = breakList[randomIndex];
        tileSource.PlayOneShot(randomClip);
    }

    public void PlayRandomPlant()
    {
        int randomIndex = Random.Range(0, plantList.Count);
        AudioClip randomClip = plantList[randomIndex];
        tileSource.PlayOneShot(randomClip);
    }

    public void PlayHarvest()
    {
        AudioClip clip = tileSource.GetComponent<TileSounds>().harvest;
        tileSource.PlayOneShot(clip);
    }

    public void PlayRestart()
    {
        restart.PlayOneShot(restart.clip);
    }

    public void PlayEnd()
    {
        end.PlayOneShot(end.clip);
    }
    
    public void PlayHaggle()
    {
        AudioClip haggle = villagerSource.GetComponent<VillagerSounds>().haggle;
        villagerSource.PlayOneShot(haggle);
    }
}
