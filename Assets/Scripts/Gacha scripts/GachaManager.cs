using UnityEngine;
using System;
public class GachaManager : MonoBehaviour
{
    [SerializeField] private GachaRates[] rarities;
    [SerializeField] private Transform parent, pos;
    [SerializeField] private GameObject alchemistGameObject;
    private GameObject alchemist;
    Alchemists card;

    // (alquimista, esNuevo, rareza). Lo escucha GachaHUD para mostrar el resultado.
    public static event Action<AlchemistInfo, bool, string> OnPulled;
    public static event Action OnNotEnoughGoldDust;

    public void Gacha()
    {
        // Cada tirada cuesta Gold Dust
        if (!PlayerInventory.Instance.TrySpendGoldDust(GameConfig.Instance.PullCost))
        {
            OnNotEnoughGoldDust?.Invoke();
            return;
        }

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

                    // Lo agrega al inventario. Si es duplicado, da Gold Dust.
                    bool isNew = PlayerInventory.Instance.AddCharacter(card.Info);
                    OnPulled?.Invoke(card.Info, isNew, rarities[i].rarity);
                    return;
                }
            }

    }

    public AlchemistInfo Reward(string rarity)
    {
        GachaRates gr = Array.Find(rarities, rt => rt.rarity == rarity);

        // El alquimista de reserva no sale en el gacha: se desbloquea con el nivel 2 del árbol
        AlchemistInfo[] reward = Array.FindAll(gr.reward, info => info != null && !info.IsReserveOnly);
        if (reward.Length == 0) reward = Array.FindAll(rarities[0].reward, info => info != null && !info.IsReserveOnly);

        int rnd = UnityEngine.Random.Range(0, reward.Length);
        return reward[rnd];
    }

}

