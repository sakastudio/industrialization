using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace MainGame.UnityView
{
    /// <summary>
    /// ネットワークからviewの変更要求が来た時、メインスレッド出ないとメソッドを実行できません
    /// そのため、このオブジェクトのキューに入れ、Updateで呼び出します
    /// </summary>
    public class QueueInsertionMainThreadByExecution : ITickable
    {
        Queue<Action> _actionQueue = new();

        public void Insert(Action action)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(action);
            }
        }

        public void Tick()
        {
            lock (_actionQueue)
            {
                while (_actionQueue.Count > 0)
                {
                    _actionQueue.Dequeue()();
                }
            }
        }
    }
}