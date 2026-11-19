using UnityEngine;

// Background sound: birds chirping somewhere outside the bedroom window, and soft music.
// The music follows the Music switch in "Your cozy corner".
public class AmbientAudio : MonoBehaviour
{
    public AudioClip birds;
    public AudioClip music;
    public Vector3 birdsAt = new Vector3(2.25f, 2.6f, 7f);   // outside, past the window
    [Range(0f, 1f)] public float birdsVolume = 0.35f;
    [Range(0f, 1f)] public float musicVolume = 0.22f;
    public float fadeIn = 3f;

    AudioSource birdSource, musicSource;

    void Start()
    {
        if (birds)
        {
            var go = new GameObject("Birds");
            go.transform.SetParent(transform, false);
            go.transform.position = birdsAt;
            birdSource = go.AddComponent<AudioSource>();
            birdSource.clip = birds;
            birdSource.loop = true;
            birdSource.spatialBlend = 1f;                     // comes from outside, so it fades as you walk away
            birdSource.rolloffMode = AudioRolloffMode.Logarithmic;
            birdSource.minDistance = 2f;
            birdSource.maxDistance = 25f;
            birdSource.spread = 90f;                          // a soft wash, not one loud bird
            birdSource.dopplerLevel = 0f;
            birdSource.volume = 0f;
            birdSource.Play();
        }
        if (music)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;                    // just in your ears, like a soundtrack
            musicSource.volume = 0f;
            musicSource.Play();
        }
    }

    void Update()
    {
        float k = 1f - Mathf.Exp(-Time.deltaTime * 3f / Mathf.Max(0.1f, fadeIn));
        if (birdSource) birdSource.volume = Mathf.Lerp(birdSource.volume, birdsVolume, k);
        if (musicSource) musicSource.volume = Mathf.Lerp(musicSource.volume, UIPrefs.Music ? musicVolume : 0f, k * 2f);
    }
}
