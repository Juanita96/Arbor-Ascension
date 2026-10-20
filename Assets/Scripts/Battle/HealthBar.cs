using UnityEngine;

[RequireComponent(typeof(Health))]
public class HealthBar : MonoBehaviour
{
    // variables
    [SerializeField] private Vector2 Offset = new Vector2(0f, 1f);
    [SerializeField] private Vector2 Size = new Vector2(1f, 0.12f);
    [SerializeField] private Color FillColor = new Color(0.35f, 0.85f, 0.35f);
    [SerializeField] private Color BackColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private bool HideWhenFull = true;

    private static Sprite WhiteSprite;

    // references
    private Health Health;
    private Transform Root;
    private Transform Fill;

    // functions
    private void Awake()
    {
        Health = GetComponent<Health>();

        // creating the bar above the unit
        Root = new GameObject("HealthBar").transform;
        Root.SetParent(transform, false);
        Root.localPosition = Offset;

        CreateBar("Back", BackColor, 10);
        Fill = CreateBar("Fill", FillColor, 11);

        // listening to health changes
        Health.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(Health.Current, Health.Max);
    }

    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private Transform CreateBar(string BarName, Color BarColor, int Order)
    {
        // creating a sprite scaled to the bar size
        GameObject Bar = new GameObject(BarName);
        Bar.transform.SetParent(Root, false);
        Bar.transform.localScale = new Vector3(Size.x, Size.y, 1f);

        SpriteRenderer BarRenderer = Bar.AddComponent<SpriteRenderer>();
        BarRenderer.sprite = GetWhiteSprite();
        BarRenderer.color = BarColor;
        BarRenderer.sortingOrder = Order;

        return Bar.transform;
    }

    public static Sprite GetWhiteSprite()
    {
        // creating a 1x1 white sprite only once and sharing it
        if (WhiteSprite == null)
        {
            Texture2D Texture = new Texture2D(1, 1);
            Texture.SetPixel(0, 0, Color.white);
            Texture.Apply();
            WhiteSprite = Sprite.Create(Texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        return WhiteSprite;
    }

    // events
    private void HandleHealthChanged(float Current, float Max)
    {
        // getting how full the bar should be
        float Ratio = 0f;
        if (Max > 0f)
        {
            Ratio = Mathf.Clamp01(Current / Max);
        }

        // the sprite pivot is in the center, so it's moved to empty the bar to the left
        Fill.localScale = new Vector3(Size.x * Ratio, Size.y, 1f);
        Fill.localPosition = new Vector3(-Size.x * (1f - Ratio) * 0.5f, 0f, 0f);

        // hiding the bar when the unit has full health
        bool IsFull = Ratio >= 1f;
        Root.gameObject.SetActive(!(HideWhenFull && IsFull));
    }
}
