using System;
using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public interface ICoreFieldModule
    {
        Task LoadMenuModuleAsync(IMenuModuleArgs args);
        void ResetModule<TConcrete>() where TConcrete : IFieldModule;
    }

    public interface IFieldModule : IModule
    {

    }

    public interface IFieldModuleArgs : IModuleArgs
    {

    }

    public readonly struct FieldModuleArgs : IFieldModuleArgs
    {
        public Type Field { get; }

        public FieldModuleArgs(Type field)
        {
            Field = field;
        }
    }

    public interface IField
    {

    }
}