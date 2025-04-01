using System.Collections;
using UnityEngine;

public class RepairArrow : MonoBehaviour
{
    // How often (in seconds) to search for a new broken object.
    public float updateInterval = 0.5f;

    // Speed at which the arrow rotates toward the target.
    public float rotationSpeed = 5f;

    // If your arrow model faces backward, set this to 180; if correct, set to 0.
    public float arrowFlipY = 180f;

    // The current target (a broken Repair object).
    private Transform target;

    // Reference to the VR camera's transform (the arrow's parent).
    private Transform playerTransform;

    void Start()
    {
        // Since the arrow is a child of the VR camera, its parent should be the camera transform.
        if (transform.parent != null)
        {
            playerTransform = transform.parent;
        }

        // Start periodically searching for a broken Repair object.
        StartCoroutine(UpdateTargetRoutine());
    }

    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            // If we have no target or the current target has been repaired, look for a new one.
            if (target == null || !target.GetComponent<Repair>().IsBroken)
            {
                target = FindClosestBrokenRepair();
            }

            // Wait before searching again.
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void Update()
    {
        // If there's no target, the arrow simply won't rotate toward anything.
        if (target == null)
            return;

        // Calculate the horizontal direction from the arrow to the target.
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // ignore vertical differences

        // Only rotate if there's a meaningful direction.
        if (direction.sqrMagnitude > 0.0001f)
        {
            // Compute the desired rotation to face the target.
            Quaternion desiredRotation = Quaternion.LookRotation(direction);

            // Apply a Y flip if your arrow model is reversed.
            desiredRotation *= Quaternion.Euler(0f, arrowFlipY, 0f);

            // Smoothly rotate toward the desired rotation.
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    private Transform FindClosestBrokenRepair()
    {
        // If we don't have a parent transform, we can't compute distances properly.
        if (playerTransform == null)
            return null;

        Repair[] repairs = FindObjectsOfType<Repair>();
        float closestDistance = Mathf.Infinity;
        Transform nearest = null;

        // Get the player's horizontal position (ignoring Y).
        Vector3 playerPos2D = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);

        foreach (Repair rep in repairs)
        {
            if (rep.IsBroken)
            {
                // Calculate horizontal distance to the broken object.
                Vector3 repPos2D = new Vector3(rep.transform.position.x, 0f, rep.transform.position.z);
                float distance = Vector3.Distance(playerPos2D, repPos2D);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearest = rep.transform;
                }
            }
        }

        return nearest;
    }
}
