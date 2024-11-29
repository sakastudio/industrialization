using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using UnitGenerator;

namespace Game.CraftChainer.CraftChain
{
    public class CraftingSolverRecipe
    {
        public readonly CraftingSolverRecipeId CraftingSolverRecipeId;
        public readonly List<CraftingSolverItem> Inputs;
        public readonly List<CraftingSolverItem> Outputs;
        
        public CraftingSolverRecipe(CraftingSolverRecipeId craftingSolverRecipeId, List<CraftingSolverItem> inputs, List<CraftingSolverItem> outputs)
        {
            CraftingSolverRecipeId = craftingSolverRecipeId;
            Inputs = inputs;
            Outputs = outputs;
        }
    }
    
    [UnitOf(typeof(int), UnitGenerateOptions.Comparable)]
    public partial struct CraftingSolverRecipeId
    {
        private static readonly Random Random = new();
        public static CraftingSolverRecipeId Create()
        {
            // 1 〜 int.Max
            var id = Random.Next(1, int.MaxValue);
            return new CraftingSolverRecipeId(id);
        }
    }
    
    [JsonObject, MessagePackObject]
    public class CraftingSolverRecipeSerializeObject
    {
        [JsonProperty("recipeId"), Key(0)] public int RecipeId;
        [JsonProperty("inputs"), Key(1)] public List<CraftingSolverItemSerializeObject> Inputs;
        [JsonProperty("outputs"), Key(2)] public List<CraftingSolverItemSerializeObject> Outputs;
        
        public CraftingSolverRecipeSerializeObject() { }

        public CraftingSolverRecipeSerializeObject(CraftingSolverRecipe craftingSolverRecipe)
        {
            RecipeId = craftingSolverRecipe.CraftingSolverRecipeId.AsPrimitive();
            Inputs = new List<CraftingSolverItemSerializeObject>();
            foreach (var input in craftingSolverRecipe.Inputs)
            {
                Inputs.Add(new CraftingSolverItemSerializeObject(input));
            }
            Outputs = new List<CraftingSolverItemSerializeObject>();
            foreach (var output in craftingSolverRecipe.Outputs)
            {
                Outputs.Add(new CraftingSolverItemSerializeObject(output));
            }
        }
        
        public CraftingSolverRecipe ToCraftingSolverRecipe()
        {
            var inputs = new List<CraftingSolverItem>();
            foreach (var input in Inputs)
            {
                inputs.Add(input.ToCraftingSolverItem());
            }
            var outputs = new List<CraftingSolverItem>();
            foreach (var output in Outputs)
            {
                outputs.Add(output.ToCraftingSolverItem());
            }
            
            return new CraftingSolverRecipe(new CraftingSolverRecipeId(RecipeId), inputs, outputs);
        }
    }
}