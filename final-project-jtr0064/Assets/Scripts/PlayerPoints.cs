using UnityEngine;

// Points earned from resource dropoffs (1 per resource), spent on PlayerUpgrades, and
// occasionally stolen by hazards like NightWisp.
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

    // Unlike TrySpend, this always takes what it can - a hazard hit isn't something the
    // player can decline, it just floors at 0 instead of going negative.
    public void RemovePoints(int amount)
    {
        if (amount <= 0) {
            return;
        }

        points = Mathf.Max(0, points - amount);
    }
}
