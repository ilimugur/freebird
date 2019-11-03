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
	public static int TriggersLayer = 13;

	public static readonly string EVENT_UI_MESSAGE = "ev_uimsg";

	public static readonly string EVENT_UPDATE_SCORE = "ev_usc";
	public static readonly string EVENT_INCREMENT_SCORE = "ev_incrscr";
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
	public static readonly string EVENT_PLANE_CRASHED = "ev_plcr";

	public static readonly string EVENT_GAIN_FUEL = "ev_gainfuel";
	public static readonly string EVENT_COLLECT_OBJECT = "ev_collobj";
	public static readonly string EVENT_JETTISON_OBJECT = "ev_jtsobj";

	public static readonly string EVENT_ACROBACY_START_VERTICAL_STANCE = "ev_stvertstan";
	public static readonly string EVENT_ACROBACY_END_VERTICAL_STANCE = "ev_endvertstan";
	public static readonly string EVENT_ACROBACY_START_LEVEL_FLIGHT = "ev_stlvlflg";
	public static readonly string EVENT_ACROBACY_END_LEVEL_FLIGHT = "ev_endlvlflg";
	public static readonly string EVENT_ACROBACY_START_FREE_DESCENT = "ev_stfrdsc";
	public static readonly string EVENT_ACROBACY_END_FREE_DESCENT = "ev_endfrdsc";
	public static readonly string EVENT_ACROBACY_COMPLETE_LOOP = "ev_cmploop";
	public static readonly string EVENT_ACROBACY_TAIL_CONTACTED_GROUND = "ev_tailcntgnd";
	public static readonly string EVENT_ACROBACY_TAIL_LEAVED_GROUND = "ev_taillvdgnd";
	public static readonly string EVENT_ACROBACY_WHEEL_CONTACTED_GROUND = "ev_whlcntgnd";
	public static readonly string EVENT_ACROBACY_WHEEL_LEAVED_GROUND = "ev_whllvdgnd";
	public static readonly string EVENT_ACROBACY_REACHED_SPACE = "ev_rchdspc";
	//HeadsDown,

	public static float FuelPerCrate = 25f;
	public static float InitialFuel = 200f;
	public static float FuelCapacity = 500f;
}
