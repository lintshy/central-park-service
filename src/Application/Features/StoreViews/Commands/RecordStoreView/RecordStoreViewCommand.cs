using CentralPark.Application.Common.Markers;

namespace CentralPark.Application.Features.StoreViews.Commands.RecordStoreView;

public sealed record RecordStoreViewCommand(Guid StoreId, Guid UserId, string? Source) : ICommand;
