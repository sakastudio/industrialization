﻿using System;
using System.Collections.Generic;
using Game.PlayerInventory.Interface;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Protocol.PacketResponse
{
    public class BlockInventoryOpenCloseProtocol : IPacketResponse
    {
        public const string Tag = "va:blockInvOpen";
        private readonly IBlockInventoryOpenStateDataStore _inventoryOpenState;

        public BlockInventoryOpenCloseProtocol(ServiceProvider serviceProvider)
        {
            _inventoryOpenState = serviceProvider.GetService<IBlockInventoryOpenStateDataStore>();
        }

        public List<List<byte>> GetResponse(List<byte> payload)
        {
            var data = MessagePackSerializer.Deserialize<BlockInventoryOpenCloseProtocolMessagePack>(payload.ToArray());

            //開く、閉じるのセット
            if (data.IsOpen)
                _inventoryOpenState.Open(data.PlayerId, data.X, data.Y);
            else
                _inventoryOpenState.Close(data.PlayerId);

            return new List<List<byte>>();
        }
    }


    [MessagePackObject(true)]
    public class BlockInventoryOpenCloseProtocolMessagePack : ProtocolMessagePackBase
    {
        [Obsolete("デシリアライズ用のコンストラクタです。基本的に使用しないでください。")]
        public BlockInventoryOpenCloseProtocolMessagePack()
        {
        }

        /// <summary>
        /// TODO このプロトコル消していいのでは（どうせステートの変化を送るなら、それと一緒にインベントリの情報を送った方が設計的に楽なのでは？
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="isOpen"></param>
        public BlockInventoryOpenCloseProtocolMessagePack(int playerId, int x, int y, bool isOpen)
        {
            Tag = BlockInventoryOpenCloseProtocol.Tag;
            PlayerId = playerId;
            X = x;
            Y = y;
            IsOpen = isOpen;
        }

        public int PlayerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsOpen { get; set; }
    }
}