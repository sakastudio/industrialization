using System.Collections.Generic;
using Core.Const;
using Core.Item;
using Core.Item.Util;
using Game.PlayerInventory.Interface;
using Game.World.Interface.DataStore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server;
using Server.StartServerSystem;
using Server.Util;
using Test.Module.TestConfig;

namespace Test.CombinedTest.Server.PacketTest
{
    public class PlaceHotBarBlockProtocolTest
    {
        private const int PlacedBlockId = 1;
        private const int BlockItemId = 1;
        private const int PlayerId = 3;
        private const int HotBarSlot = 3;

        [Test]
        public void BlockPlaceTest()
        {
            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
            
            //パケットでプレイヤーインベントリを生成
            
            //ホットバーにアイテムとしてのブロックをセットする
            var slot = PlayerInventoryConst.HotBarSlotToInventorySlot(HotBarSlot);
            var inventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId);
            inventory.MainOpenableInventory.SetItem(slot, itemStackFactory.Create(BlockItemId,3));
            
            //ブロックを置く
            packet.GetPacketResponse(CreateUseHotBarProtocol(2, 4,0));
            
            //ブロックが置かれているかチェック
            var world = serviceProvider.GetService<IWorldBlockDatastore>();
            Assert.AreEqual(PlacedBlockId, world.GetBlock(2, 4).GetBlockId());
            //アイテムが減っているかチェック
            Assert.AreEqual(2, inventory.MainOpenableInventory.GetItem(slot).Count);
            
            
            
            //既にブロックがあるところに置こうとしてもアイテムが減らないテスト
            packet.GetPacketResponse(CreateUseHotBarProtocol(2, 4,0));
            //アイテムが減っていないかのチェック
            Assert.AreEqual(2,
                inventory.MainOpenableInventory.GetItem(slot).Count);
            
            //ホットバー内のアイテムを使い切る
            packet.GetPacketResponse(CreateUseHotBarProtocol(3, 4,0));
            packet.GetPacketResponse(CreateUseHotBarProtocol(4, 4,0));
            //ホットバーのアイテムが空になっているかのテスト
            Assert.AreEqual(itemStackFactory.CreatEmpty(), inventory.MainOpenableInventory.GetItem(slot));
            
            
            //さらにブロックを置こうとしても置けないテスト
            packet.GetPacketResponse(CreateUseHotBarProtocol(10, 10,0));
            Assert.AreEqual(BlockConst.EmptyBlockId, world.GetBlock(10,10).GetBlockId());
        }


        
        //ブロックの設置する向きが正しいかテスト
        [Test]
        public void PlaceDirectionTest()
        {
            var (packet, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);
            var itemStackFactory = serviceProvider.GetService<ItemStackFactory>();
            var worldBlockDatastore = serviceProvider.GetService<IWorldBlockDatastore>();
            
            //パケットでプレイヤーインベントリを生成
            
            //ホットバーにアイテムとしてのブロックをセットする
            var slot = PlayerInventoryConst.HotBarSlotToInventorySlot(HotBarSlot);
            var inventory = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(PlayerId);
            inventory.MainOpenableInventory.SetItem(slot, itemStackFactory.Create(BlockItemId,4));
            
            
            //ブロックを置く
            packet.GetPacketResponse(CreateUseHotBarProtocol(2, 4,0));
            packet.GetPacketResponse(CreateUseHotBarProtocol(2, 5,1));
            packet.GetPacketResponse(CreateUseHotBarProtocol(2, 6,2));
            packet.GetPacketResponse(CreateUseHotBarProtocol(2, 7,3));
            
            //ブロックの向きをチェック
            Assert.AreEqual(BlockDirection.North,worldBlockDatastore.GetBlockDirection(2,4));
            Assert.AreEqual(BlockDirection.East,worldBlockDatastore.GetBlockDirection(2,5));
            Assert.AreEqual(BlockDirection.South,worldBlockDatastore.GetBlockDirection(2,6));
            Assert.AreEqual(BlockDirection.West,worldBlockDatastore.GetBlockDirection(2,7));
        }

        private List<byte> CreateUseHotBarProtocol(int x,int y,byte blockDirection)
        {
            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 8));
            payload.AddRange(ToByteList.Convert((short) HotBarSlot));
            payload.AddRange(ToByteList.Convert(x));
            payload.AddRange(ToByteList.Convert(y));
            payload.AddRange(ToByteList.Convert(PlayerId));
            payload.Add(blockDirection);
            return payload;
        }


    }
}