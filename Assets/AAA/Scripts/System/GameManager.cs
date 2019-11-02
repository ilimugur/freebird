using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
	void Awake()
	{
		Application.targetFrameRate = 60;

		UIManager.Instance.ShowStartScreen(0);
		EventManager.Instance.StartListening(Constants.EVENT_GAME_START, StartGame);

		Load();

	}

	void Start()
	{
		
	}

	public void RegisterGameOver()
	{
		StartCoroutine(TriggerGameOver());
	}

	private void StartGame()
	{
		StartCoroutine(StartGameCo());
	}

	private IEnumerator StartGameCo()
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("Starting Game");
		Analytics.CustomEvent("StartGame");

		//Game.StartCurrentLevel();
		//EventManager.Instance.StopListening(GameConstants.EVENT_START_GAME, StartGame);
		//EventManager.Instance.StartListening("LEVEL_UP", Game.Instance.OnLevelUp);
		//EventManager.Instance.StartListening("SCORE_INCREMENT", Game.Instance.OnScoreIncremented);
	}

	private IEnumerator TriggerGameOver()
	{
		Debug.Log("Game Over");
		//EventManager.Instance.StopListening("LEVEL_UP", Game.Instance.OnLevelUp);
		//EventManager.Instance.StopListening("SCORE_INCREMENT", Game.Instance.OnScoreIncremented);

		//CamShaker.Shake(GameParameters.CameraShakeDuration, GameParameters.CameraShakeIntensity);
		Time.timeScale = 1;

		yield return new WaitForSeconds(0.35f);
		Save();
		//EventManager.Instance.TriggerEvent("GAME_OVER");
		//EventManager.Instance.StartListening("START_GAME", StartGame);
	}

	void OnApplicationQuit()
	{
		Save();
	}

	void OnApplicationFocus()
	{
		
	}

	private void Save()
	{
		//Game.Save();
	}

	private void Load()
	{
		Debug.Log("Loading Game");
		Time.timeScale = 1;
		//if (Game == null)
		//{
		//	var obj = new GameObject("Game");
		//	Game = obj.AddComponent<Game>();
		//}
		//Game.Load();
	}

}


