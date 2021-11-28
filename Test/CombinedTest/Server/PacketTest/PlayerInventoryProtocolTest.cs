using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Item.Util;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PlayerInventory;
using Server;
using Server.PacketHandle;
using Server.Util;

namespace Test.CombinedTest.Server.PacketTest
{
    public class PlayerInventoryProtocolTest
    {
        [Test]
        public void GetPlayerInventoryProtocolTest()
        {
            int playerId = 1;
            
            var (packet, serviceProvider) = PacketResponseCreatorGenerators.Create();
            
            
            //1回目のデータ要求
            var payload = new List<byte>();
            payload.AddRange(ByteListConverter.ToByteArray((short)3));
            payload.AddRange(ByteListConverter.ToByteArray(playerId));

            var response = new ByteArrayEnumerator(packet.GetPacketResponse(payload)[0].ToList());

            Assert.AreEqual(4,response.MoveNextToGetShort());
            Assert.AreEqual(playerId,response.MoveNextToGetInt());
            Assert.AreEqual(0,response.MoveNextToGetShort());
            for (int i = 0; i < PlayerInventoryConst.MainInventoryColumns; i++)
            {
                Assert.AreEqual(ItemConst.NullItemId,response.MoveNextToGetInt());
                Assert.AreEqual(0,response.MoveNextToGetInt());
            }

            var playerInventoryData = serviceProvider.GetService<PlayerInventoryDataStore>().GetInventoryData(playerId);
            playerInventoryData.InsertItem(0,ItemStackFactory.Create(1, 5));
            playerInventoryData.InsertItem(20,ItemStackFactory.Create(3, 1));
            playerInventoryData.InsertItem(34,ItemStackFactory.Create(10, 7));
            
            
            response = new ByteArrayEnumerator(packet.GetPacketResponse(payload)[0].ToList());
            Assert.AreEqual(4,response.MoveNextToGetShort());
            Assert.AreEqual(playerId,response.MoveNextToGetInt());
            Assert.AreEqual(0,response.MoveNextToGetShort());

            //データの検証
            for (int i = 0; i < PlayerInventoryConst.MainInventoryColumns; i++)
            {
                if (i == 0)
                {
                    Assert.AreEqual(1,response.MoveNextToGetInt());
                    Assert.AreEqual(5,response.MoveNextToGetInt());
                }else if (i == 20)
                {
                    Assert.AreEqual(3,response.MoveNextToGetInt());
                    Assert.AreEqual(1,response.MoveNextToGetInt());
                }else if (i == 34)
                {
                    Assert.AreEqual(10,response.MoveNextToGetInt());
                    Assert.AreEqual(7,response.MoveNextToGetInt());
                }
                else
                {
                    Assert.AreEqual(ItemConst.NullItemId,response.MoveNextToGetInt());
                    Assert.AreEqual(0,response.MoveNextToGetInt());
                }
            }


        }
    }
}