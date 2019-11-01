using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
	public static int BallLayer = 9;
	public static int ScoreTriggerLayer = 10;
	public static int ObstaclesLayer = 11;
	public static int BallTriggerLayer = 12;
	public static int CollectiblesLayer = 13;
	public static int NoRenderLayer = 14;
	public static int NoCollisionLayer = 15;
	public static int GroundLayer = 16;
	public static int CollideOnlyWithGroundLayer = 17;

	public static readonly string EVENT_UPDATE_SCORE = "ev_usc";
	public static readonly string EVENT_UPDATE_HIGHSCORE = "ev_uhsc";
	public static readonly string EVENT_SET_PROGRESSBAR = "ev_setpb";
	public static readonly string EVENT_RESET_PROGRESSBAR = "ev_rstpb";

	public static readonly string EVENT_START_GAME = "ev_stgm";
	public static readonly string EVENT_START_LEVEL = "ev_stlv";
	public static readonly string EVENT_START_NEXT_LEVEL = "ev_stnlv";
	public static readonly string EVENT_GAME_OVER = "ev_gov";
	public static readonly string EVENT_GAME_COMPLETED = "ev_gcd";
	public static readonly string EVENT_GAME_CONTINUE_AFTER_GAME_OVER = "ev_gcnt";
	public static readonly string EVENT_LEVEL_COMPLETED = "ev_lcmp";

	public static readonly string EVENT_INCREMENT_SHOT_POSITION = "ev_incsp";
	public static readonly string EVENT_DECREMENT_SHOT_POSITION = "ev_dccsp";
	public static readonly string EVENT_RESTART_LEVEL = "ev_rstlv";
	//public static readonly string EVENT_FINISH_LEVEL = "ev_fnslv";
	public static readonly string EVENT_SET_SHOT_POSITION = "ev_setsp";

	public static readonly string EVENT_BALL_BOUNCED_ON_GROUND="ev_bog";
	public static readonly string EVENT_BALL_GROUND_BOUNCE_COMPLETED="ev_bgbc";
	public static readonly string EVENT_BALL_READY_AT_SHOT_POSITION="ev_brfs";
	public static readonly string EVENT_BALL_START_SHOT="ev_stsht";
	public static readonly string EVENT_BALL_SCORED_SHOT="ev_scs";
	public static readonly string EVENT_BALL_MISSED_SHOT="ev_miss";
	public static readonly string EVENT_BALL_HIT_HOOP_PERIMETER="ev_bhhp";
	public static readonly string EVENT_BALL_HIT_OBSTACLE = "ev_bhobs";
	public static readonly string EVENT_BALL_HIT_BONUS_OBJECT="ev_bhbo";
}
