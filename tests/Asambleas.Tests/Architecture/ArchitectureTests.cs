using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Asambleas.Tests.Architecture;

/// <summary>
/// Tests de arquitectura que validan las dependencias entre capas.
/// Usa NetArchTest para verificar que el dominio no referencia infraestructura.
/// </summary>
public class ArchitectureTests
{
    private static readonly Assembly MainAssembly = typeof(Program).Assembly;

    [Fact]
    public void Entities_ShouldNotDependOn_Infrastructure()
    {
        // Las entidades en Features/*/Entities no deben depender de Infrastructure
        var result = Types.InAssembly(MainAssembly)
            .That()
            .ResideInNamespaceContaining(".Entities")
            .ShouldNot()
            .HaveDependencyOn("Asambleas.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Entidades no deben depender de Infrastructure. Violaciones: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Entities_ShouldNotDependOn_Controllers()
    {
        // Las entidades no deben depender de Controllers ni MVC
        var result = Types.InAssembly(MainAssembly)
            .That()
            .ResideInNamespaceContaining(".Entities")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Mvc")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Entidades no deben depender de MVC. Violaciones: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommonLayer_ShouldNotDependOn_Features()
    {
        // Common no debe depender de Features (evitar dependencias circulares)
        var result = Types.InAssembly(MainAssembly)
            .That()
            .ResideInNamespace("Asambleas.Common")
            .ShouldNot()
            .HaveDependencyOn("Asambleas.Features")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Common no debe depender de Features. Violaciones: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommonLayer_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(MainAssembly)
            .That()
            .ResideInNamespace("Asambleas.Common")
            .ShouldNot()
            .HaveDependencyOn("Asambleas.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Common no debe depender de Infrastructure. Violaciones: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Validators_ShouldResideIn_ValidatorsNamespace()
    {
        var result = Types.InAssembly(MainAssembly)
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .ResideInNamespaceContaining("Validators")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Todos los validators deben estar en un namespace Validators. Violaciones: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Services_ShouldHaveInterfaces()
    {
        // Los servicios registrados deben implementar al menos una interfaz propia
        var serviceTypes = Types.InAssembly(MainAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .DoNotResideInNamespaceContaining("Jobs")
            .GetTypes();

        var violators = serviceTypes
            .Where(t => !typeof(Microsoft.Extensions.Hosting.BackgroundService).IsAssignableFrom(t))
            .Where(t => t.Name != "JwtTokenService") // Servicio interno de infraestructura
            .Where(t => !t.GetInterfaces().Any(i =>
                i.Namespace != null && i.Namespace.StartsWith("Asambleas")))
            .Select(t => t.FullName)
            .ToList();

        violators.Should().BeEmpty(
            $"Los siguientes servicios no implementan una interfaz propia: {string.Join(", ", violators)}");
    }
}
