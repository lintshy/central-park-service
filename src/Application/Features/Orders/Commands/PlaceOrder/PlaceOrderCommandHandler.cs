using CentralPark.Application.Common.Interfaces;
using CentralPark.Application.Features.Stores.Errors;
using CentralPark.Domain.Entities;
using CentralPark.Domain.Enums;
using CentralPark.Shared;
using MediatR;

namespace CentralPark.Application.Features.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
{
    public async Task<Result<PlaceOrderResponse>> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var customerProfiles = await uow.Repository<CustomerProfile>()
            .FindAsync(cp => cp.UserId == cmd.RequesterId, ct);
        var customerProfile = customerProfiles.FirstOrDefault();
        if (customerProfile is null)
            return Result<PlaceOrderResponse>.Failure(
                new Error("User.NotFound", "Customer profile not found for this user."));

        var store = await uow.Repository<Store>().GetByIdAsync(cmd.StoreId, ct);
        if (store is null)
            return Result<PlaceOrderResponse>.Failure(StoreErrors.NotFound);

        if (!store.IsAcceptingOrders)
            return Result<PlaceOrderResponse>.Failure(StoreErrors.NotAcceptingOrders);

        var order = Order.Create(customerProfile.Id, store.Id, cmd.Notes);
        foreach (var item in cmd.Items)
            order.AddItem(item.ProductName, item.Quantity, item.UnitPrice);

        await uow.Repository<Order>().AddAsync(order, ct);

        return Result<PlaceOrderResponse>.Success(
            new PlaceOrderResponse(order.Id, order.Status.ToString(), order.TotalAmount, order.CreatedAt));
    }
}
