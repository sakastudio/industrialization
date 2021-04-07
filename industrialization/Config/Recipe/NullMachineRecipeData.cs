﻿using industrialization.Item;

namespace industrialization.Config.Recipe
{
    public class NullMachineRecipeData : IMachineRecipeData
    {
        public IItemStack[] ItemInputs => System.Array.Empty<IItemStack>();
        public ItemOutput[] ItemOutputs => System.Array.Empty<ItemOutput>();
        public double Time => 0;

        public bool RecipeConfirmation(IItemStack[] inputSlot)
        {
            return false;
        }
    }
}