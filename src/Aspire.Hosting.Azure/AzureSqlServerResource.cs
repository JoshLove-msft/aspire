// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Azure;

/// <summary>
/// Represents an Azure Sql Server resource.
/// </summary>
/// <param name="innerResource">The <see cref="SqlServerServerResource"/> that this resource wraps.</param>
public class AzureSqlServerResource(SqlServerServerResource innerResource) :
    AzureBicepResource(innerResource.Name, templateResourceName: "Aspire.Hosting.Azure.Bicep.sql.bicep"),
    IResourceWithConnectionString
{
    /// <summary>
    /// Gets the fully qualified domain name (FQDN) output reference from the bicep template for the Azure SQL Server resource.
    /// </summary>
    public BicepOutputReference FullyQualifiedDomainName => new("sqlServerFqdn", this);

    private ReferenceExpression ConnectionString =>
        ReferenceExpression.Create(
            $"Server=tcp:{FullyQualifiedDomainName},1433;Encrypt=True;Authentication=\"Active Directory Default\"");

    /// <summary>
    /// Gets the connection template for the manifest for the Azure SQL Server resource.
    /// </summary>
    public string ConnectionStringExpression =>
        ConnectionString.ValueExpression;

    /// <summary>
    /// Gets the connection string for the Azure SQL Server resource.
    /// </summary>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The connection string for the Azure SQL Server resource.</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionString.GetValueAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override string Name => innerResource.Name;

    /// <inheritdoc />
    public override ResourceAnnotationCollection Annotations => innerResource.Annotations;
}

/// <summary>
/// Represents an Azure Sql Server resource.
/// </summary>
/// <param name="innerResource">The <see cref="SqlServerServerResource"/> that this resource wraps.</param>
public class AzureSqlServerConstructResource(SqlServerServerResource innerResource) : AzureConstructResource(innerResource.Name), IResourceWithConnectionString
{
    /// <summary>
    /// Gets the fully qualified domain name (FQDN) output reference from the bicep template for the Azure SQL Server resource.
    /// </summary>
    public BicepOutputReference FullyQualifiedDomainName => new("sqlServerFqdn", this);

    private ReferenceExpression ConnectionString =>
        ReferenceExpression.Create(
            $"Server=tcp:{FullyQualifiedDomainName},1433;Encrypt=True;Authentication=\"Active Directory Default\"");

    /// <summary>
    /// Gets the connection template for the manifest for the Azure SQL Server resource.
    /// </summary>
    public string ConnectionStringExpression =>
        ConnectionString.ValueExpression;

    /// <summary>
    /// Gets the connection string for the Azure SQL Server resource.
    /// </summary>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The connection string for the Azure SQL Server resource.</returns>
    public ValueTask<string?> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        return ConnectionString.GetValueAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override string Name => innerResource.Name;

    /// <inheritdoc />
    public override ResourceAnnotationCollection Annotations => innerResource.Annotations;
}
