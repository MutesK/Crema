﻿using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace Ntreev.Crema.Javascript.Methods.ListenerHosts.Users
{
    [Export(typeof(CremaEventListenerHost))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    class UserItemDeletedEventListenerHost : CremaEventListenerHost
    {
        private readonly ICremaHost cremaHost;

        [ImportingConstructor]
        public UserItemDeletedEventListenerHost(ICremaHost cremaHost)
            : base(CremaEvents.UserItemDeleted)
        {
            this.cremaHost = cremaHost;
        }

        protected override void OnSubscribe()
        {
            if (this.cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                userContext.Dispatcher.Invoke(() => userContext.ItemsDeleted += UserContext_ItemsDeleted);
            }
        }

        protected override void OnUnsubscribe()
        {
            if (this.cremaHost.GetService(typeof(IUserContext)) is IUserContext userContext)
            {
                userContext.Dispatcher.Invoke(() => userContext.ItemsDeleted -= UserContext_ItemsDeleted);
            }
        }

        private void UserContext_ItemsDeleted(object sender, ItemsDeletedEventArgs<IUserItem> e)
        {
            this.Invoke(null);
        }
    }
}
