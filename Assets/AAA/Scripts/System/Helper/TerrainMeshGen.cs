using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainMeshGen : MonoBehaviour
{
    private struct Segment
    {
        public int Index { get; set; }
        public MeshFilter MeshFilter { get; set; }
    }

    public float SegmentLength = 10;

    public int SegmentResolution = 32;

    public int MeshCount = 6; // ensure VisibleMeshes >= MeshCount + 2 to avoid latency in generating terrain

    public int VisibleMeshes = 4;

    // the prefab including MeshFilter and MeshRenderer
    public MeshFilter SegmentPrefab;

    // helper array to generate new segment without further allocations
    private Vector3[] _vertexArray;

    private List<MeshFilter> _freeMeshFilters = new List<MeshFilter>();

    private List<Segment> _usedSegments = new List<Segment>();

    void Awake()
    {
        // Create vertex array helper
        _vertexArray = new Vector3[SegmentResolution * 2];

        // Build triangles array. For all meshes this array always will
        // look the same, so I am generating it once 
        int iterations = _vertexArray.Length / 2 - 1;
        var triangles = new int[(_vertexArray.Length - 2) * 3];

        for (int i = 0; i < iterations; ++i)
        {
            int i2 = i * 6;
            int i3 = i * 2;

            triangles[i2] = i3 + 2;
            triangles[i2 + 1] = i3 + 1;
            triangles[i2 + 2] = i3 + 0;

            triangles[i2 + 3] = i3 + 2;
            triangles[i2 + 4] = i3 + 3;
            triangles[i2 + 5] = i3 + 1;
        }

        // Create game objects (with MeshFilter) instances.
        // Assign vertices, triangles, deactivate and add to the pool.
        for (int i = 0; i < MeshCount; ++i)
        {
            MeshFilter filter = Instantiate(SegmentPrefab, this.transform);

            Mesh mesh = filter.mesh;
            mesh.Clear();

            mesh.vertices = _vertexArray;
            mesh.triangles = triangles;

            filter.gameObject.SetActive(false);
            _freeMeshFilters.Add(filter);
        }
        this.transform.position = transform.position + new Vector3(0, -25, 0);
    }

    // Gets the height of terrain at current position.
    // Modify this function to get different terrain configuration.
    private float GetHeight(float position)
    {
        if(position < 5f)
            return (Mathf.Sin(position * 0.1f) * 3f + Mathf.Sin(position * 0.2f) * 4f) / 2f;
        else
            return (Mathf.Sin(position * 0.1f) * 6f + Mathf.Sin(position * 0.2f) * 8f) / 2f;
            //return (Mathf.Sin(position * 0.1f) * 6f + 3.5f + Mathf.Sin(position * 0.2f) * 4f + 8f) / 2f;
            //return (Mathf.Sin(position * 0.1f) * 12f + 7f + Mathf.Sin(position * 0.2f) * 8f + 8f) / 2f;
    }

    // This function generates a mesh segment.
    // Index is a segment index (starting with 0).
    // Mesh is a mesh that this segment should be written to.
    public void GenerateSegment(int index, ref Mesh mesh)
    {
        float startPosition = index * SegmentLength;
        float step = SegmentLength / (SegmentResolution - 1);

        for (int i = 0; i < SegmentResolution; ++i)
        {
            // get the relative x position
            float xPos = step * i;

            // top vertex
            float yPosTop = GetHeight(startPosition + xPos); // position passed to GetHeight() must be absolute
            _vertexArray[i * 2] = new Vector3(xPos, yPosTop, 0);

            // bottom vertex always at y=0
            _vertexArray[i * 2 + 1] = new Vector3(xPos, -14, 0);
        }

        mesh.vertices = _vertexArray;

        // need to recalculate bounds, because mesh can disappear too early
        mesh.RecalculateBounds();
    }

	private void UpdatePolygonCollider2DFrommesh(MeshFilter meshFilter, PolygonCollider2D polygonCollider)
	{
		// Get triangles and vertices from mesh
		int[] triangles = meshFilter.mesh.triangles;
		Vector3[] vertices = meshFilter.mesh.vertices;

		// Get just the outer edges from the mesh's triangles (ignore or remove any shared edges)
		Dictionary<string, KeyValuePair<int, int>> edges = new Dictionary<string, KeyValuePair<int, int>>();
		for (int i = 0; i < triangles.Length; i += 3)
		{
			for (int e = 0; e < 3; e++)
			{
				int vert1 = triangles[i + e];
				int vert2 = triangles[i + e + 1 > i + 2 ? i : i + e + 1];
				string edge = Mathf.Min(vert1, vert2) + ":" + Mathf.Max(vert1, vert2);
				if (edges.ContainsKey(edge))
				{
					edges.Remove(edge);
				}
				else
				{
					edges.Add(edge, new KeyValuePair<int, int>(vert1, vert2));
				}
			}
		}

		// Create edge lookup (Key is first vertex, Value is second vertex, of each edge)
		Dictionary<int, int> lookup = new Dictionary<int, int>();
		foreach (KeyValuePair<int, int> edge in edges.Values)
		{
			if (lookup.ContainsKey(edge.Key) == false)
			{
				lookup.Add(edge.Key, edge.Value);
			}
		}

		// Create empty polygon collider
		//PolygonCollider2D polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
		polygonCollider.pathCount = 0;

		// Loop through edge vertices in order
		int startVert = 0;
		int nextVert = startVert;
		int highestVert = startVert;
		List<Vector2> colliderPath = new List<Vector2>();
		while (true)
		{

			// Add vertex to collider path
			colliderPath.Add(vertices[nextVert]);

			// Get next vertex
			nextVert = lookup[nextVert];

			// Store highest vertex (to know what shape to move to next)
			if (nextVert > highestVert)
			{
				highestVert = nextVert;
			}

			// Shape complete
			if (nextVert == startVert)
			{

				// Add path to polygon collider
				polygonCollider.pathCount++;
				polygonCollider.SetPath(polygonCollider.pathCount - 1, colliderPath.ToArray());
				colliderPath.Clear();

				// Go to next shape if one exists
				if (lookup.ContainsKey(highestVert + 1))
				{

					// Set starting and next vertices
					startVert = highestVert + 1;
					nextVert = startVert;

					// Continue to next loop
					continue;
				}

				// No more verts
				break;
			}
		}
	}

	private bool IsSegmentInSight(int index)
    {
        Vector3 worldLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));

        // check left and right segment side
        float x1 = index * SegmentLength;
        float x2 = x1 + SegmentLength;

        bool result = x1 - SegmentLength <= worldRight.x && x2 + SegmentLength >= worldLeft.x;
        return result;
    }


    private bool IsSegmentVisible(int index)
    {
        return SegmentCurrentlyVisibleListIndex(index) != -1;
    }

    private int SegmentCurrentlyVisibleListIndex(int index)
    {
        for (int i = 0; i < _usedSegments.Count; ++i)
        {
            if (_usedSegments[i].Index == index)
            {
                return i;
            }
        }

        return -1;
    }

    private void EnsureSegmentVisible(int index)
    {
        if (!IsSegmentVisible(index))
        {
            // get from the pool
            int meshIndex = _freeMeshFilters.Count - 1;
            MeshFilter filter = _freeMeshFilters[meshIndex];
            _freeMeshFilters.RemoveAt(meshIndex);

            // generate
            Mesh mesh = filter.mesh;
            GenerateSegment(index, ref mesh);

	        PolygonCollider2D polygonCollider2D;
	        if (filter.gameObject.GetComponent<PolygonCollider2D>() == null)
	        {
		        polygonCollider2D = filter.gameObject.AddComponent<PolygonCollider2D>();
		        filter.gameObject.layer = Constants.GroundLayer;
	        }
	        else
	        {
		        polygonCollider2D = filter.gameObject.GetComponent<PolygonCollider2D>();
	        }
			UpdatePolygonCollider2DFrommesh(filter, polygonCollider2D);
            // position
            filter.transform.position = new Vector3(index * SegmentLength, 0, 0);

            // make visible
            filter.gameObject.SetActive(true);

            // register as visible segment
            var segment = new Segment();
            segment.Index = index;
            segment.MeshFilter = filter;

            _usedSegments.Add(segment);
        }
    }

    private void EnsureSegmentNotVisible(int index)
    {
        if (IsSegmentVisible(index))
        {
            int listIndex = SegmentCurrentlyVisibleListIndex(index);
            Segment segment = _usedSegments[listIndex];
            _usedSegments.RemoveAt(listIndex);

            MeshFilter filter = segment.MeshFilter;
            filter.gameObject.SetActive(false);

            _freeMeshFilters.Add(filter);
        }
    }
	
    void Update()
    {
        // get the index of visible segment by finding the center point world position
        Vector3 worldCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        int currentSegment = (int)(worldCenter.x / SegmentLength);

        // Test visible segments for visibility and hide those if not visible.
        for (int i = 0; i < _usedSegments.Count;)
        {
            int segmentIndex = _usedSegments[i].Index;
            if (!IsSegmentInSight(segmentIndex))
            {
                EnsureSegmentNotVisible(segmentIndex);
            }
            else
            {
                // EnsureSegmentNotVisible will remove the segment from the list
                // that's why I increase the counter based on that condition
                ++i;
            }
        }

        // Test neighbor segment indexes for visibility and display those if should be visible.
        //for (int i = currentSegment - VisibleMeshes / 2; i < currentSegment + VisibleMeshes / 2; ++i)
        //for (int i = 0; i < MeshCount; ++i)
//        for (int i = currentSegment - VisibleMeshes; i <= currentSegment + VisibleMeshes; ++i)
//        for (int i = currentSegment - VisibleMeshes / 2; i < currentSegment + VisibleMeshes / 2; ++i)
        for (int i = currentSegment - VisibleMeshes; i <= currentSegment + VisibleMeshes; ++i)
        {
            if (IsSegmentInSight(i))
            {
                EnsureSegmentVisible(i);
            }
        }
    }
}