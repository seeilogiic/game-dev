using UnityEngine;

// Points earned from resource dropoffs (1 per resource), spent on PlayerUpgrades.
public class PlayerPoints : MonoBehaviour
{
    public int points;

    public void AddPoints(int amount)
    {
        if (amount <= 0) {
            return;
        }

        points += amount;
    }

    public bool TrySpend(int cost)
    {
        if (points < cost) {
            return false;
        }

        points -= cost;
        return true;
    }
}
