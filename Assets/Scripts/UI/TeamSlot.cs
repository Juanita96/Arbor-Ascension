using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class TeamSlot : MonoBehaviour, IPointerClickHandler
{
    // variables
    [SerializeField] private Color LeaderColor = new Color(0.90f, 0.74f, 0.30f);
    [SerializeField] private Color NormalColor = new Color(0.22f, 0.25f, 0.31f);

    private UnityAction LeaderAction;
    private UnityAction RemoveAction;

    // references
    [SerializeField] private GameObject FilledRoot;
    [SerializeField] private TextMeshProUGUI EmptyLabel;
    [SerializeField] private Image Portrait;
    [SerializeField] private TextMeshProUGUI NameLabel;
    [SerializeField] private TextMeshProUGUI DetailsLabel;
    [SerializeField] private Button LeaderButton;
    [SerializeField] private TextMeshProUGUI LeaderLabel;
    [SerializeField] private Button RemoveButton;

    // functions
    public void ShowEmpty(string Text)
    {
        // hiding the alchemist info and showing the empty text
        FilledRoot.SetActive(false);
        EmptyLabel.gameObject.SetActive(true);
        EmptyLabel.text = Text;

        LeaderAction = null;
        RemoveAction = null;
    }

    public void Show(AlchemistInfo Info, string Prefix, bool IsLeader, UnityAction OnLeader, UnityAction OnRemove)
    {
        // showing the alchemist info
        FilledRoot.SetActive(true);
        EmptyLabel.gameObject.SetActive(false);

        Portrait.sprite = Info.image;
        if (Info.image != null)
        {
            Portrait.color = Color.white;
        }
        else
        {
            Portrait.color = UIUtils.ElementColor(Info.Element);
        }

        NameLabel.text = Prefix + Info.DisplayName;
        DetailsLabel.text = UIUtils.KitLabel(Info.Kit) + " - " + UIUtils.ElementLabel(Info.Element);

        // highlighting the leader button
        if (IsLeader)
        {
            LeaderLabel.text = "LEADER";
            LeaderButton.targetGraphic.color = LeaderColor;
        }
        else
        {
            LeaderLabel.text = "MAKE LEADER";
            LeaderButton.targetGraphic.color = NormalColor;
        }

        // saving the actions for the mouse clicks
        LeaderAction = OnLeader;
        RemoveAction = OnRemove;

        // connecting the buttons
        LeaderButton.onClick.RemoveAllListeners();
        LeaderButton.onClick.AddListener(OnLeader);
        RemoveButton.onClick.RemoveAllListeners();
        RemoveButton.onClick.AddListener(OnRemove);
    }

    // events
    public void OnPointerClick(PointerEventData EventData)
    {
        // left click marks as leader, right click removes from the team
        if (EventData.button == PointerEventData.InputButton.Left && LeaderAction != null)
        {
            LeaderAction();
        }
        else if (EventData.button == PointerEventData.InputButton.Right && RemoveAction != null)
        {
            RemoveAction();
        }
    }
}
