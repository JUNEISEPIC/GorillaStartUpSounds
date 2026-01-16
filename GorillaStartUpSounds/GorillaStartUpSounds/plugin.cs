using BepInEx;
using BepInEx.Configuration;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[BepInPlugin("skellon.startupsounds", "Gorilla Startup Sounds", "1.0.0")]
public class GorillaStartUpSounds : BaseUnityPlugin
{
    internal static ConfigEntry<string> SoundPath = null!;
    private void Awake()
    {
        GorillaTagger.OnPlayerSpawned(() => StartCoroutine(PlayStartupSound()));
        SoundPath = Config.Bind(new ConfigDefinition("General", "Sound Path"), "null", new("The path to the MP3 that gets played on startup"));
    }
    private IEnumerator PlayStartupSound()
    {
        yield return new WaitForSeconds(1f);
        if (SoundPath.Value == (string)SoundPath.DefaultValue || !File.Exists(SoundPath.Value))
        {
            yield return LoadAndPlay("https://raw.githubusercontent.com/JUNEISEPIC/GorillaStartUpSounds/main/Main-StartUp-Sound.mp3");
            yield break;
        }

        yield return LoadAndPlay("file://" + SoundPath.Value);
    }
    private IEnumerator LoadAndPlay(string url)
    {
        UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);

            if (clip is null) yield break;
            VRRig.LocalRig.leftHandPlayer.GTPlayOneShot(clip);
        }
    }
}
