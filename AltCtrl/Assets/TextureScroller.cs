using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    public Renderer targetRenderer; // Renderer du plane
    public Vector2 scrollDirection = Vector2.right; // Direction du scroll
    public float scrollSpeed = 1f; // Vitesse actuelle
    public float targetSpeed = 1f; // Vitesse souhaitée
    public float smoothTime = 1f; // Temps pour atteindre la vitesse cible

    private float currentSpeed;
    private Vector2 currentOffset;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        currentSpeed = scrollSpeed;
    }

    void Update()
    {
        // Lissage de la vitesse vers la targetSpeed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);

        // Mise à jour de l'offset
        currentOffset += scrollDirection * currentSpeed * Time.deltaTime;

        // Appliquer l'offset à la texture
        targetRenderer.material.mainTextureOffset = currentOffset;
    }

    // Optionnel : méthode pour changer de vitesse dynamiquement
    public void SetTargetSpeed(float newSpeed)
    {
        targetSpeed = newSpeed;
    }
}