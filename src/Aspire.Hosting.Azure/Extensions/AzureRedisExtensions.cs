// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Azure.Provisioning;
using Azure.Provisioning.KeyVaults;
using Azure.Provisioning.Redis;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding the Azure Redis resources to the application model.
/// </summary>
public static class AzureRedisExtensions
{
    /// <summary>
    /// Configures the resource to be published as Azure Cache for Redis when deployed via Azure Developer CLI.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{RedisResource}"/> builder.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{RedisResource}"/> builder.</returns>
    public static IResourceBuilder<RedisResource> PublishAsAzureRedis(this IResourceBuilder<RedisResource> builder)
    {
#pragma warning disable CA2252 // This API requires opting into preview features
        return builder.PublishAsAzureRedis((_, _, _) => { });
#pragma warning restore CA2252 // This API requires opting into preview features
    }

    /// <summary>
    /// Configures the resource to be published as Azure Cache for Redis when deployed via Azure Developer CLI.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{RedisResource}"/> builder.</param>
    /// <param name="configureResource">Callback to configure Azure resource.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{RedisResource}"/> builder.</returns>
    [RequiresPreviewFeatures]
    public static IResourceBuilder<RedisResource> PublishAsAzureRedis(this IResourceBuilder<RedisResource> builder, Action<IResourceBuilder<AzureRedisResource>, ResourceModuleConstruct, RedisCache>? configureResource)
    {
        return builder.PublishAsAzureRedisInternal(configureResource);
    }

    /// <summary>
    /// Configures the resource to be published as Azure Cache for Redis when deployed via Azure Developer CLI.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{RedisResource}"/> builder.</param>
    /// <param name="configureResource">Callback to configure Azure resource.</param>
    /// <param name="useProvisioner"></param>
    /// <returns>A reference to the <see cref="IResourceBuilder{RedisResource}"/> builder.</returns>
    internal static IResourceBuilder<RedisResource> PublishAsAzureRedisInternal(this IResourceBuilder<RedisResource> builder, Action<IResourceBuilder<AzureRedisResource>, ResourceModuleConstruct, RedisCache>? configureResource = null, bool useProvisioner = false)
    {
        var configureConstruct = (ResourceModuleConstruct construct) =>
        {
            var redisCache = new RedisCache(construct, name: builder.Resource.Name);

            redisCache.Properties.Tags["aspire-resource-name"] = construct.Resource.Name;

            var vaultNameParameter = new Parameter("keyVaultName");
            construct.AddParameter(vaultNameParameter);

            var  keyVault = KeyVault.FromExisting(construct, "keyVaultName");

            var keyVaultSecret = new KeyVaultSecret(construct, keyVault, "connectionString");
            keyVaultSecret.AssignProperty(
                x => x.Properties.Value,
                $$"""'${{{redisCache.Name}}.properties.hostName},ssl=true,password=${{{redisCache.Name}}.listKeys({{redisCache.Name}}.apiVersion).primaryKey}'"""
                );

            if (configureResource != null)
            {
                var resource = (AzureRedisResource)construct.Resource;
                var resourceBuilder = builder.ApplicationBuilder.CreateResourceBuilder(resource);
                configureResource(resourceBuilder, construct, redisCache);
            }
        };

        var resource = new AzureRedisResource(builder.Resource, configureConstruct);
        var resourceBuilder = builder.ApplicationBuilder.CreateResourceBuilder(resource)
                                     .WithParameter(AzureBicepResource.KnownParameters.PrincipalId)
                                     .WithParameter(AzureBicepResource.KnownParameters.KeyVaultName)
                                     .WithManifestPublishingCallback(resource.WriteToManifest);

        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode)
        {
            resourceBuilder.WithParameter(AzureBicepResource.KnownParameters.PrincipalType);
        }

        if (useProvisioner)
        {
            // Used to hold a reference to the azure surrogate for use with the provisioner.
            builder.WithAnnotation(new AzureBicepResourceAnnotation(resource));
            builder.WithConnectionStringRedirection(resource);

            // Remove the container annotation so that DCP doesn't do anything with it.
            if (builder.Resource.Annotations.OfType<ContainerImageAnnotation>().SingleOrDefault() is { } containerAnnotation)
            {
                builder.Resource.Annotations.Remove(containerAnnotation);
            }
        }

        return builder;
    }

    /// <summary>
    /// Configures resource to use Azure for local development and when doing a deployment via the Azure Developer CLI.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{RedisResource}"/> builder.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{RedisResource}"/> builder.</returns>
    public static IResourceBuilder<RedisResource> AsAzureRedis(this IResourceBuilder<RedisResource> builder)
    {
#pragma warning disable CA2252 // This API requires opting into preview features
        return builder.AsAzureRedis((_, _, _) => { });
#pragma warning restore CA2252 // This API requires opting into preview features
    }

    /// <summary>
    /// Configures resource to use Azure for local development and when doing a deployment via the Azure Developer CLI.
    /// </summary>
    /// <param name="builder">The <see cref="IResourceBuilder{RedisResource}"/> builder.</param>
    /// <param name="configureResource">Callback to configure Azure resource.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{RedisResource}"/> builder.</returns>
    [RequiresPreviewFeatures]
    public static IResourceBuilder<RedisResource> AsAzureRedis(this IResourceBuilder<RedisResource> builder, Action<IResourceBuilder<AzureRedisResource>, ResourceModuleConstruct, RedisCache>? configureResource)
    {
        return builder.PublishAsAzureRedisInternal(configureResource, useProvisioner: true);
    }
}
