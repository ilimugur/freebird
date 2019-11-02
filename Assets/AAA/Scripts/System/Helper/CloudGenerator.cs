using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudGenerator : MonoBehaviour
{
    public int AvailableCloudCount = 50;
    public float MinDistanceBetweenClouds = 2f;
    public float MaxDistanceBetweenClouds = 3f;
    public float Depth = -5f;
    public float MinCloudHeightFromGround = 0.5f; // TODO: doesn't work as intended
    public float CloudSpeedCoefficient = 0f;
    public float CloudScaleCoefficient = 1f;
    public Color CloudColor;

    public Sprite[] availableSprites;

    private static System.Random _rand = new System.Random();

    private float _nextGenerationX;
    private Vector2 _previousScreenCenter;

    private List<SpriteRenderer> _freeSpriteRenderers = new List<SpriteRenderer>();

    private List<SpriteRenderer> _usedClouds = new List<SpriteRenderer>();

    void Awake()
    {
        _nextGenerationX = 0;

       for (int i = 0; i < AvailableCloudCount; ++i)
        {
            GameObject gameObject = new GameObject();
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
            spriteRenderer.color = CloudColor;
            int index = i % availableSprites.Length;
            spriteRenderer.sprite = availableSprites[index];
            spriteRenderer.transform.localScale *= CloudScaleCoefficient;

            spriteRenderer.gameObject.SetActive(false);
            _freeSpriteRenderers.Add(spriteRenderer);
        }
    }

    private bool ShouldCloudBeActive(float xCoord, float yCoord)
    {
        Vector3 worldBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 worldTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));

        // TODO: Check top and bottom distance? (may need to introduce a y-index into ActiveCloud for this)

        // find screen size
        float width = System.Math.Abs(worldBottomRight.x - worldBottomLeft.x);
        float height = System.Math.Abs(worldTopLeft.x - worldBottomLeft.x);

        bool result = (worldBottomLeft.x - xCoord <= width && xCoord - worldBottomRight.x <= width) &&
                      (worldBottomLeft.y - yCoord <= height && yCoord - worldTopLeft.y <= height);
        return result;
    }

    private void EnsureCloudVisible(float xCoord, float yCenter, float radius)
    {
        if (float.IsNaN(xCoord))
        {
            float osman = 2 * 5f;
            osman += 4f - osman;
        }

        // get from the pool
        int rendererIndex = _freeSpriteRenderers.Count - 1;
        SpriteRenderer renderer = _freeSpriteRenderers[rendererIndex];
        _freeSpriteRenderers.RemoveAt(rendererIndex);

        // generate
        //            Mesh mesh = renderer.mesh;
        //            GenerateSegment(index, ref mesh);

        // position
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(xCoord, yCenter), new Vector2(0, -1));
        float minPossible;
        float maxPossible = yCenter + radius;
        if (hit.collider != null)
        {
            //minPossible = Mathf.Max(-hit.distance + MinCloudHeightFromGround, -radius);
            minPossible = yCenter - hit.distance + MinCloudHeightFromGround;
        }
        else
        {
            minPossible = yCenter - radius;
        }
        float yCoord = (maxPossible + 1f - minPossible) * (float) _rand.NextDouble();
        renderer.transform.position = new Vector3(xCoord, yCoord, Depth);

        // make visible

        renderer.gameObject.SetActive(true);

        // register as visible segment

        _usedClouds.Add(renderer);
    }

    private void EnsureCloudNotVisible(int index)
    {
        SpriteRenderer renderer = _usedClouds[index];
        _usedClouds.RemoveAt(index);

        renderer.gameObject.SetActive(false);

        _freeSpriteRenderers.Add(renderer);
    }

    void Update()
    {
        // get the index of visible segment by finding the center point world position
        Vector3 worldCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Vector2 delta = new Vector2(worldCenter.x - _previousScreenCenter.x, worldCenter.y - _previousScreenCenter.y);
        int currentCoordinate = (int)worldCenter.x;

        // Test visible segments for visibility and hide those if not visible.
        for (int i = 0; i < _usedClouds.Count;)
        {
            float xCoord = _usedClouds[i].transform.position.x;
            float yCoord = _usedClouds[i].transform.position.y;
            float zCoord = _usedClouds[i].transform.position.z;
            xCoord += delta.x * CloudSpeedCoefficient;
            _usedClouds[i].transform.position = new Vector3(xCoord, yCoord, zCoord);
            if (!ShouldCloudBeActive(xCoord, yCoord))
            {
                EnsureCloudNotVisible(i);
            }
            else
            {
                // EnsureSegmentNotVisible will remove the segment from the list
                // that's why I increase the counter based on that condition
                ++i;
            }
        }
        _previousScreenCenter += delta;

        if(currentCoordinate >= _nextGenerationX && _freeSpriteRenderers.Count > 0)
        {
            Vector3 worldLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 worldRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
            Vector3 worldTop = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
            float width = System.Math.Abs(worldRight.x - worldLeft.x);
            float height = System.Math.Abs(worldTop.y - worldLeft.y);

            float yMidPoint = worldCenter.y;
            Vector3 cameraVelocity = Camera.main.velocity;
            float xFactor = (cameraVelocity.magnitude > 0f ? cameraVelocity.x / cameraVelocity.magnitude : 1f);
            EnsureCloudVisible(_nextGenerationX + width * xFactor, yMidPoint, height / 2);

            // Generate a new coordinate upon reaching which a new cloud will be spawned
            _nextGenerationX = currentCoordinate + (MaxDistanceBetweenClouds + 1f - MinDistanceBetweenClouds) * (float)_rand.NextDouble();
        }
    }
}