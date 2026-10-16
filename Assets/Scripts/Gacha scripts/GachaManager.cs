using UnityEngine;
using System;
public class GachaManager : MonoBehaviour
{
    [SerializeField] private GachaRates[] rarities;
    [SerializeField] private Transform parent, pos;
    [SerializeField] private GameObject alchemistGameObject;
    private GameObject alchemist;
    Alchemists card;


    public void Gacha()
    {
        if (alchemist == null)
        {
            alchemist = Instantiate(alchemistGameObject, pos.position, Quaternion.identity) as GameObject;
            alchemist.transform.SetParent(parent);
            alchemist.transform.localScale = new Vector3(1, 1, 1);
            card = alchemist.GetComponent<Alchemists>();
        }
            int random = UnityEngine.Random.Range(1, 101);
        Debug.Log(random);
            for(int i = 0;i < rarities.Length; i++)
            {
                if (random <= rarities[i].rate)
                {
                    card.Info = Reward(rarities[i].rarity);
                    card.rewardRarity.text = rarities[i].rarity;
                    return;
                }
            }
        
    }
    
    public AlchemistInfo Reward(string rarity)
    {
        GachaRates gr = Array.Find(rarities, rt => rt.rarity == rarity);
        AlchemistInfo[] reward = gr.reward;

        int rnd = UnityEngine.Random.Range(0, reward.Length);
        return reward[rnd];
    }

}


