using System.Collections.Generic;
using System.Linq;
using Core.Block;
using Core.Block.Machine;
using Core.Block.Machine.util;
using Core.Item;
using Core.Item.Util;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PlayerInventory;
using Server.Util;
using World;

namespace Test.CombinedTest.Server.PacketTest
{
    public class BlockInventoryPlayerInventoryMoveItemProtocolTest
    {
        [Test]
        public void ItemMoveTest()
        {
            int playerId = 1;
            int playerSlotIndex = 2;
            int blockInventorySlotIndex = 0;
            
            //初期設定----------------------------------------------------------
            
            var (packet, serviceProvider) = PacketResponseCreatorGenerators.Create();
            //ブロックの設置
            var blockDataStore = serviceProvider.GetService<WorldBlockDatastore>();
            var block = NormalMachineFactory.Create(1,1,new NullIBlockInventory());
            blockDataStore.AddBlock(block, 0, 0, block);
            //ブロックにアイテムを挿入
            block.InsertItem(ItemStackFactory.Create(1,5));
            Assert.AreEqual(1,block.InputSlot[blockInventorySlotIndex].Id);
            Assert.AreEqual(5,block.InputSlot[blockInventorySlotIndex].Amount);
            
            //プレイヤーのインベントリの設定
            var payload = new List<byte>();
            payload.AddRange(ByteListConverter.ToByteArray((short)3));
            payload.AddRange(ByteListConverter.ToByteArray(playerId));
            packet.GetPacketResponse(payload);
            var playerInventoryData = serviceProvider.GetService<PlayerInventoryDataStore>().GetInventoryData(playerId);
            
            
            //実際にアイテムを移動するテスト--------------------------------------------------------
            
            //ブロックインベントリからプレイヤーインベントリへアイテムを移す
            packet.GetPacketResponse(CreateReplacePayload(1,playerId,playerSlotIndex,0,0,blockInventorySlotIndex,5));
            //実際に移動できたか確認
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Id,ItemConst.NullItemId);
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Amount,0);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Id,1);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Amount,5);
            
            
            
            //プレイヤーインベントリからブロックインベントリへアイテムを移す
            packet.GetPacketResponse(CreateReplacePayload(0,playerId,playerSlotIndex,0,0,blockInventorySlotIndex,5));
            //きちんと移動できたか確認
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Id,1);
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Amount,5);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Id,ItemConst.NullItemId);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Amount,0);
            
            
            
            //別のアイテムIDが在ったとき、全て選択していれば入れ替える
            //別IDのアイテム挿入
            playerInventoryData.InsertItem(playerSlotIndex, ItemStackFactory.Create(2,3));
            //プレイヤーインベントリからブロックインベントリへ全てのアイテムを移す
            packet.GetPacketResponse(CreateReplacePayload(0,playerId,playerSlotIndex,0,0,blockInventorySlotIndex,3));
            //きちんと移動できたか確認
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Id,2);
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Amount,3);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Id,1);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Amount,5);
            
            
            
            //ブロックから一部だけ移動させようとしても移動できないテスト
            packet.GetPacketResponse(CreateReplacePayload(0,playerId,playerSlotIndex,0,0,blockInventorySlotIndex,3));
            //移動できてないかの確認
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Id,2);
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Amount,3);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Id,1);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Amount,5);
            
            //一部だけ移動させようとしても移動できないテスト
            packet.GetPacketResponse(CreateReplacePayload(1,playerId,playerSlotIndex,0,0,blockInventorySlotIndex,2));
            //移動できてないかの確認
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Id,2);
            Assert.AreEqual(block.InputSlot[blockInventorySlotIndex].Amount,3);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Id,1);
            Assert.AreEqual(playerInventoryData.GetItem(playerSlotIndex).Amount,5);
        }

        private List<byte> CreateReplacePayload(short playerToBlockFlag,int playerId,int playerSlotIndex,int x,int y,int blockSlotIndex,int moveItemNum)
        {
            var payload = new List<byte>();
            payload.AddRange(ByteListConverter.ToByteArray((short)3));
            payload.AddRange(ByteListConverter.ToByteArray(playerToBlockFlag)); //ブロック→プレイヤーのフラグ
            payload.AddRange(ByteListConverter.ToByteArray(playerId));
            payload.AddRange(ByteListConverter.ToByteArray(playerSlotIndex)); //プレイヤーインベントリの移動先スロット
            payload.AddRange(ByteListConverter.ToByteArray(x)); //ブロックX座標
            payload.AddRange(ByteListConverter.ToByteArray(y)); //ブロックY座標
            payload.AddRange(ByteListConverter.ToByteArray(blockSlotIndex)); //ブロックインベントリインデクス
            payload.AddRange(ByteListConverter.ToByteArray(moveItemNum)); //移動するアイテム数
            return payload;
        }
    }
}