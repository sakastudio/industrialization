using System;
using System.Collections.Generic;
using Core.Item;
using Core.Item.Config;
using Game.Crafting.Interface;
using Game.PlayerInventory.Interface;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server;
using Server.StartServerSystem;
using Server.Util;
using Test.Module.TestConfig;

namespace Test.CombinedTest.Server.PacketTest
{
    public class CraftProtocolTest
    {
        private const short PacketId = 14;
        private const int PlayerId = 1;
        private const int TestCraftItemId = 6;
        
        [Test]
        public void CraftTest()
        {
            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);
            //クラフトインベントリの作成
            var craftInventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId).CraftingOpenableInventory;
            var grabInventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId).GrabInventory;
            //CraftConfigの作成
            var craftConfig = serviceProvider.GetService<ICraftingConfig>().GetCraftingConfigList()[0];
            
            //craftingInventoryにアイテムを入れる
            for (int i = 0; i < craftConfig.Items.Count; i++)
            {
                craftInventory.SetItem(i,craftConfig.Items[i]);
            }
            
            
            
            //プロトコルでクラフト実行
            var payLoad = new List<byte>();
            payLoad.AddRange(ToByteList.Convert(PacketId));
            payLoad.AddRange(ToByteList.Convert(PlayerId));
            payLoad.Add(0);
            packet.GetPacketResponse(payLoad);
            
            
            //クラフト結果がResultSlotにアイテムが入っているかチェック
            Assert.AreEqual(craftConfig.Result,grabInventory.GetItem(0));
        }

        [Test]
        public void AllCraftTest()
        {
            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);
            
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
            var itemConfig = serviceProvider.GetService<IItemConfig>();
            var craftInventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId).CraftingOpenableInventory;
            var mainInventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId).MainOpenableInventory;
            var craftConfig = serviceProvider.GetService<ICraftingConfig>().GetCraftingConfigList()[2]; //id2のレシピはこのテスト用のレシピ
            
            //craftingInventoryに2つ分のアイテムを入れる
            craftInventory.SetItem(0,itemStackFactory.Create(TestCraftItemId,2));
            
            
            
            //プロトコルでクラフト実行
            var payLoad = new List<byte>();
            payLoad.AddRange(ToByteList.Convert(PacketId));
            payLoad.AddRange(ToByteList.Convert(PlayerId));
            payLoad.Add(1);
            packet.GetPacketResponse(payLoad);
            
            
            //クラフト結果がメインインベントリにアイテムが入っているかチェック
            Assert.AreEqual(craftConfig.Result.Id,mainInventory.GetItem(0 ).Id);
            Assert.AreEqual(100,mainInventory.GetItem(0 ).Count);
            Assert.AreEqual(60,mainInventory.GetItem(1 ).Count);
        }

        [Test]
        public void OneStackCraftTest()
        {
            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);
            
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
            var craftInventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId).CraftingOpenableInventory;
            var mainInventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId).MainOpenableInventory;
            var craftConfig = serviceProvider.GetService<ICraftingConfig>().GetCraftingConfigList()[2]; //id2のレシピはこのテスト用のレシピ
            
            //craftingInventoryに2つ分のアイテムを入れる
            craftInventory.SetItem(0,itemStackFactory.Create(TestCraftItemId,2));
            
            
            
            //プロトコルでクラフト実行
            var payLoad = new List<byte>();
            payLoad.AddRange(ToByteList.Convert(PacketId));
            payLoad.AddRange(ToByteList.Convert(PlayerId));
            payLoad.Add(2);
            packet.GetPacketResponse(payLoad);
            
            
            //クラフト結果がメインインベントリにアイテムが入っているかチェック
            Assert.AreEqual(craftConfig.Result.Id,mainInventory.GetItem(0 ).Id);
            Assert.AreEqual(80,mainInventory.GetItem(0 ).Count);
        }
    }
}