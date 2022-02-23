using System.Collections.Generic;
using Core.Inventory;
using Core.Item;
using Game.PlayerInventory.Interface;
using Game.PlayerInventory.Interface.Event;
using PlayerInventory.Event;

namespace PlayerInventory.ItemManaged
{
    public class MainInventoryData : IInventory
    {
        private readonly int _playerId;
        private readonly MainInventoryUpdateEvent _mainInventoryUpdateEvent;
        private readonly InventoryItemDataStoreService _inventoryService;

        public MainInventoryData(int playerId, MainInventoryUpdateEvent mainInventoryUpdateEvent,
            ItemStackFactory itemStackFactory)
        {
            _playerId = playerId;
            _mainInventoryUpdateEvent = mainInventoryUpdateEvent;
            _inventoryService = new InventoryItemDataStoreService(InvokeEvent,
                itemStackFactory, PlayerInventoryConst.MainInventorySize);
        }
        public MainInventoryData(int playerId, MainInventoryUpdateEvent mainInventoryUpdateEvent, ItemStackFactory itemStackFactory,List<IItemStack> itemStacks) : 
            this(playerId, mainInventoryUpdateEvent, itemStackFactory)
        {
            for (int i = 0; i < itemStacks.Count; i++)
            {
                _inventoryService.SetItemWithoutEvent(i,itemStacks[i]);
            }
        }

        

        private void InvokeEvent(int slot, IItemStack itemStack)
        {
            _mainInventoryUpdateEvent.OnInventoryUpdateInvoke(new PlayerInventoryUpdateEventProperties(
                _playerId,slot,itemStack));
        }
        
        public IItemStack GetItem(int slot) { return _inventoryService.GetItem(slot); }
        public void SetItem(int slot, IItemStack itemStack) { _inventoryService.SetItem(slot, itemStack); }
        public void SetItem(int slot, int itemId, int count) { _inventoryService.SetItem(slot, itemId,count); }
        public IItemStack ReplaceItem(int slot, IItemStack itemStack) { return _inventoryService.ReplaceItem(slot, itemStack); }
        public IItemStack ReplaceItem(int slot, int itemId, int count) { return _inventoryService.ReplaceItem(slot, itemId,count); }
        public IItemStack InsertItem(IItemStack itemStack) { return _inventoryService.InsertItem(itemStack); }
        public IItemStack InsertItem(int itemId, int count) { return _inventoryService.InsertItem(itemId,count); }
        public int GetSlotSize() { return _inventoryService.GetSlotSize(); }
    }
}