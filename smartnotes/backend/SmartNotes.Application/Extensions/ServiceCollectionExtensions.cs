using Microsoft.Extensions.DependencyInjection;
using SmartNotes.Application.UseCases;

namespace SmartNotes.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all use cases with scoped lifetime for proper DI management
        services.AddScoped<CreateNoteUseCase>();
        services.AddScoped<GetAllNotesUseCase>();
        services.AddScoped<GetNoteUseCase>();
        services.AddScoped<UpdateNoteUseCase>();
        services.AddScoped<DeleteNoteUseCase>();
        services.AddScoped<SearchNotesUseCase>();

        return services;
    }
}