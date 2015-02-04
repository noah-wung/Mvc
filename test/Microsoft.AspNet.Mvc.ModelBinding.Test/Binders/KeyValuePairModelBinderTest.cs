// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class KeyValuePairModelBinderTest
    {
        [Fact]
        public async Task BindModel_MissingKey_ReturnsTrue_AndAddsModelValidationError()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider();

            // Create string binder to create the value but not the key.
            var bindingContext = GetBindingContext(valueProvider, CreateStringBinder());
            var binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Null(bindingContext.Model);
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("someName", bindingContext.ModelName);
            Assert.Equal(bindingContext.ModelState["someName.Key"].Errors.First().ErrorMessage, "A value is required.");
        }

        [Fact]
        public async Task BindModel_MissingValue_ReturnsTrue_AndAddsModelValidationError()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider();

            // Create int binder to create the value but not the key.
            var bindingContext = GetBindingContext(valueProvider, CreateIntBinder());
            var binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Null(bindingContext.Model);
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("someName", bindingContext.ModelName);
            Assert.Equal(bindingContext.ModelState["someName.Value"].Errors.First().ErrorMessage, "A value is required.");
        }

        [Fact]
        public async Task BindModel_MissingKyAndMissingValue_DoNotAddModelStateError()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider();

            // Create int binder to create the value but not the key.
            var bindingContext = GetBindingContext(valueProvider, CreateIntBinder());
            var binder = new KeyValuePairModelBinder<int, string>();

            // Act
            bool retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Null(bindingContext.Model);
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("someName", bindingContext.ModelName);
            Assert.Empty(bindingContext.ModelState["someName.Value"].Errors);
        }

        [Fact]
        public async Task BindModel_SubBindingSucceeds()
        {
            // Arrange
            var innerBinder = new CompositeModelBinder(new[] { CreateStringBinder(), CreateIntBinder() });
            var valueProvider = new SimpleHttpValueProvider();
            var bindingContext = GetBindingContext(valueProvider, innerBinder);

            var binder = new KeyValuePairModelBinder<int, string>();

            // Act
            var retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(retVal);
            Assert.Equal(new KeyValuePair<int, string>(42, "some-value"), bindingContext.Model);
        }

        [Fact]
        public async Task TryBindStrongModel_BinderExists_BinderReturnsCorrectlyTypedObject_ReturnsTrue()
        {
            // Arrange
            ModelBindingContext bindingContext = GetBindingContext(new SimpleHttpValueProvider());
            var binder = new KeyValuePairModelBinder<int, string>();

            // Act
            var retVal = await binder.TryBindStrongModel<int>(bindingContext, "key");

            // Assert
            Assert.True(retVal.Success);
            Assert.Equal(42, retVal.Model);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public async Task TryBindStrongModel_BinderExists_BinderReturnsIncorrectlyTypedObject_ReturnsTrue()
        {
            // Arrange
            var innerBinder = new Mock<IModelBinder>();
            innerBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns((ModelBindingContext mbc) =>
                {
                    Assert.Equal("someName.key", mbc.ModelName);
                    return Task.FromResult(true);
                });
            var bindingContext = GetBindingContext(new SimpleHttpValueProvider(), innerBinder.Object);


            var binder = new KeyValuePairModelBinder<int, string>();

            // Act
            var retVal = await binder.TryBindStrongModel<int>(bindingContext, "key");

            // Assert
            Assert.True(retVal.Success);
            Assert.Equal(default(int), retVal.Model);
            Assert.Empty(bindingContext.ModelState);
        }

        private static ModelBindingContext GetBindingContext(
            IValueProvider valueProvider,
            IModelBinder innerBinder = null,
            Type keyValuePairType = null)
        {
            var metataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = metataProvider.GetMetadataForType(null, keyValuePairType ?? typeof(KeyValuePair<int, string>)),
                ModelName = "someName",
                ValueProvider = valueProvider,
                OperationBindingContext = new OperationBindingContext
                {
                    ModelBinder = innerBinder ?? CreateIntBinder(),
                    MetadataProvider = metataProvider,
                    ValidatorProvider = new DataAnnotationsModelValidatorProvider()
                }
            };
            return bindingContext;
        }

        private static IModelBinder CreateIntBinder()
        {
            var mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns((ModelBindingContext mbc) =>
                {
                    if (mbc.ModelType == typeof(int))
                    {
                        mbc.Model = 42;
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(false);
                });
            return mockIntBinder.Object;
        }

        private static IModelBinder CreateStringBinder()
        {
            var mockStringBinder = new Mock<IModelBinder>();
            mockStringBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns((ModelBindingContext mbc) =>
                {
                    if (mbc.ModelType == typeof(string))
                    {
                        mbc.Model = "some-value";
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(false);
                });
            return mockStringBinder.Object;
        }
    }
}
#endif
