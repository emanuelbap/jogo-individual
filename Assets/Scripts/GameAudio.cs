using UnityEngine;

public class GameAudio : MonoBehaviour
{
    public AudioSource music;
    public AudioSource effects;
    public AudioClip startSound;
    public AudioClip hitSound;
    public AudioClip gameOverSound;
    public bool Muted { get; private set; }

    void Awake()
    {
        Muted = PlayerPrefs.GetInt("Muted", 0) == 1;
        ApplyMute();
    }
    public void StartMusic() { music.Stop(); music.Play(); }
    public void StopMusic() { music.Stop(); }
    public void PlayStart() { effects.PlayOneShot(startSound); }
    public void PlayHit() { effects.PlayOneShot(hitSound); }
    public void PlayGameOver() { effects.PlayOneShot(gameOverSound); }
    public void SetPaused(bool paused) { if (paused) music.Pause(); else music.UnPause(); }
    public void ToggleMute()
    {
        Muted = !Muted;
        ApplyMute();
        PlayerPrefs.SetInt("Muted", Muted ? 1 : 0);
        PlayerPrefs.Save();
    }
    void ApplyMute() { music.mute = Muted; effects.mute = Muted; }
}
