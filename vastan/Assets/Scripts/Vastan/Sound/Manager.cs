using UnityEngine;

namespace Vastan.Sound
{
	class Manager : MonoBehaviour
	{
		bool server_mode = false;
		int tempAudioId = 0;

		public Manager() {

		}

		public AudioSource PlayClipAt(AudioClip clip, Vector3 pos)
		{
			GameObject temp = new GameObject("TempAudio"+tempAudioId);
			tempAudioId++;
			temp.transform.position = pos;

			AudioSource aSource = temp.AddComponent<AudioSource>();
			aSource.clip = clip;
			aSource.spatialBlend = 1.0f;
			aSource.dopplerLevel = 1.2f;
			aSource.Play();
			Destroy(temp, clip.length);
			return aSource;
		}
	}
}