# AspectCore.Net4
[AspectCore Framework](https://github.com/dotnetcore/AspectCore-Framework)是以AOP为核心的跨平台开发库，支持 .NET Standard1.6和.NET4.5及以上。
为在.NET4.0使用AspectCore Framework，特此在此仓库维护AspectCore的.NET分支。此分支包括：
* AspectCore的核心库
* AspectCore的Autofac扩展
## nuget
```
    PM> Install-Package AspectCore.Extensions.Net4
```
AspectCore提供RegisterAspectCore扩展方法在Autofac的Container中注册动态代理需要的服务，并提供AsInterfacesProxy和AsClassProxy扩展方法启用interface和class的代理：
```
    var container = new ContainerBuilder();

    container.RegisterAspectCore();

    container.RegisterType<CustomService>().As<ICustomService>().InstancePerDependency().AsInterfacesProxy();
```
在Asp.Net Mvc中使用AspectCore参考 [https://github.com/AspectCore/Sample/tree/master/net45/AspectCore.Sample.Net45](https://github.com/AspectCore/Sample/tree/master/net45/AspectCore.Sample.Net45)