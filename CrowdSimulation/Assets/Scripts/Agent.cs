using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Agent : MonoBehaviour 
{
	public float m_Speed = 1;
	public Waypoint m_GoalWaypoint = null;
	public uint m_AStarId = 0;

	bool _ReachedGoal = false;
	Waypoint _CurrentWaypoint;
	AStar _AStar;
	Rigidbody _CachedRigidbody;
	Stack<Waypoint> _Path;	

	void Start () 
	{
		if(m_AStarId < PathPlanningManager.Instance.m_AStarPathPlanners.Length)
		{
			_AStar = PathPlanningManager.Instance.m_AStarPathPlanners[m_AStarId];
		}

		if(_AStar != null)
		{
			_CurrentWaypoint = _AStar.GetClosestWaypoint(transform.position);
            _Path = _AStar.GetPath(_CurrentWaypoint, m_GoalWaypoint);

            // Uncomment to see the algorithm processing.      
            //StartCoroutine(_AStar.ShowPath(_CurrentWaypoint, m_GoalWaypoint));
		}

		_CachedRigidbody = GetComponent<Rigidbody>();
	}

	// Check if reached the current waypoint, if so move to the next waypoint, if any. Otherwise, reached the goal.
	void CheckWaypoint()
	{
		if(_ReachedGoal)
		{
			return;
		}

		if(_Path != null && _CurrentWaypoint != null && Vector3.Distance(transform.position, _CurrentWaypoint.transform.position) < 0.5f)
		{
			if(_CurrentWaypoint == m_GoalWaypoint || _Path == null || _Path.Count == 0 )
			{
				_ReachedGoal = true;
			} else
			{
				_CurrentWaypoint = _Path.Pop();
			}
		}
	}

	// Move towards the next waypoint using the given speed.
	void Move()
	{
		_CachedRigidbody.MovePosition(_CachedRigidbody.position + (_CurrentWaypoint.transform.position - _CachedRigidbody.position).normalized * m_Speed * Time.deltaTime);
	}

	void Update () 
	{
		CheckWaypoint();

		if(!_ReachedGoal && _Path != null && _CurrentWaypoint != null)
		{
			Move();
		}		
	}

	// Draws next waypoint location.
	void OnDrawGizmos()
	{
		if(_CurrentWaypoint != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(_CurrentWaypoint.transform.position, 0.2f);
			Gizmos.color = Color.white;			
		}
	}
}
