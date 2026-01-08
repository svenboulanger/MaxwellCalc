using MaxwellCalc.Core.Attributes;
using MaxwellCalc.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// A factory that supports making <see cref="JsonConverter{T}"/> for a given workspace.
/// Makes heavy use of reflection. All assemblies are explored and domains and helper methods are automatically found.
/// </summary>
public class WorkspaceJsonConverterFactory : JsonConverterFactory
{
    private readonly Dictionary<Type, List<Type>> _helpers = [];
    private readonly Dictionary<Type, ConstructorInfo> _domains = [];
    private readonly Dictionary<Type, Func<JsonConverter>> _converters = [];

    /// <summary>
    /// Creates a new <see cref="WorkspaceJsonConverterFactory"/>.
    /// </summary>
    public WorkspaceJsonConverterFactory()
    {
        // Try to go over the assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                // Allow helper classes
                bool isHelper = false;
                foreach (var attribute in type.GetCustomAttributes<CalculatorHelperAttribute>())
                {
                    if (!_helpers.TryGetValue(attribute.Type, out var list))
                    {
                        list = [];
                        _helpers.Add(attribute.Type, list);
                    }
                    list.Add(type);
                    isHelper = true;
                }
                if (isHelper)
                    continue;
                if (type.IsGenericTypeDefinition)
                    continue;

                // Find domains
                var itfs = type.GetInterfaces();
                foreach (var itf in itfs)
                {
                    if (!itf.IsGenericType)
                        continue;
                    if (itf.GetGenericTypeDefinition() != typeof(IDomain<>))
                        continue;
                    var genericType = itf.GetGenericArguments()[0];
                    var constructorInfo = type.GetConstructor(Type.EmptyTypes);
                    if (constructorInfo is not null)
                        _domains[genericType] = constructorInfo;
                }
            }
        }
    }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (TryGetWorkspaceDomain(typeToConvert, out _, out var scalarType) &&
            scalarType is not null &&
            _domains.ContainsKey(scalarType))
            return true;
        return false;
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!_converters.TryGetValue(typeToConvert, out var converterFactory))
        {
            if (!TryGetWorkspaceDomain(typeToConvert, out var workspaceType, out var scalarType) ||
                workspaceType is null ||
                scalarType is null)
                throw new ArgumentException(nameof(typeToConvert));
            if (!_domains.TryGetValue(scalarType, out var domainConstructor))
                throw new ArgumentException(nameof(typeToConvert));

            // Use Expressions to build our function that creates a JsonConverter
            var factoryType = typeof(Func<>).MakeGenericType(workspaceType);
            var workspaceJsonConverterType = typeof(WorkspaceJsonConverter<>).MakeGenericType(scalarType) ?? throw new InvalidOperationException();
            var workspaceJsonConverterConstructor = workspaceJsonConverterType.GetConstructor([factoryType]) ?? throw new InvalidOperationException();

            var domainType = typeof(IDomain<>).MakeGenericType(scalarType) ?? throw new InvalidOperationException();
            var directWorkspaceType = typeof(Workspace<>).MakeGenericType(scalarType) ?? throw new InvalidOperationException();
            var directWorkspaceConstructor = directWorkspaceType.GetConstructor([domainType]) ?? throw new InvalidOperationException();

            // Make the body of the factory for the workspace
            List<Expression> statements = [];
            var workspace = Expression.Variable(directWorkspaceType, "workspace");
            var returnTarget = Expression.Label();
            statements.Add(Expression.Assign(
                    workspace,
                    Expression.New(
                        directWorkspaceConstructor,
                        Expression.New(domainConstructor))));

            // Apply any method helpers for it
            if (_helpers.TryGetValue(workspaceType, out var list))
            {
                // Use this type to register constants
                foreach (var item in list)
                {
                    var method = typeof(WorkspaceHelper).GetMethod("RegisterBuiltInMethods", BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException();
                    statements.Add(Expression.Call(method, workspace, Expression.Constant(item)));
                }
            }

            statements.Add(workspace);
            var funcBody = Expression.Block([workspace], statements);
            var funcWorkspace = Expression.Lambda(typeof(Func<>).MakeGenericType(workspaceType), funcBody).Compile();
            var body = Expression.New(workspaceJsonConverterConstructor, Expression.Constant(funcWorkspace));
            converterFactory = Expression.Lambda<Func<JsonConverter>>(body).Compile();
            _converters[typeToConvert] = converterFactory;
        }
        return converterFactory();
    }

    private bool TryGetWorkspaceDomain(Type type, out Type? workspaceType, out Type? scalarType)
    {
        if (type.IsInterface &&
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IWorkspace<>))
        {
            workspaceType = type;
            scalarType = type.GetGenericArguments()[0];
            return true;
        }
        else
        {
            var itfs = type.GetInterfaces();
            foreach (var itf in itfs)
            {
                if (!itf.IsGenericType)
                    continue;
                if (itf.GetGenericTypeDefinition() != typeof(IWorkspace<>))
                    continue;
                workspaceType = itf;
                scalarType = itf.GetGenericArguments()[0];
            }
        }
        workspaceType = null;
        scalarType = null;
        return false;
    }
}
