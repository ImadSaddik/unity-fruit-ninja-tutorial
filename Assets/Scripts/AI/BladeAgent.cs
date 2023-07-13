using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class BladeAgent : Agent
{
    private Camera mainCamera;
    public Bounds screenBounds;
    private Collider sliceCollider;
    private TrailRenderer sliceTrail;
    public Vector3 direction { get; private set; }
    public float sliceForce = 5f;
    public float minSliceVelocity = 0.01f;
    private bool slicing;
    private bool mouseDown = false;
    private bool isMouseDown = true;
    private float mouseXClickPourcentage;
    private float mouseYClickPourcentage;

    public override void Initialize()
    {
        mainCamera = Camera.main;
        sliceCollider = GetComponent<Collider>();
        sliceTrail = GetComponentInChildren<TrailRenderer>();

        initializeScreenBounds();
    }

    private void initializeScreenBounds()
    {
        screenBounds = new Bounds();
        screenBounds.Encapsulate(Camera.main.ScreenToWorldPoint(Vector3.zero));
        screenBounds.Encapsulate(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f)));
    }

    public override void OnEpisodeBegin()
    {
        StopSlice();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);
        sensor.AddObservation(direction);
    }

    
    public override void OnActionReceived(ActionBuffers actions)
    {
        mouseDown = actions.DiscreteActions[0] == 1;
        mouseXClickPourcentage = actions.ContinuousActions[0];
        mouseYClickPourcentage = actions.ContinuousActions[1];
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        discreteActions[0] = Input.GetMouseButton(0) ? 1 : 0;
        setContinuousActions(continuousActions);
    }

    private void setContinuousActions(ActionSegment<float> continuousActions)
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float x = mousePosition.x / screenBounds.max.x;
        float y = mousePosition.y / screenBounds.max.y;

        continuousActions[0] = Mathf.Clamp(x, -1f, 1f);
        continuousActions[1] = Mathf.Clamp(y, -1f, 1f);
    }

    private void Update()
    {
        if (mouseDown && isMouseDown)
        {
            StartSlice();
            isMouseDown = false;
        } 
        else if (!mouseDown && !mouseDown)
        {
            StopSlice();
            isMouseDown = true;
        } 
        else if (slicing)
        {
            ContinueSlice();
        }
    }

    private void StartSlice()
    {
        Vector3 position = CalculatePosition();
        position.z = 0f;
        transform.position = position;

        SetSlicingColliderAndTrail(true);
        sliceTrail.Clear();
    }

    private Vector3 CalculatePosition()
    {
        float x = mouseXClickPourcentage * screenBounds.max.x;
        float y = mouseYClickPourcentage * screenBounds.max.y;

        return new Vector3(x, y, 0f);
    }

    private void StopSlice()
    {
        SetSlicingColliderAndTrail(false);
    }

    private void SetSlicingColliderAndTrail(bool enabledOrNot)
    {
        slicing = enabledOrNot;
        sliceCollider.enabled = enabledOrNot;
        sliceTrail.enabled = enabledOrNot;
    }

    private void ContinueSlice()
    {
        Vector3 newPosition = CalculatePosition();
        newPosition.z = 0f;

        direction = newPosition - transform.position;

        float velocity = direction.magnitude / Time.deltaTime;
        sliceCollider.enabled = velocity > minSliceVelocity;

        transform.position = newPosition;
    }
}
