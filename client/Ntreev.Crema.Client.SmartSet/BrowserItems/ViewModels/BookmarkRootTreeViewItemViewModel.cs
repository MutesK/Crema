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

using Ntreev.Crema.Client.SmartSet.Properties;
using Ntreev.Crema.Services;
using Ntreev.ModernUI.Framework;
using Ntreev.ModernUI.Framework.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ntreev.ModernUI.Framework.Dialogs.ViewModels;
using System.ComponentModel.Composition;
using Ntreev.Library.IO;
using Ntreev.Library.ObjectModel;
using Ntreev.Crema.Client.SmartSet.Dialogs.ViewModels;
using Ntreev.Crema.Client.Framework;

namespace Ntreev.Crema.Client.SmartSet.BrowserItems.ViewModels
{
    // ☆Bookmark
    abstract class BookmarkRootTreeViewItemViewModel : TreeViewItemViewModel
    {
        protected BookmarkRootTreeViewItemViewModel(SmartSetBrowserViewModel browser)
        {
            this.Owner = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        public override string DisplayName
        {
            get { return Resources.Title_Bookmark; }
        }

        public SmartSetBrowserViewModel Browser
        {
            get { return this.Owner as SmartSetBrowserViewModel; }
        }

        public void NewFolder()
        {
            var query = from item in this.Items
                        where item is BookmarkCategoryTreeViewItemViewModel
                        let viewModel = item as BookmarkCategoryTreeViewItemViewModel
                        select viewModel.DisplayName;

            var dialog = new NewCategoryViewModel(PathUtility.Separator, query.ToArray());
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var viewModel = this.CreateInstance(dialog.CategoryPath, this.Browser);
                this.Items.Add(viewModel);
                this.Items.Reposition(viewModel);
                this.IsExpanded = true;
                this.Browser.UpdateBookmarkItems();
            }
            catch (Exception e)
            {
                AppMessageBox.ShowError(e);
            }
        }

        public abstract BookmarkCategoryTreeViewItemViewModel CreateInstance(string path, SmartSetBrowserViewModel browser);
    }
}
