﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Block;
using Core.Block.Config;
using Core.Block.Machine;
using Core.Block.Machine.util;
using Core.Electric;
using Core.Item;
using Core.Item.Implementation;
using Core.Item.Util;
using Core.Update;
using NUnit.Framework;
using Test.CombinedTest.Core.Generate;
using Test.Util;

namespace Test.CombinedTest.Core
{
    public class MachineIoTest
    {
        [TestCase(true,new int[1]{1}, new int[1]{1})]
        [TestCase(true,new int[2]{100,101}, new int[2]{10,10})]
        [TestCase(true,new int[3]{10,11,15}, new int[3]{10,5,8})]
        [TestCase(false,new int[3]{0,5,1}, new int[3]{10,5,8})]
        [TestCase(false,new int[2]{1,0}, new int[2]{10,5})]
        [TestCase(false,new int[2]{0,0}, new int[2]{10,5})]
        public void MachineInputTest(bool isEquals,int[] id,int[] amount)
        {
            var machine = NormalMachineFactory.Create(4, Int32.MaxValue, new DummyBlockInventory(1),new TestBlockConfig());
            var items = new List<IItemStack>();
            for (int i = 0; i < id.Length; i++)
            {
                items.Add(ItemStackFactory.Create(id[i], amount[i]));
            }

            foreach (var item in items)
            {
                machine.InsertItem(item);
            }

            if (isEquals)
            {
                Assert.AreEqual(items.ToArray(), machine.InputSlotWithoutNullItemStack.ToArray());   
            }
            else
            {
                Assert.AreNotEqual(items.ToArray(), machine.InputSlotWithoutNullItemStack.ToArray());
            }
            
        }
        [TestCase(new int[2]{1,1}, new int[2]{1,1}, new int[1]{1}, new int[1]{2})]
        [TestCase(new int[2]{3,1}, new int[2]{1,1}, new int[2]{1,3}, new int[2]{1,1})]
        public void MachineAddInputTest(int[] id,int[] amount,int[] ansid,int[] ansamount)
        {
            var machine = NormalMachineFactory.Create(4,Int32.MaxValue,new DummyBlockInventory(),new TestBlockConfig());
            for (int i = 0; i < id.Length; i++)
            {
                machine.InsertItem(ItemStackFactory.Create(id[i], amount[i]));
            }

            var ansItem = new List<IItemStack>();
            for (int i = 0; i < ansid.Length; i++)
            {
                ansItem.Add(ItemStackFactory.Create(ansid[i],ansamount[i]));
            }

            for (int i = 0; i < ansItem.Count; i++)
            {
                var m = machine.InputSlotWithoutNullItemStack;
                Console.WriteLine(m[i].ToString()+" "+ansItem[i].ToString());
                Assert.AreEqual(ansItem[i],m[i]);
            }
            
        }
        
        //アイテムが通常通り処理されるかのテスト
        [Test]
        public void ItemProcessingAndChangeConnectorTest()
        {
            int seed = 2119350917;
            int recipeNum = 20;
            var recipes = MachineIoGenerate.MachineIoTestCase(RecipeGenerate.MakeRecipe(seed,recipeNum), seed);
            
            
            var machineList = new List<NormalMachine>();
            var MaxDateTime = DateTime.Now;
            
            //機械の作成とアイテムの挿入
            foreach (var m in recipes)
            {
                var machine = NormalMachineFactory.Create(m.installtionId,Int32.MaxValue, new NullIBlockInventory(),new TestBlockConfig());

                foreach (var minput in m.input)
                {
                    machine.InsertItem(ItemStackFactory.Create(minput.Id,minput.Amount));
                }

                var electrical = new ElectricSegment();
                electrical.AddBlockElectric(machine);
                electrical.AddGenerator(new TestPowerGenerator(1000,0));
                
                machineList.Add(machine);
                
                DateTime endTime = DateTime.Now.AddMilliseconds(m.time*m.CraftCnt);
                if (endTime.CompareTo(MaxDateTime) == 1)
                {
                    MaxDateTime = endTime;
                }
            }
            
            //最大クラフト時間を超過するまでクラフトする
            while (MaxDateTime.AddSeconds(0.2).CompareTo(DateTime.Now) == 1)
            {
                GameUpdate.Update();
            }
            
            //検証その1
            for (int i = 0; i < machineList.Count; i++)
            {
                Console.WriteLine(i);
                var machine = machineList[i];
                var machineIoTest = recipes[i];
                
                Assert.False(machine.OutputSlotWithoutNullItemStack.Count <= 0);

                for (int j = 0; j < machine.OutputSlotWithoutNullItemStack.Count; j++)
                {
                    Assert.AreEqual(machineIoTest.output[j],machine.OutputSlotWithoutNullItemStack[j]);
                }
                
                var inputRemainder = machineIoTest.inputRemainder.Where(i => i.Id != ItemConst.NullItemId).ToList();
                inputRemainder.Sort((a, b) => a.Id - b.Id);
                for (int j = 0; j < machine.InputSlotWithoutNullItemStack.Count; j++)
                {
                    Assert.True(inputRemainder[j].Equals(machine.InputSlotWithoutNullItemStack[j]));
                }
            }
            
            var dummyBlockList = new List<DummyBlockInventory>();
            //コネクターを変える
            for (int i = 0; i < recipes.Length; i++)
            {
                var dummy = new DummyBlockInventory(recipes[i].output.Count);
                machineList[i].ChangeConnector(dummy);
                dummyBlockList.Add(dummy);
            }
            GameUpdate.Update();
            //検証その2
            for (int i = 0; i < machineList.Count; i++)
            {
                Console.WriteLine(i);
                var machine = machineList[i];
                var dummy = dummyBlockList[i];
                var machineIoTest = recipes[i];
                
                Assert.True(machine.OutputSlotWithoutNullItemStack.Count == 0);

                for (int j = 0; j < machineIoTest.output.Count; j++)
                {
                    Assert.True(machineIoTest.output[j].Equals(dummy.InsertedItems[j]));
                }
                
                var inputRemainder = machineIoTest.inputRemainder.Where(i => i.Id != ItemConst.NullItemId).ToList();
                inputRemainder.Sort((a, b) => a.Id - b.Id);
                for (int j = 0; j < inputRemainder.Count; j++)
                {
                    Assert.True(inputRemainder[j].Equals(machine.InputSlotWithoutNullItemStack[j]));
                }
            }
        }
        
        //アイテムが通常通り処理されるかのテスト
        [Test]
        public void ItemProcessingOutputTest()
        {
            int seed = 2119350917;
            int recipeNum = 20;
            var recipes = MachineIoGenerate.MachineIoTestCase(RecipeGenerate.MakeRecipe(seed,recipeNum), seed);
            
            
            var machineList = new List<NormalMachine>();
            var dummyBlockList = new List<DummyBlockInventory>();
            var MaxDateTime = DateTime.Now;
            
            //機械の作成とアイテムの挿入
            foreach (var m in recipes)
            {
                var connect = new DummyBlockInventory(m.output.Count);
                var machine = NormalMachineFactory.Create(m.installtionId,Int32.MaxValue, connect,new TestBlockConfig());

                foreach (var minput in m.input)
                {
                    machine.InsertItem(ItemStackFactory.Create(minput.Id,minput.Amount));
                }

                var electrical = new ElectricSegment();
                electrical.AddBlockElectric(machine);
                electrical.AddGenerator(new TestPowerGenerator(1000,0));
                
                dummyBlockList.Add(connect);
                machineList.Add(machine);
                
                DateTime endTime = DateTime.Now.AddMilliseconds(m.time*m.CraftCnt);
                if (endTime.CompareTo(MaxDateTime) == 1)
                {
                    MaxDateTime = endTime;
                }
            }
            
            //最大クラフト時間を超過するまでクラフトする
            while (MaxDateTime.AddSeconds(0.2).CompareTo(DateTime.Now) == 1)
            {
                GameUpdate.Update();
            }
            
            //検証
            for (int i = 0; i < machineList.Count; i++)
            {
                Console.WriteLine(i);
                var machine = machineList[i];
                var connect = dummyBlockList[i];
                var machineIoTest = recipes[i];
                
                var output = connect.InsertedItems;
                Assert.False(output.Count <= 0);

                for (int j = 0; j < output.Count; j++)
                {
                    Assert.True(machineIoTest.output[j].Equals(output[j]));
                }
                
                var inputRemainder = machineIoTest.inputRemainder.Where(i => i.Id != ItemConst.NullItemId).ToList();
                inputRemainder.Sort((a, b) => a.Id - b.Id);
                for (int j = 0; j < machine.InputSlotWithoutNullItemStack.Count; j++)
                {
                    Assert.True(inputRemainder[j].Equals(machine.InputSlotWithoutNullItemStack[j]));
                }
            }
        }
    }
}