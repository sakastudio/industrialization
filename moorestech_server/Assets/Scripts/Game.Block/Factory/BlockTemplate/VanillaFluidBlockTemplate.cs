﻿using System.Collections.Generic;
using Game.Block.Blocks;
using Game.Block.Blocks.Fluid;
using Game.Block.Component;
using Game.Block.Interface;
using Game.Block.Interface.Component;
using Mooresmaster.Model.BlocksModule;
using Mooresmaster.Model.InventoryConnectsModule;

namespace Game.Block.Factory.BlockTemplate
{
    public class VanillaFluidBlockTemplate : IBlockTemplate
    {
        public IBlock New(BlockMasterElement blockMasterElement, BlockInstanceId blockInstanceId, BlockPositionInfo blockPositionInfo)
        {
            return GetBlock(blockInstanceId, blockMasterElement, blockPositionInfo);
        }
        public IBlock Load(Dictionary<string, string> componentStates, BlockMasterElement blockMasterElement, BlockInstanceId blockInstanceId, BlockPositionInfo blockPositionInfo)
        {
            return GetBlock(blockInstanceId, blockMasterElement, blockPositionInfo);
        }
        
        private BlockSystem GetBlock(BlockInstanceId blockInstanceId, BlockMasterElement blockMasterElement, BlockPositionInfo blockPositionInfo)
        {
            var inventoryConnects = new InventoryConnects();
            BlockConnectorComponent<IFluidInventory> connectorComponent = FluidSystem.CreateFluidInventoryConnector(inventoryConnects, blockPositionInfo);
            
            var fluidPipeComponent = new FluidPipeComponent(blockPositionInfo, connectorComponent);
            var components = new List<IBlockComponent>
            {
                fluidPipeComponent,
                connectorComponent,
            };
            
            return new BlockSystem(blockInstanceId, blockMasterElement.BlockGuid, components, blockPositionInfo);
        }
    }
}