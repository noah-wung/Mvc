﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class UrlHelperTest
    {
        [Theory]
        [InlineData("", "/Home/About", "/Home/About")]
        [InlineData("/myapproot", "/test", "/test")]
        public void Content_ReturnsContentPath_WhenItDoesNotStartWithToken(string appRoot,
                                                                           string contentPath,
                                                                           string expectedPath)
        {
            // Arrange
            var context = CreateHttpContext(GetServices(), appRoot);
            var contextAccessor = CreateActionContext(context);
            var urlHelper = CreateUrlHelper(contextAccessor);

            // Act
            var path = urlHelper.Content(contentPath);

            // Assert
            Assert.Equal(expectedPath, path);
        }

        [Theory]
        [InlineData(null, "~/Home/About", "/Home/About")]
        [InlineData("/", "~/Home/About", "/Home/About")]
        [InlineData("/", "~/", "/")]
        [InlineData("/myapproot", "~/", "/myapproot/")]
        [InlineData("", "~/Home/About", "/Home/About")]
        [InlineData("/myapproot", "~/Content/bootstrap.css", "/myapproot/Content/bootstrap.css")]
        public void Content_ReturnsAppRelativePath_WhenItStartsWithToken(string appRoot,
                                                                         string contentPath,
                                                                         string expectedPath)
        {
            // Arrange
            var context = CreateHttpContext(GetServices(), appRoot);
            var contextAccessor = CreateActionContext(context);
            var urlHelper = CreateUrlHelper(contextAccessor);

            // Act
            var path = urlHelper.Content(contentPath);

            // Assert
            Assert.Equal(expectedPath, path);
        }

        // UrlHelper.IsLocalUrl depends on the UrlUtility.IsLocalUrl method.
        // To avoid duplicate tests, all the tests exercising IsLocalUrl verify
        // both of UrlHelper.IsLocalUrl and UrlUtility.IsLocalUrl
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void IsLocalUrl_ReturnsFalseOnEmpty(string url)
        {
            // Arrange
            var helper = CreateUrlHelper();

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("/foo.html")]
        [InlineData("/www.example.com")]
        [InlineData("/")]
        public void IsLocalUrl_AcceptsRootedUrls(string url)
        {
            // Arrange
            var helper = CreateUrlHelper();

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.True(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("~/")]
        [InlineData("~/foo.html")]
        public void IsLocalUrl_AcceptsApplicationRelativeUrls(string url)
        {
            // Arrange
            var helper = CreateUrlHelper();

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.True(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("foo.html")]
        [InlineData("../foo.html")]
        [InlineData("fold/foo.html")]
        public void IsLocalUrl_RejectsRelativeUrls(string url)
        {
            // Arrange
            var helper = CreateUrlHelper();

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("http:/foo.html")]
        [InlineData("hTtP:foo.html")]
        [InlineData("http:/www.example.com")]
        [InlineData("HtTpS:/www.example.com")]
        public void IsLocalUrl_RejectValidButUnsafeRelativeUrls(string url)
        {
            // Arrange
            var helper = CreateUrlHelper();

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("http://www.mysite.com/appDir/foo.html")]
        [InlineData("http://WWW.MYSITE.COM")]
        public void IsLocalUrl_RejectsUrlsOnTheSameHost(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("http://localhost/foobar.html")]
        [InlineData("http://127.0.0.1/foobar.html")]
        public void IsLocalUrl_RejectsUrlsOnLocalHost(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("https://www.mysite.com/")]
        public void IsLocalUrl_RejectsUrlsOnTheSameHostButDifferentScheme(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("http://www.example.com")]
        [InlineData("https://www.example.com")]
        [InlineData("hTtP://www.example.com")]
        [InlineData("HtTpS://www.example.com")]
        public void IsLocalUrl_RejectsUrlsOnDifferentHost(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("http://///www.example.com/foo.html")]
        [InlineData("https://///www.example.com/foo.html")]
        [InlineData("HtTpS://///www.example.com/foo.html")]
        [InlineData("http:///www.example.com/foo.html")]
        [InlineData("http:////www.example.com/foo.html")]
        public void IsLocalUrl_RejectsUrlsWithTooManySchemeSeparatorCharacters(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("//www.example.com")]
        [InlineData("//www.example.com?")]
        [InlineData("//www.example.com:80")]
        [InlineData("//www.example.com/foobar.html")]
        [InlineData("///www.example.com")]
        [InlineData("//////www.example.com")]
        public void IsLocalUrl_RejectsUrlsWithMissingSchemeName(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("http:\\\\www.example.com")]
        [InlineData("http:\\\\www.example.com\\")]
        [InlineData("/\\")]
        [InlineData("/\\foo")]
        public void IsLocalUrl_RejectsInvalidUrls(string url)
        {
            // Arrange
            var helper = CreateUrlHelper("www.mysite.com");

            // Act
            var result = helper.IsLocalUrl(url);

            // Assert
            Assert.False(result);

            // Arrange & Act
            result = UrlUtility.IsLocalUrl(url);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RouteUrlWithDictionary()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(values: new RouteValueDictionary(
                                                                    new
                                                                    {
                                                                        Action = "newaction",
                                                                        Controller = "home2",
                                                                        id = "someid"
                                                                    }));

            // Assert
            Assert.Equal("/app/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithEmptyHostName()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new RouteValueDictionary(
                                                                    new
                                                                    {
                                                                        Action = "newaction",
                                                                        Controller = "home2",
                                                                        id = "someid"
                                                                    }),
                                         protocol: "http",
                                         host: string.Empty);

            // Assert
            Assert.Equal("http://localhost/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithEmptyProtocol()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new RouteValueDictionary(
                                                                    new
                                                                    {
                                                                        Action = "newaction",
                                                                        Controller = "home2",
                                                                        id = "someid"
                                                                    }),
                                         protocol: string.Empty,
                                         host: "foo.bar.com");

            // Assert
            Assert.Equal("http://foo.bar.com/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithNullProtocol()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new RouteValueDictionary(
                                                                    new
                                                                    {
                                                                        Action = "newaction",
                                                                        Controller = "home2",
                                                                        id = "someid"
                                                                    }),
                                         protocol: null,
                                         host: "foo.bar.com");

            // Assert
            Assert.Equal("http://foo.bar.com/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithNullProtocolAndNullHostName()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new RouteValueDictionary(
                                                                    new
                                                                    {
                                                                        Action = "newaction",
                                                                        Controller = "home2",
                                                                        id = "someid"
                                                                    }),
                                         protocol: null,
                                         host: null);

            // Assert
            Assert.Equal("/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithObjectProperties()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(new { Action = "newaction", Controller = "home2", id = "someid" });

            // Assert
            Assert.Equal("/app/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithProtocol()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new
                                         {
                                             Action = "newaction",
                                             Controller = "home2",
                                             id = "someid"
                                         },
                                         protocol: "https");

            // Assert
            Assert.Equal("https://localhost/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrl_WithUnicodeHost_DoesNotPunyEncodeTheHost()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new
                                         {
                                             Action = "newaction",
                                             Controller = "home2",
                                             id = "someid"
                                         },
                                         protocol: "https",
                                         host: "pingüino");

            // Assert
            Assert.Equal("https://pingüino/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithRouteNameAndDefaults()
        {
            // Arrange
            var services = GetServices();
            var routeCollection = GetRouter(services, "MyRouteName", "any/url");
            var urlHelper = CreateUrlHelper("/app", routeCollection);

            // Act
            var url = urlHelper.RouteUrl("MyRouteName");

            // Assert
            Assert.Equal("/app/any/url", url);
        }

        [Fact]
        public void RouteUrlWithRouteNameAndDictionary()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new RouteValueDictionary(
                                                            new
                                                            {
                                                                Action = "newaction",
                                                                Controller = "home2",
                                                                id = "someid"
                                                            }));

            // Assert
            Assert.Equal("/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithRouteNameAndObjectProperties()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute",
                                         values: new
                                         {
                                             Action = "newaction",
                                             Controller = "home2",
                                             id = "someid"
                                         });

            // Assert
            Assert.Equal("/app/named/home2/newaction/someid", url);
        }

        [Fact]
        public void RouteUrlWithUrlRouteContext_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            var routeContext = new UrlRouteContext()
            {
                RouteName = "namedroute",
                Values = new
                {
                    Action = "newaction",
                    Controller = "home2",
                    id = "someid"
                },
                Fragment = "somefragment",
                Host = "remotetown",
                Protocol = "ftp"
            };

            // Act
            var url = urlHelper.RouteUrl(routeContext);

            // Assert
            Assert.Equal("ftp://remotetown/app/named/home2/newaction/someid#somefragment", url);
        }

        [Fact]
        public void RouteUrlWithAllParameters_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.RouteUrl(
                routeName: "namedroute",
                values: new
                {
                    Action = "newaction",
                    Controller = "home2",
                    id = "someid"
                },
                fragment: "somefragment",
                host: "remotetown",
                protocol: "https");

            // Assert
            Assert.Equal("https://remotetown/app/named/home2/newaction/someid#somefragment", url);
        }

        [Fact]
        public void UrlAction_RouteValuesAsDictionary_CaseSensitive()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // We're using a dictionary with a case-sensitive comparer and loading it with data
            // using casings differently from the route. This should still successfully generate a link.
            var dict = new Dictionary<string, object>();
            var id = "suppliedid";
            var isprint = "true";
            dict["ID"] = id;
            dict["isprint"] = isprint;

            // Act
            var url = urlHelper.Action(
                                    action: "contact",
                                    controller: "home",
                                    values: dict);

            // Assert
            Assert.Equal(2, dict.Count);
            Assert.Same(id, dict["ID"]);
            Assert.Same(isprint, dict["isprint"]);
            Assert.Equal("/app/home/contact/suppliedid?isprint=true", url);
        }

        [Fact]
        public void UrlAction_WithUnicodeHost_DoesNotPunyEncodeTheHost()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Action(
                                    action: "contact",
                                    controller: "home",
                                    values: null,
                                    protocol: "http",
                                    host: "pingüino");

            // Assert
            Assert.Equal("http://pingüino/app/home/contact", url);
        }

        [Fact]
        public void UrlRouteUrl_RouteValuesAsDictionary_CaseSensitive()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // We're using a dictionary with a case-sensitive comparer and loading it with data
            // using casings differently from the route. This should still successfully generate a link.
            var dict = new Dictionary<string, object>();
            var action = "contact";
            var controller = "home";
            var id = "suppliedid";

            dict["ACTION"] = action;
            dict["Controller"] = controller;
            dict["ID"] = id;

            // Act
            var url = urlHelper.RouteUrl(routeName: "namedroute", values: dict);

            // Assert
            Assert.Equal(3, dict.Count);
            Assert.Same(action, dict["ACTION"]);
            Assert.Same(controller, dict["Controller"]);
            Assert.Same(id, dict["ID"]);
            Assert.Equal("/app/named/home/contact/suppliedid", url);
        }

        [Fact]
        public void UrlActionWithUrlActionContext_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            var actionContext = new UrlActionContext()
            {
                Action = "contact",
                Controller = "home3",
                Values = new { id = "idone" },
                Protocol = "ftp",
                Host = "remotelyhost",
                Fragment = "somefragment"
            };

            // Act
            var url = urlHelper.Action(actionContext);

            // Assert
            Assert.Equal("ftp://remotelyhost/app/home3/contact/idone#somefragment", url);
        }

        [Fact]
        public void UrlActionWithAllParameters_ReturnsExpectedResult()
        {
            // Arrange
            var services = GetServices();
            var urlHelper = CreateUrlHelperWithRouteCollection(services, "/app");

            // Act
            var url = urlHelper.Action(
                controller: "home3",
                action: "contact",
                values: null,
                protocol: "https",
                host: "remotelyhost",
                fragment: "somefragment"
                );

            // Assert
            Assert.Equal("https://remotelyhost/app/home3/contact#somefragment", url);
        }

        private static HttpContext CreateHttpContext(
            IServiceProvider services,
            string appRoot)
        {
            var context = new DefaultHttpContext();
            context.RequestServices = services;

            context.Request.PathBase = new PathString(appRoot);
            context.Request.Host = new HostString("localhost");

            return context;
        }

        private static IScopedInstance<ActionContext> CreateActionContext(HttpContext context)
        {
            return CreateActionContext(context, (new Mock<IRouter>()).Object);
        }

        private static IScopedInstance<ActionContext> CreateActionContext(HttpContext context, IRouter router)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(router);

            var actionContext = new ActionContext(context,
                                                  routeData,
                                                  new ActionDescriptor());
            var contextAccessor = new Mock<IScopedInstance<ActionContext>>();
            contextAccessor.SetupGet(c => c.Value)
                           .Returns(actionContext);
            return contextAccessor.Object;
        }

        private static UrlHelper CreateUrlHelper()
        {
            var services = GetServices();
            var context = CreateHttpContext(services, string.Empty);
            var actionContext = CreateActionContext(context);

            var actionSelector = new Mock<IActionSelector>(MockBehavior.Strict);
            return new UrlHelper(actionContext, actionSelector.Object);
        }

        private static UrlHelper CreateUrlHelper(string host)
        {
            var services = GetServices();
            var context = CreateHttpContext(services, string.Empty);
            context.Request.Host = new HostString(host);

            var actionContext = CreateActionContext(context);

            var actionSelector = new Mock<IActionSelector>(MockBehavior.Strict);
            return new UrlHelper(actionContext, actionSelector.Object);
        }

        private static UrlHelper CreateUrlHelper(IScopedInstance<ActionContext> contextAccessor)
        {
            var actionSelector = new Mock<IActionSelector>(MockBehavior.Strict);
            return new UrlHelper(contextAccessor, actionSelector.Object);
        }

        private static UrlHelper CreateUrlHelper(string appBase, IRouter router)
        {
            var services = GetServices();
            var context = CreateHttpContext(services, appBase);
            var actionContext = CreateActionContext(context, router);

            var actionSelector = new Mock<IActionSelector>(MockBehavior.Strict);
            return new UrlHelper(actionContext, actionSelector.Object);
        }

        private static UrlHelper CreateUrlHelperWithRouteCollection(IServiceProvider services, string appPrefix)
        {
            var routeCollection = GetRouter(services);
            return CreateUrlHelper(appPrefix, routeCollection);
        }

        private static IRouter GetRouter(IServiceProvider services)
        {
            return GetRouter(services, "mockRoute", "/mockTemplate");
        }

        private static IServiceProvider GetServices()
        {
            var services = new Mock<IServiceProvider>();

            var optionsAccessor = new Mock<IOptions<RouteOptions>>();
            optionsAccessor
                .SetupGet(o => o.Options)
                .Returns(new RouteOptions());
            services
                .Setup(s => s.GetService(typeof(IOptions<RouteOptions>)))
                .Returns(optionsAccessor.Object);

            services
                .Setup(s => s.GetService(typeof(IInlineConstraintResolver)))
                .Returns(new DefaultInlineConstraintResolver(optionsAccessor.Object));

            services
                .Setup(s => s.GetService(typeof(ILoggerFactory)))
                .Returns(NullLoggerFactory.Instance);

            return services.Object;
        }


        private static IRouter GetRouter(
            IServiceProvider services, 
            string mockRouteName,
            string mockTemplateValue)
        {
            var routeBuilder = new RouteBuilder();
            routeBuilder.ServiceProvider = services;

            var target = new Mock<IRouter>(MockBehavior.Strict);
            target
                .Setup(router => router.GetVirtualPath(It.IsAny<VirtualPathContext>()))
                .Callback<VirtualPathContext>(context => context.IsBound = true)
                .Returns<VirtualPathContext>(context => null);
            routeBuilder.DefaultHandler = target.Object;

            routeBuilder.MapRoute(string.Empty,
                        "{controller}/{action}/{id}",
                        new RouteValueDictionary(new { id = "defaultid" }));

            routeBuilder.MapRoute("namedroute",
                        "named/{controller}/{action}/{id}",
                        new RouteValueDictionary(new { id = "defaultid" }));

            var mockHttpRoute = new Mock<IRouter>();
            mockHttpRoute
                .Setup(mock => mock.GetVirtualPath(It.Is<VirtualPathContext>(c => string.Equals(c.RouteName, mockRouteName))))
                .Callback<VirtualPathContext>(c => c.IsBound = true)
                .Returns(mockTemplateValue);

            routeBuilder.Routes.Add(mockHttpRoute.Object);
            return routeBuilder.Build();
        }
    }
}