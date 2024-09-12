using System;
using Game.Block.Blocks.Machine.Inventory;
using Game.Block.Blocks.Util;
using Game.Block.Interface;
using Game.Block.Interface.Component;
using Game.Block.Interface.State;
using Game.EnergySystem;
using MessagePack;
using Mooresmaster.Model.MachineRecipesModule;
using UniRx;

namespace Game.Block.Blocks.Machine
{
    public class VanillaMachineProcessorComponent : IBlockStateChange, IUpdatableBlockComponent
    {
        public ProcessState CurrentState { get; private set; } = ProcessState.Idle;
        
        public double RemainingSecond { get; private set; }
        
        public Guid RecipeGuid => _processingRecipe.MachineRecipeGuid;
        public IObservable<BlockState> OnChangeBlockState => _changeState;
        
        private readonly Subject<BlockState> _changeState = new();
        
        private readonly VanillaMachineInputInventory _vanillaMachineInputInventory;
        private readonly VanillaMachineOutputInventory _vanillaMachineOutputInventory;
        
        public readonly ElectricPower RequestPower;
        
        private ElectricPower _currentPower;
        private ProcessState _lastState = ProcessState.Idle;
        private MachineRecipeMasterElement _processingRecipe;
        
        
        public VanillaMachineProcessorComponent(
            VanillaMachineInputInventory vanillaMachineInputInventory,
            VanillaMachineOutputInventory vanillaMachineOutputInventory,
            MachineRecipeMasterElement machineRecipe, ElectricPower requestPower)
        {
            _vanillaMachineInputInventory = vanillaMachineInputInventory;
            _vanillaMachineOutputInventory = vanillaMachineOutputInventory;
            _processingRecipe = machineRecipe;
            RequestPower = requestPower;
            
            //TODO コンポーネント化する
        }
        
        public VanillaMachineProcessorComponent(
            VanillaMachineInputInventory vanillaMachineInputInventory,
            VanillaMachineOutputInventory vanillaMachineOutputInventory,
            ProcessState currentState, double remainingSecond, MachineRecipeMasterElement processingRecipe,
            ElectricPower requestPower)
        {
            _vanillaMachineInputInventory = vanillaMachineInputInventory;
            _vanillaMachineOutputInventory = vanillaMachineOutputInventory;
            
            _processingRecipe = processingRecipe;
            RequestPower = requestPower;
            RemainingSecond = remainingSecond;
            
            CurrentState = currentState;
        }
        
        public BlockState GetBlockState()
        {
            BlockException.CheckDestroy(this);
            
            var processingRate = _processingRecipe != null ? 1 - (float)RemainingSecond / _processingRecipe.Time : 0;
            var currentState = MessagePackSerializer.Serialize(new CommonMachineBlockStateChangeData(_currentPower.AsPrimitive(), RequestPower.AsPrimitive(), processingRate));
            return new BlockState(CurrentState.ToStr(), _lastState.ToStr(),currentState);
        }
        
        public void SupplyPower(ElectricPower power)
        {
            _currentPower = power;
        }
        
        public void Update()
        {
            BlockException.CheckDestroy(this);
            
            switch (CurrentState)
            {
                case ProcessState.Idle:
                    Idle();
                    break;
                case ProcessState.Processing:
                    Processing();
                    break;
            }
            
            //ステートの変化を検知した時か、ステートが処理中の時はイベントを発火させる
            if (_lastState != CurrentState || CurrentState == ProcessState.Processing)
            {
                var state = GetBlockState();
                _changeState.OnNext(state);
                _lastState = CurrentState;
            }
        }
        
        private void Idle()
        {
            var isGetRecipe = _vanillaMachineInputInventory.TryGetRecipeElement(out var recipe);
            var isStartProcess = CurrentState == ProcessState.Idle && isGetRecipe &&
                   _vanillaMachineInputInventory.IsAllowedToStartProcess() &&
                   _vanillaMachineOutputInventory.IsAllowedToOutputItem(recipe);
            
            if (isStartProcess)
            {
                CurrentState = ProcessState.Processing;
                _processingRecipe = recipe;
                _vanillaMachineInputInventory.ReduceInputSlot(_processingRecipe);
                RemainingSecond = _processingRecipe.Time;
            }
        }
        
        private void Processing()
        {
            RemainingSecond -= MachineCurrentPowerToSubSecond.GetSubSecond(_currentPower, RequestPower);
            if (RemainingSecond <= 0)
            {
                CurrentState = ProcessState.Idle;
                _vanillaMachineOutputInventory.InsertOutputSlot(_processingRecipe);
            }
            
            //電力を消費する
            _currentPower = new ElectricPower(0);
        }
        
        public bool IsDestroy { get; private set; }
        public void Destroy()
        {
            IsDestroy = true;
        }
    }
    
    public static class ProcessStateExtension
    {
        /// <summary>
        ///     <see cref="ProcessState" />をStringに変換します。
        ///     EnumのToStringを使わない理由はアロケーションによる速度低下をなくすためです。
        /// </summary>
        public static string ToStr(this ProcessState state)
        {
            return state switch
            {
                ProcessState.Idle => VanillaMachineBlockStateConst.IdleState,
                ProcessState.Processing => VanillaMachineBlockStateConst.ProcessingState,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            };
        }
    }
    
    public enum ProcessState
    {
        Idle,
        Processing,
    }
}