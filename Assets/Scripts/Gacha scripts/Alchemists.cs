using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Alchemists : MonoBehaviour
{
    public AlchemistInfo Info;
    [SerializeField] private TextMeshProUGUI RewardText;
    [SerializeField] private Image RewardImage;
    public TextMeshProUGUI rewardRarity;
    
    void Start()
    {
        
    }

    void GetReward()
    {
        RewardImage.sprite = Info.image;
        RewardText.text = Info.charName;
    }
    void Update()
    {
        GetReward();
    }
}
