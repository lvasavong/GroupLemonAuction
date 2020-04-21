using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using LemonAuction.Services;
using LemonAuction.Identity;
using Microsoft.AspNetCore.Identity;
using LemonAuction.DTO;
namespace LemonAuction.Hubs
{
    // [Authorize]
    public class BiddingHub : Hub<IBiddingClient>
    {

        private readonly IBiddingService _biddingService;
        private readonly UserManager<LemonUser> _userManager;
        public BiddingHub(IBiddingService biddingService, UserManager<LemonUser> userManager) {
            _biddingService = biddingService;
            _userManager = userManager;
        }

        public async Task AddToBiddingGroup(int productId)
        {

            var product =  _biddingService.ProductWithId(productId, true);
            var productIdString = productId.ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, productIdString);
            await Clients.Group(productIdString).ReceiveJoinBidding(new UserInfoDto(await _userManager.GetUserAsync(Context.User)));
            await Clients.Caller.RecieveProduct(new ProductDetailDTO(product, true));

        }

        public async Task RemoveFromBiddingGroup(int productId)
        {
            var product =  _biddingService.ProductWithId(productId, true);
            var productIdString = productId.ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, productIdString);
            await Clients.Group(productIdString).ReceiveLeaveBidding(new UserInfoDto(await _userManager.GetUserAsync(Context.User)));
        }

        // public override async Task OnDisconnectedAsync(Exception exception) {
        //     await 
        //     await base.OnDisconnectedAsync(exception);
        // }

    }
}