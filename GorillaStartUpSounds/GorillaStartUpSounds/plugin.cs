using BepInEx;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using GorillaLocomotion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class GorillaStartUpSounds : BaseUnityPlugin
{
    private const string PluginGuid = "juneisepic.gorillatag.startupsounds";
    private const string PluginName = "GorillaStartUpSounds";
    private const string PluginVersion = "1.0.0";

    private const string SoundsFolderName = "sounds";
    private const string FallbackSoundUrl =
        "https://raw.githubusercontent.com/JUNEISEPIC/GorillaStartUpSounds/main/Main-StartUp-Sound.mp3";

    private static bool hasPlayed;

    private AudioSource audioSource;
    private GameObject audioObject;

    private void Awake()
    {
        if (hasPlayed)
        {
            Destroy(gameObject);
            return;
        }

        hasPlayed = true;
        DontDestroyOnLoad(gameObject);

        audioObject = new GameObject("GorillaStartUpSounds_Audio");
        DontDestroyOnLoad(audioObject);

        audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (GTPlayer.Instance == null)
            yield return null;

        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(PlayStartupSound());
    }

    private IEnumerator PlayStartupSound()
    {
        string pluginDir = Path.GetDirectoryName(Info.Location);
        string soundsDir = Path.Combine(pluginDir, SoundsFolderName);

        if (Directory.Exists(soundsDir))
        {
            string[] files = Directory.GetFiles(soundsDir, "*.mp3");
            if (files.Length > 0)
            {
                yield return StartCoroutine(LoadAndPlay("file://" + files[0]));
                yield break;
            }
        }

        yield return StartCoroutine(LoadAndPlay(FallbackSoundUrl));
    }

    private IEnumerator LoadAndPlay(string url)
    {
        UnityWebRequest req =
            UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

        yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
            yield break;
#else
        if (req.isNetworkError || req.isHttpError)
            yield break;
#endif

        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
        if (clip == null)
            yield break;

        audioSource.clip = clip;
        audioSource.Play();
    }
}
