using MediatR;
using CentralPark.Shared;

namespace CentralPark.Application.Common.Markers;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
