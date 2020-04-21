using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LemonAuction.Models;
using LemonAuction.Services;
using Microsoft.AspNetCore.SignalR;
using LemonAuction.Hubs;
using Microsoft.AspNetCore.Authorization;
using LemonAuction.Identity;
using Microsoft.AspNetCore.Identity;
using LemonAuction.DTO;
using System.IO;
using LemonAuction.Services.Exceptions;

namespace LemonAuction.Controllers
{
    [Middleware.ExceptionMiddelware]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class BiddingController : ControllerBase
    {
        private readonly IBiddingService _biddingService;
        private readonly IHubContext<BiddingHub, IBiddingClient> _biddingHubContext;

        private readonly UserManager<LemonUser> _userManager;

        public BiddingController(IBiddingService biddingService, IHubContext<BiddingHub, IBiddingClient> biddingHubContext, UserManager<LemonUser> userManager) {
            _biddingService = biddingService;
            _biddingHubContext = biddingHubContext;
            _userManager = userManager;
            // if (_context.Bids.Count() == 0)
            // {
            //     // Create a new TodoItem if collection is empty,
            //     _context.Bids.Add(new Bid { BidAmount = 100 });
            //     _context.SaveChanges();
            // }
        }

        [HttpGet("products")]
        public async Task<IEnumerable<ProductDetailDTO>> ProductsAsync([FromQuery]  bool activeOnly = false)
        {
            return (await _biddingService.ProductsAsync(activeOnly)).Select(p => new ProductDetailDTO(p));
        }

        [HttpGet("products/{productId}")]
        public ProductDetailDTO ProductWithId(int productId, [FromQuery] bool activeOnly = false)
        {
            if (ModelState.IsValid) {
                return  new ProductDetailDTO(_biddingService.ProductWithId(productId, activeOnly));
            }
            throw new LemonInvalidException("Invalid productId");
        }

        [HttpGet("products/search/{searchQuery}")]
        public async Task<IEnumerable<ProductDetailDTO>> GetActiveProductsForSearchAsync(string searchQuery)
        {
            if (ModelState.IsValid) {
                return (await _biddingService.GetActiveProductsForSearchAsync(searchQuery)).Select(p => new ProductDetailDTO(p));
            }
            throw new LemonInvalidException("Invalid search query");
        }

        [HttpGet("products/{productId}/bids")]
        public IEnumerable<BidInfoDTO> GetBidsForProductAsync(int productId)
        {
            if (ModelState.IsValid) {
                return _biddingService.GetBidsForProduct(productId, false).Select(b => new BidInfoDTO(b));
            }
            throw new LemonInvalidException("Invalid productId");
        }

        [HttpGet("bids")]
        public IEnumerable<BidInfoDTO> GetBidsForUserAsync(string userId)
        {
            if (ModelState.IsValid) {
                return  _biddingService.GetBidsForUser(userId).Select(b => new BidInfoDTO(b));
            }
            throw new LemonInvalidException("Invalid userId");
        }

        [HttpGet("products/{productId}/bids/max")]
        public Bid GetMaxBidForProductAsync(int productId)
        {
            if (ModelState.IsValid) {
                return _biddingService.GetMaxBidForProduct(productId);
            }
            throw new LemonInvalidException("Invalid productId");
        }


        [HttpGet("products/{productId}/image")]
        public IActionResult ProductImage(int productId) {
            if (ModelState.IsValid) {
                var product = _biddingService.ProductWithId(productId, false);

                return File(product.Image, "image/png");
            }
            return BadRequest("Invalid productId");

        }

        [HttpPost("bids/add")]
        [Authorize]
        public async Task MakeBid([FromBody] MakeBidDTO bidInfo)
        {
            if (ModelState.IsValid) {
                var newBid = await _biddingService.MakeBid(_userManager.GetUserId(User), bidInfo.ProductId, bidInfo.BidAmount);
                await _biddingHubContext.Clients.Group(bidInfo.ProductId.ToString()).ReceiveBid(new BidInfoDTO(newBid));
            } else {
                throw new LemonInvalidException("Invalid bid");
            }
            // _biddingHubContext.Clients.Group(productId.)
           
        }

        [HttpPost("products/add")]
        [Authorize]
        public async Task<ProductInfoDTO> PutUpForAuction([FromForm] AddProductDTO productData) 
        {
            if (ModelState.IsValid) {
                
                byte[] productImage;
                using (var memoryStream = new MemoryStream()) {
                    await productData.ProductImage.CopyToAsync(memoryStream);
                    productImage = memoryStream.ToArray();
                }

                var product = await _biddingService.PutUpForAuction(
                    _userManager.GetUserId(User),
                    productData.ProductInfo.Name,
                    productData.ProductInfo.Category,
                    productData.ProductInfo.StartingPrice,
                    DateTime.UtcNow.AddMinutes(productData.ProductInfo.Duration),
                    productData.ProductInfo.Description,
                    productImage
                );

                return new ProductInfoDTO(product);

            } else {
                throw new LemonInvalidException("Bad product data");
            }
        }

    }
}
