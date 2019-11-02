﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MoreMountains.NiceVibrations;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
	public LandingStrip LandingStripPrefab;
	private LandingStrip LandingStrip;
	public PlaneController PlaneControllerPrefab;
	private PlaneController PlaneController;

	public Vector3 LandingStripPosition = new Vector3(0,4,0);

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

		UIManager.Instance.ShowStartScreen(0);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_START, StartLevel);
		EventManager.Instance.StartListening(Constants.EVENT_LEVEL_RESTART, RestartLevel);

		Load();

		LandingStrip = Instantiate(LandingStripPrefab, LandingStripPosition,Quaternion.identity,this.transform);
		PlaneController = Instantiate(PlaneControllerPrefab, LandingStrip.PlaneSpawnPosition.position, 
			Quaternion.identity, this.transform);

		PlaneController.SpawnLocation = LandingStrip.PlaneSpawnPosition.position;
	}

	void Start()
	{
		
	}

	public void RegisterGameOver()
	{
		StartCoroutine(TriggerGameOver());
	}

	private void StartLevel()
	{
		StartCoroutine(StartGameCo());
	}

	private void RestartLevel()
	{

		StartCoroutine(StartGameCo());
	}

	private IEnumerator StartGameCo()
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("Starting Game");
		Analytics.CustomEvent("StartGame");

		DOTween.To(() => PlaneController.CurrentHorizontalSpeed, x => PlaneController.CurrentHorizontalSpeed = x, PlaneController.TargetHorizontalSpeed, 2f);
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
		Time.timeScale = 1;
		RoundEndTime = Time.time;

		yield return new WaitForSeconds(0.35f);
		Save();
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


