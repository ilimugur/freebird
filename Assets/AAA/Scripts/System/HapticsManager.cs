using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.NiceVibrations;
using UnityEngine;

public class HapticsManager : Singleton<HapticsManager>
{
	[Header("Haptics and Failover Vibration Settings")]
	public HapticDefinition ShotStarted;
	public HapticDefinition RegularScored;
	public HapticDefinition LuckyScored;
	public HapticDefinition PerfectScored;
	public HapticDefinition ShotMissed;
	public HapticDefinition BallHitObstacle;
	public HapticDefinition BallHitHoopPerimeter;
	public HapticDefinition BallHitGround;
	public HapticDefinition LevelCompleted;
	public HapticDefinition GameStarted;
	public HapticDefinition GameOver;

	void Awake()
	{
		if (MMVibrationManager.HapticsSupported())
			MMVibrationManager.iOSInitializeHaptics();

		SetListeners();
	}

	private void SetListeners()
	{
		if (EventManager.Instance)
		{
			EventManager.Instance.StartListening(Constants.EVENT_LEVEL_COMPLETED, OnLevelCompleted);
			EventManager.Instance.StartListening(Constants.EVENT_GAME_START, OnGameStarted);
			EventManager.Instance.StartListening(Constants.EVENT_GAME_OVER, OnGameOver);
		}
		else
		{
			Debug.LogError("Trying to start listening to events, but there is no EventManager!");
		}
	}

	private void Unsetlisteners()
	{
		if (EventManager.Instance)
		{
			EventManager.Instance.StopListening(Constants.EVENT_LEVEL_COMPLETED, OnLevelCompleted);
			EventManager.Instance.StopListening(Constants.EVENT_GAME_START, OnGameStarted);
			EventManager.Instance.StopListening(Constants.EVENT_GAME_OVER, OnGameOver);
		}
	}


	private void OnGameStarted()
	{
		Vibrate(GameStarted);
	}

	private void OnGameOver()
	{
		Vibrate(GameOver);
	}

	private void OnLevelCompleted()
	{
		Vibrate(LevelCompleted);
	}

	public void Vibrate(HapticDefinition def)
	{
		if (MMVibrationManager.HapticsSupported())
		{
			if(def!=null)
				MMVibrationManager.Haptic(def.HapticType);
		}
		else if (MMVibrationManager.Android())
		{
			if(def!=null)
				MMVibrationManager.AndroidVibrate((long)def.FailoverVibrationDuration, (int)def.FailoverVibrationStrength);
		}
	}

	void OnDestroy()
	{
		Unsetlisteners();
		if (MMVibrationManager.HapticsSupported())
			MMVibrationManager.iOSReleaseHaptics();
	}
}
