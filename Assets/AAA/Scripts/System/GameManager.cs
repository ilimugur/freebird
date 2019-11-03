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

	private float _fuel;
	public float Fuel
	{
		get { return _fuel; }
		private set
		{
			_fuel = value;
			EventManager.Instance.TriggerEvent(Constants.EVENT_SET_PROGRESSBAR, _fuel/Constants.FuelCapacity);
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
		EventManager.Instance.StartListening(Constants.EVENT_GAIN_FUEL, (float value) => OnGainFuel(value));

		LandingStrip = Instantiate(LandingStripPrefab, LandingStripPosition, Quaternion.identity, this.transform);
		PlaneController = Instantiate(PlaneControllerPrefab, LandingStrip.PlaneSpawnPosition.position,
		                              Quaternion.identity, this.transform);

		PlaneController.SpawnLocation = LandingStrip.PlaneSpawnPosition.position;

		RestartLevel();
	}

	private void OnLevelCompleted()
	{
		EventManager.Instance.TriggerEvent(Constants.EVENT_UPDATE_SCORE, _score);
		_score = 0;
	}

	public void OnGainFuel(float value)
	{
		Fuel += value;
	}

	private void LoadLevel()
	{
		StartCoroutine(LoadLevelCo());
	}

	private IEnumerator LoadLevelCo()
	{
		yield return new WaitUntil(() => !Input.GetMouseButton(0));
		yield return null;
		yield return null;
		yield return null;

		GameStartTime = 0f;
		GameEndTime = 0f;
		RoundStartTime = 0f;
		RoundEndTime = 0f;
		IsGameFinished = false;
		IsGameFinishScreenDisplayed = false;

		CameraDirector.Instance.transform.position = PlaneController.SpawnLocation;
		PlaneController.PlaceToSpawnLocation();
		UIManager.Instance.ShowStartScreen(0);
		Time.timeScale = 1;
	}



	private void StartLevel()
	{
		Score = 0;
		Fuel = Constants.InitialFuel;
		StartCoroutine(StartLevelCo());
	}

	private void RestartLevel()
	{
		Score = 0;
		Fuel = Constants.InitialFuel;
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_LOAD);
		// StartCoroutine(StartGameCo());
	}

	private IEnumerator StartLevelCo()
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("Starting Game");
		Analytics.CustomEvent("StartGame");

		GameStartTime = Time.time;
		GameEndTime = 0f;
	}

	public void StartRound()
	{
		EventManager.Instance.TriggerEvent(Constants.EVENT_ENABLE_CONTROLS);
		RoundStartTime = Time.time;
		RoundEndTime = 0f;
	}

	private IEnumerator TriggerGameOver()
	{
		Debug.Log("Game Over");
		RoundEndTime = Time.time;
		GameEndTime = Time.time;

		// Wait before end screen.
		yield return new WaitForSeconds(2f);

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
