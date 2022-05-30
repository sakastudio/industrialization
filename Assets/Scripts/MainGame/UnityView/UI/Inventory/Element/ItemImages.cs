﻿using System;
using System.Collections.Generic;
using GameConst;
using MainGame.Basic;
using MainGame.Mod;
using SinglePlay;
using UnityEngine;
using VContainer;

namespace MainGame.UnityView.UI.Inventory.Element
{
    public class ItemImages
    {
        private readonly List<ItemViewData> _itemImageList = new ();
        private readonly ItemViewData _emptyItemImage = new(null,"Empty");
        private readonly ItemViewData _nothingIndexItemImage;

        public ItemImages(SinglePlayInterface singlePlayInterface)
        {
            _nothingIndexItemImage = new ItemViewData(null,"Item not found");
            _itemImageList.Add(_emptyItemImage);
            
            var textures = ItemTextureLoader.GetItemTexture(ServerConst.ServerModsDirectory,singlePlayInterface);
            foreach (var texture in textures)
            {
                _itemImageList.Add(new ItemViewData(texture.texture2D.ToSprite(),texture.name));
            }
        }


        public ItemViewData GetItemView(int index)
        {
            if (_itemImageList.Count <= index)
            {
                return _nothingIndexItemImage;
            }

            return _itemImageList[index];
        }

        public int GetItemNum() { return _itemImageList.Count; }
    }

    public class ItemViewData
    {
        public readonly Sprite itemImage;
        public readonly string itemName;

        public ItemViewData(Sprite itemImage, string itemName)
        {
            this.itemImage = itemImage;
            this.itemName = itemName;
        }
    }
}