using UnityEngine;
using System.Collections;
 
public static class AudioFade {
 
    public static IEnumerator FadeOutThenFadeIn (AudioSource audioSourceOut, float fadeTime, AudioSource audioSourceIn, float endVolume)  {
        //Out
        float startVolume = audioSourceOut.volume;
        while (audioSourceOut.volume > 0) {
            audioSourceOut.volume -= startVolume * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        audioSourceOut.Stop();
        audioSourceOut.volume = startVolume;

        //In
        audioSourceIn.volume=0;
        audioSourceIn.Play();
        while (audioSourceIn.volume < endVolume) {
            audioSourceIn.volume += endVolume * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        audioSourceIn.volume = endVolume;
    }

    public static IEnumerator FadeOut (AudioSource audioSource, float fadeTime) {
        float startVolume = audioSource.volume;
 
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public static IEnumerator FadeIn (AudioSource audioSource, float fadeTime, float endVolume) { 
        audioSource.volume=0;
        audioSource.Play();
        while (audioSource.volume < endVolume) {
            audioSource.volume += endVolume * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        audioSource.volume = endVolume;
    }
 
}