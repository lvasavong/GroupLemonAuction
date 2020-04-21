
[![Build Status](https://travis-ci.com/jc0541/LemonAuction.svg?token=ymCRShP4VePy17CYZL1D&branch=master)](https://travis-ci.com/jc0541/LemonAuction)


![Lemon Works Logo](LemonAuction/wwwroot/images/LemonAuction_Logo_bgblack.svg)


############################ Project Structure ######################################

## LemonAuction/Controllers
This is where we implement the Web Api, which is used for the backend of the site, including bidding, putting up products for auction, and account creation
1. `AccountController`: Responsible for managing user registration and log-ins.  Authentication is implemented through .NET Core Identity using JSON Web Tokens (JWT).
2. `BiddingController`:  Responsible for all bidding related API requests, and product information
3. `HomeController`: Fallback route, to be used when all other routing fails

## LemonAuction/DTO
This is where we define out Data Transfer Objects (DTO).  We want touse these instead of using the Entity's from Entity Framework directly because the client does not need or should't have access to certain parts of the data.

1. `CategoryEnum`: Since we have a fixed set of product categories, we use enums, so that we don't have to validate the string of the categories of new products ourselves, and let the JSON serializers handle that for us
2. `UserInfoDto`: Useful when all that is needed is Id and username, useful when used to represent a user in relationship to a particular bid where we don't need all of the extra data associated with the user entity itself.
3. `LoginDto`: This is the object used for model binding when the user sends a login api call, requiring them to supply their username and password.
4. `UserInfoRegisterDto`: This is much the same as the above DTO, except for representing a user in the context of registration, which also requires an email.
5. `RegisterDto`: This DTO wraps the above DTO so that we can send api required that includes both an image and JSON data when submitting registration, through using a FormData requires.  This requires some custom ModelBinding that we found online, and we sourced the link of how to do it.
6. `AddProductDto`: This is the DTO used for model binding when a user makes a call to put up a new product on the web api.  Uses the same Form Data strategy Dto and represents all of the data other than the product image in a sub property dto
7. `AddProductInfoDto`: See above.
8. `ProductDetailDto`: Useful when the user needs detailed information about a particular product, useful for single product request, but it might make more sense in other situations to use a DTO with less Data if making a bulk request for products.
9.  `ProductInfoDto`: The dto eluded to above
10.  `BidInfoDto`: Detailed bid info dto


## LemonAuction/Hubs
This is where we implement the signalar portion of the project, used for the real time bidding.

1. `IBiddingClient`: Used so we can check at compile time the methods  that server can have clients call.
2. `BiddingHub`: The server side logic for the signal for bidding.  Creates groups for every product where at least one is on the page and will add users to those groups when they go the page.


## LemonAuction/Middleware

This is where we define a custom filter attribute that when an exceptions is thrown, it will intercept the request and send back a JSON object with the error message, useful as it only intercepts on things where you apply the attribute

## LemonAuction/Model

This is where we define the entities that are used in the database with .NET Core for Entity Framework.

1. `LemonModel`: Where we define the database model, custom behavior to work with the backend database.
2. `LemonUser`: Our custom user to be used by .Net Core Identity used with entity framework, a subclass of IdentityUser.


## LemonAuction/Services
This is where nearly all of the business logic is abstracted into, so it can be used in both in SignalR and Web API.  Also contains custom exceptions for errors.


## Lemon/wwwroot

Client side application, html, css, javascript, sample images, and site asset images.

########################### ADDRESSING REQUIREMENTS ######################################

# Database

## Data
We used a PostgresSQL database to persist our app's data to create, maintain, and design our application. In LemonModel.cs and LemonUser.cs, we have three tables: 

1. Product
- ProductID, Name, Category, Seller, Description, MinimumBid, Bids, BiddingStartTime, BiddingEndTime, Active, Image

2. Bid
- BidId, Bidder, OnProduct, MadeAt, BidAmount, WinningBid

3. User
- FirstName, LastName, Avatar, DateCreated, Bids, ProductsPutUp

## Structure
We established the relationships for the tables so that we can manipulate product, user, and bid information: LemonUser has many Bids with one Bidder, LemonUser has many ProductsPutUp with one Seller, and Product has many Bids with one OnProduct.

## Connectivity

Connected to database installed locally (localhost) on machine (PostgreSQL), connected through entity framework with connection string template in config, with username set dynamically to the host machine user name.  

# Web API

## REST
The `BiddingController.cs` contains the functions called from the BiddingService.cs to GET "products" for `ProductsAsync`, `GET` "products" with their `productId` for `ProductWithId`, GET "products" with a search query for GetActiveProductsForSearchAsync, GET "products" with their productId along with their corresponding bids for GetBidsForProductAsync, GET "bids" for GetBidsForUserAsync, GET "products" with their productId so to get max bid for GetMaxBidForProductAsync, GET "products" with their productId for the image for ProductImage, POST for "bids" for an addition of bid for MakeBid, and POST for "products" for an addition of product for PutUpForAuction.

## Use of MVC or other framework
Used .NET Core for Entity Framework with the Model folder containing the the database structure: LemonModel.cs and LemonUser.cs with the PostgreSQL Database. View is the wwwroot folder making up the display of the application by using Vue.js, and the Controllers folder containing AccountController.cs, BiddingController.cs, and HomeController.cs. 

# Web Client

## Usability
Our web client interacts with our API by creating an account and then being able to login to the LemonAuction site. The user is able to see the products that are up for auction in marketplace.html. The user is able to click on a product and see live auctioning taking place on the product as well as the product description and the maximum bid that was placed on the product. The user is only allowed to bid on the product if their bid is greater than the maximum bid that they see.  Additionally, a user cannot bid on items they have put on sale.  Live auctioning can be seen by a user placing a live bid on a product while another user is on the same product web page. The user is able to go back to the marketplace.html from the product_detail.html and can sell a product on their own by clicking on the sell product link in the footer, which is the sell_product.html. User is able to set the product name, minimum bid, auction length, product description, and choosing a product image file in png that will be displayed.


## API Consumption
In the controllers folder where it controls user account information after the user creates an account, product information after the user creates a product to sell for the martketplace to be populated with products that are up for auction, and bid information for when users bid on a product. SignalR is used for real time bidding so that users see live auctioning happening while they are on the same product page where the bidding happens.


# Technologies Used 

**Database**:

* PostgreSQL for Relational Database
* Entity Framework Core for Object Relational Mapping

**Client**:
* VueJS for reactivity and data binding
* axios for Web API consumption (calls to the web api through ajax)
* moment.js for timestamps

**Web API**:
* .NET Core MVC
* RESTFUL, (POST and GET where appropriate)

**Realtime Services**:
* SignalR Core


# NOTES

1. Tests were never implemented, only configuration.
2. Google Sign in is configured in the setup.cs but never utilized.
3. Some of the Dtos are not used.
4. Some of the API methods are not called by the client itself but could be available potentially to a user who wants the info.
5. The WnningBid boolean property on Product is not implemented, used, or set (this is just computed as needed)
6. the node_modules folder in LemonAuction is not used
7. NameUserIdProvider is not usd
8. Views are only used for fallback SPA, just has a "Loading..." message.


