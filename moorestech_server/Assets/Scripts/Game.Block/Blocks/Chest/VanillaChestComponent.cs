using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Inventory;
using Core.Item.Interface;
using Core.Update;
using Game.Block.Blocks.Service;
using Game.Block.Component;
using Game.Block.Event;
using Game.Block.Interface;
using Game.Block.Interface.Component;
using Game.Block.Interface.Event;
using Game.Context;
using Newtonsoft.Json;
using UniRx;

namespace Game.Block.Blocks.Chest
{
    public class VanillaChestComponent : IOpenableBlockInventoryComponent, IBlockSaveState
    {
        public ReadOnlyCollection<IItemStack> Items => _itemDataStoreService.Items;
        
        private readonly ConnectingInventoryListPriorityInsertItemService _connectInventoryService;
        private readonly OpenableInventoryItemDataStoreService _itemDataStoreService;
        
        private readonly IDisposable _updateObservable;
        
        public VanillaChestComponent(BlockInstanceId blockInstanceId, int slotNum, BlockConnectorComponent<IBlockInventory> blockConnectorComponent)
        {
            BlockInstanceId = blockInstanceId;
            
            _connectInventoryService = new ConnectingInventoryListPriorityInsertItemService(blockConnectorComponent);
            _itemDataStoreService = new OpenableInventoryItemDataStoreService(InvokeEvent, ServerContext.ItemStackFactory, slotNum);
            
            _updateObservable = GameUpdater.UpdateObservable.Subscribe(_ => Update());
        }
        
        public VanillaChestComponent(string saveData, BlockInstanceId blockInstanceId, int slotNum, BlockConnectorComponent<IBlockInventory> blockConnectorComponent) :
            this(blockInstanceId, slotNum, blockConnectorComponent)
        {
            var itemJsons = JsonConvert.DeserializeObject<List<ItemStackJsonObject>>(saveData);
            for (var i = 0; i < itemJsons.Count; i++)
            {
                var itemStack = itemJsons[i].ToItem();
                _itemDataStoreService.SetItem(i, itemStack);
            }
        }
        
        public BlockInstanceId BlockInstanceId { get; }
        public bool IsDestroy { get; private set; }
        
        public void SetItem(int slot, IItemStack itemStack)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            _itemDataStoreService.SetItem(slot, itemStack);
        }
        
        public IItemStack InsertItem(IItemStack itemStack)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.InsertItem(itemStack);
        }
        
        public int GetSlotSize()
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.GetSlotSize();
        }
        
        public IItemStack GetItem(int slot)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.GetItem(slot);
        }
        
        public void Destroy()
        {
            IsDestroy = true;
            _updateObservable.Dispose();
        }
        
        public string GetSaveState()
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            var itemJson = new List<ItemStackJsonObject>();
            foreach (var item in _itemDataStoreService.Inventory)
            {
                itemJson.Add(new ItemStackJsonObject(item));
            }
            
            return JsonConvert.SerializeObject(itemJson);
        }
        
        public void SetItem(int slot, int itemId, int count)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            _itemDataStoreService.SetItem(slot, itemId, count);
        }
        
        public IItemStack ReplaceItem(int slot, IItemStack itemStack)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.ReplaceItem(slot, itemStack);
        }
        
        public IItemStack ReplaceItem(int slot, int itemId, int count)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.ReplaceItem(slot, itemId, count);
        }
        
        public IItemStack InsertItem(int itemId, int count)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.InsertItem(itemId, count);
        }
        
        public List<IItemStack> InsertItem(List<IItemStack> itemStacks)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            return _itemDataStoreService.InsertItem(itemStacks);
        }
        
        public bool InsertionCheck(List<IItemStack> itemStacks)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            return _itemDataStoreService.InsertionCheck(itemStacks);
        }
        
        private void Update()
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            for (var i = 0; i < _itemDataStoreService.Inventory.Count; i++)
                _itemDataStoreService.SetItem(i,
                    _connectInventoryService.InsertItem(_itemDataStoreService.Inventory[i]));
        }
        
        private void InvokeEvent(int slot, IItemStack itemStack)
        {
            if (IsDestroy) throw BlockException.IsDestroyedException;
            
            var blockInventoryUpdate = (BlockOpenableInventoryUpdateEvent)ServerContext.BlockOpenableInventoryUpdateEvent;
            blockInventoryUpdate.OnInventoryUpdateInvoke(new BlockOpenableInventoryUpdateEventProperties(BlockInstanceId, slot, itemStack));
        }
    }
}