using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class AlchemistCard : MonoBehaviour
{
    // references
    [SerializeField] private Button Button;
    [SerializeField] private Image Background;
    [SerializeField] private Image Portrait;
    [SerializeField] private TextMeshProUGUI NameLabel;
    [SerializeField] private TextMeshProUGUI DetailsLabel;
    [SerializeField] private TextMeshProUGUI FooterLabel;

    // functions
    public void Setup(AlchemistInfo Info, string Footer, Color BackgroundColor, UnityAction OnClick)
    {
        // showing the image, or the element color if the alchemist has no image
        Portrait.sprite = Info.image;
        if (Info.image != null)
        {
            Portrait.color = Color.white;
        }
        else
        {
            Portrait.color = UIUtils.ElementColor(Info.Element);
        }

        // filling the texts
        NameLabel.text = Info.DisplayName;
        DetailsLabel.text = UIUtils.KitLabel(Info.Kit) + " - " + UIUtils.ElementLabel(Info.Element);
        FooterLabel.text = Footer;

        Background.color = BackgroundColor;

        // the card is only clickable if an action was passed
        Button.onClick.RemoveAllListeners();
        Button.enabled = OnClick != null;

        if (OnClick != null)
        {
            Button.onClick.AddListener(OnClick);
        }
    }
}
