using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    // variables
    [SerializeField] private float MaxHealth = 100f;

    public float Max => MaxHealth;
    public float Current { get; private set; }
    public bool IsDead { get; private set; }

    // 1 means normal damage, 0.5 means half damage (used by the defense shield)
    public float DamageTakenMultiplier { get; set; } = 1f;

    // functions
    private void Awake()
    {
        // starting with full health
        Current = MaxHealth;
    }

    public void SetMax(float NewMax, bool Refill = true)
    {
        MaxHealth = NewMax;

        // refilling or clamping the current health to the new max
        if (Refill)
        {
            Current = NewMax;
        }
        else
        {
            Current = Mathf.Min(Current, NewMax);
        }

        IsDead = false;
        NotifyChanged();
    }

    public void TakeDamage(float Amount)
    {
        // ignoring damage when already dead or when the amount is not valid
        if (IsDead || Amount <= 0f)
        {
            return;
        }

        // applying the damage with the current multiplier
        Current = Mathf.Max(0f, Current - Amount * DamageTakenMultiplier);
        NotifyChanged();

        // dying when health reaches zero
        if (Current <= 0f)
        {
            IsDead = true;

            if (OnDeath != null)
            {
                OnDeath.Invoke(this);
            }
        }
    }

    public void Heal(float Amount)
    {
        // dead units can't be healed
        if (IsDead || Amount <= 0f)
        {
            return;
        }

        // healing without going over the max
        Current = Mathf.Min(MaxHealth, Current + Amount);
        NotifyChanged();
    }

    public void HealPercent(float Percent)
    {
        // healing a percentage of the max health
        Heal(MaxHealth * Percent);
    }

    private void NotifyChanged()
    {
        // telling health bars and ui that the value changed
        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(Current, MaxHealth);
        }
    }

    // events
    public event Action<Health> OnDeath;
    public event Action<float, float> OnHealthChanged;
}
