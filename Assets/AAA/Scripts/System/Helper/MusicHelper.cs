using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicHelper : MonoBehaviour
{
    public AudioClip MenuMusic;
    public AudioClip GameMusic;

    private AudioSource _menuSound;
    private AudioSource _gameSound;

    void Awake()
    {
        _menuSound = gameObject.AddComponent<AudioSource>();
        _menuSound.clip = MenuMusic;
        _menuSound.loop = true;

        _gameSound = gameObject.AddComponent<AudioSource>();
        _gameSound.clip = GameMusic;
        _gameSound.loop = true;
    }

    void Update()
    {
        if (!_menuSound.isPlaying && GameManager.Instance.IsGameStarted == false)
        {
            _menuSound.Play();
        }
        else if (_menuSound.isPlaying && GameManager.Instance.IsGameStarted == true)
        {
            _menuSound.Stop();
        }

        if (!_gameSound.isPlaying && GameManager.Instance.IsGameStarted == true && GameManager.Instance.IsGameFinished == false)
        {
            _gameSound.Play();
        }
        else if (_gameSound.isPlaying && GameManager.Instance.IsGameStarted == false)
        {
            _gameSound.Stop();
        }
    }
}