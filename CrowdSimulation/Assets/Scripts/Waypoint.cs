using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour 
{
	public int id = -1;
	[HideInInspector]
	public Waypoint m_Previous = null;
	[HideInInspector]
	public bool m_Visited = false;
	[HideInInspector]
	public float m_Cost = float.MaxValue;
	[HideInInspector]
	public List<Waypoint> m_Neighbors = new List<Waypoint>();

    // For debug purposes.
    public MeshRenderer m_Renderer;

    public void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
    }

    public void AddNeighbor(Waypoint neighbor)
	{
		m_Neighbors.Add(neighbor);	
	}

	public bool IsNeighbor(Waypoint waypoint)
	{
		return m_Neighbors.Contains(waypoint);
	}
	
	public void Reset()
	{
		m_Previous = null;
		m_Visited = false;
		m_Cost = float.MaxValue;
	}

    // For debug purposes.
    public void SetColor(Color color)
    {
        if(m_Renderer != null)
        {
            m_Renderer.material.color = color;
        }
    }
}
