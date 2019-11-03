using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OilGenerator : MonoBehaviour
{
    public int AvailableClusterCount = 72;
    public float MinDistanceBetweenClusters = 20f;
    public float MaxDistanceBetweenClusters = 35f;
    public int ClusterWidth = 3;
    public int ClusterHeight = 3;
    public float ClusterSpeedCoefficient = 0f;
    public float Depth = -2f;
    public float MinClusterHeight = 20f;
    public float SkyBound = -1;

    public SpriteRenderer OilStripeRenderer;
    public float SpaceBetweenClusterElements = 0.25f;

    private float _clusterElementWidth;
    private float _clusterElementHeight;

    private static System.Random _rand = new System.Random();

    private Vector2 _lastGeneration;
    private float _distToGenerate;
    private Vector2 _previousScreenCenter;

    private List<GameObject> _freeGameObjects = new List<GameObject>();

    private List<GameObject> _usedClusters = new List<GameObject>();

    private int _cratesCollected = 0;

    private string[] _uiMessages = { "Collector!", "Fuel beast!", "Get 'em all!" };

    void Awake()
    {
        _lastGeneration = new Vector2(0, 0);
        _distToGenerate = MinDistanceBetweenClusters;

       for (int i = 0; i < AvailableClusterCount; ++i)
        {
            GameObject gameObject = new GameObject();
            GameObject oilDrumGameObject = Instantiate(OilStripeRenderer.gameObject, this.transform);
            BoxCollider2D collider = oilDrumGameObject.AddComponent<BoxCollider2D>();
            SpriteRenderer spriteRenderer = oilDrumGameObject.GetComponent<SpriteRenderer>();
            collider.isTrigger = true;

            oilDrumGameObject.SetActive(false);
            _clusterElementWidth = spriteRenderer.size.x;
            _clusterElementHeight = spriteRenderer.size.y;
            _freeGameObjects.Add(oilDrumGameObject);
        }
    }

    private bool ShouldCloudBeActive(float xCoord, float yCoord)
    {
        Vector3 worldBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 worldBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f));
        Vector3 worldTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
        Vector3 worldCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        float width = System.Math.Abs(worldBottomRight.x - worldBottomLeft.x);
        float height = System.Math.Abs(worldTopLeft.y - worldBottomLeft.y);
        width += width;
        height += height;

        bool result = (Mathf.Abs(worldBottomLeft.x - xCoord) <= width && Mathf.Abs(xCoord - worldBottomRight.x) <= width) &&
                      (Mathf.Abs(worldBottomLeft.y - yCoord) <= height && Mathf.Abs(yCoord - worldTopLeft.y) <= height);
        return result;
    }

    private void EnsureClusterElementVisible(float xCoord, float yCoord)
    {
        // get from the pool
        int gameObjectIndex = _freeGameObjects.Count - 1;
        GameObject oilDrumGameObject = _freeGameObjects[gameObjectIndex];
        _freeGameObjects.RemoveAt(gameObjectIndex);

        RaycastHit2D hit = Physics2D.Raycast(new Vector2(xCoord, yCoord), new Vector2(0, -1));
        SpriteRenderer spriteRenderer = oilDrumGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.transform.position = new Vector3(xCoord, yCoord, Depth);

        OilClusterElement element = oilDrumGameObject.GetComponent<OilClusterElement>();

        // register as visible segment
        _usedClusters.Add(oilDrumGameObject);
        element.OilGeneratorScript = this;

        // make visible
        oilDrumGameObject.SetActive(true);

    }

    public void HideEnteredClusterElement(GameObject oilDrumGameObject)
    {
        for (int i = 0; i < _usedClusters.Count; i++)
        {
            if (oilDrumGameObject == _usedClusters[i])
            {
                EnsureClusterElementNotVisible(i);
                EventManager.Instance.TriggerEvent(Constants.EVENT_GAIN_FUEL, Constants.FuelPerCrate);
                EventManager.Instance.TriggerEvent(Constants.EVENT_INCREMENT_SCORE, Constants.ScoreBonusPerCrate);
                if (++_cratesCollected % 5 == 0)
                {
                    int index = _rand.Next(0, _uiMessages.Length);
                    EventManager.Instance.TriggerEvent(Constants.EVENT_UI_MESSAGE, _uiMessages[index]);
                }
                break;
            }
        }
    }

    private void EnsureClusterElementNotVisible(int index)
    {
        GameObject oilDrumGameObject = _usedClusters[index];
        _usedClusters.RemoveAt(index);

        oilDrumGameObject.gameObject.SetActive(false);

        _freeGameObjects.Add(oilDrumGameObject);
    }

    void Update()
    {
        // get the index of visible segment by finding the center point world position
        Vector3 worldCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Vector2 delta = new Vector2(worldCenter.x - _previousScreenCenter.x, worldCenter.y - _previousScreenCenter.y);
        float currentXCoordinate = worldCenter.x;
        float currentYCoordinate = worldCenter.y;
        Vector2 currentCoordinate = new Vector2(currentXCoordinate, currentYCoordinate);

        // Test visible segments for visibility and hide those if not visible.
        for (int i = 0; i < _usedClusters.Count;)
        {
            float xCoordinate = _usedClusters[i].transform.position.x;
            float yCoordinate = _usedClusters[i].transform.position.y;
            float zCoordinate = _usedClusters[i].transform.position.z;
            xCoordinate += delta.x * ClusterSpeedCoefficient;
            _usedClusters[i].transform.position = new Vector3(xCoordinate, yCoordinate, zCoordinate);
            if (!ShouldCloudBeActive(xCoordinate, yCoordinate))
            {
                EnsureClusterElementNotVisible(i);
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
        float height = System.Math.Abs(worldTop.y - worldLeft.y);


        if ((currentCoordinate - _lastGeneration).magnitude >= _distToGenerate)
        {
            Vector3 cameraVelocity = Camera.main.velocity;
            float xFactor = (float)_rand.NextDouble() + 0.6f;
            float yFactor = (float)_rand.NextDouble() - 0.5f;
            float xCoord = currentXCoordinate + width * xFactor;
            float yCoord = Mathf.Max(currentYCoordinate, MinClusterHeight) + height * yFactor;
            yCoord = Mathf.Max(yCoord, MinClusterHeight + height / 2f * (float) _rand.NextDouble());

            if (yCoord + ClusterHeight * (_clusterElementHeight + SpaceBetweenClusterElements) <= SkyBound &&
                ClusterHeight * ClusterWidth <= _freeGameObjects.Count)
            {
                // TODO: Instead of using a hard-coded variable, look into the actual top bound of the sky strip
                for (int k = 0; k < ClusterHeight; ++k)
                {
                    for (int j = 0; j < ClusterWidth; ++j)
                    {
                        float x = xCoord + k * (_clusterElementWidth + SpaceBetweenClusterElements);
                        float y = yCoord + j * (_clusterElementHeight + SpaceBetweenClusterElements);

                        EnsureClusterElementVisible(x, y);
                    }
                }
            }

            _lastGeneration = currentCoordinate;
            _distToGenerate = MinDistanceBetweenClusters + (MaxDistanceBetweenClusters + 1f - MinDistanceBetweenClusters) * (float)_rand.NextDouble();
        }
    }
}