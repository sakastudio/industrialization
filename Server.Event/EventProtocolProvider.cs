using System;
using System.Collections.Generic;
using MessagePack;

namespace Server.Event
{
    /// <summary>
    /// サーバー内で起こったイベントの中で、各プレイヤーに送る必要があるイベントを管理します。
    /// 送る必要のある各イベントはEventReceiveフォルダの中に入っています
    /// </summary>
    public class EventProtocolProvider
    {
        private Dictionary<int, List<List<byte>>> _events = new();

        public void AddEvent(int playerId, List<byte> eventByteArray)
        {
            if (_events.TryGetValue(playerId, out var eventList))
            {
                Console.WriteLine($"{eventList.Count}番目 {MessagePackSerializer.ConvertToJson(eventByteArray.ToArray())}");
                eventList.Add(eventByteArray);
            }
            else
            {
                Console.WriteLine($"{0}番目 {MessagePackSerializer.ConvertToJson(eventByteArray.ToArray())}");
                _events.Add(playerId, new List<List<byte>>() {eventByteArray});
            }
        }

        public void AddBroadcastEvent(List<byte> eventByteArray)
        {
            foreach (var key in _events.Keys)
            {
                Console.WriteLine($"{_events[key].Count}番目 {MessagePackSerializer.ConvertToJson(eventByteArray.ToArray())}");
                _events[key].Add(eventByteArray);
            }
        }

        public List<List<byte>> GetEventBytesList(int playerId)
        {
            if (_events.ContainsKey(playerId))
            {
                var data = _events[playerId].Copy();
                _events[playerId].Clear();
                return data;
            }
            else
            {
                //ブロードキャストイベントの時に使うので、Dictionaryにキーを追加しておく
                _events.Add(playerId, new List<List<byte>>());
                return _events[playerId];
            }
        }
    }
}