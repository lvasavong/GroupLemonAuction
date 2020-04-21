using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace LemonAuction.Identity {
    public class LemonUser : IdentityUser
    {
        // public int UserId { get; set; }
        public string FirstName {get; set;}
        public string LastName {get; set;}
        // public string Username {get; set;}
        /*
        public string profilePic { get; set; }
        [Column(TypeName = "text")]
        */

        public byte[] Avatar { get; set; }
        public DateTime DateCreated { get; set; }
        // public string EmailAddress { get; set; }

        // RELATIONAL
        public ICollection<Models.Bid> Bids { get; } = new List<Models.Bid>();
        public ICollection<Models.Product> ProductsPutUp { get; } = new List<Models.Product>();   
     }
}