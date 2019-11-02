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

    private float _lastGenerationX;
    private float _distXToGenerate;
    private Vector2 _previousScreenCenter;

    private List<SpriteRenderer> _freeSpriteRenderers = new List<SpriteRenderer>();

    private List<SpriteRenderer> _usedClouds = new List<SpriteRenderer>();

    void Awake()
    {
        _lastGenerationX = -(MaxDistanceBetweenClouds + 1);

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
        Vector3 worldBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 worldBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f));
        Vector3 worldTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
        Vector3 worldCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        // TODO: Check top and bottom distance? (may need to introduce a y-index into ActiveCloud for this)

        float width = System.Math.Abs(worldBottomRight.x - worldBottomLeft.x);
        float height = System.Math.Abs(worldTopLeft.y - worldBottomLeft.y);
        width += width;
        height += height;

        bool result = (Mathf.Abs(worldBottomLeft.x - xCoord) <= width && Mathf.Abs(xCoord - worldBottomRight.x) <= width) &&
                      (Mathf.Abs(worldBottomLeft.y - yCoord) <= height && Mathf.Abs(yCoord - worldTopLeft.y) <= height);
        return result;
    }

    private void EnsureCloudVisible(float xCoord, float yCenter, float radius)
    {
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
        float yCoord = minPossible + (maxPossible + 1f - minPossible) * (float) _rand.NextDouble();
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
        float currentCoordinate = worldCenter.x;

        // Test visible segments for visibility and hide those if not visible.
        for (int i = 0; i < _usedClouds.Count;)
        {
            float xCoordinate = _usedClouds[i].transform.position.x;
            float yCoordinate = _usedClouds[i].transform.position.y;
            float zCoordinate = _usedClouds[i].transform.position.z;
            xCoordinate += delta.x * CloudSpeedCoefficient;
            _usedClouds[i].transform.position = new Vector3(xCoordinate, yCoordinate, zCoordinate);
            if (!ShouldCloudBeActive(xCoordinate, yCoordinate))
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

        Vector3 worldLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        Vector3 worldTop = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        float width = System.Math.Abs(worldRight.x - worldLeft.x);

        Vector3 cameraVelocity = Camera.main.velocity;
        float xFactor = 0.7f + 0.3f * (cameraVelocity.magnitude > 0f ? cameraVelocity.x / cameraVelocity.magnitude : 1f);
        float xCoord = currentCoordinate + width * xFactor;
        if(Mathf.Abs(xCoord - _lastGenerationX) >= _distXToGenerate && _freeSpriteRenderers.Count > 0)
        {
            float height = System.Math.Abs(worldTop.y - worldLeft.y);
            float yMidPoint = worldCenter.y;
            EnsureCloudVisible(xCoord, yMidPoint, height / 2);

            // Generate a new coordinate upon reaching which a new cloud will be spawned
            _lastGenerationX = xCoord;
            _distXToGenerate = MinDistanceBetweenClouds + (MaxDistanceBetweenClouds + 1f - MinDistanceBetweenClouds) * (float)_rand.NextDouble();
        }
    }
}