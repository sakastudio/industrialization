using System.Collections.Generic;
using System.Linq;
using Game.Train.RailGraph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
namespace Game.Train.RailGraph
{
    public class RailGraphDatastore
    {
        private readonly Dictionary<RailNode, int> railIdDic;//RailNode→Id辞書。下の逆引き
        private readonly List<RailNode> railNodes;//Id→RailNode辞書。上の逆引き
        MinHeap<int> nextidQueue;//上のリストで穴開き状態をなるべく防ぐために、次使う最小Idを取得するためのキュー。そのためだけにminheapを実装している

        //以下は経路探索で使う変数
        private readonly List<List<(int, int)>> connectNodes;//ノード接続情報。connectNodes[NodeId_A]:NodeId_Aのつながる先のNodeIdリスト。(Id,距離)

        public RailGraphDatastore()
        {
            railIdDic = new Dictionary<RailNode, int>();
            railNodes = new List<RailNode>();
            nextidQueue = new MinHeap<int>();
            connectNodes = new List<List<(int, int)>>();
        }

        public void AddNode(RailNode node)
        {
            //すでにnodeが登録されている場合は何もしない
            if (railIdDic.ContainsKey(node))
                return;

            int nextid;
            if ((nextidQueue.IsEmpty) || (railNodes.Count < nextidQueue.Peek()))
                nextidQueue.Insert(railNodes.Count);
            nextid = nextidQueue.RemoveMin();//より小さいIdを使いたい
            //この時点でnextid<=railNodes.Countは確定
            if (nextid == railNodes.Count)
            {
                railNodes.Add(node);
                connectNodes.Add(new List<(int, int)>());
            }
            else
            {
                railNodes[nextid] = node;
            }

            railIdDic[node] = nextid;
        }

        //接続元RailNode、接続先RailNode、int距離
        public void ConnectNode(RailNode node, RailNode targetNode, int distance)
        {
            //nodeが辞書になければ追加
            if (!railIdDic.ContainsKey(node))
                AddNode(node);
            var nodeid = railIdDic[node];
            //targetが辞書になければ追加
            if (!railIdDic.ContainsKey(targetNode))
                AddNode(targetNode);
            var targetid = railIdDic[targetNode];
            connectNodes[nodeid].Add((targetid, distance));
        }
        //接続削除
        public void DisconnectNode(RailNode node, RailNode targetNode)
        {
            var nodeid = railIdDic[node];
            var targetid = railIdDic[targetNode];
            connectNodes[nodeid].RemoveAll(x => x.Item1 == targetid);
        }

        //ノードの削除。削除対象のノードに向かう経路の削除は別に行う必要がある
        public void RemoveNode(RailNode node)
        {
            var nodeid = railIdDic[node];
            railIdDic.Remove(node);
            railNodes[nodeid] = null;
            nextidQueue.Insert(nodeid);
            connectNodes[nodeid].Clear();
        }

        //RailNodeの入力に対しRailNodeのリストで返すので少しややこしいことをしている
        public List<RailNode> GetConnectedNodes(RailNode node)
        {
            if (!railIdDic.ContainsKey(node))
                return new List<RailNode>();
            int nodeId = railIdDic[node];
            return connectNodes[nodeId].Select(x => railNodes[x.Item1]).ToList();
        }


        /// <summary>
        /// ダイクストラ法を用いて開始ノードから目的地ノードまでの最短経路を計算します。
        /// この高速化のためにノードをintのIDで管理しています。
        /// generated by chat gpt 4o
        /// <returns>最短経路のノード順リスト</returns>
        public List<RailNode> FindShortestPath(RailNode startNode, RailNode targetNode)
        {
            return FindShortestPath(railIdDic[startNode], railIdDic[targetNode]);
        }

        public List<RailNode> FindShortestPath(int startid, int targetid)
        {
            // 優先度付きキュー（距離が小さい順）
            var priorityQueue = new PriorityQueue<int, int>();
            // 各ノードへの最短距離を記録する（初期値は無限大を表す int.MaxValue）
            List<int> distances = new List<int>();// 各ノードへの最短距離を記録する（初期値は無限大を表す int.MaxValue）
            List<int> previousNodes = new List<int>();// 各ノードの前に訪れたノード
            for (int i = 0; i < railNodes.Count; i++)
                distances.Add(int.MaxValue);
            for (int i = 0; i < railNodes.Count; i++)
                previousNodes.Add(-1);


            // 開始ノードの距離を0に設定し、優先度付きキューに追加
            distances[startid] = 0;
            priorityQueue.Enqueue(startid, 0);

            while (priorityQueue.Count > 0)
            {
                // 現在のノードを取得
                var currentNodecnt = priorityQueue.Dequeue();
                // 目的地に到達したら終了
                if (currentNodecnt == targetid)
                {
                    break;
                }

                // 現在のノードからつながる全てのノードを確認
                foreach (var (neighbor, distance) in connectNodes[currentNodecnt])
                {
                    int newDistance = distances[currentNodecnt] + distance;
                    // なにかの間違いでintがオーバーフローした場合(経路が長すぎたり)
                    if (newDistance < 0)
                        continue;
                    // より短い距離が見つかった場合
                    if (newDistance < distances[neighbor])
                    {
                        distances[neighbor] = newDistance;
                        previousNodes[neighbor] = currentNodecnt;
                        // キューに隣接ノードを追加または更新
                        priorityQueue.Enqueue(neighbor, newDistance);
                    }
                }
            }

            // 経路を逆順でたどる
            var path = new List<int>();
            var current = targetid;
            while (current != -1)
            {
                path.Add(current);
                current = previousNodes[current];
            }

            // 開始ノードまでたどり着けなかった場合は空のリストを返す
            if (path.Last() != startid)
            {
                return new List<RailNode>();
            }

            // 経路を正しい順序に並べて返す
            path.Reverse();
            var pathNodes = path.Select(id => railNodes[id]).ToList();
            return pathNodes;
        }












    }
}
