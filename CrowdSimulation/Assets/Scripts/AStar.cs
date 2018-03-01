using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AStar : MonoBehaviour 
{
	public Waypoint[] m_Waypoints; 

	// Two-dimensional array keeping euclidian distances between any pair of different waypoints.
	// _Distances[A,B] == _Distances[B,A]
	float[,] _Distances;

	void Awake()
	{
		Preprocess();		
	}

	/// Check for viable paths and compute distances between waypoints.
	void Preprocess()
	{
		if(m_Waypoints != null)
		{
			_Distances = new float[m_Waypoints.Length,m_Waypoints.Length];
			
			// Clear all list of neighbors.
			for(int waypoint1Id = 0; waypoint1Id < m_Waypoints.Length; ++waypoint1Id)
			{
				Waypoint waypoint1 = m_Waypoints[waypoint1Id];				
				waypoint1.m_Neighbors.Clear();
				waypoint1.id = waypoint1Id;								
			}

			for(int waypoint1Id = 0; waypoint1Id < m_Waypoints.Length; ++waypoint1Id)
			{
				Waypoint waypoint1 = m_Waypoints[waypoint1Id];
				for(int waypoint2Id = waypoint1Id + 1; waypoint2Id < m_Waypoints.Length; ++waypoint2Id)
				{
					if(waypoint1Id != waypoint2Id)
					{
						Waypoint waypoint2 = m_Waypoints[waypoint2Id];
						Vector3 direction = waypoint2.transform.position - waypoint1.transform.position;
						float distance = direction.magnitude;

						// If there is no obstacle between them, add each other to own neighborhood.
						if(!Physics.Raycast(waypoint1.transform.position,direction.normalized, distance))
						{
							waypoint1.AddNeighbor(waypoint2);
							waypoint2.AddNeighbor(waypoint1);	
						}

						// Store distances.
						_Distances[waypoint1Id, waypoint2Id] = distance;
						_Distances[waypoint2Id, waypoint1Id] = distance;
					}
				}
			}
		}
	}

	// Store visited waypoints. Once the path is computed clear only the visited ones.
	Queue<Waypoint> _VisitedWaypoints = new Queue<Waypoint>();
	// list with waypoints and the cost of moving to it, sorted by cost. 
	SortedList<float, Waypoint> _SortedList = new SortedList<float, Waypoint>();

	///
	/// Compute a path from startWaypoint to endWaypoint using A-Star algorithm.
	///
	public Stack<Waypoint> GetPath(Waypoint startWaypoint, Waypoint endWaypoint)
	{
		Stack<Waypoint> path = new Stack<Waypoint>();

		// If endWaypoint has a direct access from startWaypoint, nothing to do here. 
		if(startWaypoint.IsNeighbor(endWaypoint))
		{
			path.Push(endWaypoint);
			path.Push(startWaypoint);
		} else
		{
			_SortedList.Clear();

			// Add startWaypoint with cost of 0.
			startWaypoint.m_Cost = 0;
			_SortedList.Add(0, startWaypoint);
			_VisitedWaypoints.Enqueue(startWaypoint);
			
			Waypoint currentWaypoint = null;

			// continue while there are waypoints to visit AND the currentWaypoint is not the endWaypoint 
			while(_SortedList.Count > 0 && (currentWaypoint = _SortedList[_SortedList.Keys[0]]) != endWaypoint)				  
			{
				// Mark waypoint as visited and remove from list.
				currentWaypoint.m_Visited = true;
				_SortedList.RemoveAt(0);

				for(int neighborId = 0; neighborId < currentWaypoint.m_Neighbors.Count; ++neighborId)
				{
					Waypoint neighborWaypoint = currentWaypoint.m_Neighbors[neighborId];
					
					// Check not visited neighbors.
					if(!neighborWaypoint.m_Visited)
					{
						// Is it cheaper to move from the current waypoint to this neighbor or the previous movement to this neighbor was better?
						// ==
						// The currentWaypoint cost + distance from currentWaypoint to this neighborWaypoint
						// is lesser than the current cost of the neighborWaypoint?
						float cost = currentWaypoint.m_Cost + _Distances[currentWaypoint.id, neighborWaypoint.id];
						if(cost < neighborWaypoint.m_Cost)
						{
							// if so, keep the new value and say that is better move from currentWaypoint to neighborWaypoint instead.
							neighborWaypoint.m_Cost = cost;
							neighborWaypoint.m_Previous = currentWaypoint;

                            
							// Add neighborWaypoint to the list to be visited, but add to its cost the distance from it to the endWaypoint.
                            // This is the heuristic we are using for the A-star. This way, the closest ones to the end will be selected first.
							_SortedList.Add(neighborWaypoint.m_Cost + _Distances[neighborWaypoint.id, endWaypoint.id], neighborWaypoint);							
							_VisitedWaypoints.Enqueue(neighborWaypoint);
						}
					}
				}
			}

			// No path found.
			if(currentWaypoint != endWaypoint)
			{
				return null;
			}

			// Track back the path, from end to start, and store in a stack (LIFO).  
			Waypoint waypoint = endWaypoint;				
			while(waypoint != null)
			{
				path.Push(waypoint);
				waypoint = waypoint.m_Previous;
			}
			
			// Reset settings of visited points.
			while(_VisitedWaypoints.Count > 0)
			{
				_VisitedWaypoints.Dequeue().Reset();
			}
		}

		return path;
	}


    // For debug purposes.
	public IEnumerator ShowPath(Waypoint startWaypoint, Waypoint endWaypoint)
	{
        startWaypoint.SetColor(Color.blue);
        endWaypoint.SetColor(Color.blue);
 
        if (!startWaypoint.IsNeighbor(endWaypoint))
        {
            _SortedList.Clear();

            startWaypoint.m_Cost = 0;
            _SortedList.Add(0, startWaypoint);
            _VisitedWaypoints.Enqueue(startWaypoint);

            Waypoint currentWaypoint = null;

            while (_SortedList.Count > 0 && (currentWaypoint = _SortedList[_SortedList.Keys[0]]) != endWaypoint)
            {
                currentWaypoint.m_Visited = true;
                currentWaypoint.SetColor(Color.black);
                _SortedList.RemoveAt(0);              

                for (int neighborId = 0; neighborId < currentWaypoint.m_Neighbors.Count; ++neighborId)
                {
                    Waypoint neighborWaypoint = currentWaypoint.m_Neighbors[neighborId];

                    if (!neighborWaypoint.m_Visited)
                    {
                        float cost = currentWaypoint.m_Cost + _Distances[currentWaypoint.id, neighborWaypoint.id];
                        if (cost < neighborWaypoint.m_Cost)
                        {
                            neighborWaypoint.m_Cost = cost;
                            neighborWaypoint.m_Previous = currentWaypoint;

                            float heuristic = neighborWaypoint.m_Cost + _Distances[neighborWaypoint.id, endWaypoint.id];
                            
                            _SortedList.Add(heuristic, neighborWaypoint);
                            _VisitedWaypoints.Enqueue(neighborWaypoint);
                        }
                    }

                   // yield return new WaitForSeconds(0.1f);
                }

                int candidateId = 1;
                foreach (KeyValuePair<float, Waypoint> candidates in _SortedList)
                {
                    candidates.Value.SetColor(Color.Lerp(Color.yellow, Color.red, (float) candidateId/_SortedList.Count));
                    candidateId++;
                }

                yield return new WaitForSeconds(1f);
            }

            if (currentWaypoint != endWaypoint)
            {
                yield return null;
            }

            Waypoint waypoint = endWaypoint;
            while (waypoint != null)
            {
                waypoint.SetColor(Color.green);
                waypoint = waypoint.m_Previous;
            }

            yield return new WaitForSeconds(10);

            while (_VisitedWaypoints.Count > 0)
            {
                Waypoint visited = _VisitedWaypoints.Dequeue();
                visited.SetColor(Color.white);
            }
        }

        yield return null;
	}

	// Given a position find the closed waypoint.
	public Waypoint GetClosestWaypoint(Vector3 position)
	{
		Waypoint closestWaypoint = null;
		float minDistance = float.MaxValue;

		if(m_Waypoints != null)
		{
			for(int waypoint1Id = 0; waypoint1Id < m_Waypoints.Length; ++waypoint1Id)
			{
				float distance = Vector3.Distance(m_Waypoints[waypoint1Id].transform.position, position); 
				if(distance < minDistance)
				{
					minDistance = distance;
					closestWaypoint = m_Waypoints[waypoint1Id];
				}
			}
		}

		return closestWaypoint;
	}

	// Draw Waypoint locations in editor.
	void OnDrawGizmos()
	{
		if(m_Waypoints != null)
		{
			for(int waypointId = 0; waypointId < m_Waypoints.Length; ++waypointId)
			{
                Waypoint waypoint = m_Waypoints[waypointId];
				Gizmos.DrawWireSphere(waypoint.transform.position, 0.1f);

                for(int neighborId = 0; neighborId < waypoint.m_Neighbors.Count; ++neighborId)
                {
                    Waypoint neighbor = waypoint.m_Neighbors[neighborId];

                    Gizmos.DrawLine(waypoint.transform.position, neighbor.transform.position);
                }
			}
		}        
	}
}
