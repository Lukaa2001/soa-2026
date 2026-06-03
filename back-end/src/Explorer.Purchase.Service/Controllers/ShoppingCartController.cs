using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Public.ShoppingCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers.Tourist.ShoppingCart
{
    // Functionality 16: shopping cart (items hold tour name, price and tour id;
    // the cart recomputes the total on add/remove).
    [Authorize(Policy = "touristPolicy")]
    [Route("api/shoppingCart/")]
    public class ShoppingCartController : BaseApiController
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        [HttpGet("{userId:int}")]
        public ActionResult<ShoppingCartDto> Get(int userId)
        {
            EnsureCart(userId);
            var result = _shoppingCartService.Get(userId);
            return CreateResponse(result);
        }

        [HttpPut("addTour/{userId:int}")]
        public ActionResult<OrderItemDto> AddTour([FromBody] OrderItemDto orderItem, int userId)
        {
            EnsureCart(userId);
            var result = _shoppingCartService.AddTour(orderItem, userId);
            return CreateResponse(result);
        }

        // Carts are created lazily on first use (the monolith created them at registration).
        private void EnsureCart(long userId)
        {
            _shoppingCartService.Create(userId); // returns Conflict if it already exists — ignored
        }

        [HttpDelete("removeTour/{cartId:int}")]
        public ActionResult<ShoppingCartDto> RemoveTour([FromQuery] int tourId, int cartId)
        {
            var result = _shoppingCartService.RemoveTour(tourId, cartId);
            return CreateResponse(result);
        }
    }
}
