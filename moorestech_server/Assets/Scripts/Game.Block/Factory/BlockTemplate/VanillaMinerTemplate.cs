using Core.Const;
using Game.Block.Blocks.Miner;
using Game.Block.Config.LoadConfig.Param;
using Game.Block.Event;
using Game.Block.Interface;
using Game.Block.Interface.BlockConfig;

namespace Game.Block.Factory.BlockTemplate
{
    public class VanillaMinerTemplate : IBlockTemplate
    {
        public delegate VanillaMinerBase LoadMiner(
            (string state, int blockId, int entityId, long blockHash, int requestPower, int outputSlotCount, BlockOpenableInventoryUpdateEvent openableInvEvent, BlockPositionInfo blockPositionInfo) data);

        public delegate VanillaMinerBase NewMiner(
            (int blockId, int entityId, long blockHash, int requestPower, int outputSlotCount, BlockOpenableInventoryUpdateEvent openableInvEvent, BlockPositionInfo blockPositionInfo) data);

        private readonly BlockOpenableInventoryUpdateEvent _blockOpenableInventoryUpdateEvent;


        public readonly LoadMiner _loadMiner;
        public readonly NewMiner _newMiner;

        public VanillaMinerTemplate(BlockOpenableInventoryUpdateEvent blockOpenableInventoryUpdateEvent, NewMiner newMiner, LoadMiner loadMiner)
        {
            _blockOpenableInventoryUpdateEvent = blockOpenableInventoryUpdateEvent;
            _newMiner = newMiner;
            _loadMiner = loadMiner;
        }

        public IBlock New(BlockConfigData param, int entityId, long blockHash, BlockPositionInfo blockPositionInfo)
        {
            var (requestPower, outputSlot) = GetData(param, entityId);

            return _newMiner((param.BlockId, entityId, blockHash, requestPower, outputSlot, _blockOpenableInventoryUpdateEvent, blockPositionInfo));
        }

        public IBlock Load(BlockConfigData param, int entityId, long blockHash, string state, BlockPositionInfo blockPositionInfo)
        {
            var (requestPower, outputSlot) = GetData(param, entityId);

            return _loadMiner((state, param.BlockId, entityId, blockHash, requestPower,
                outputSlot, _blockOpenableInventoryUpdateEvent, blockPositionInfo));
        }

        private (int, int) GetData(BlockConfigData param, int entityId)
        {
            var minerParam = param.Param as MinerBlockConfigParam;

            var oreItem = ItemConst.EmptyItemId;
            var requestPower = minerParam.RequiredPower;
            var miningTime = int.MaxValue;

            return (requestPower, minerParam.OutputSlot);
        }
    }
}