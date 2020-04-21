using LemonAuction.Models;
using LemonAuction.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace LemonAuction.DTO
{


    public enum CategoryEnum {
        
        Books,
        Fashion,
        Music,
        Technology,
        Other
    }

    public class UserInfoDto {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Username { get; set; }

        public UserInfoDto(LemonUser user) {
            this.Id = user.Id;
            this.Username = user.UserName;
        }

    }




    public class UserDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // public string Username {get; set;}
        /*
        public string profilePic { get; set; }
        [Column(TypeName = "text")]
        */
        public DateTime DateCreated { get; set; }

        // public UserDTO()
        public UserDTO(LemonUser user)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            DateCreated = user.DateCreated;
        }
    }


    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UserInfoRegisterDto {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
    public class RegisterDto
    {

        [Required]
        [ModelBinder(BinderType = typeof(Utils.FormDataJsonBinder))]
        public UserInfoRegisterDto UserInfo { get; set; }

        [Required]
        public IFormFile Avatar { get; set; }
    }

    public class AddProductInfoDTO {
        [Required]
        public string Name;
        [Required]
        public CategoryEnum Category;
        [Required]
        public decimal StartingPrice;
        [Required]
        public int Duration;
        [Required]
        public string Description;
    }
    public class AddProductDTO
    {
        
        [Required]
        [ModelBinder(BinderType = typeof(Utils.FormDataJsonBinder))]
        public AddProductInfoDTO ProductInfo {get; set;}

        [Required]
        public IFormFile ProductImage {get; set;}
    }


    public class ProductDetailDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public CategoryEnum Category { get; set; }

        // RELATIONAL?

        public UserInfoDto Seller { get; set; }

        public string Description { get; set; }
        public decimal MinimumBid { get; set; }
        public DateTime BiddingStartTime { get; set; }
        public DateTime BiddingEndTime { get; set; }
        public int BidCount { get; set; }
        public decimal CurrentPrice { get; set; }

        public IEnumerable<BidInfoDTO> Bids { get; set; } = new List<BidInfoDTO>();
        public ProductDetailDTO(Product product, bool withBids = false)
        {
            ProductId = product.ProductId;
            Name = product.Name;
            CategoryEnum temp;
            if (Enum.TryParse(product.Category, out temp)) {
                Category = temp;
            }
            if (withBids) {
                Bids = product.Bids.Select(b => new BidInfoDTO(b)).ToList();
            }
            BidCount = product.Bids.Count;
            CurrentPrice = BidCount == 0 ? product.MinimumBid  : product.Bids.Max(x => x.BidAmount);
            Seller = new UserInfoDto(product.Seller);
            Description = product.Description;
            MinimumBid = product.MinimumBid;
            BiddingStartTime = product.BiddingStartTime;
            BiddingEndTime = product.BiddingEndTime;
        }

    }
    public class BidInfoDTO
    {
        public int BidId { get; set; }
        public UserInfoDto Bidder { get; set; }
        public int OnProductId { get; set; }
        public DateTime MadeAt { get; set; }
        public decimal BidAmount { get; set; }
        public bool WinningBid { get; set; }
        public BidInfoDTO(Bid bid)
        {
            BidId = bid.BidId;
            Bidder = new UserInfoDto(bid.Bidder);
            OnProductId = bid.OnProduct.ProductId;
            MadeAt = bid.MadeAt;
            BidAmount = bid.BidAmount;
            WinningBid = bid.WinningBid;
        }
    }

    public class ProductInfoDTO {
        public int Id { get; set; }
        public string Name { get; set; }

        public ProductInfoDTO(Product product) {
            Id = product.ProductId;
            Name = product.Name;
        }
    }

    public class MakeBidDTO {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public decimal BidAmount { get; set; }
    }


    public class BidDTO
    {
        public int BidId { get; set; }

        // RELATIONAL?
        public string BidderId { get; set; }
        public int OnProductId { get; set; }
        public DateTime MadeAt { get; set; }
        public decimal BidAmount { get; set; }
        public bool WinningBid { get; set; }

        public BidDTO(Bid bid)
        {
            BidId = bid.BidId;
            BidderId = bid.Bidder.Id;
            OnProductId = bid.OnProduct.ProductId;
            MadeAt = bid.MadeAt;
            BidAmount = bid.BidAmount;
            WinningBid = bid.WinningBid;
        }
    }

    namespace Utils
    {
        //https://stackoverflow.com/a/51615696
        public class FormDataJsonBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw new ArgumentNullException(nameof(bindingContext));
                }

                string fieldName = bindingContext.FieldName;
                var valueProviderResult = bindingContext.ValueProvider.GetValue(fieldName);

                if (valueProviderResult == ValueProviderResult.None)
                {
                    return Task.CompletedTask;
                }
                else
                {
                    bindingContext.ModelState.SetModelValue(fieldName, valueProviderResult);
                }

                string value = valueProviderResult.FirstValue;
                if (string.IsNullOrEmpty(value))
                {
                    return Task.CompletedTask;
                }

                try
                {
                    object result = JsonConvert.DeserializeObject(value, bindingContext.ModelType);
                    bindingContext.Result = ModelBindingResult.Success(result);
                }
                catch (JsonException)
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                }

                return Task.CompletedTask;
            }
        }
    }


}