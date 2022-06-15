using System;
using System.Collections.Generic;
using MessagePack;
using Server.Protocol.PacketResponse;
using UnityEngine;

namespace MainGame.Network.Receive
{
    public class ReceiveInitialHandshakeProtocol: IAnalysisPacket
    { 
        public event Action<Vector2> OnReceiveInitialHandshake;
        public void Analysis(List<byte> packet)
        {
            var data = MessagePackSerializer.Deserialize<ResponseInitialHandshakeMessagePack>(packet.ToArray());
            
            var position = new Vector2(data.PlayerPos.X,data.PlayerPos.Y);
            
            OnReceiveInitialHandshake?.Invoke(position);
        }
    }
}