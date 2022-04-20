using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Item.Util;
using Game.PlayerInventory.Interface;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PlayerInventory;
using Server;
using Server.Event;
using Server.Util;
using Test.Module.TestConfig;
using World.Event;

namespace Test.CombinedTest.Server.PacketTest.Event
{
    public class PlayerMainInventoryUpdateTest
    {
        private const int PlayerId = 0;
        [Test]
        public void UpdateTest()
        {
            var (packetResponse, serviceProvider) = new PacketResponseCreatorDiContainerGenerators().Create(TestModuleConfigPath.FolderPath);

            var response = packetResponse.GetPacketResponse(EventRequestData(0));
            Assert.AreEqual(0, response.Count);

            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 3));
            payload.AddRange(ToByteList.Convert(0));
            packetResponse.GetPacketResponse(payload);

            //インベントリにアイテムを追加
            var playerInventoryData = serviceProvider.GetService<IPlayerInventoryDataStore>().GetInventoryData(0);
            playerInventoryData.MainOpenableInventory.SetItem(5, serviceProvider.GetService<ItemStackFactory>().Create(1, 5));
            
            //追加時のイベントのキャッチ
            response = packetResponse.GetPacketResponse(EventRequestData(0));
            Assert.AreEqual(1, response.Count);
            //チェック
            var byteData = new ByteListEnumerator(response[0].ToList());
            byteData.MoveNextToGetShort();
            Assert.AreEqual(1, byteData.MoveNextToGetShort());
            Assert.AreEqual(5, byteData.MoveNextToGetInt());
            Assert.AreEqual(1, byteData.MoveNextToGetInt());
            Assert.AreEqual(5, byteData.MoveNextToGetInt());
            
            
            
            

            //インベントリ内のアイテムの移動を実際に移動のプロトコルを用いてテストする
            //分割のイベントのテスト
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(true,5,  3));
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(false,4, 3));
            
            response = packetResponse.GetPacketResponse(EventRequestData(0));
            
            Assert.AreEqual(2, response.Count);
            var byteData1 = new ByteListEnumerator(response[0].ToList());
            var byteData2 = new ByteListEnumerator(response[1].ToList());
            byteData1.MoveNextToGetShort();
            byteData2.MoveNextToGetShort();

            Assert.AreEqual(1, byteData1.MoveNextToGetShort()); //イベントIDの確認
            Assert.AreEqual(1, byteData2.MoveNextToGetShort());

            Assert.AreEqual(5,byteData1.MoveNextToGetInt()); //移動時のスロット確認
            Assert.AreEqual(4,byteData2.MoveNextToGetInt());

            Assert.AreEqual(1, byteData1.MoveNextToGetInt()); //アイテムIDの確認
            Assert.AreEqual(1, byteData2.MoveNextToGetInt());

            Assert.AreEqual(2,byteData1.MoveNextToGetInt()); //アイテム数の確認
            Assert.AreEqual(3,byteData2.MoveNextToGetInt());

            
            
            
            

            //合成のテスト
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(true,4,  3));
            packetResponse.GetPacketResponse(PlayerInventoryItemMove(false,5, 3));
            
            response = packetResponse.GetPacketResponse(EventRequestData(0));
            
            Assert.AreEqual(2, response.Count);
            byteData1 = new ByteListEnumerator(response[0].ToList());
            byteData2 = new ByteListEnumerator(response[1].ToList());
            byteData1.MoveNextToGetShort();
            byteData2.MoveNextToGetShort();
            Assert.AreEqual(1, byteData1.MoveNextToGetShort());
            Assert.AreEqual(1, byteData2.MoveNextToGetShort()); //イベントIDの確認

            Assert.AreEqual(4,byteData1.MoveNextToGetInt());
            Assert.AreEqual(5,byteData2.MoveNextToGetInt()); //移動時のスロット確認

            Assert.AreEqual(0,byteData1.MoveNextToGetInt());
            Assert.AreEqual(1,byteData2.MoveNextToGetInt()); //アイテムIDの確認

            Assert.AreEqual(0,byteData1.MoveNextToGetInt());
            Assert.AreEqual(5,byteData2.MoveNextToGetInt()); //アイテム数の確認
        }


        List<byte> EventRequestData(int plyaerID)
        {
            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 4));
            payload.AddRange(ToByteList.Convert(plyaerID));
            return payload;
        }
        private List<byte> PlayerInventoryItemMove(bool toGrab,int inventorySlot,int itemCount)
        {
            var payload = new List<byte>();
            payload.AddRange(ToByteList.Convert((short) 5));
            payload.Add(toGrab ? (byte) 0 : (byte) 1);
            payload.Add(0);
            payload.AddRange(ToByteList.Convert(PlayerId));
            payload.AddRange(ToByteList.Convert(inventorySlot));
            payload.AddRange(ToByteList.Convert(itemCount));
            payload.AddRange(ToByteList.Convert(0));
            payload.AddRange(ToByteList.Convert(0));

            return payload;
        }
    }
}