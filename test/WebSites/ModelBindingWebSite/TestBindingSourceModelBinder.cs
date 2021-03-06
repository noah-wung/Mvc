﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace ModelBindingWebSite
{
    public class TestBindingSourceModelBinder : BindingSourceModelBinder
    {
        public TestBindingSourceModelBinder()
            : base(FromTestAttribute.TestBindingSource)
        {
        }

        protected override Task BindModelCoreAsync(ModelBindingContext bindingContext)
        {
            var metadata = (FromTestAttribute)bindingContext.ModelMetadata.BinderMetadata;
            bindingContext.Model = metadata.Value;

            if (!IsSimpleType(bindingContext.ModelType))
            {
                bindingContext.Model = Activator.CreateInstance(bindingContext.ModelType);
            }

            return Task.FromResult(true);
        }

        private bool IsSimpleType(Type type)
        {
            return type.GetTypeInfo().IsPrimitive ||
                type.Equals(typeof(decimal)) ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(DateTime)) ||
                type.Equals(typeof(Guid)) ||
                type.Equals(typeof(DateTimeOffset)) ||
                type.Equals(typeof(TimeSpan));
        }
    }
}