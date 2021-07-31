﻿using System;
using System.Collections.Generic;
using System.Linq;
using industrialization.Core.Config.Installation;
using industrialization.Core.Config.Recipe.Data;
using industrialization.Core.GameSystem;
using industrialization.Core.Item;
using industrialization.Core.Util;

namespace industrialization.Core.Installation.Machine
{
    //TODO アウトプットのほうもつくる
    public class NormalMachine : InstallationBase,IInstallationInventory,IUpdate
    {
        private readonly NormalMachineInputInventory _normalMachineInputInventory;
        private readonly NormalMachineOutputInventory _normalMachineOutputInventory;
        private ProcessState _state = ProcessState.Idle;
        
        public NormalMachine(int installationId, int intId,
            NormalMachineInputInventory normalMachineInputInventory,
            NormalMachineOutputInventory normalMachineOutputInventory ) : base(installationId, intId)
        {
            _normalMachineInputInventory = normalMachineInputInventory;
            _normalMachineOutputInventory = normalMachineOutputInventory;
            intId = intId;
            InstallationID = installationId;
            GameUpdate.AddUpdateObject(this);
        }
        public IItemStack InsertItem(IItemStack itemStack)
        {
            //アイテムをインプットスロットに入れた後、プロセス開始できるなら開始
            var item = _normalMachineInputInventory.InsertItem(itemStack);
            return item;
        }
        public void ChangeConnector(IInstallationInventory installationInventory)
        {
            _normalMachineOutputInventory.ChangeConnectInventory(installationInventory);
        }

        private bool IsAllowedToStartProcess
        {
            get
            {
                var recipe = _normalMachineInputInventory.GetRecipeData();
                return _state == ProcessState.Idle && 
                       _normalMachineInputInventory.IsAllowedToStartProcess && 
                       _normalMachineOutputInventory.IsAllowedToOutputItem(recipe);
            }
        }
            

        public void Update()
        {
            switch (_state)
            {
                case ProcessState.Idle :
                    Idle();
                    break;
                case ProcessState.Processing :
                    Processing();
                    break;
                case ProcessState.ProcessingExit :
                    ProcessingExit();
                    break;
            }
        }
        private void Idle()
        {
            if (IsAllowedToStartProcess) StartProcessing();
        }

        private IMachineRecipeData _processingRecipeData;
        private void StartProcessing()
        {
            _state = ProcessState.Processing;
            _processingRecipeData = _normalMachineInputInventory.GetRecipeData();
            _normalMachineInputInventory.ReduceInputSlot(_processingRecipeData);
        }
        private void Processing()
        {
            
        }
        private void ProcessingExit()
        {
            
        }
    }
}