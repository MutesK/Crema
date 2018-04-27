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

using Ntreev.Crema.Data.Xml.Schema;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;

namespace Ntreev.Crema.Javascript.Methods.TableTemplate
{
    [Export(typeof(IScriptMethod))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Category(nameof(TableTemplate))]
    class GetTableTemplateColumnPropertiesMethod : DomainScriptMethodBase
    {
        [ImportingConstructor]
        public GetTableTemplateColumnPropertiesMethod(ICremaHost cremaHost)
            : base(cremaHost)
        {

        }

        protected override Delegate CreateDelegate()
        {
            return new Func<string, string, IDictionary<string, object>>(this.GetTableTemplateColumnProperties);
        }

        private IDictionary<string, object> GetTableTemplateColumnProperties(string domainID, string columnName)
        {
            var template = this.GetDomainHost<ITableTemplate>(domainID);
            return template.Dispatcher.Invoke(() =>
            {
                var column = template[columnName];
                if (column == null)
                    throw new ItemNotFoundException(columnName);
                return new Dictionary<string, object>()
                {
                    { nameof(TableColumnProperties.Index), column.Index },
                    { nameof(TableColumnProperties.IsKey), column.IsKey },
                    { nameof(TableColumnProperties.IsUnique), column.IsUnique },
                    { nameof(TableColumnProperties.Name), column.Name },
                    { nameof(TableColumnProperties.DataType), column.DataType },
                    { nameof(TableColumnProperties.DefaultValue), column.DefaultValue },
                    { nameof(TableColumnProperties.Comment), column.Comment },
                    { nameof(TableColumnProperties.AutoIncrement), column.AutoIncrement },
                    { nameof(TableColumnProperties.Tags), column.Tags },
                    { nameof(TableColumnProperties.IsReadOnly), column.IsReadOnly },
                    { nameof(TableColumnProperties.AllowNull), column.AllowNull },
                };
            });
        }
    }
}