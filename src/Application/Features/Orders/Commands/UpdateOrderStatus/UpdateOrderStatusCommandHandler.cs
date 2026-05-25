using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Orders.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Domain.Enums;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateOrderStatusCommand, Result<UpdateOrderStatusResponse>>
{
    public async Task<Result<UpdateOrderStatusResponse>> Handle(UpdateOrderStatusCommand cmd, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(cmd.NewStatus, ignoreCase: true, out var newStatus))
            return Result<UpdateOrderStatusResponse>.Failure(OrderErrors.InvalidTransition);

        var order = await uow.Repository<Order>().GetByIdAsync(cmd.OrderId, ct);
        if (order is null)
            return Result<UpdateOrderStatusResponse>.Failure(OrderErrors.NotFound);

        var customerProfiles = await uow.Repository<CustomerProfile>()
            .FindAsync(cp => cp.Id == order.CustomerProfileId, ct);
        var customerProfile = customerProfiles.FirstOrDefault();
        var isCustomer = customerProfile?.UserId == cmd.RequesterId;

        var stores = await uow.Repository<Store>().FindAsync(s => s.Id == order.StoreId, ct);
        var store = stores.FirstOrDefault();
        bool isStoreOwner = false;
        if (store is not null)
        {
            var ownerProfiles = await uow.Repository<BusinessOwnerProfile>()
                .FindAsync(b => b.Id == store.BusinessOwnerProfileId, ct);
            isStoreOwner = ownerProfiles.FirstOrDefault()?.UserId == cmd.RequesterId;
        }

        if (!isCustomer && !isStoreOwner)
            return Result<UpdateOrderStatusResponse>.Failure(OrderErrors.Forbidden);

        if (!IsValidTransition(order.Status, newStatus, isStoreOwner))
            return Result<UpdateOrderStatusResponse>.Failure(OrderErrors.InvalidTransition);

        order.UpdateStatus(newStatus);
        uow.Repository<Order>().Update(order);

        return Result<UpdateOrderStatusResponse>.Success(
            new UpdateOrderStatusResponse(order.Id, order.Status.ToString(), order.UpdatedAt ?? DateTime.UtcNow));
    }

    private static bool IsValidTransition(OrderStatus current, OrderStatus next, bool isStoreOwner) =>
        (current, next) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => isStoreOwner,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => isStoreOwner,
            (OrderStatus.Preparing, OrderStatus.Ready) => isStoreOwner,
            (OrderStatus.Ready, OrderStatus.Completed) => isStoreOwner,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => isStoreOwner,
            _ => false
        };
}
