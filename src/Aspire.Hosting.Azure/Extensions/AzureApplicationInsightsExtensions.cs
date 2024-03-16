// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Azure.Provisioning.ApplicationInsights;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding the Azure ApplicationInsights resources to the application model.
/// </summary>
public static class AzureApplicationInsightsExtensions
{
    /// <summary>
    /// Adds an Azure Application Insights resource to the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{AzureApplicationInsightsResource}"/>.</returns>
    public static IResourceBuilder<AzureApplicationInsightsResource> AddAzureApplicationInsights(this IDistributedApplicationBuilder builder, string name)
    {
#pragma warning disable CA2252
        return builder.AddAzureApplicationInsights(name, (_, _, _) => { });
#pragma warning restore CA2252
    }

    /// <summary>
    /// Adds an Azure Application Insights resource to the application model.
    /// </summary>
    /// <param name="builder">The builder for the distributed application.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="configureResource">Optional callback to configure the Application Insights resource.</param>
    /// <returns></returns>
    [RequiresPreviewFeatures]
    public static IResourceBuilder<AzureApplicationInsightsResource> AddAzureApplicationInsights(this IDistributedApplicationBuilder builder, string name, Action<IResourceBuilder<AzureApplicationInsightsResource>, ResourceModuleConstruct, ApplicationInsightsComponent>? configureResource)
    {
        var configureConstruct = (ResourceModuleConstruct construct) =>
        {
            var appInsights = new ApplicationInsightsComponent(construct, name: name);
            appInsights.Properties.Tags["aspire-resource-name"] = construct.Resource.Name;
            appInsights.AssignProperty(p => p.ApplicationType, new Parameter("applicationType", defaultValue: "web"));
            appInsights.AssignProperty(p => p.Kind, new Parameter("kind", defaultValue: "web"));
            appInsights.AssignProperty(p => p.WorkspaceResourceId, new Parameter(AzureBicepResource.KnownParameters.LogAnalyticsWorkspaceId));

            appInsights.AddOutput("appInsightsConnectionString", p => p.ConnectionString);

            if (configureResource != null)
            {
                var resource = (AzureApplicationInsightsResource)construct.Resource;
                var resourceBuilder = builder.CreateResourceBuilder(resource);
                configureResource(resourceBuilder, construct, appInsights);
            }
        };
        var resource = new AzureApplicationInsightsResource(name, configureConstruct);

        return builder.AddResource(resource)
                      .WithParameter(AzureBicepResource.KnownParameters.PrincipalId)
                      .WithParameter(AzureBicepResource.KnownParameters.PrincipalType)
                      .WithManifestPublishingCallback(resource.WriteToManifest);
    }
}
