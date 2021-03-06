﻿//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Ntreev.Crema.Commands.Consoles.Properties;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using Ntreev.Library;
using Ntreev.Library.Commands;
using Ntreev.Library.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Ntreev.Crema.Commands.Consoles
{
    abstract class LockCommandBase : ConsoleCommandBase
    {
        protected LockCommandBase(string name)
            : base(name)
        {

        }

        public override bool IsEnabled
        {
            get
            {
                if (this.CommandContext.IsOnline == false)
                    return false;
                return this.CommandContext.Drive is DataBasesConsoleDrive;
            }
        }

        protected string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path) == true)
                return this.CommandContext.Path;
            return this.CommandContext.GetAbsolutePath(path);
        }

        protected ILockable GetObject(Authentication authentication, string path)
        {
            var drive = this.CommandContext.Drive as DataBasesConsoleDrive;
            if (drive.GetObject(authentication, path) is ILockable lockable)
            {
                return lockable;
            }
            throw new NotImplementedException();
        }

        protected void Invoke(Authentication authentication, ILockable lockable, Action action)
        {
            using (UsingDataBase.Set(lockable as IServiceProvider, authentication, true))
            {
                if (lockable is IDispatcherObject dispatcherObject)
                {
                    dispatcherObject.Dispatcher.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }

        protected T Invoke<T>(Authentication authentication, ILockable lockable, Func<T> func)
        {
            using (UsingDataBase.Set(lockable as IServiceProvider, authentication, true))
            {
                if (lockable is IDispatcherObject dispatcherObject)
                {
                    return dispatcherObject.Dispatcher.Invoke(func);
                }
                else
                {
                    return func();
                }
            }
        }
    }
}
