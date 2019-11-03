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
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_START_NEXT, RestartLevel);

		LandingStrip = Instantiate(LandingStripPrefab, LandingStripPosition, Quaternion.identity, this.transform);
		PlaneController = Instantiate(PlaneControllerPrefab, LandingStrip.PlaneSpawnPosition.position,
		                              Quaternion.identity, this.transform);

		PlaneController.SpawnLocation = LandingStrip.PlaneSpawnPosition.position;

		RestartLevel();
	}

	public void RegisterGameOver()
	{
		StartCoroutine(TriggerGameOver());
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

		CameraDirector.Instance.transform.position = PlaneController.SpawnLocation;
		PlaneController.PlaceToSpawnLocation();
		UIManager.Instance.ShowStartScreen(0);
		Time.timeScale = 1;
	}

	private void StartLevel()
	{
		StartCoroutine(StartGameCo());
	}

	private void RestartLevel()
	{
		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_LOAD);
	}

	private IEnumerator StartGameCo()
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("Starting Game");
		Analytics.CustomEvent("StartGame");

		IsGameEndInformed = false;
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

		EventManager.Instance.TriggerEvent(Constants.EVENT_LEVEL_COMPLETED);
		yield break;
	}

	private bool IsGameEndInformed;

	public void InformGameEnd()
	{
		if (!IsGameEndInformed)
		{
			IsGameEndInformed = true;
			Invoke(nameof(DelayedGameEnd), 2f);
		}
	}

	private void DelayedGameEnd()
	{
		RegisterGameOver();
	}
}
