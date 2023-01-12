using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour {
    [SerializeField] List<AudioSource> music;
    [SerializeField] List<AudioClip> sounds;
    [SerializeField] AudioSource audioSource;
    int playingMusic=-1;
    public Scrollbar musicScrollbar;
    public Scrollbar sfxScrollbar;



    public void fadeInMusic(int selectedMusic, float fadeInTime, float endVolume) {

        if (playingMusic>=0) {
            StartCoroutine (AudioFade.FadeOutThenFadeIn(music[playingMusic], fadeInTime, music[selectedMusic], endVolume*musicScrollbar.value));
        }
        else {
            StartCoroutine (AudioFade.FadeIn(music[selectedMusic], fadeInTime, endVolume*musicScrollbar.value));
        }
        playingMusic=selectedMusic;
    }

    public void fadeOutMusic(int selectedMusic, float fadeOutTime) {
        StartCoroutine (AudioFade.FadeOut(music[selectedMusic], fadeOutTime));
    }

    public void playSound(int selectedSound, float volume) {
        audioSource.PlayOneShot(sounds[selectedSound], volume*sfxScrollbar.value);
    }

    public void setMusicVolume() {
        music[playingMusic].volume=musicScrollbar.value;
        Debug.Log("AudioSource Value set to "+musicScrollbar.value);
    }
    
    public void muteMusic() {
        music[playingMusic].volume=0f;
        StopAllCoroutines();
    }
}
