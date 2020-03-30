using System;
using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour {
	[SerializeField] AudioSource audioSource;
	[SerializeField] float fadeTime = 1.0f;

	Coroutine currSource;
	Coroutine currVolumeChanger;

	void Start() {

	}

	void OnUserEnterArea(AudioClip clip) {
		if (currVolumeChanger != null)
			StopCoroutine(currVolumeChanger);
		currVolumeChanger = StartCoroutine(ChangeVolume(0.0f, fadeTime, ()=> {
			audioSource.clip = clip;
			currVolumeChanger = StartCoroutine(ChangeVolume(1.0f, fadeTime));
		}));
	}

	void ProcessLoop() {
		LeanTween.delayedCall(audioSource.clip.length - fadeTime * 2, () => {
			if (currVolumeChanger != null)
				StopCoroutine(currVolumeChanger);
			currVolumeChanger = StartCoroutine(ChangeVolume(0.0f, fadeTime, () => {
				currVolumeChanger = StartCoroutine(ChangeVolume(1.0f, fadeTime));
				ProcessLoop();
			}));
		});
	}

	IEnumerator ChangeVolume(float newVolume, float totalTime, Action endCallback = null) {
		float startVolume = audioSource.volume;
		float volumeDelta = newVolume - audioSource.volume;
		float passedTime = Time.deltaTime;

		while (passedTime < totalTime) {
			audioSource.volume = startVolume + volumeDelta * passedTime / totalTime;

			passedTime += Time.deltaTime;
			yield return null;
		}

		audioSource.volume = newVolume;

		endCallback?.Invoke();
	}
}
