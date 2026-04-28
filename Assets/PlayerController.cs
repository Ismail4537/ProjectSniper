using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject overlay;
    Camera mainCamera;
    public AudioSource shootSound;
    public LayerMask targetLayerMask;
    [Header("Mouse Follow")]
    public float moveSpeed = 10f;
    public bool useSmoothMovement = false;
    public float smoothTime = 0.05f;
    public float stopDistance = 0.1f;

    [Header("Zoom")]
    public float zoomSpeed = 1f;
    public float minZoom = 3f;
    public float maxZoom = 5f;
    private float targetZoom;
    private float baseOrthographicSize;
    private Vector3 baseOverlayScale;

    private Vector3 currentVelocity;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            targetZoom = mainCamera.orthographicSize;
            baseOrthographicSize = mainCamera.orthographicSize;
        }

        if (overlay != null)
            baseOverlayScale = overlay.transform.localScale;
    }

    void Update()
    {
        FollowMouse();
        zoomControll();

        if (Input.GetMouseButtonDown(0))
        {
            print("Shoot!");
            shoot();
        }
    }

    private void FollowMouse()
    {
        if (Camera.main == null)
            return;

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        targetPosition.z = transform.position.z;

        if (Vector3.Distance(transform.position, targetPosition) > stopDistance)
        {
            if (useSmoothMovement)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime, moveSpeed);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }

    private void shoot()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, Mathf.Infinity, targetLayerMask);
        shootSound.Play();
        if (hit.collider != null)
        {
            print("Hit: " + hit.collider.name);
            TargetController target = hit.collider.GetComponent<TargetController>();
            if (target != null)
            {
                print("Target hit: " + target.name);
                target.DestroyTarget();
            }
        }
    }

    void zoomControll()
    {
        if (mainCamera == null || overlay == null)
            return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollDelta) > 0.001f)
        {
            targetZoom = Mathf.Clamp(targetZoom - scrollDelta * zoomSpeed, minZoom, maxZoom);
        }

        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * 5f);

        if (baseOrthographicSize <= 0f)
            baseOrthographicSize = mainCamera.orthographicSize;

        float zoomRatio = mainCamera.orthographicSize / baseOrthographicSize;
        Vector3 targetScale = baseOverlayScale * zoomRatio;
        overlay.transform.localScale = Vector3.Lerp(overlay.transform.localScale, targetScale, Time.deltaTime * 5f);
    }
}
