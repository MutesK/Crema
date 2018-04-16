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

using Jint;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ntreev.Crema.Commands;
using Ntreev.Crema.Commands.Consoles;
using Ntreev.Crema.ServiceModel;
using Ntreev.Crema.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Crema.Javascript
{
    public abstract class ScriptContextBase
    {
        private readonly string name;
        private readonly ICremaHost cremaHost;
        private Authentication authentication;
        private TextWriter writer;

        public ScriptContextBase(string name, ICremaHost cremaHost)
        {
            this.name = name;
            this.cremaHost = cremaHost;
            this.cremaHost.Opened += CremaHost_Opened;
            this.cremaHost.Closed += CremaHost_Closed;
        }

        public void Run(string script, string functionName, IDictionary<string, object> properties, object state)
        {
            if (functionName == null)
                throw new ArgumentNullException(nameof(functionName));
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var context = this.CreateContext(state);
            var engine = new Engine(cfg => cfg.CatchClrExceptions());
            foreach (var item in properties)
            {
                engine.SetValue(item.Key, item.Value);
                context.Properties.Add(item.Key, item.Value);
            }

            var methodItems = this.CreateMethods();
            foreach (var item in methodItems)
            {
                item.Context = context;
                engine.SetValue(item.Name, item.Delegate);
            }
            try
            {
                engine.Execute(script);
                if (functionName != string.Empty)
                {
                    engine.Invoke(functionName);
                }
            }
            finally
            {
                foreach (var item in methodItems)
                {
                    item.Dispose();
                }
            }
        }

        public void RunFromFile(string filename, string functionName, IDictionary<string, object> properties, object state)
        {
            this.Run(File.ReadAllText(filename), functionName, properties, state);
        }

        public Task RunAsync(string script, string functionName, IDictionary<string, object> properties, object state)
        {
            return Task.Run(() => this.Run(script, functionName, properties, state));
        }

        public string GenerateDeclaration()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"// declaration for {this.name}");

            var methodItems = this.CreateMethods().OrderBy(item => item.Name);
            try
            {
                if (methodItems.Any() == true)
                    sb.AppendLine();

                foreach (var item in methodItems)
                {
                    sb.Append($"declare function {item.Name} ");
                    sb.Append("(");
                    var methodInfo = item.Delegate.Method;
                    var parameters = methodInfo.GetParameters();

                    var argsString = string.Join(", ", methodInfo.GetParameters().Select(i => this.GetParameterString(i)));
                    sb.Append(argsString);
                    sb.Append($"): {this.GetTypeString(methodInfo.ReturnType)};");

                    sb.AppendLine();
                }

                return sb.ToString();
            }
            finally
            {
                foreach (var item in methodItems)
                {
                    item.Dispose();
                }
            }
        }

        public string GenerateArguments(IDictionary<string, Type> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            var sb = new StringBuilder();
            sb.AppendLine($"// arguments for {this.name}");

            if (properties.Any() == true)
                sb.AppendLine();

            foreach (var item in properties)
            {
                sb.AppendLine($"declare var {item.Key}: {this.GetTypeString(item.Value)}; ");
            }

            return sb.ToString();
        }

        public string GetString(object value)
        {
            if (value != null && ScriptContextBase.IsDictionaryType(value.GetType()))
            {
                return JsonConvert.SerializeObject(value, Formatting.Indented);
            }
            else if (value is System.Dynamic.ExpandoObject exobj)
            {
                return JsonConvert.SerializeObject(exobj, Formatting.Indented, new ExpandoObjectConverter());
            }
            else if (value != null && value.GetType().IsArray)
            {
                return JsonConvert.SerializeObject(value, Formatting.Indented);
            }
            else if (value is bool b)
            {
                return b.ToString().ToLower();
            }
            else
            {
                return value.ToString();
            }
        }

        public TextWriter Out
        {
            get { return this.writer ?? Console.Out; }
            set
            {
                this.writer = value;
            }
        }

        protected abstract IScriptMethod[] CreateMethods();

        protected abstract IScriptMethodContext CreateContext(object state);

        protected void Initialize(Authentication authentication)
        {
            this.authentication = authentication;
        }

        protected void Release()
        {
#if SERVER
            if (this.authentication != null)
            {
                this.authentication.Expired -= Authentication_Expired;
            }
#endif
            this.authentication = null;
        }

        protected Authentication Authentication
        {
            get { return this.authentication; }
        }

#if SERVER
        private void Authentication_Expired(object sender, EventArgs e)
        {

        }
#endif

        private void CremaHost_Opened(object sender, EventArgs e)
        {

        }

        private void CremaHost_Closed(object sender, ClosedEventArgs e)
        {

        }

        private string GetParameterString(ParameterInfo p)
        {
            if (IsNullableType(p.ParameterType) == true)
            {
                return $"{p.Name}?: {this.GetTypeString(p.ParameterType)}";
            }
            else
            {
                return $"{p.Name}: {this.GetTypeString(p.ParameterType)}";
            }
        }

        private string GetTypeString(Type type)
        {
            if (type.IsArray == true)
                return this.GetTypeString(type.GetElementType()) + "[]";
            else if (type == typeof(object))
                return "any";
            else if (type == typeof(bool))
                return "boolean";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(void))
                return "void";
            else if (type == typeof(int))
                return "number";
            else if (type == typeof(uint))
                return "number";
            else if (type == typeof(long))
                return "number";
            else if (type == typeof(ulong))
                return "number";
            else if (type == typeof(float))
                return "number";
            else if (type == typeof(double))
                return "number";
            else if (type == typeof(decimal))
                return "number";
            else if (IsDictionaryType(type))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                return $"{{ [key: {this.GetTypeString(keyType)}]: {this.GetTypeString(valueType)}; }}";
            }
            return "number";
        }

        internal static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>) || type.GetGenericTypeDefinition() == typeof(Dictionary<,>));
        }

        internal static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}