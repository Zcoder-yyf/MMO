using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class TerrainAudioController : MonoBehaviour
{
    private AudioSource[] audioSources;

    private void Start()
    {
        audioSources = GetComponentsInChildren<AudioSource>();
        EventSystem.AddTypeEventListener<MusicVolumeChangeEvent>(OnMusicVolumeChangeEvent);
    }

    private void OnMusicVolumeChangeEvent(MusicVolumeChangeEvent arg)
    {
        foreach (AudioSource item in audioSources)
        {
            item.volume = AudioSystem.BGVolume;
        }
    }
}
