using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    [SerializeField] List<AudioSource> music;
    [SerializeField] List<AudioClip> sounds;
    [SerializeField] AudioSource audioSource;
    int playingMusic=-1;

    public void fadeInMusic(int selectedMusic, float fadeInTime, float endVolume) {

        if (playingMusic>=0) {
            StartCoroutine (AudioFade.FadeOutThenFadeIn(music[playingMusic], fadeInTime, music[selectedMusic], endVolume));
        }
        else {
            StartCoroutine (AudioFade.FadeIn(music[selectedMusic], fadeInTime, endVolume));
        }
        playingMusic=selectedMusic;
    }

    public void fadeOutMusic(int selectedMusic, float fadeOutTime) {
        StartCoroutine (AudioFade.FadeOut(music[selectedMusic], fadeOutTime));
    }

    public void playSound(int selectedSound, float volume) {
        audioSource.PlayOneShot(sounds[selectedSound], volume);
    }
}
