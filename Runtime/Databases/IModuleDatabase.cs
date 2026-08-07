using System;

namespace RPGFramework.Core.Databases
{
    public interface IModuleDatabase
    {
        Type GetModuleType(byte moduleId);
    }
}