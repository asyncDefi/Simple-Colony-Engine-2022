using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "SCE22/Recipe/New")]
public class Recipe : ScriptableObject
{
    public virtual string UID { get { return this.name; } }

    [field: SerializeField] public RecipeCell[] Cells { get; private set; }
    [field: SerializeField] public float WorkCost { get; private set; }

    [field: SerializeField, Space(5)] public RecipeCell Out { get; private set; }

    public static class Tools
    {
        public static bool IsEnoughtOnMapFor(Recipe recipe, int multiplier = 1)
        {
            if (recipe == null)
            {
                Debug.LogError($"Recipe [{recipe.UID}]: IsEnoughtOnMapFor called with null recipe.");
                return false;
            }

            if (recipe.Cells == null || recipe.Cells.Length == 0)
            {
                return true;
            }

            if (multiplier <= 0)
            {
                Debug.LogWarning($"Recipe [{recipe.UID}]: IsEnoughtOnMapFor called with invalid multiplier: {multiplier}. false.");
                return false;
            }

            foreach (var cell in recipe.Cells)
            {
                if (cell.ItemPrefab == null) continue;

                int requiredAmount = cell.Quantity * multiplier;

                int availableAmount = Item.Tools.AmountOfFreeFromReservation(cell.ItemPrefab);


                if (availableAmount < requiredAmount)
                {
                    return false;
                }
            }

            return true;
        }
        public static bool IsEnoughtInInventory(Inventory inventory, Recipe recipe)
        {
            foreach (var cell in recipe.Cells)
            {
                if (inventory.AmountOf(cell.ItemPrefab) < cell.Quantity)
                    return false;
            }

            return true;
        }

    }
}

[System.Serializable]
public class RecipeCell
{
    [field: SerializeField] public ItemPrefab ItemPrefab { get; private set; }
    [field: SerializeField] public int Quantity { get; private set; }
}
