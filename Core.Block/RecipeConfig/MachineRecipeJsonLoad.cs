﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Core.Block.Config;
using Core.Block.RecipeConfig.Data;
using Core.Item;
using Newtonsoft.Json;

namespace Core.Block.RecipeConfig
{
    internal class MachineRecipeJsonLoad
    {
        internal List<IMachineRecipeData> LoadConfig(IBlockConfig blockConfig,ItemStackFactory itemStackFactory,List<string> configJsons)
        {
            var recipes = new List<IMachineRecipeData>();
            foreach (var json in configJsons)
            {
                recipes.AddRange(Load(blockConfig,itemStackFactory,json));
            }

            return recipes;
        }

        private List<IMachineRecipeData> Load(IBlockConfig blockConfig,ItemStackFactory itemStackFactory,string json)
        {
            //JSONデータの読み込み
            var data = JsonConvert.DeserializeObject<MachineRecipeJsonData[]>(json);

            //レシピデータを実際に使用する形式に変換
            var r = data.ToList().Select((r, index) =>
            {
                var inputItem =
                    r.ItemInputs.ToList().Select(item => itemStackFactory.Create(item.ModId,item.ItemName, item.Count)).ToList();


                inputItem = inputItem.OrderBy(i => i.Id).ToList();

                var outputs =
                    r.ItemOutputs.Select(r => new ItemOutput(itemStackFactory.Create(r.ModId,r.ItemName, r.Count), r.Percent));

                var modId = r.BlockModId;
                var blockName = r.BlockName;

                var blockId = blockConfig.GetBlockConfig(modId, blockName).BlockId;
                
                return (IMachineRecipeData) new MachineRecipeData(blockId, r.Time, inputItem, outputs.ToList(),
                    index);
            });

            return r.ToList();
        }
    }

    //JSONからのデータを格納するためのクラス
    [JsonObject]
    class MachineRecipeJsonData
    {
        [JsonProperty("blockModId")] private string _blockModId;
        [JsonProperty("blockName")] private string _blockName;
        [JsonProperty("time")] private int _time;
        [JsonProperty("input")] private MachineRecipeInput[] _itemInputs;
        [JsonProperty("output")] private MachineRecipeOutput[] _itemOutputs;

        public MachineRecipeOutput[] ItemOutputs => _itemOutputs;

        public MachineRecipeInput[] ItemInputs => _itemInputs;

        public int Time => _time;
        public string BlockName => _blockName;
        public string BlockModId => _blockModId;
    }

    [JsonObject]
    internal class MachineRecipeInput
    {
        [JsonProperty("modId")] private string _modId;
        [JsonProperty("itemName")] private string _itemName;
        [JsonProperty("count")] private int _count;

        public int Count => _count;

        public string ItemName => _itemName;

        public string ModId => _modId;
    }

    [JsonObject]
    internal class MachineRecipeOutput
    {
        [JsonProperty("modId")] private string _modId;
        [JsonProperty("itemName")] private string _itemName;
        [JsonProperty("count")] private int _count;
        [JsonProperty("percent")] private double _percent;

        public double Percent => _percent;

        public int Count => _count;

        public string ItemName => _itemName;

        public string ModId => _modId;
    }
}