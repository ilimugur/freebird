using System;
using UnityEngine;
using System.Collections;
[Serializable]
public class AcrobacyEvent
{
	public enum AcrobacyEventType
	{
		StartVerticalStance, //angle>=80 && angle <= 100
		EndVerticalStance,//angle < 80 || angle <100
		StartLevelFlight,//angle<10 && angle>=-10
		EndLevelFlight,//angle>10 || angle <-10
		StartFreeDescent,//that 30° downward free flight
		EndFreeDescent,//that 30° downward free flight
		Loop,
		HeadsDown,
		TailContactsGround,
		TailLeavesGround,
		WheelTouchesGround,
		WheelLeavesGround,
		ReachedSpace,


	}
	public Vector3 Position;
	public float Time;
	public AcrobacyEventType Type;


}
