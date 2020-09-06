// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;

namespace MDSDK.JPEG2000.Utils
{
    internal class GenericDisposable : IDisposable
    {
        private readonly Action _disposeAction;

        public GenericDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction.Invoke();
        }
    }
}
