using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LemonAuction.Models;
using LemonAuction.Identity;
using LemonAuction.DTO;
namespace LemonAuction.Services {
    public interface IBiddingService {

        #region GetData
        Task<IEnumerable<Product>> ProductsAsync(bool activeOnly);

        Product ProductWithId(int productId, bool activeOnly);

        Task<IEnumerable<Product>> GetActiveProductsForSearchAsync(string searchQuery);
        ICollection<Bid> GetBidsForProduct(int productId, bool activeOnly);

        ICollection<Bid> GetBidsForUser(string userId);

        Bid GetMaxBidForProduct(int productId);


        #endregion

        #region InsertUpdate

        Task<Bid> MakeBid(string userID, int productId, decimal amount);

        Task<Product> PutUpForAuction(string userId, string name, CategoryEnum category, decimal startingPrice, DateTime endingTime, string description, byte[] productImage);

        #endregion


        
    }
}