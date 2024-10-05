using System.Collections.Generic;
using Core.Item.Interface;
using Core.Master;
using Game.Block.Interface;
using Game.Challenge;
using Mooresmaster.Model.ChallengesModule;
using Server.Event.EventReceive;
using Server.Protocol.PacketResponse;
using Server.Util.MessagePack;
using UnityEngine;

namespace Client.Network.API
{
    public class InitialHandshakeResponse
    {
        public Vector2 PlayerPos { get; }
        public WorldDataResponse WorldData { get; }
        public List<MapObjectsInfoMessagePack> MapObjects { get; }
        public PlayerInventoryResponse Inventory { get; }
        public ChallengeResponse Challenge { get; }
        public List<ChangeBlockStateMessagePack> BlockStates { get; }
        
        public InitialHandshakeResponse(ResponseInitialHandshakeMessagePack response, WorldDataResponse worldData, List<MapObjectsInfoMessagePack> mapObjects, PlayerInventoryResponse inventory, ChallengeResponse challenge, List<ChangeBlockStateMessagePack> blockStates)
        {
            PlayerPos = response.PlayerPos;
            WorldData = worldData;
            MapObjects = mapObjects;
            Inventory = inventory;
            Challenge = challenge;
            BlockStates = blockStates;
        }
    }
    
    public class PlayerInventoryResponse
    {
        public PlayerInventoryResponse(List<IItemStack> mainInventory, IItemStack grabItem)
        {
            MainInventory = mainInventory;
            GrabItem = grabItem;
        }
        
        public List<IItemStack> MainInventory { get; }
        public IItemStack GrabItem { get; }
    }
    
    public class WorldDataResponse
    {
        public readonly List<BlockInfo> Blocks;
        public readonly List<EntityResponse> Entities;
        
        public WorldDataResponse(List<BlockInfo> blocks, List<EntityResponse> entities)
        {
            Blocks = blocks;
            Entities = entities;
        }
    }
    
    public class BlockInfo
    {
        public readonly BlockDirection BlockDirection;
        public readonly BlockId BlockId;
        public readonly Vector3Int BlockPos;
        
        public BlockInfo(BlockDataMessagePack blockDataMessagePack)
        {
            BlockPos = blockDataMessagePack.BlockPos;
            BlockId = blockDataMessagePack.BlockId;
            BlockDirection = blockDataMessagePack.BlockDirection;
        }
    }
    
    public class EntityResponse
    {
        public readonly long InstanceId;
        public readonly Vector3 Position;
        public readonly string State;
        public readonly string Type;
        
        public EntityResponse(EntityMessagePack entityMessagePack)
        {
            InstanceId = entityMessagePack.InstanceId;
            Type = entityMessagePack.Type;
            Position = entityMessagePack.Position;
            State = entityMessagePack.State;
        }
    }
    
    public class ChallengeResponse
    {
        public readonly List<ChallengeMasterElement> CompletedChallenges;
        public readonly List<ChallengeMasterElement> CurrentChallenges;
        
        public ChallengeResponse(List<ChallengeMasterElement> currentChallenges, List<ChallengeMasterElement> completedChallenges)
        {
            CurrentChallenges = currentChallenges;
            CompletedChallenges = completedChallenges;
        }
    }
}