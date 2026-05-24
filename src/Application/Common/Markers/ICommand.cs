using MediatR;
using CentralPark.Shared;

namespace CentralPark.Application.Common.Markers;

public interface ICommand : IRequest<Result>;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
