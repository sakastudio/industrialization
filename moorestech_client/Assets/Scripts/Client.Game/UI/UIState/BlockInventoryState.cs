using System;
using System.Collections.Generic;
using System.Threading;
using Client.Game.Context;
using Client.Game.Control.MouseKeyboard;
using Client.Game.UI.Inventory.Main;
using Client.Game.UI.Inventory.Sub;
using Core.Item;
using Core.Item.Interface;
using Cysharp.Threading.Tasks;
using Game.Block;
using Game.Context;
using MainGame.UnityView.Chunk;
using MainGame.UnityView.Control;
using UnityEngine;

namespace Client.Game.UI.UIState
{
    public class BlockInventoryState : IUIState
    {
        private readonly BlockInventoryView _blockInventoryView;
        private readonly ChunkBlockGameObjectDataStore _chunkBlockGameObjectDataStore;
        private readonly PlayerInventoryViewController _playerInventoryViewController;

        private CancellationTokenSource _cancellationTokenSource;

        private Vector3Int _openBlockPos;

        public BlockInventoryState(BlockInventoryView blockInventoryView, ChunkBlockGameObjectDataStore chunkBlockGameObjectDataStore, PlayerInventoryViewController playerInventoryViewController)
        {
            _chunkBlockGameObjectDataStore = chunkBlockGameObjectDataStore;
            _playerInventoryViewController = playerInventoryViewController;
            _blockInventoryView = blockInventoryView;

            blockInventoryView.SetActive(false);
        }

        public UIStateEnum GetNext()
        {
            if (InputManager.UI.CloseUI.GetKeyDown || InputManager.UI.OpenInventory.GetKeyDown) return UIStateEnum.GameScreen;

            return UIStateEnum.Current;
        }

        public void OnEnter(UIStateEnum lastStateEnum)
        {
            if (!BlockClickDetect.TryGetCursorOnBlockPosition(out _openBlockPos)) Debug.LogError("開いたブロックの座標が取得できませんでした。UIステートに不具合があります。");
            if (!_chunkBlockGameObjectDataStore.ContainsBlockGameObject(_openBlockPos)) return;

            InputManager.MouseCursorVisible(true);

            //サーバーのリクエスト
            MoorestechContext.VanillaApi.SendOnly.SetOpenCloseBlock(_openBlockPos, true);
            _cancellationTokenSource = new CancellationTokenSource();
            UpdateBlockInventory(_openBlockPos, _cancellationTokenSource.Token).Forget();


            //ブロックインベントリのビューを設定する
            var id = _chunkBlockGameObjectDataStore.GetBlockGameObject(_openBlockPos).BlockId;
            var config = ServerContext.BlockConfig.GetBlockConfig(id);

            var type = config.Type switch
            {
                VanillaBlockType.Chest => BlockInventoryType.Chest,
                VanillaBlockType.Miner => BlockInventoryType.Miner,
                VanillaBlockType.Machine => BlockInventoryType.Machine,
                VanillaBlockType.Generator => BlockInventoryType.Generator,
                _ => throw new ArgumentOutOfRangeException(),
            };

            _blockInventoryView.SetBlockInventoryType(type, _openBlockPos, config.Param, id);

            //UIのオブジェクトをオンにする
            _blockInventoryView.SetActive(true);
            _playerInventoryViewController.SetActive(true);
            _playerInventoryViewController.SetSubInventory(_blockInventoryView);
        }

        public void OnExit()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;

            MoorestechContext.VanillaApi.SendOnly.SetOpenCloseBlock(_openBlockPos, false);

            _blockInventoryView.SetActive(false);
            _playerInventoryViewController.SetActive(false);
        }

        private async UniTask UpdateBlockInventory(Vector3Int pos, CancellationToken ct)
        {
            List<IItemStack> response = await MoorestechContext.VanillaApi.Response.GetBlockInventory(pos, ct);
            _blockInventoryView.SetItemList(response);
        }
    }
}