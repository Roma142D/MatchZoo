using System;
using System.Collections;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.Audio;

namespace Sound
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioSource _audioSourceSFX;
        [SerializeField] private AudioSource _audioSourceMusic;
        [SerializeField] private float _musicTimer;
        [SerializeField] private AudioClip[] _BGMusicClips;
        [SerializeField] private SFXClips _sfxClips;
        private float _currentMusicTimer;
        private float currentVolume;

        private void Start()
        {
            StartCoroutine(PlayRandomAudio());
            _currentMusicTimer = _musicTimer;

            UIManager.Instance.settingsTab.MasterVolumeSlider.onValueChanged.AddListener(ChangeAllSoundsVolume);
            var soundValue = PlayerPrefs.GetFloat(GlobalData.SFX_VOLUME, 0f);
            UIManager.Instance.settingsTab.MasterVolumeSlider.value = soundValue;

            UIManager.Instance.settingsTab.MusicVolumeSlider.onValueChanged.AddListener(ChangeMusicSoundValue);
            var musicValue = PlayerPrefs.GetFloat(GlobalData.MUSIC_VOLUME, 0f);
            UIManager.Instance.settingsTab.MusicVolumeSlider.value = musicValue;
        }
        private void Update()
        {
            _currentMusicTimer -= Time.deltaTime;
            if (_currentMusicTimer <= 0)
            {
                _currentMusicTimer = _musicTimer;
                StartCoroutine(PlayRandomAudio());
            }
        }
        private IEnumerator PlayRandomAudio()
        {
            if (_audioSourceMusic.clip != null)
            {
                yield return new WaitUntil(() => _audioSourceMusic.time >= _audioSourceMusic.clip.length - 0.5f);
                var currentClip = _BGMusicClips.Where(clip => clip == _audioSourceMusic.clip);
                _audioSourceMusic.clip = _BGMusicClips.Except(currentClip).ToArray()[UnityEngine.Random.Range(0, _BGMusicClips.Length - 1)];
            }
            else
            {
                _audioSourceMusic.clip = _BGMusicClips[UnityEngine.Random.Range(0, _BGMusicClips.Length)];
            }
            _audioSourceMusic.Play();
        }
        public void OnButtonClickSoundPlay()
        {
            PlaySound(GlobalData.AudioClipType.OnButtonClick);
        }
        public void OnTileClickSoundPlay()
        {
            PlaySound(GlobalData.AudioClipType.OnTileClick);
        }
        public void PlaySound(GlobalData.AudioClipType audioClipType)
        {
            switch (audioClipType)
            {
                case GlobalData.AudioClipType.OnButtonClick :
                    _audioSourceSFX.clip = _sfxClips.OnUIButtonClickSound;
                    _audioSourceSFX.Play();
                    break;
                case GlobalData.AudioClipType.OnTileClick :
                    _audioSourceSFX.clip = _sfxClips.OnTileClickSound;
                    _audioSourceSFX.Play();
                    break;
                case GlobalData.AudioClipType.OnMatch :
                    _audioSourceSFX.clip = _sfxClips.OnMatchSound;
                    _audioSourceSFX.Play();
                    break;
                case GlobalData.AudioClipType.OnWin :
                    //_audioSourceMusic.clip = _sfxClips.OnWinSound;
                    _audioSourceMusic.PlayOneShot(_sfxClips.OnWinSound);
                    break;
                case GlobalData.AudioClipType.OnLose :
                    //_audioSourceMusic.clip = _sfxClips.OnLoseSound;
                    _audioSourceMusic.PlayOneShot(_sfxClips.OnLoseSound);
                    break;
                case GlobalData.AudioClipType.Explosion :
                    //_audioSourceSFX.clip = _sfxClips.ExplosionSound;
                    _audioSourceSFX.PlayOneShot(_sfxClips.ExplosionSound);
                    break;
                
            }
        }

        public void ToggleSound(string groupeName)
        {
            switch (groupeName)
            {
                case GlobalData.MASTER_VOLUME :
                    if (_audioMixer.GetFloat(GlobalData.MASTER_VOLUME, out currentVolume))
                    {
                        if (currentVolume > -60f)
                        {
                            ChangeAllSoundsVolume(-60f);
                            UIManager.Instance.settingsTab.MasterVolumeSlider.value = -60f;
                            //_settingsTab.MasterSoundButtonImage.sprite = _settingsTab.SoundOffSprite;
                        }
                        else
                        {
                            ChangeAllSoundsVolume(0f);
                            UIManager.Instance.settingsTab.MasterVolumeSlider.value = 0f;
                            //_settingsTab.MasterSoundButtonImage.sprite = _settingsTab.SoundOnSprite;
                        }
                        //_audioMixer.SetFloat(GlobalData.MASTER_VOLUME, currentVolume > -80f ? -80f : 0);
                    }
                    break;

                case GlobalData.MUSIC_VOLUME :
                    if (_audioMixer.GetFloat(GlobalData.MUSIC_VOLUME, out currentVolume))
                    {
                        if (currentVolume > -60f)
                        {
                            ChangeMusicSoundValue(-60f);
                            UIManager.Instance.settingsTab.MusicVolumeSlider.value = -60f;
                            //_settingsTab.MusicButtonImage.sprite = _settingsTab.MusicOffSprite;
                        }
                        else
                        {
                            ChangeMusicSoundValue(0f);
                            UIManager.Instance.settingsTab.MusicVolumeSlider.value = 0f;
                            //_settingsTab.MusicButtonImage.sprite = _settingsTab.MusicOnSprite;
                        }
                    }
                    break;
                
            }
           
        }
        public void ChangeAllSoundsVolume(float value)
        {
            //var value = _settingsTab.MasterVolumeSlider.value;
            _audioMixer.SetFloat(GlobalData.MASTER_VOLUME, value);

            UIManager.Instance.settingsTab.MasterSoundButtonImage.sprite = value <= -60f ? UIManager.Instance.settingsTab.SoundOffSprite 
                                                                                        : UIManager.Instance.settingsTab.SoundOnSprite;
            
            PlayerPrefs.SetFloat(GlobalData.MASTER_VOLUME, value);
            PlayerPrefs.Save();
        }
        public void ChangeMusicSoundValue(float value)
        {
            _audioMixer.SetFloat(GlobalData.MUSIC_VOLUME, value);

            UIManager.Instance.settingsTab.MusicButtonImage.sprite = value <= -60f ? UIManager.Instance.settingsTab.MusicOffSprite 
                                                                                : UIManager.Instance.settingsTab.MusicOnSprite;
            
            PlayerPrefs.SetFloat(GlobalData.MUSIC_VOLUME, value);
            PlayerPrefs.Save();
        }

        [Serializable]
        public struct SFXClips
        {
            public AudioClip OnUIButtonClickSound;
            public AudioClip OnTileClickSound;
            public AudioClip OnMatchSound;
            public AudioClip ExplosionSound;
            public AudioClip OnWinSound;
            public AudioClip OnLoseSound;
        }
    }
}
