using System;
using System.Collections;
using System.Collections.Generic;
//using MoreMountains.NiceVibrations;
using UnityEngine;

public class HapticsManager : Singleton<HapticsManager>
{
	[Header("Haptics and Failover Vibration Settings")]
	public HapticDefinition PlaneCrashed;
	public HapticDefinition UIMessageShown;
	public HapticDefinition GameStarted;
    public HapticDefinition OutOfFuel;

	void Awake()
	{
        /*
		if (MMVibrationManager.HapticsSupported())
			MMVibrationManager.iOSInitializeHaptics();
        */

		SetListeners();
	}

	private void SetListeners()
	{
		if (EventManager.Instance)
		{
			EventManager.Instance.StartListening(Constants.EVENT_GAME_START, OnGameStarted);
			EventManager.Instance.StartListening(Constants.EVENT_UI_MESSAGE, OnUIMessageShown);
			EventManager.Instance.StartListening(Constants.EVENT_PLANE_CRASHED, OnPlaneCrashed);
			EventManager.Instance.StartListening(Constants.EVENT_OUT_OF_FUEL, OnOutOfFuel);
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
			EventManager.Instance.StopListening(Constants.EVENT_GAME_START, OnGameStarted);
			EventManager.Instance.StartListening(Constants.EVENT_UI_MESSAGE, OnUIMessageShown);
			EventManager.Instance.StopListening(Constants.EVENT_PLANE_CRASHED, OnPlaneCrashed);
		}
	}


	private void OnGameStarted()
	{
		Vibrate(GameStarted);
	}

	private void OnUIMessageShown()
	{
		Vibrate(UIMessageShown);
	}

	private void OnPlaneCrashed()
	{
		Vibrate(PlaneCrashed);
	}

	private void OnOutOfFuel()
	{
		Vibrate(OutOfFuel);
	}

	public void Vibrate(HapticDefinition def)
	{
        /*
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
        */
	}

	void OnDestroy()
	{
		Unsetlisteners();
        /*
		if (MMVibrationManager.HapticsSupported())
			MMVibrationManager.iOSReleaseHaptics();
        */
	}
}
