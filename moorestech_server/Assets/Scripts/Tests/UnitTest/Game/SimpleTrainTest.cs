using System.Collections.Generic;
using System.Linq;
using Game.Block.Interface;
using Game.Block.Interface.Extension;
using Game.Block.Blocks.TrainRail;
using Game.Context;
using Game.Train.Train;
using Game.Train.RailGraph;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Server.Boot;
using Tests.Module.TestMod;
using UnityEngine;
using UnityEditor.Playables;
using Game.Train.Utility;
using System;

namespace Tests.UnitTest.Game
{
    public class SimpleTrainTest
    {
        
        [Test]
        // レールに乗っている列車が指定された駅に向かって移動するテスト
        // A test in which a train on rails moves towards a designated station
        public void SimpleTrainMoveTest()
        {
            var (_, saveServiceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);

            // TODO レールブロック1を設置
            // TODO レールブロック2を設置
            // TODO レールブロック同士がつながっていることを確認

            // TODO レールの両端に駅を設置

            // TODO レールに動力車1台を設置
            // TODO 列車に指定された駅に行くように指示

            // TODO 列車が駅に到着するまで待つ

            // TODO 列車が駅に到着すればpass、指定時間以内に到着しなければfail
            //
        }

        [Test]
        //ダイクストラ法が正しく動いているか 0-1-2-3
        public void DijkstraTest0()
        {

            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            //railGraphDatastoreに登録
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            var node0 = new RailNode();
            var node1 = new RailNode();
            var node2 = new RailNode();
            var node3 = new RailNode();
            node0.ConnectNode(node1, 1);
            node1.ConnectNode(node2, 1);
            node2.ConnectNode(node3, 1);

            //ダイクストラ法を実行 node0からnode3までの最短経路を求める
            var outListPath = RailGraphDatastore.FindShortestPath(node0, node3);

            //結果が正しいか
            Assert.AreEqual(4, outListPath.Count);
            Assert.AreEqual(node0, outListPath[0]);
            Assert.AreEqual(node1, outListPath[1]);
            Assert.AreEqual(node2, outListPath[2]);
            Assert.AreEqual(node3, outListPath[3]);
        }

        [Test]
        //ダイクストラ法が正しく動いているか、分岐あり 0=(1,2)=3
        public void DijkstraTest1()
        {
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            //railGraphDatastoreに登録
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            var node0 = new RailNode();
            var node1 = new RailNode();
            var node2 = new RailNode();
            var node3 = new RailNode();
            node0.ConnectNode(node1, 123);
            node0.ConnectNode(node2, 345);
            node1.ConnectNode(node3, 400);
            node2.ConnectNode(node3, 1);

            //ダイクストラ法を実行 node0からnode3までの最短経路を求める
            var outListPath = RailGraphDatastore.FindShortestPath(node0, node3);

            //結果が正しいか
            Assert.AreEqual(3, outListPath.Count);
            Assert.AreEqual(node0, outListPath[0]);
            Assert.AreEqual(node2, outListPath[1]);
            Assert.AreEqual(node3, outListPath[2]);
        }


        [Test]
        //ダイクストラ法が正しく動いているか、複雑
        public void DijkstraTest2()
        {
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            //10000個のノードを作成し、それぞれが10つのノードにつながる。距離は1
            const int nodenum_powerexponent = 4;
            int nodenum = (int)System.Math.Pow(10, nodenum_powerexponent);

            RailNode[] nodeList = new RailNode[nodenum];
            for (int i = 0; i < nodenum; i++)
            {
                nodeList[i] = new RailNode();
            }
            //つながる規則は桁シフト(*10)して下位桁の数字を0-9とし、そのノードに対してつながる
            for (int i = 0; i < nodenum; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var next = (i * 10) % nodenum + j;
                    nodeList[i].ConnectNode(nodeList[next], 1);
                }
            }

            //ダイクストラ法を実行、ランダムに。必ず距離4以内に任意のノードにつながるはず
            //例 1145から1419までの最短経路を求める
            //1145①→1451②
            //1451②→4514③
            //4514③→5141④
            //5141④→1419⑤
            int testnum = 1234;//1234567でも大丈夫なことを確認
            for (int i = 0; i < testnum; i++)
            {
                int rand0 = UnityEngine.Random.Range(0, nodenum);
                int rand1 = UnityEngine.Random.Range(0, nodenum);
                var node_start = nodeList[rand0];
                var node_end = nodeList[rand1];
                var outListPath = RailGraphDatastore.FindShortestPath(node_start, node_end);
                //結果が正しいか outListPathは4+1以内のはず
                if (outListPath.Count > 5)
                {
                    Debug.Log(rand0);
                    Debug.Log(rand1);
                }
                Assert.LessOrEqual(outListPath.Count, nodenum_powerexponent + 1);
            }
        }




        [Test]
        //Yの字の形に設置して、ノードが正しくつながっているかチェック
        public void Y_NodeCheck()
        {
            //Notionの図を参照
            //Yの字の左上がA、右上がB、真ん中がC1とC2、下がD1とD2
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            //railGraphDatastoreに登録
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            var nodeA = new RailNode();
            var nodeB = new RailNode();
            var nodeC1 = new RailNode();
            var nodeC2 = new RailNode();
            var nodeD1 = new RailNode();
            var nodeD2 = new RailNode();
            nodeA.ConnectNode(nodeC1, 3782);
            nodeB.ConnectNode(nodeC1, 67329);
            nodeC1.ConnectNode(nodeD1, 71894);
            nodeD2.ConnectNode(nodeC2, 17380);
            nodeC2.ConnectNode(nodeA, 28973);
            nodeC2.ConnectNode(nodeB, 718);

            //上から下
            //ダイクストラ法を実行 nodeAからnodeDまでの最短経路を求める
            var outListPath = RailGraphDatastore.FindShortestPath(nodeA, nodeD1);

            //結果が正しいか
            Assert.AreEqual(3, outListPath.Count);
            Assert.AreEqual(nodeA, outListPath[0]);
            Assert.AreEqual(nodeC1, outListPath[1]);
            Assert.AreEqual(nodeD1, outListPath[2]);

            //下から上
            outListPath = RailGraphDatastore.FindShortestPath(nodeD2, nodeA);

            //結果が正しいか
            Assert.AreEqual(3, outListPath.Count);
            Assert.AreEqual(nodeD2, outListPath[0]);
            Assert.AreEqual(nodeC2, outListPath[1]);
            Assert.AreEqual(nodeA, outListPath[2]);

            //AからBは繋がらない
            outListPath = RailGraphDatastore.FindShortestPath(nodeA, nodeB);
            Assert.AreEqual(0, outListPath.Count);

            //ここでD1とD2を繋げると
            nodeD1.ConnectNode(nodeD2, 721);
            outListPath = RailGraphDatastore.FindShortestPath(nodeA, nodeB);
            Assert.AreEqual(6, outListPath.Count);
            Assert.AreEqual(nodeA, outListPath[0]);
            Assert.AreEqual(nodeC1, outListPath[1]);
            Assert.AreEqual(nodeD1, outListPath[2]);
            Assert.AreEqual(nodeD2, outListPath[3]);
            Assert.AreEqual(nodeC2, outListPath[4]);
            Assert.AreEqual(nodeB, outListPath[5]);
        }



        //RailGraphDatastoreに実装したGetConnectedNodesのテスト
        [Test]
        public void ConnectedNodesTest()
        {
            //Yの字の左上がA、右上がB、真ん中がC1とC2、下がD1とD2
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            //railGraphDatastoreに登録
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            var nodeA = new RailNode();
            var nodeB = new RailNode();
            var nodeC = new RailNode();

            nodeA.ConnectNode(nodeB, 10);
            nodeA.ConnectNode(nodeC, 20);

            var connectedNodes = nodeA.ConnectedNodes.ToList();

            Assert.AreEqual(2, connectedNodes.Count);
            Assert.IsTrue(connectedNodes.Contains(nodeB));
            Assert.IsTrue(connectedNodes.Contains(nodeC));
        }
        //RailPositionのmoveForwardのテストその1
        [Test]
        public void MoveForward_LongTrain_MovesAcrossMultipleNodes()
        {
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraph = serviceProvider.GetService<RailGraphDatastore>();

            // ノードを準備
            var nodeA = new RailNode();
            var nodeB = new RailNode();
            var nodeC = new RailNode();
            var nodeD = new RailNode();
            var nodeE = new RailNode();

            // ノードを接続
            nodeB.ConnectNode(nodeA, 10);//9から列車
            nodeC.ConnectNode(nodeB, 15);//列車
            nodeD.ConnectNode(nodeC, 20);//列車
            nodeE.ConnectNode(nodeD, 25);//14まで列車

            // 長い列車（列車長50）をノードAからEにまたがる状態に配置
            var nodes = new List<RailNode> { nodeA, nodeB, nodeC, nodeD, nodeE };
            var railPosition = new RailPosition(nodes, 50, 9); // 先頭はノードAとBの間の9地点

            //進む
            var remainingDistance = railPosition.MoveForward(6); // 6進む（ノードAに近づく）
            // Assert
            Assert.AreEqual(0, remainingDistance); // ノードAに到達するまでに残り3

            //地道に全部チェック。ノードEの情報はまだ消えてない
            var list = railPosition.TestGet_railNodes();
            Assert.AreEqual(nodeA, list[0]);
            Assert.AreEqual(nodeB, list[1]);
            Assert.AreEqual(nodeC, list[2]);
            Assert.AreEqual(nodeD, list[3]);
            Assert.AreEqual(nodeE, list[4]);

            //進む、残りの進むべき距離
            remainingDistance = railPosition.MoveForward(4); // 3進んでAで停止、残り1
            // Assert
            Assert.AreEqual(nodeA, railPosition.GetNodeApproaching()); // 
            Assert.AreEqual(1, remainingDistance); //
            Assert.AreEqual(nodeB, railPosition.GetNodeJustPassed()); // 
        }

        //RailPositionのmoveForwardのテストその2
        [Test]
        public void MoveBackward_LongTrain_MovesAcrossMultipleNodes()
        {
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraph = serviceProvider.GetService<RailGraphDatastore>();

            // ノードを準備
            // 表
            var nodeA1 = new RailNode();
            var nodeB1 = new RailNode();
            var nodeC1 = new RailNode();
            var nodeD1 = new RailNode();
            var nodeE1 = new RailNode();
            // 裏
            var nodeA2 = new RailNode();
            var nodeB2 = new RailNode();
            var nodeC2 = new RailNode();
            var nodeD2 = new RailNode();
            var nodeE2 = new RailNode();

            // ノードを接続
            nodeB1.ConnectNode(nodeA1, 10);//5から列車
            nodeC1.ConnectNode(nodeB1, 15);//列車
            nodeD1.ConnectNode(nodeC1, 20);//列車
            nodeE1.ConnectNode(nodeD1, 25);//10まで列車

            nodeD2.ConnectNode(nodeE2, 25);
            nodeC2.ConnectNode(nodeD2, 20);
            nodeB2.ConnectNode(nodeC2, 15);
            nodeA2.ConnectNode(nodeB2, 10);

            nodeA1.SetOppositeNode(nodeA2);//ここは本来RailConmponentのコンストラクタでやる
            nodeB1.SetOppositeNode(nodeB2);
            nodeC1.SetOppositeNode(nodeC2);
            nodeD1.SetOppositeNode(nodeD2);
            nodeE1.SetOppositeNode(nodeE2);
            nodeA2.SetOppositeNode(nodeA1);
            nodeB2.SetOppositeNode(nodeB1);
            nodeC2.SetOppositeNode(nodeC1);
            nodeD2.SetOppositeNode(nodeD1);
            nodeE2.SetOppositeNode(nodeE1);
            {  //Reverseを使ってMoveForward(マイナス)を使わないパターン
                var nodes = new List<RailNode> { nodeA1, nodeB1, nodeC1, nodeD1, nodeE1 };
                var railPosition = new RailPosition(nodes, 50, 5); // 先頭はノードAとBの間の5地点
                railPosition.Reverse();//ノードEまで15になる
                //地道に全部チェック。ノードEの情報はまだ消えてない
                var list = railPosition.TestGet_railNodes();
                Assert.AreEqual(nodeE2, list[0]);
                Assert.AreEqual(nodeD2, list[1]);
                Assert.AreEqual(nodeC2, list[2]);
                Assert.AreEqual(nodeB2, list[3]);
                Assert.AreEqual(nodeA2, list[4]);
                Assert.AreEqual(15, railPosition.GetDistanceToNextNode());
                var remainingDistance = railPosition.MoveForward(6); // 6すすむ（ノードEに近づく）
                Assert.AreEqual(9, railPosition.GetDistanceToNextNode());
                Assert.AreEqual(0, remainingDistance);

                list = railPosition.TestGet_railNodes();//後輪が完全にB-C間にいるためノードAの情報は削除される
                Assert.AreEqual(4, list.Count);
                Assert.AreEqual(nodeE2, list[0]);
                Assert.AreEqual(nodeD2, list[1]);
                Assert.AreEqual(nodeC2, list[2]);
                Assert.AreEqual(nodeB2, list[3]);
            }

            { //MoveForward(マイナス)を使うパターン
                // 長い列車（列車長50）をノードAからEにまたがる状態に配置
                var nodes = new List<RailNode> { nodeA1, nodeB1, nodeC1, nodeD1, nodeE1 };
                var railPosition = new RailPosition(nodes, 50, 5); // 先頭はノードAとBの間の5地点

                //進む、残りの進むべき距離
                var remainingDistance = railPosition.MoveForward(-11); // 11後退（ノードEに近づく）

                Assert.AreEqual(6, railPosition.GetDistanceToNextNode()); //
                Assert.AreEqual(nodeB1, railPosition.GetNodeApproaching()); // nodeAまで5のところから11後退してる
                Assert.AreEqual(nodeC1, railPosition.GetNodeJustPassed()); // 

                var list = railPosition.TestGet_railNodes(); Assert.AreEqual(4, list.Count);
                Assert.AreEqual(nodeB1, list[0]);
                Assert.AreEqual(nodeC1, list[1]);
                Assert.AreEqual(nodeD1, list[2]);
                Assert.AreEqual(nodeE1, list[3]);
            }
        }

        
        //ブロック設置してrailComponentの表裏テスト
        [Test]
        public void TestRailComponentsAreConnected()
        {
            // Initialize the RailGraphDatastore
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(0, 0, 0), BlockDirection.North, out var rail1);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(1, 0, 0), BlockDirection.North, out var rail2);

            //assert rail1が存在する
            Assert.NotNull(rail1, "Rail1 does not exist.");

            // Get two RailComponents
            var railComponent1 = rail1.GetComponent<RailComponent>();
            var railComponent2 = rail2.GetComponent<RailComponent>();

            // Connect the two RailComponents
            railComponent1.ConnectRailComponent(railComponent2, true, true); // Front of railComponent1 to front of railComponent2

            // Validate connections
            var connectedNodes = railComponent1.FrontNode.ConnectedNodesWithDistance;
            var connectedNode = connectedNodes.FirstOrDefault();

            Assert.NotNull(connectedNode, "RailComponent1 is not connected to RailComponent2.");
            Assert.AreEqual(railComponent2.FrontNode, connectedNode.Item1, "RailComponent1's FrontNode is not connected to RailComponent2's FrontNode.");
            //Assert.AreEqual(1, connectedNode.Item2, "The connection distance is not correct.");
            //Debug.Log("Node1からNode2の距離" + connectedNode.Item2);

            //ダイクストラ法を実行 node000からnode494949までの最短経路を求める
            //表
            var outListPath = RailGraphDatastore.FindShortestPath(railComponent1.FrontNode, railComponent2.FrontNode);
            // outListPathの長さが0でないことを確認
            Assert.AreNotEqual(0, outListPath.Count);
            //裏
            outListPath = RailGraphDatastore.FindShortestPath(railComponent2.BackNode, railComponent2.BackNode);
            // outListPathの長さが0でないことを確認
            Assert.AreNotEqual(0, outListPath.Count);

        }

        //50*50*50個のRailComponentの立方体の中にレールグラフを構築したり、内部だけくりぬいたりしてテスト。
        //ブロック大量設置してと思っていたがブロック設置が1000個で1秒以上かかるため断念
        //railComponentのみを大量設置する
        [Test]
        public void TestRailComponentsRandomCase()
        {
            //listを入力とし、順番をシャッフルする関数
            List<(int, int, int)> ShuffleList(List<(int, int, int)> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    int j = UnityEngine.Random.Range(i, list.Count);
                    var tmp = list[i];
                    list[i] = list[j];
                    list[j] = tmp;
                }
                return list;
            }
            // Initialize the RailGraphDatastore
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();
            const int size = 12;//立方体の一辺の長さ40でも通ることを確認。計算量はO(size^6)以上


            //これから作るべきRailComponentの場所のリストの宣言
            var listIsDestroy = new List<(int, int, int)>();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        listIsDestroy.Add((x, y, z));
                    }
                }
            }
            listIsDestroy = ShuffleList(listIsDestroy);
            //すでに作られているRailComponentのリスト
            var listIsCreated = new List<(int, int, int)>();
            var railBlocks = new RailComponent[size, size, size];


            while (listIsDestroy.Count != 0)
            {
                //ランダムにRailComponent作成
                var (x, y, z) = listIsDestroy[UnityEngine.Random.Range(0, listIsDestroy.Count)];
                listIsCreated.Add((x, y, z));
                listIsDestroy.Remove((x, y, z));
                railBlocks[x, y, z] = new RailComponent(new BlockPositionInfo(new Vector3Int(x, y, z), BlockDirection.North, new Vector3Int(0, 0, 0)));
                //ランダムに経路をつなげる
                //2つ選ぶ
                var (x1, y1, z1) = listIsCreated[UnityEngine.Random.Range(0, listIsCreated.Count)];
                var (x2, y2, z2) = listIsCreated[UnityEngine.Random.Range(0, listIsCreated.Count)];
                //場所が外周ならやらない   
                if (x1 == 0 || x1 == size - 1 || y1 == 0 || y1 == size - 1 || z1 == 0 || z1 == size - 1) continue;
                railBlocks[x1, y1, z1].ConnectRailComponent(railBlocks[x2, y2, z2], true, true);

                //2分の1の確率でcontinue
                if (UnityEngine.Random.Range(0, 2) == 0) continue;

                //ランダムにRailComponentを削除
                var (x3, y3, z3) = listIsCreated[UnityEngine.Random.Range(0, listIsCreated.Count)];
                railBlocks[x3, y3, z3].Destroy();
                listIsCreated.Remove((x3, y3, z3));
                listIsDestroy.Add((x3, y3, z3));
            }


            //自分から+1方向につなげていく。まずはランダムに
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        //2分の1の確率でcontinue
                        if (UnityEngine.Random.Range(0, 2) == 0) continue;
                        if (x < size - 1) railBlocks[x, y, z].ConnectRailComponent(railBlocks[x + 1, y, z], true, true);
                        if (y < size - 1) railBlocks[x, y, z].ConnectRailComponent(railBlocks[x, y + 1, z], true, true);
                        if (z < size - 1) railBlocks[x, y, z].ConnectRailComponent(railBlocks[x, y, z + 1], true, true);
                    }
                }
            }

            //残りを全部やる(全部順番にやるとランダムケースで起こるバグを拾えない可能性を考慮し)
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        if (x > 0) railBlocks[x, y, z].ConnectRailComponent(railBlocks[x - 1, y, z], false, false);
                        if (y > 0) railBlocks[x, y, z].ConnectRailComponent(railBlocks[x, y - 1, z], false, false);
                        if (z > 0) railBlocks[x, y, z].ConnectRailComponent(railBlocks[x, y, z - 1], false, false);
                    }
                }
            }
            //この時点で
            //立方体の0,0,0から49,49,49まで経路があるか
            var node_s = railBlocks[0, 0, 0].FrontNode;
            var node_e = railBlocks[size - 1, size - 1, size - 1].FrontNode;

            //ダイクストラ法を実行 経路を求める
            var outListPath = RailGraphDatastore.FindShortestPath(node_s, node_e);
            // outListPathの長さが0でないことを確認
            Assert.AreNotEqual(0, outListPath.Count);

            //次に余分なpathを削除してちゃんと外周をたどるか
            for (int x = 1; x < size - 1; x++)
            {
                for (int y = 1; y < size - 1; y++)
                {
                    for (int z = 1; z < size - 1; z++)
                    {
                        railBlocks[x, y, z].Destroy();
                    }
                }
            }

            //ダイクストラ
            outListPath = RailGraphDatastore.FindShortestPath(node_s, node_e);
            Assert.AreEqual(3 * (size - 1) + 1, outListPath.Count);
        }






        
        /// <summary>
        /// 列車を編成分割できることをテスト。
        /// 前後の車両数・RailPosition の列車長さが正しく更新されているか確認します。
        /// </summary>
        [Test]
        public void SplitTrain_BasicTest()
        {
            /// RailPosition の列車長をテスト用に取得するためのヘルパーメソッド。
            int GetTrainLengthForTest(RailPosition railPosition)
            {
                // railPosition が null なら -1 などを返しておく
                if (railPosition == null) return -1;
                var fieldInfo = typeof(RailPosition)
                    .GetField("_trainLength", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo == null) return -1;
                return (int)fieldInfo.GetValue(railPosition);
            }

            // --- 1. レールノードを用意 ---
            // 例として直線上のノード3つ (A <- B <- C) を作り、距離を設定
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(0, 0, 0), BlockDirection.North, out var railA);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(1, 0, 0), BlockDirection.North, out var railB);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(1, 0, 0), BlockDirection.North, out var railC);
            // A - B の距離を 20,  B - C の距離を 40 とする
            var railComponentA = railA.GetComponent<RailComponent>();
            var railComponentB = railB.GetComponent<RailComponent>();
            var railComponentC = railC.GetComponent<RailComponent>();

            // Connect the two RailComponents
            railComponentC.ConnectRailComponent(railComponentB, true, true, 40);
            railComponentB.ConnectRailComponent(railComponentA, true, true, 20);

            // これで A -> B -> C の合計距離は 60
            var nodeA = railComponentA.FrontNode;
            var nodeB = railComponentB.FrontNode;
            var nodeC = railComponentC.FrontNode;

            // --- 2. 編成を構成する車両を用意 ---
            // 例：5両編成で各車両の長さは 10, 20, 5, 5, 10 (トータル 50)
            var cars = new List<TrainCar>
            {
                new TrainCar(tractionForce: 1000, inventorySlots: 0, length: 10),  // 仮: 動力車
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 20),   // 貨車
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 5),
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 5),
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 10),
            };
            int totalTrainLength = cars.Sum(car => car.Length);  // 10+20+5+5+10 = 50
            // --- 3. 初期の RailPosition を用意 ---
            //   ノードリスト = [A, B, C], 列車長さ = 50
            //   先頭が “A にあと10 で到達する位置” とする → initialDistanceToNextNode=10
            //   （イメージ：A--(10進んだ場所で先頭)-->B----->C ...合計60）
            var railNodes = new List<RailNode> { nodeA, nodeB, nodeC };
            var initialRailPosition = new RailPosition(
                railNodes,
                totalTrainLength,
                initialDistanceToNextNode: 10  // 先頭が A まであと10
            );

            // --- 4. TrainUnit を生成 ---
            var destination = nodeA;   // 適当な目的地を A にしておく
            var trainUnit = new TrainUnit(initialRailPosition, destination, cars);

            // --- 5. SplitTrain(...) で後ろから 2 両切り離す ---
            //   5両 → (前3両) + (後ろ2両) に分割
            var splittedUnit = trainUnit.SplitTrain(2);

            // --- 6. 結果の検証 ---
            // 6-1) 戻り値（splittedUnit）は null ではない
            Assert.NotNull(splittedUnit, "SplitTrain の結果が null になっています。");

            // 6-2) オリジナル列車の車両数は 3 両になっている
            //      新たに生成された列車の車両数は 2 両
            Assert.AreEqual(3, trainUnit
                .GetType()
                .GetField("_cars", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(trainUnit) is List<TrainCar> carsAfterSplit1
                    ? carsAfterSplit1.Count
                    : -1);

            Assert.AreEqual(2, splittedUnit
                .GetType()
                .GetField("_cars", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(splittedUnit) is List<TrainCar> carsAfterSplit2
                    ? carsAfterSplit2.Count
                    : -1);

            // 6-3) 列車長さが正しく更新されているか
            // オリジナル列車: 前3両 = 10 + 20 + 5 = 35
            // 後続列車: 後ろ2両 = 5 + 10 = 15
            // ※上の例では 10,20,5,5,10 の順で「後ろ2両」は後ろから 5,10 のはずなので合計15
            // SplitTrain 内で _railPosition.SetTrainLength(...) を行うことで長さが更新されているはず
            
            var mainRailPos = trainUnit._railPosition;
            var splittedRailPos = splittedUnit._railPosition;

            var nodelist1 = mainRailPos.TestGet_railNodes();
            var nodelist2 = splittedRailPos.TestGet_railNodes();
            //nodelist1のid表示
            //RailGraphDatastore._instance.Test_ListIdLog(nodelist1);
            //nodelist2のid表示
            //RailGraphDatastore._instance.Test_ListIdLog(nodelist2);
            // RailPosition の列車長を直接取得するための Getter が無い場合は、
            // 同様に Reflection や専用のテスト用メソッド (TestGetTrainLength() 等) を用意する形になります。
            // ここではテスト用に「TestGetTrainLength」があると仮定している例を示します。
            var mainTrainLength = GetTrainLengthForTest(mainRailPos);
            var splittedTrainLength = GetTrainLengthForTest(splittedRailPos);

            Assert.AreEqual(35, mainTrainLength, "分割後の先頭列車の長さが想定外です。");
            Assert.AreEqual(15, splittedTrainLength, "分割後の後続列車の長さが想定外です。");

            //mainRailPosはnodeAから10の距離にいるはず
            Assert.AreEqual(nodeA, mainRailPos.GetNodeApproaching());
            Assert.AreEqual(10, mainRailPos.GetDistanceToNextNode());
            mainRailPos.Reverse();
            //nodeCまで15の距離にいるはず
            Assert.AreEqual(nodeC.OppositeNode, mainRailPos.GetNodeApproaching());
            Assert.AreEqual(15, mainRailPos.GetDistanceToNextNode());

            // 6-4) 新しい後続列車の RailPosition が「後ろ側」に連続した状態で生成されているか
            //      → SplitTrain 内部では DeepCopy + Reverse + SetTrainLength + Reverse で位置を調整。
            //splittedRailPosはnodeBから25の距離にいるはず
            Assert.AreEqual(nodeB, splittedRailPos.GetNodeApproaching());
            Assert.AreEqual(25, splittedRailPos.GetDistanceToNextNode());
        }




        //列車編成が目的地にいけるかテスト、簡単
        [Test]
        public void Train_Approaching_light()
        {
            const bool DEBUG_LOG_FLAG = false;
            // --- 1. レールノードを用意 ---
            // 例として直線上のノード3つ (A <- B <- C) を作る
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);
            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(0, 0, 0), BlockDirection.North, out var railA);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(2162, 2, -1667), BlockDirection.North, out var railB);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(-924, 12, 974), BlockDirection.North, out var railC);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(1149, 0, 347), BlockDirection.North, out var railD);
            var railComponentA = railA.GetComponent<RailComponent>();
            var railComponentB = railB.GetComponent<RailComponent>();
            var railComponentC = railC.GetComponent<RailComponent>();
            var railComponentD = railD.GetComponent<RailComponent>();

            // Connect the two RailComponents
            railComponentD.ConnectRailComponent(railComponentC, true, true);
            railComponentC.ConnectRailComponent(railComponentB, true, true);
            railComponentB.ConnectRailComponent(railComponentA, true, true);

            var nodeA = railComponentA.FrontNode;
            var nodeB = railComponentB.FrontNode;
            var nodeC = railComponentC.FrontNode;
            var nodeD = railComponentD.FrontNode;

            // --- 2. 編成を構成する車両を用意 ---
            // 例：5両編成で各車両の長さは 10, 20, 5, 5, 10 (トータル 50)
            var cars = new List<TrainCar>
            {
                new TrainCar(tractionForce: 600000, inventorySlots: 0, length: 80),  // 仮: 動力車
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 60),   // 貨車
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 65),
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 65),
                new TrainCar(tractionForce: 0, inventorySlots: 10, length: 60),
            };
            var railNodes = new List<RailNode> { nodeC, nodeD };
            int totalTrainLength = cars.Sum(car => car.Length);  // 10+20+5+5+10 = 50
            var initialRailPosition = new RailPosition(
                railNodes,
                totalTrainLength,
                initialDistanceToNextNode: 10  // 先頭が C まであと10
            );

            // --- 4. TrainUnit を生成 ---
            var destination = nodeA;   // 適当な目的地を A にしておく
            var trainUnit = new TrainUnit(initialRailPosition, destination, cars);
            trainUnit._isUseDestination = true;//factorioでいう自動運転on
            //while (trainUnit._isUseDestination) //目的地に到達するまで
            int totaldist = 0;
            for (int i = 0; i < 65535; i++)//目的地に到達するまで→testフリーズは避けたいので有限で
            {
                int calceddist = 0;
                trainUnit.UpdateTrain(1f / 60f, out calceddist);
                totaldist += calceddist;
                if ((i % 60 == 0) & (DEBUG_LOG_FLAG))
                {
                    Debug.Log("列車速度" + trainUnit._currentSpeed);
                    Debug.Log("1フレームにすすむ距離int" + calceddist);
                    Debug.Log("現在向かっているnodeのID");
                    RailGraphDatastore._instance.Test_NodeIdLog(trainUnit._railPosition.GetNodeApproaching());
                }
                if (!trainUnit._isUseDestination)
                {
                    if (DEBUG_LOG_FLAG)
                    {
                        Debug.Log("" + i + "フレームでつきました。約" + (i / 60) + "秒");
                        Debug.Log("実装距離(int)" + totaldist + "");
                        Debug.Log("実装距離(world座標換算)" + ((float)totaldist / BezierUtility.RAIL_LENGTH_SCALE) + "");
                    }
                    break;
                }
            }
            
            Assert.AreEqual(nodeA, trainUnit._railPosition.GetNodeApproaching());
            Assert.AreEqual(0, trainUnit._railPosition.GetDistanceToNextNode());
            if (DEBUG_LOG_FLAG)
                Debug.Log("列車編成が無事目的地につきました");
        }








        /// <summary>
        /// 複数レールを一直線に配置し、列車長を大きめにしつつ前後に移動させるテスト
        /// レールの一部を削除してルートが分断されるか確認
        /// </summary>

        [Test]
        public void LargeTrainAndRemoveRailTest()
        {
            // サーバーDIを立てて、WorldBlockDatastore や RailGraphDatastore を取得
            var (_, serviceProvider) = new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);

            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            // 1) ワールド上にいくつかレールを「TryAddBlock」して、RailComponentを取得
            //    例として4本だけ設置
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(0, 0, 0), BlockDirection.North, out var railBlockA);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(2162, 2, -1667), BlockDirection.East, out var railBlockB);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(-924, 12, 974), BlockDirection.West, out var railBlockC);
            worldBlockDatastore.TryAddBlock(ForUnitTestModBlockId.TestTrainRail, new Vector3Int(1149, 0, 347), BlockDirection.South, out var railBlockD);

            // RailComponent を取得
            var railComponentA = railBlockA.GetComponent<RailComponent>();
            var railComponentB = railBlockB.GetComponent<RailComponent>();
            var railComponentC = railBlockC.GetComponent<RailComponent>();
            var railComponentD = railBlockD.GetComponent<RailComponent>();

            // 2) レールどうしを Connect
            //    レールが曲線か直線かは RailComponent 内部の BezierUtility などで計算。
            //    connect の例： Aの表(front)->Bの表(front)
            //                  Bの裏(back)->Aの裏(back)
            //    defaultdistance=-1 ならばベジェ曲線長が自動計算される
            railComponentA.ConnectRailComponent(railComponentB, isFront_this: true, isFront_target: true, defaultdistance: -1);
            railComponentB.ConnectRailComponent(railComponentC, isFront_this: false, isFront_target: true, defaultdistance: -1);
            railComponentC.ConnectRailComponent(railComponentD, isFront_this: true, isFront_target: true, defaultdistance: -1);

            // これでA→B→C→D (ベジェ) の順で何らかのルートがつながったはず
            // ノード列を組み立てる
            // まずAのFrontNodeを先頭に、connect された順序を辿るなどしてノード列を取得
            // 実際にはダイクストラなどで最短経路を取るほうが正確
            var nodeList = new List<RailNode>();
            nodeList.Add(railComponentA.FrontNode);
            nodeList.Add(railComponentB.FrontNode);
            nodeList.Add(railComponentB.BackNode);
            nodeList.Add(railComponentC.FrontNode);
            nodeList.Add(railComponentC.BackNode);
            nodeList.Add(railComponentD.FrontNode);

            // 3) 距離合計をざっくり計算 (※ベジェ内部で距離計算しているため、ConnectRailComponentが何らかの形で距離を保持している想定)
            //    ここでは RailNode.GetDistanceToNode(...) をループで合計する例
            int totalDist = 0;
            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                int dist = nodeList[i].GetDistanceToNode(nodeList[i + 1]);
                if (dist > 0) totalDist += dist;
            }

            // 大きめの列車を作る
            int trainLength = (int)(totalDist * 0.7f);
            if (trainLength < 10) trainLength = 10; //最低10

            // 4) RailPosition を作って先頭を配置
            //    initialDistanceToNextNode=5あたりから開始する例
            var railPosition = new RailPosition(nodeList, trainLength, 5);

            // 5) いくつか進めてみる
            var remain1 = railPosition.MoveForward(12);
            Debug.Log($"1回目: Move=12, remain={remain1} / approach={railPosition.GetNodeApproaching()}");

            // 6) 中途でレールを削除する (例: railComponentC を破壊)
            railComponentC.Destroy();
            // worldBlockDatastore.RemoveBlock(...) などで物理的にワールドから消す想定
            worldBlockDatastore.RemoveBlock(railComponentC.GetPosition());

            // Connect情報を切り離すには DisconnectRailComponent(...) を適宜呼ぶ
            railComponentB.DisconnectRailComponent(railComponentC, isFront_this: false, isFront_target: true);
            railComponentC.DisconnectRailComponent(railComponentD, isFront_this: true, isFront_target: true);

            // 7) 再度 forward
            var remain2 = railPosition.MoveForward(30);
            Debug.Log($"2回目: Move=30, remain={remain2} / approach={railPosition.GetNodeApproaching()}");

            // あとは最終的に何個のノードが残っているかなど
            var restNodes = railPosition.TestGet_railNodes();
            Debug.Log($"現在ノード列: count={restNodes.Count}");

            // 8) 適宜アサート 
            Assert.IsTrue(restNodes.Count <= 3, "レールCを削除したので、後半のノードが切断されているはず");
        }


        /// <summary>
        /// レールをランダムに配置・接続して列車を走行させるテスト
        /// </summary>

        /// <summary>
        /// レールを複数ランダム配置し、適当に connect してから列車を走らせるサンプルテスト。
        /// 簡易的なルート（ノード列）を作り、RailPositionを動かしてみる。
        /// </summary>
        [Test]
        public void RandomRailAndTrainTest()
        {
            const int TestSeed = 123456;
            // サーバーDIを立てて、WorldBlockDatastore や RailGraphDatastore を取得
            var (_, serviceProvider) =
                new MoorestechServerDIContainerGenerator().Create(TestModDirectory.ForUnitTestModDirectory);

            var worldBlockDatastore = ServerContext.WorldBlockDatastore;
            var railGraphDatastore = serviceProvider.GetService<RailGraphDatastore>();

            // 乱数
            var random = new System.Random(TestSeed);

            // ----- 1) ランダムにレールブロックを配置 -----
            // 個数 n は適当に
            const int n = 8;
            var railComponents = new List<RailComponent>(n);
            var blockDirections = new BlockDirection[]
            {
                BlockDirection.North, BlockDirection.East,
                BlockDirection.South, BlockDirection.West
            };

            for (int i = 0; i < n; i++)
            {
                // ランダム座標(そこまで広くしすぎない程度)
                var x = random.Next(-500, 501);
                var y = 0;               // yは高さ。必要があれば random.Next(0,10)などでもOK
                var z = random.Next(-500, 501);
                var pos = new Vector3Int(x, y, z);

                // ランダム向き
                var dir = blockDirections[random.Next(blockDirections.Length)];

                // TryAddBlock
                worldBlockDatastore.TryAddBlock(
                    ForUnitTestModBlockId.TestTrainRail,
                    pos, dir, out var railBlock);

                // RailComponent を取得
                var railComp = railBlock.GetComponent<RailComponent>();
                railComponents.Add(railComp);

                Debug.Log($"[RandomRailAndTrain] Created rail {i} at {pos}, dir={dir}");
            }

            // ----- 2) レール同士を適当にconnect -----
            // ※ すべてが連結される保証はなく、部分的につながらない可能性あり
            //   ここでは簡易的に「隣のインデックス同士をconnect」＋「ランダムで何回か追加connect」
            for (int i = 0; i < n - 1; i++)
            {
                // railComponents[i] → railComponents[i+1] を適当に繋ぐ
                // isFront/Back の組合せをランダム化
                bool isFrontThis = (random.Next(2) == 0);
                bool isFrontNext = (random.Next(2) == 0);

                railComponents[i].ConnectRailComponent(
                    railComponents[i + 1],
                    isFront_this: isFrontThis,
                    isFront_target: isFrontNext,
                    defaultdistance: -1);
            }

            // 追加で 1~3回ほど余分にconnect
            int extraConnects = random.Next(1, 4);
            for (int c = 0; c < extraConnects; c++)
            {
                var idxA = random.Next(n);
                var idxB = random.Next(n);
                if (idxA == idxB) continue;

                bool isFrontA = (random.Next(2) == 0);
                bool isFrontB = (random.Next(2) == 0);
                railComponents[idxA].ConnectRailComponent(
                    railComponents[idxB],
                    isFront_this: isFrontA,
                    isFront_target: isFrontB,
                    defaultdistance: -1);
            }

            // ----- 3) ノード列を適当に組み立て -----
            //   ここでは最初のレールのfrontNodeから順に並べるだけ。（実際にはダイクストラ等推奨）
            var nodeList = new List<RailNode>();
            var firstRail = railComponents[0];
            nodeList.Add(firstRail.FrontNode);   // 適当に frontNode を始点とする
            nodeList.Add(firstRail.BackNode);

            // 残りのレールの backNodeなども加えてみるだけ
            // (正しくは node の繋がりを BFS or ダイクストラ などで経路を抽出)
            for (int i = 1; i < n; i++)
            {
                nodeList.Add(railComponents[i].FrontNode);
                nodeList.Add(railComponents[i].BackNode);
            }
            // 重複ノードがいる可能性などは一旦無視、あくまでサンプル

            // ----- 4) 距離合計を算出して列車長を決める -----
            int totalDist = 0;
            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                int dist = nodeList[i].GetDistanceToNode(nodeList[i + 1]);
                if (dist > 0) totalDist += dist;
            }
            // 列車長
            int trainLength = Math.Max(8, totalDist / 2);
            var railPos = new RailPosition(nodeList, trainLength, initialDistanceToNextNode: 5);

            Debug.Log($"[RandomRailAndTrain] totalDist={totalDist}, trainLength={trainLength}");

            // ----- 5) ランダムに前進・後退を試す -----
            int steps = random.Next(6, 12); // 適当に6~12回動かす
            for (int s = 0; s < steps; s++)
            {
                // ±1~20くらいの移動量
                int moveDist = random.Next(1, 21);
                if (random.NextDouble() < 0.3)
                {
                    moveDist = -moveDist; // 30%で後退
                }

                int remain = railPos.MoveForward(moveDist);

                Debug.Log($" Step:{s}  move={moveDist}, remain={remain}, " +
                          $" nowApproach={railPos.GetNodeApproaching()} " +
                          $" NodeCount={railPos.TestGet_railNodes().Count}");

                if (railPos.TestGet_railNodes().Count < 2)
                {
                    // もうほぼルートが切れていそうなので終了
                    break;
                }

                // 何らかのawait相当があるならあっても良い
                // GameUpdater.UpdateWithWait(); // もし必要なら(物理的ワールド進行など)
            }

            // ----- 結果を一応アサート -----
            // とりあえずエラーなくここまで動作したらPass
            Assert.Pass("RandomRailAndTrainTest: 正常に完走しました");
        }

        /// <summary>
        /// RailComponent の front/back ノードをすべて railGraph から切断するユーティリティ例
        /// </summary>
        private void DisconnectRailNodes(RailComponent rail)
        {
            var front = rail.FrontNode;
            var back = rail.BackNode;

            // frontに繋がっているノードとの接続をすべて外す
            // railGraphDatastore 上で nodeA, nodeB を DisconnectNode
            DisconnectAllConnections(front);
            DisconnectAllConnections(back);
        }

        private void DisconnectAllConnections(RailNode node)
        {
            // node.ConnectedNodes は IEnumerable なので一旦リスト化
            var connected = node.ConnectedNodes.ToList();
            foreach (var cn in connected)
            {
                // node -> cn を外す
                // RailGraphDatastore のAPIを呼ぶ必要があるが、本来は node 直呼びはラップメソッドがいる
                // ここでは簡略化で
                node.ConnectNode(cn, 0); // こうすると大抵ダミーになる
                // ↑実際は ConnectNode ではなく railGraphDatastore.DisconnectNode(node, cn) などが必要
                //   ただしサンプルコードなので実装は読み替えてください
            }
        }



    }
}