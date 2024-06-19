using UnityEngine;

public class MouseIndicator : MonoBehaviour
{
    void Update()
    {
        RotateCombatMouseCircle();
    }

    public void RotateCombatMouseCircle() 
    {
        Vector3 mousePosition = GetMouseWorldPosition();
        Vector3 playerPosition = transform.position;

        Vector3 direction = mousePosition - playerPosition;
        direction.y = 0f;

        float angleRadians = Mathf.Atan2(direction.z, direction.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        // Mantener la rotación en X en 90 grados y ajustar solo la rotación en Z
        transform.rotation = Quaternion.Euler(90, 0, angleDegrees);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) return hit.point;

        return Vector3.zero;
    }
}
