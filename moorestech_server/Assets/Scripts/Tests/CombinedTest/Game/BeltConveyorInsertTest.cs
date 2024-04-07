using System;
using Core.Update;
using Game.Block.Interface;
using Game.Block.Blocks.Chest;
using Game.Block.Interface;
using Game.Context;
using Game.World.Interface.DataStore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.Boot;
using Tests.Module.TestMod;
using UnityEngine;

namespace Tests.CombinedTest.Game
{
    public class BeltConveyorInsertTest
    {
        //2つのアイテムがチェストから出されてベルトコンベアに入り、全てチェストに入るテスト
        [Test]
        public void TwoItemIoTest()
        {
            var (_, saveServiceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            GameUpdater.ResetUpdate();
            
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var blockFactory = ServerContext.BlockFactory;

            var inputChestPosInfo = new BlockPositionInfo(new Vector3Int(0, 0,0), BlockDirection.North, Vector3Int.one);
            var inputChest = (VanillaChest)blockFactory.Create(UnitTestModBlockId.ChestId, 1,inputChestPosInfo);
            
            var beltPosInfo = new BlockPositionInfo(new Vector3Int(0, 0,1), BlockDirection.North, Vector3Int.one);
            var beltConveyor = blockFactory.Create(UnitTestModBlockId.BeltConveyorId, 2,beltPosInfo);
            
            var outputChestPosInfo = new BlockPositionInfo(new Vector3Int(0, 0,2), BlockDirection.North, Vector3Int.one);
            var outputChest = (VanillaChest)blockFactory.Create(UnitTestModBlockId.ChestId, 3,outputChestPosInfo);

            //それぞれを設置
            worldBlockDatastore.AddBlock(inputChest);
            worldBlockDatastore.AddBlock(beltConveyor);
            worldBlockDatastore.AddBlock(outputChest);

            //インプットチェストにアイテムを2つ入れる
            inputChest.SetItem(0, 1, 2);

            //ベルトコンベアのアイテムが出てから入るまでの6秒間アップデートする
            var now = DateTime.Now;
            while (DateTime.Now - now < TimeSpan.FromSeconds(5)) GameUpdater.UpdateWithWait();

            //アイテムが出ているか確認
            Assert.AreEqual(0, inputChest.GetItem(0).Count);
            //アイテムが入っているか確認
            Assert.AreEqual(2, outputChest.GetItem(0).Count);
        }
    }
}