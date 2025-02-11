using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PearlPlanet : MonoBehaviour
{
    [SerializeField] private List<PearlCluster> _pearlClusters;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _missAudioClip;
    [SerializeField] private AudioClip _collectAudioClip;


    public void SetPearlClusters(List<PearlCluster> pearlClusters)
    {
        _pearlClusters = pearlClusters;
    }

    public void Collect()
    {
        _audioSource.clip = _collectAudioClip;
        _audioSource.Play();
    }

    public void Reject()
    {
        _audioSource.clip = _missAudioClip;
        _audioSource.Play();
    }
}
