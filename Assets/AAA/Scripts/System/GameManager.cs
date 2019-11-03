using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
	public LandingStrip LandingStripPrefab;
	private LandingStrip LandingStrip;
	public PlaneController PlaneControllerPrefab;
	private PlaneController PlaneController;

	public Vector3 LandingStripPosition = new Vector3(0,4,0);

	private int _score;
	public int Score
	{
		get { return _score; }
		private set
		{
			_score = value;
			EventManager.Instance.TriggerEvent(Constants.EVENT_UPDATE_SCORE, _score);
		}
	}

	public bool IsGameStarted => GameStartTime > 0f;
	public bool IsRoundStarted => RoundStartTime > 0f;

	public float GameStartTime { get; private set; }
	public float GameEndTime { get; private set; }
	public float GamePassedTime
	{
		get
		{
			if (GameStartTime <= 0f)
			{
				return 0f;
			}
			if (GameEndTime > 0f)
			{
				return GameEndTime - GameStartTime;
			}
			return Time.time - GameStartTime;
		}
	}

	public float RoundStartTime { get; private set; }
	public float RoundEndTime { get; private set; }
	public float RoundPassedTime
	{
		get
		{
			if (RoundStartTime <= 0f)
			{
				return 0f;
			}
			if (RoundEndTime > 0f)
			{
				return RoundEndTime - RoundStartTime;
			}
			return Time.time - RoundStartTime;
		}
	}

	void Awake()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_LOAD, LoadLevel);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_START, StartLevel);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_RESTART, RestartLevel);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_COMPLETED, OnLevelCompleted);
		EventManager.Instance.StartListening(Constants.EVENT_INCREMENT_SCORE, OnIncrementScore);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_START_NEXT, RestartLevel);

		LandingStrip = Instantiate(LandingStripPrefab, LandingStripPosition, Quaternion.identity, this.transform);
		PlaneController = Instantiate(PlaneControllerPrefab, LandingStrip.PlaneSpawnPosition.position,
		                              Quaternion.identity, this.transform);

		PlaneController.SpawnLocation = LandingStrip.PlaneSpawnPosition.position;

		RestartLevel();
	}

	private void OnLevelCompleted()
	{
		Debug.Log("Level completed");
		EventManager.Instance.TriggerEvent(Constants.EVENT_UPDATE_SCORE, _score);
		_score = 0;
	}

	private void LoadLevel()
	{
		StartCoroutine(LoadLevelCo());
	}

	private IEnumerator LoadLevelCo()
	{
		Debug.Log("Loading level - Step 1");
		// Wait for the mouse button to be released. Otherwise, start screen will be passed immediately.
		yield return new WaitUntil(() => !Input.GetMouseButton(0));
		yield return null;
		yield return null;
		yield return null;
		Debug.Log("Loading level - Step 2");

		GameStartTime = 0f;
		GameEndTime = 0f;
		RoundStartTime = 0f;
		RoundEndTime = 0f;
		IsGameFinished = false;
		IsGameFinishScreenDisplayed = false;
		Score = 0;

		CameraDirector.Instance.transform.position = PlaneController.SpawnLocation;
		PlaneController.PlaceToSpawnLocation();
		UIManager.Instance.ShowStartScreen(0);
		Time.timeScale = 1;
	}



	private void StartLevel()
	{
		Debug.Log("Starting level");
		StartCoroutine(StartLevelCo());
	}

	private void RestartLevel()
	{
		Debug.Log("Restarting level");
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_LOAD);
		// StartCoroutine(StartGameCo());
	}

	private IEnumerator StartLevelCo()
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("Starting game");
		Analytics.CustomEvent("StartGame");

		GameStartTime = Time.time;
		GameEndTime = 0f;
	}

	public void StartRound()
	{
		if (IsRoundStarted)
			return;

		Debug.Log("Starting round");
		EventManager.Instance.TriggerEvent(Constants.EVENT_ENABLE_CONTROLS);
		RoundStartTime = Time.time;
		RoundEndTime = 0f;
	}

	private IEnumerator TriggerGameOver()
	{
		Debug.Log("Game over");
		RoundEndTime = Time.time;
		GameEndTime = Time.time;

		// Wait before end screen.
		yield return new WaitForSeconds(2f);

		Debug.Log("Displaying game over screen.");
		IsGameFinishScreenDisplayed = true;
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_COMPLETED);
	}

	void OnIncrementScore(int value)
	{
		Score += value;
	}

	public bool IsGameFinished { get; private set; }
	public bool IsGameFinishScreenDisplayed { get; private set; }

	public void InformGameEnd()
	{
		if (!IsGameFinished)
		{
			IsGameFinished = true;
			StartCoroutine(TriggerGameOver());
		}
	}
}
