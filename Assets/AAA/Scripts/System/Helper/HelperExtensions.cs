using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HelperExtensions
{
	public static class VectorExtensions
	{
		public static Vector3 WithX(this Vector3 vector, float value)
		{
			return new Vector3(value, vector.y, vector.z);
		}

		public static Vector3 WithY(this Vector3 vector, float value)
		{
			return new Vector3(vector.x, value, vector.z);
		}

		public static Vector3 WithZ(this Vector3 vector, float value)
		{
			return new Vector3(vector.x, vector.y, value);
		}

		public static Vector2 WithX(this Vector2 vector, float value)
		{
			return new Vector2(value, vector.y);
		}

		public static Vector2 WithY(this Vector2 vector, float value)
		{
			return new Vector2(vector.x, value);
		}
	}

	public static class ColorExtensions
	{
		public static bool Equals(this Color32 c1, Color32 c2)
		{
			return (c1.r == c2.r && c1.g == c2.g && c1.b == c2.b);
		}
	}

	public static class ListExtensions
	{
		public static T GetRandomElement<T>(this List<T> list)
		{
			return list[Random.Range(0, list.Count)];
		}
	}

	public static class GeneralExtensions
	{
		public static string GetHierarchyPath(this Transform t)
		{
			string str = t.name;
			while (t.parent != null)
			{
				t = t.parent;
				str = t.name +"\\"+ str;
			}
			return str;
		}
	}
}