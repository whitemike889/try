using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Pocket;

namespace MLS.Agent
{
    internal class ControllerActivator : IControllerActivator
    {
        private readonly PocketContainer _container;

        public ControllerActivator(PocketContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object Create(ControllerContext c) =>
            _container.Resolve(c.ActionDescriptor.ControllerTypeInfo.AsType());

        public void Release(ControllerContext controllerContext, object controller)
        {
            if (controller is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}