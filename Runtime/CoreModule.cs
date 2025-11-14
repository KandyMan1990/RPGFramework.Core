using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RPGFramework.Core
{
    public class CoreModule : IEntryPoint, ICoreFieldModule, ICoreMenuModule
    {
        private readonly Dictionary<Type, Func<object>> m_Bindings;
        private readonly IEntryPoint                    m_EntryPoint;

        private IModule m_CurrentModule;

        private CoreModule()
        {
            m_Bindings      = new Dictionary<Type, Func<object>>();
            m_CurrentModule = new NullModule();
            m_EntryPoint    = this;

            m_EntryPoint.BindSingletonFromInstance<ICoreFieldModule>(this);
            m_EntryPoint.BindSingletonFromInstance<ICoreMenuModule>(this);
        }

        public static IEntryPoint Create()
        {
            return new CoreModule();
        }

        void IEntryPoint.Bind<TInterface, TConcrete>()
        {
            m_Bindings[typeof(TInterface)] = () => CreateInstance<TInterface, TConcrete>()!;
        }

        void IEntryPoint.BindSingleton<TInterface, TConcrete>()
        {
            BindSingleton<TInterface, TConcrete>();
        }

        void IEntryPoint.BindSingletonFromInstance<TInterface>(TInterface instance)
        {
            m_Bindings[typeof(TInterface)] = () => instance!;
        }

        Task IEntryPoint.StartGameAsync()
        {
            return LoadModuleAsync<IMenuModule>(new MenuModuleArgs());
        }

        void ICoreFieldModule.ResetModule<TInterface, TConcrete>()
        {
            BindSingleton<TInterface, TConcrete>();
        }

        void ICoreMenuModule.ResetModule<TInterface, TConcrete>()
        {
            BindSingleton<TInterface, TConcrete>();
        }

        private void BindSingleton<TInterface, TConcrete>()
        {
            Lazy<TInterface> lazy = new Lazy<TInterface>(() => CreateInstance<TInterface, TConcrete>()!);
            m_EntryPoint.BindSingletonFromInstance(lazy.Value);
        }

        private TInterface CreateInstance<TInterface, TConcrete>()
        {
            ConstructorInfo constructor = typeof(TConcrete)
                                         .GetConstructors()
                                         .OrderByDescending(c => c.GetParameters().Length)
                                         .First();

            object[] parameters = constructor
                                 .GetParameters()
                                 .Select(p => Resolve(p.ParameterType))
                                 .ToArray();

            return (TInterface)Activator.CreateInstance(typeof(TConcrete), parameters)!;
        }

        private T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type type)
        {
            Func<object> creator = m_Bindings[type];

            return creator();
        }

        private Task LoadModuleAsync<T>(IModuleArgs args) where T : IModule
        {
            m_CurrentModule = Resolve<T>();
            
            return m_CurrentModule.OnEnterAsync(args);
        }
    }
}