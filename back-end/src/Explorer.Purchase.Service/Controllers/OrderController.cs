using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Public.ShoppingCart;
using Explorer.Payments.Core.Domain;
using Explorer.Payments.Core.Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Tourist.ShoppingCart
{
    // Functionality 16: checkout — each cart item becomes a purchase record
    // (TourPurchaseToken) and the cart is emptied. The monolith's wallet/coins
    // and e-mail steps are out of scope and intentionally omitted.
    [Authorize(Policy = "touristPolicy")]
    [Route("api/order/")]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderService orderService, IShoppingCartService shoppingCartService, IOrderRepository orderRepository)
        {
            _orderService = orderService;
            _shoppingCartService = shoppingCartService;
            _orderRepository = orderRepository;
        }

        [HttpPost("{userId:int}")]
        public ActionResult CreateOrder([FromBody] List<long> tourIds, int userId)
        {
            var cart = _shoppingCartService.Get(userId);
            if (cart.IsFailed) return CreateErrorResponse(cart.Errors);

            foreach (var item in cart.Value.Items)
            {
                _orderRepository.CreateOrder(new Order(userId, item.TourId, item.Price));
            }

            _shoppingCartService.ClearCart(userId);
            return Ok();
        }

        [HttpGet("orders/{userId:int}")]
        public ActionResult<PagedResult<OrderDto>> GetTouristOrders(int userId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var result = _orderService.GetByUser(page, pageSize, userId);
            return CreateResponse(result);
        }

        // Internal service-to-service check (Tours -> Purchase): has the user bought this tour?
        [AllowAnonymous]
        [HttpGet("internal/has-purchased/{userId:int}/{tourId:int}")]
        public ActionResult<bool> HasPurchased(int userId, int tourId)
        {
            var orders = _orderService.GetByUser(1, 10000, userId);
            var purchased = orders.IsSuccess && orders.Value.Results.Any(o => o.TourId == tourId);
            return Ok(purchased);
        }
    }
}
