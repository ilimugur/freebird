using UnityEngine;
using System.Collections;
//using MoreMountains.NiceVibrations;


[CreateAssetMenu(fileName = "Assets/AAA/Predefined Assets/Vibrations/New Haptic Definition", menuName = "Bouncy Balls/New Haptic Definition", order = 54)]

public class HapticDefinition : ScriptableObject
{
	//public HapticTypes HapticType =HapticTypes.None;
	public VibrationDurationDefType FailoverVibrationDuration = VibrationDurationDefType.None;
	public VibrationStrengthDefType FailoverVibrationStrength = VibrationStrengthDefType.None;

	public enum VibrationStrengthDefType
	{
		None=0,
		Light = 20,
		Medium = 40,
		Heavy = 80,
	}

	public enum VibrationDurationDefType
	{
		None=0,
		Light = 40,
		Medium = 120,
		Heavy = 255,
	}
}
