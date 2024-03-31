﻿using Server.Core.EnergySystem.Electric;
using Server.Core.Item;
using Game.Block.Blocks.Machine.InventoryController;
using Game.Block.Blocks.Machine.SaveLoad;
using Game.Block.Component.IOConnector;
using Game.Block.Interface;

namespace Game.Block.Blocks.Machine
{
    public class VanillaElectricMachine : VanillaMachineBase, IBlockElectricConsumer
    {
        public VanillaElectricMachine(
            (int blockId, int entityId, long blockHash, VanillaMachineBlockInventory vanillaMachineBlockInventory,
                VanillaMachineSave vanillaMachineSave, VanillaMachineRunProcess vanillaMachineRunProcess,
                ItemStackFactory itemStackFactory, BlockPositionInfo blockPositionInfo, InputConnectorComponent inputConnectorComponent) data)
            : base(data.blockId, data.entityId, data.blockHash, data.vanillaMachineBlockInventory,
                data.vanillaMachineSave, data.vanillaMachineRunProcess, data.itemStackFactory, data.blockPositionInfo, data.inputConnectorComponent)
        {
        }
    }
}