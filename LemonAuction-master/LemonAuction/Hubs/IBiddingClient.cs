using System.Threading.Tasks;
using LemonAuction.DTO;
namespace LemonAuction.Hubs {
    public interface IBiddingClient
    {
        Task ReceiveJoinBidding(UserInfoDto user);
        Task ReceiveLeaveBidding(UserInfoDto user);
        Task ReceiveBid(BidInfoDTO bid);
        Task RecieveProduct(ProductDetailDTO product);
    }
}