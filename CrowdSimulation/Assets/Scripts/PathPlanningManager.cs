using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlanningManager : MonoBehaviour 
{
	#region Instance
	static PathPlanningManager _Instance = null;

	public static PathPlanningManager Instance
	{
		get
		{
			if(_Instance == null)
			{
				_Instance = FindObjectOfType<PathPlanningManager>();

				if(_Instance == null)
				{
					GameObject go = new GameObject();
					go.name = "PathPlanner";
					_Instance = go.AddComponent<PathPlanningManager>();
				}
			}

			return _Instance;
		}
	}
	#endregion

	public AStar[] m_AStarPathPlanners;
}
