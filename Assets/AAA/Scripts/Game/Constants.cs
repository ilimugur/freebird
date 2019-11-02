using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
	public static int PlaneLayer = 8;
	public static int GroundLayer = 9;
	public static int ObstacleLayer = 10;
	public static int CollectibleLayer = 11;
	public static int NoCollisionLayer = 12;

	public static readonly string EVENT_UPDATE_SCORE = "ev_usc";
	public static readonly string EVENT_UPDATE_HIGHSCORE = "ev_uhsc";
	public static readonly string EVENT_SET_PROGRESSBAR = "ev_setpb";
	public static readonly string EVENT_RESET_PROGRESSBAR = "ev_rstpb";

	public static readonly string EVENT_GAME_START = "ev_stgm";
	public static readonly string EVENT_GAME_OVER = "ev_gov";
	public static readonly string EVENT_GAME_COMPLETED = "ev_gcd";
	public static readonly string EVENT_GAME_CONTINUE_AFTER_GAME_OVER = "ev_gcnt";
	public static readonly string EVENT_LEVEL_START = "ev_stlv";
	public static readonly string EVENT_LEVEL_START_NEXT = "ev_stnxlv";
	public static readonly string EVENT_LEVEL_RESTART = "ev_rstlv";
	public static readonly string EVENT_LEVEL_COMPLETED = "ev_lcmp";
	public static readonly string EVENT_ENABLE_CONTROLS = "ev_enct";


	public static readonly string EVENT_COLLECT_OBJECT = "ev_collobj";
	public static readonly string EVENT_JETTISON_OBJECT = "ev_jtsobj";

}
