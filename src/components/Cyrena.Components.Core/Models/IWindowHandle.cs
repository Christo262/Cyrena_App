using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Models
{
    public interface IWindowHandle
    {
        Task CloseAsync();
    }

    public sealed class CompatRootComponent
    {
        public CompatRootComponent(Type componentType, string selector)
        {
            ComponentType = componentType;
            Selector = selector;
        }

        public Type ComponentType { get; }
        public string Selector { get; }
    }

    public sealed class CompatRootComponentBuilder
    {
        private readonly List<CompatRootComponent> _components;
        public CompatRootComponentBuilder()
        {
            _components = new List<CompatRootComponent>();
        }

        public void Add<TComponent>(string selector)
            where TComponent:ComponentBase
        {
            if (_components.Any(x => x.Selector == selector))
                throw new InvalidOperationException($"{selector} already added");
            _components.Add(new CompatRootComponent(typeof(TComponent), selector));
        }

        public void Add(string selector, Type componentType)
        {
            if (_components.Any(x => x.Selector == selector))
                throw new InvalidOperationException($"{selector} already added");
            _components.Add(new CompatRootComponent(componentType, selector));
        }

        public IReadOnlyList<CompatRootComponent> Components => _components.AsReadOnly();
    }
}
