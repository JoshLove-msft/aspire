// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Azure.Provisioning.ServiceBus;

namespace Aspire.Hosting.Azure;

/// <summary>
/// Represents an Azure Service Bus resource.
/// </summary>
/// <param name="name">The name of the resource.</param>
public class AzureServiceBusResource(string name) :
    AzureBicepResource(name, templateResourceName: "Aspire.Hosting.Azure.Bicep.servicebus.bicep"),
    IResourceWithConnectionString
{
    internal List<string> Queues { get; } = [];
    internal Dictionary<string, string[]> Topics { get; } = [];

    /// <summary>
    /// Gets the "serviceBusEndpoint" output reference from the bicep template for the Azure Service Bus endpoint.
    /// </summary>
    public BicepOutputReference ServiceBusEndpoint => new("serviceBusEndpoint", this);

    /// <summary>
    /// Gets the connection string template for the manifest for the Azure Service Bus endpoint.
    /// </summary>
    public string ConnectionStringExpression => ServiceBusEndpoint.ValueExpression;

    /// <summary>
    /// Gets the connection string for the Azure Service Bus endpoint.
    /// </summary>
    /// <returns>The connection string for the Azure Service Bus endpoint.</returns>
    public string? GetConnectionString() => ServiceBusEndpoint.Value;

    /// <summary>
    /// Gets the connection string for the Azure Service Bus endpoint.
    /// </summary>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The connection string for the Azure Service Bus endpoint.</returns>
    public async ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (ProvisioningTaskCompletionSource is not null)
        {
            await ProvisioningTaskCompletionSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        return GetConnectionString();
    }
}

/// <summary>
/// A resource that represents an Azure Key Vault.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="configureConstruct"></param>
public class AzureServiceBusConstructResource(string name, Action<ResourceModuleConstruct> configureConstruct)
    : AzureConstructResource(name, configureConstruct), IResourceWithConnectionString
{
    internal List<(string Name, Action<ResourceModuleConstruct, ServiceBusQueue>? Configure)> Queues { get; } = [];
    internal List<(string Name, Action<ResourceModuleConstruct, ServiceBusTopic>? Configure)> Topics { get; } = [];
    internal List<(string TopicName, string Name, Action<ResourceModuleConstruct, ServiceBusSubscription>? Configure)> Subscriptions { get; } = [];

    /// <summary>
    /// Gets the "serviceBusEndpoint" output reference from the bicep template for the Azure Storage resource.
    /// </summary>
    public BicepOutputReference ServiceBusEndpoint => new("serviceBusEndpoint", this);

    /// <summary>
    /// Gets the connection string template for the manifest for the Azure Service Bus endpoint.
    /// </summary>
    public string ConnectionStringExpression => ServiceBusEndpoint.ValueExpression;

    /// <summary>
    /// Gets the connection string for the Azure Service Bus endpoint.
    /// </summary>
    /// <returns>The connection string for the Azure Service Bus endpoint.</returns>
    public string? GetConnectionString() => ServiceBusEndpoint.Value;

    /// <summary>
    /// Gets the connection string for the Azure Service Bus endpoint.
    /// </summary>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The connection string for the Azure Service Bus endpoint.</returns>
    public async ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (ProvisioningTaskCompletionSource is not null)
        {
            await ProvisioningTaskCompletionSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        return GetConnectionString();
    }
}
