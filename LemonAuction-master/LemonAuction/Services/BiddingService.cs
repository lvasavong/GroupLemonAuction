using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using LemonAuction.Identity;
using LemonAuction.Models;
using System.Linq;
using LemonAuction.DTO;

namespace LemonAuction.Services {


    namespace Exceptions {
        public class LemonNotFoundException : Exception {
            public LemonNotFoundException() { }

            public LemonNotFoundException(string message) : base(message) { }

            public LemonNotFoundException(string message, Exception inner) : base(message, inner) { }
            
        }

        public class LemonInvalidException: Exception {

            public LemonInvalidException() { }

            public LemonInvalidException(string message) : base(message) { }

            public LemonInvalidException(string message, Exception inner) : base(message, inner) { }

        }


    }

    public class BiddingService : IBiddingService
    {
        private readonly LemonContext _lemonContext;

        public BiddingService(LemonContext lemonContext) {
            _lemonContext = lemonContext;
        }

        // check if query is null, if null then throw argumentexception!!
        // if getter methods return one thing, use firstordefault


        // gets products up on auction
        public async Task<IEnumerable<Product>> ProductsAsync(bool activeOnly)
        {
            //throw new NotImplementedException();
            //using (var db = new Models.LemonContext())
            //{

                List<Product> products = await (activeOnly ?  
                    (from p in _lemonContext.Products
                        where p.Active
                        // orderby p.Name
                        select p
                    ) : _lemonContext.Products)
                        .Include(p => p.Bids)
                        .Include(p => p.Seller)
                        .ToListAsync();
                
                    return products;
                
            //}
        }

        // gets productid of products up on auction whether user wants active or non-active product
        public Product ProductWithId(int productId, bool activeOnly)
        {
            //throw new NotImplementedException();
            var product =  _lemonContext.Products
                .Include(p => p.Seller)
                .Include(p => p.Bids)
                    .ThenInclude(b => b.Bidder)
                .FirstOrDefault(p => (!activeOnly || p.Active) && p.ProductId == productId);
            if(product == null)
            {
                throw new Exceptions.LemonNotFoundException($"No product with Id: ${productId} with activeOnly set to: ${activeOnly}");
            }
            // _lemonContext.Entry(product)
            //     .Reference(p => p.Seller)
            //     .Query()
            //     .AsNoTracking()
            //     .Load();
            return product;
        }

        // has to match category || description || name
        // on standby
        public async Task<IEnumerable<Product>> GetActiveProductsForSearchAsync(string searchQuery)
        {
            //throw new NotImplementedException();
            //using (var db = new Models.LemonContext())
            //{
                var products =  await (from p in _lemonContext.Products
                        where(p.Active == true && p.Name.Contains(searchQuery))
                        orderby p.Name
                        select p
                    ).Include(p => p.Seller)
                        .Include(p => p.Bids)
                        .ToListAsync();


                // var products = await
                //     (from p in _lemonContext.Products
                //         where(p.Active == true && p.Name.Contains(searchQuery))
                //         orderby p.Name
                //         select p
                //     ).ToListAsync();
                //     products.
              
                // if(products == null)
                // {
                //     throw new Exceptions.LemonNotFoundException($"No such active product with searchQuerry: ${searchQuery}");
                // }
                return products;
            //}
        }

        // whether user wants active or non-active product
        public ICollection<Bid> GetBidsForProduct(int productId, bool activeOnly)
        {
            var product = _lemonContext.Products
                .FirstOrDefault(p => (p.Active == activeOnly && p.ProductId == productId));
            if(product == null)
            {
                throw new Exceptions.LemonNotFoundException($"No such product with productId: ${productId} and active status: ${activeOnly}");
            }
            else 
            {
                _lemonContext.Entry(product)
                    .Collection(p => p.Bids)
                    .Load();
                return product.Bids;
            }
        }

        // get collection of bids from user
        public ICollection<Bid> GetBidsForUser(string userId)
        {
            var user =  _lemonContext.Users
                .FirstOrDefault(u => (u.Id == userId));
            if(user == null)
            {
                throw new Exceptions.LemonNotFoundException($"No such userId: ${userId}");
            }
            else
            {
                _lemonContext.Entry(user)
                    .Collection(u => u.Bids)
                    .Load();
                return user.Bids;
            }
        }

        // gets max bid for a product
        // throw exception if there are no bids (no bid objects that someone has made)
        public Bid GetMaxBidForProduct(int productId)
        {
            var product =  _lemonContext.Products
                .FirstOrDefault(p => (p.ProductId == productId));
            if(product == null)
            {
                throw new Exceptions.LemonNotFoundException($"No product with Id: ${productId}");
            }
            else 
            {
                _lemonContext.Entry(product)
                    .Collection(p => p.Bids)
                    .Load();
                if(product.Bids.Count == 0)
                {
                    throw new Exceptions.LemonInvalidException($"There are no bids made on product with productId: ${productId}");
                }
                else
                {
                    return product.Bids.FirstOrDefault(b => b.BidAmount == product.Bids.Max(x => x.BidAmount));
                }
            }
        }

        // make sure that userId exists, productId exists, check if amount is greater than or equal to the current bid 
        // has to be active
        // use as setter, not getter
        public async Task<Bid> MakeBid(string userId, int productId, decimal amount)
        {
            //throw new NotImplementedException();
            var user =  _lemonContext.Users
                .FirstOrDefault(u => (u.Id == userId));
            var product =  _lemonContext.Products
                .Include(p => p.Bids)
                .Include(p => p.Seller)
                .FirstOrDefault(p => (p.ProductId == productId) && (p.Active == true));
            //var isActive =  _lemonContext.Products
            //   .FirstOrDefault(p => (p.ProductId == productId) && (p.Active == true));
            if(user == null)
            {
                throw new Exceptions.LemonNotFoundException($"No such userId: ${userId}");
            }
            else if(product == null)
            {
                throw new Exceptions.LemonNotFoundException($"No such productId: ${productId} is active");
            }
            else if(amount == 0)
            {
                throw new Exceptions.LemonInvalidException($"Can't bid 0");
            } else if (product.Seller.Id == userId) {
                throw new Exceptions.LemonInvalidException("Cannot place bid on owned product");
            }
            else
            {
                var bids = product.Bids;
                if(bids.Count == 0)
                {
                    var minBid = product.MinimumBid;
                    if(amount > minBid)
                    {
                        var newBid = new Bid
                        {
                            BidAmount = amount,
                            Bidder = user,
                            OnProduct = product,
                            MadeAt = DateTime.UtcNow,
                        };
                        _lemonContext.Bids.Add(newBid);
                        await _lemonContext.SaveChangesAsync();
                        return newBid;
                    } else {
                        throw new Exceptions.LemonInvalidException($"Bid amount less than starting price!");
                    }
                }
                else
                {
                    var currMaxBid = product.Bids.FirstOrDefault(b => b.BidAmount == product.Bids.Max(x => x.BidAmount));
                    if(currMaxBid == null)
                    {
                        throw new Exceptions.LemonInvalidException($"Current Max Bid is null");
                    }
                    if(amount > currMaxBid.BidAmount)
                    {
                       var newBid = new Bid
                        {
                            BidAmount = amount,
                            Bidder = user,
                            OnProduct = product,
                            MadeAt = DateTime.UtcNow,
                        };
                        _lemonContext.Bids.Add(newBid);
                        await _lemonContext.SaveChangesAsync();
                        return newBid;
                    }
                    else 
                    {
                        throw new Exceptions.LemonInvalidException($"Cannot bid amount: ${amount} lower than the current price");
                    }
                }
            }
        }

        // use as setter, also check for validation
        public async Task<Product> PutUpForAuction(string userId, string name, CategoryEnum category, decimal startingPrice, DateTime endingTime, string description, byte[] productImage)
        {
            //throw new NotImplementedException();
            var user = _lemonContext.Users
                .FirstOrDefault(u => (u.Id == userId));
            var productName = _lemonContext.Products
                .FirstOrDefault(u => (u.Name == name));
            if(user == null)
            {
                throw new Exceptions.LemonNotFoundException($"No such userId: ${userId}");
            }
            // else if(category == null)
            // {
            //     throw new Exceptions.LemonInvalidException($"Category selected is null");
            // }
            // else if(productName != null)
            // {
            //     throw new ArgumentException($"Product name is already taken: ${name}");
            // }
            else if(name == null)
            {
                throw new Exceptions.LemonInvalidException($"Product name is null");
            }
            else if(startingPrice == 0) // 0 is null in this case
            {
                throw new Exceptions.LemonInvalidException($"Starting price must be non zero");
            }
            else if(endingTime == null)
            {
                throw new Exceptions.LemonInvalidException($"Ending time is null");
            }
            else if(description == null)
            {
                throw new Exceptions.LemonInvalidException($"Description is null");
            }
            else if(productImage == null)
            {
                throw new Exceptions.LemonInvalidException($"Product image is null");
            }
            else
            {
                var product = new Product
                    {
                        Name = name,
                        Category = category.ToString(),
                        Seller = user,
                        Description = description,
                        MinimumBid = startingPrice,
                        BiddingStartTime = DateTime.UtcNow,
                        BiddingEndTime = endingTime,
                        Image = productImage
                    };
                _lemonContext.Products.Add(product);
                await _lemonContext.SaveChangesAsync();
                return product;
            }
        }
    }
}