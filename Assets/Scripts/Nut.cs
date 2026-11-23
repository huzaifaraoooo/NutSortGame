using UnityEngine;

public class Nut : MonoBehaviour
{
    [Header("Nut Settings")]
    public string nutColor;
    public MeshRenderer meshRenderer;
    public Bolt currentBolt;

    private Color originalColor;
    private bool isSelected = false;

    [Header("Animation Settings")]
    public float hoverHeight = 1.5f;
    public float rotateSpeed = 200f;

    private Vector3 targetHoverPosition;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Naye variables
    private float currentRotateSpeed;
    private float rotateTimer;

    void Start()
    {
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;

        originalRotation = transform.localRotation;
        originalPosition = transform.position;
    }

    void Update()
    {
        if (isSelected)
        {
            // 1. ARAM SE UPAR JAYE (Speed ko 10f kar diya hai taake smoothly jaye)
            transform.position = Vector3.Lerp(transform.position, targetHoverPosition, Time.deltaTime * 10f);

            // 2. GHoomna: Jab tak speed khatam na ho, ghoomta rahe
            if (currentRotateSpeed > 1f)
            {
                transform.Rotate(Vector3.forward, currentRotateSpeed * Time.deltaTime);

                // Aram se ghoomne ki speed kam hoti jaye
                currentRotateSpeed = Mathf.Lerp(currentRotateSpeed, 0f, Time.deltaTime * 2f);
            }
        }
        else
        {
            if (transform.parent == null)
            {
                transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 15f);
            }
        }
    }

    public void SetColor(string colorName, Color color)
    {
        nutColor = colorName;
        if (meshRenderer != null)
        {
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.material.color = color;
            originalColor = color;
        }
    }

    public void SetSelected(bool selected)
    {
        if (isSelected == selected) return;
        isSelected = selected;

        if (selected)
        {
            originalPosition = transform.position;
            transform.SetParent(null, true);

            if (currentBolt != null)
            {
                Vector3 boltTop = currentBolt.GetBoltTopPoint();
                targetHoverPosition = boltTop + new Vector3(0, hoverHeight, 0);
            }
            else
            {
                targetHoverPosition = originalPosition + new Vector3(0, hoverHeight, 0);
            }

            // Jab select ho, to ghoomne ki speed start karo
            currentRotateSpeed = rotateSpeed;

            if (meshRenderer != null)
            {
                Color brightColor = originalColor * 1.3f;
                brightColor.a = originalColor.a;
                meshRenderer.material.color = brightColor;
            }
        }
        else
        {
            transform.rotation = originalRotation;
            if (meshRenderer != null)
                meshRenderer.material.color = originalColor;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}