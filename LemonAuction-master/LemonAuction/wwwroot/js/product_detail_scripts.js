var userId = window.localStorage.getItem('userId');

const HUB_CLIENT_EVENTS = {
    ReceiveJoinBidding: 'ReceiveJoinBidding',
    ReceiveLeaveBidding: 'ReceiveLeaveBidding',
    ReceiveBid: 'ReceiveBid',
    ReceiveProduct: 'RecieveProduct'
};

const HUB_SERVER_EVENTS = {
    AddToBiddingGroup: 'AddToBiddingGroup',
    RemoveFromBiddingGroup: 'RemoveFromBiddingGroup'
};

function getMaxOfArray(numArray) {
    return Math.max.apply(null, numArray);
}


Vue.component('category-button', {
    props: ['category'],
    template: '<button>{{ category }}</button>'
})


Vue.component('bid-component', {
    props: ['bid', 'color', 'index'],
    data: function() {
        return {
            bidTime: moment(this.bid.madeAt, moment.ISO_8601),
            bidMsg: ''
        };
    },
    template: `
        <transition name="slide">
            <div @mouseover="updateBidMsg" v-bind:title="bidMsg" class="bid">
                <div class="bid-name">{{bid.bidder.username}}</div>
                <div v-if="this.index % 2 === 0" v-bind:style="{ backgroundColor: color }" class="bid-bar"></div>
                <div class="bid-amount">$ {{bid.bidAmount}}</div>
                <div v-if="this.index % 2 !== 0" v-bind:style="{ backgroundColor: color }" class="bid-bar"></div>
            </div>
        </transition>
        `,
    methods: {
        updateBidMsg() {
            this.bidMsg = this.bidTime.fromNow();
        }
    }
})

Vue.component('product-detail', {
    props: ['productDetail'],
    template: `
        <div id="product-detail">    
            <div class="product">
                <div class="product-image">
                    <img v-bind:src="'/api/bidding/products/' + productDetail.productId + '/image'">
                </div>
                <article class="product-information">
                    <h1> {{ productDetail.name }} </h1>
                    <div>
                        <p class="bold-text">Description:</p>
                        <p class="description multiline-text" > {{ productDetail.description }} </p>
                        <p class="bold-text">Seller: {{ productDetail.seller.username }} </p>
                    </div>
                </article>
            </div>
        </div>
        `
})


new Vue({
    el: "#product-detail-grid",
    data: {
        back_button: "Back",
        marketplace_link: "marketplace.html",
        bids_history_label: "BIDS HISTORY",
        place_bid_button: "PLACE BID",
        bid_placeholder: "$",
        connection: null,
        bidCount: 0,
        bidTimeRemaining: null,
        bidEnd: null,
        bidAmount: 0,
        intervalKeeper: null,
        currentPrice: 0,
        winningBidAmount: null,
        shouldScroll: true,
        productId: new URLSearchParams(window.location.search).get('productId'),
        product: {},
        bids: [],
        colorMap: {},
        colorOptions: ['#e6194b', '#3cb44b', '#4363d8', '#f58231', '#911eb4', '#46f0f0', '#f032e6', '#bcf60c', '#fabebe', '#008080', '#e6beff', '#9a6324', '#fffac8', '#800000', '#aaffc3', '#808000', '#ffd8b1', '#000075', '#808080'],
        nextIndexToAssign: 0

    },
    created: function() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/bidding", { accessTokenFactory: () => localStorage.getItem('userKey') })
            .configureLogging(signalR.LogLevel.Information)
            .build();
        this.connection.on(HUB_CLIENT_EVENTS.ReceiveJoinBidding, this.handleJoinBidding);
        this.connection.on(HUB_CLIENT_EVENTS.RemoveFromBiddingGroup, this.handleLeftBiddingd);
        this.connection.on(HUB_CLIENT_EVENTS.ReceiveBid, this.handleReceiveBid);
        this.connection.on(HUB_CLIENT_EVENTS.ReceiveProduct, this.handleReceiveProduct);
        this.connection.start()
            .then(this.handleHubConnected)
            .catch(this.handleHubConnectionFailed);

        // axios.get(`/api/bidding/products/${this.productId}?activeOnly=true`)
        //     .then(response => {

        //     })
        //     .catch(error => {
        //         console.log(error);
        //     });
    },
    updated: function() {
        this.$nextTick(() => this.scrollToEnd());
    },
    destroyed: function() {
        window.clearInterval(this.intervalKeeper);
    },
    methods: {
        handleHubConnected() {
            this.connection.invoke(HUB_SERVER_EVENTS.AddToBiddingGroup, this.productId);
        },
        handleReceiveProduct(productInfo) {
            this.product = productInfo;
            this.bidCount += this.product.bidCount;
            this.bids = this.bids.concat(this.product.bids);
            // this.$nextTick(() => this.scrollToEnd());
            this.sortBids();
            this.bids.forEach(bid => {
                if (!this.colorMap.hasOwnProperty(bid.bidder.username)) {
                    this.colorMap[bid.bidder.username] = this.colorOptions[this.nextIndexToAssign];
                    this.nextIndexToAssign = (this.nextIndexToAssign + 1) % this.colorOptions.length;
                }
            });
            this.currentPrice = this.product.currentPrice;
            if (this.bids.length > 0) {
                this.bidAmount = this.bids[this.bids.length - 1].bidAmount + 0.5;
            } else {
                this.bidAmount = this.currentPrice;
            }
            this.bidEnd = moment(this.product.biddingEndTime, moment.ISO_8601);
            this.getTimeLeft();
            this.intervalKeeper = window.setInterval(this.getTimeLeft, 30000);
        },
        handleHubConnectionFailed(error) {
            window.alert(error);

        },
        getTimeLeft() {
            this.bidTimeRemaining = moment.utc().to(this.bidEnd, true);
        },
        placeBid() {
            const requestData = {
                ProductId: this.productId,
                BidAmount: this.bidAmount
            };
            axios.post('/api/bidding/bids/add', requestData, {
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('userKey')}`
                    }
                })
                .catch(error => {
                    window.alert(error.response.data.error);
                });
        },
        handleJoinBidding(userInfo) {
            console.log(userInfo);
        },
        scrollToEnd() {
            // console.log(this.$refs);
            const scroller = this.$refs.bidScroller;
            // console.log(`BEFORE: scrollTop: ${scroller.scrollTop}\t scrollHeight: ${scroller.scrollHeight}`);
            scroller.scrollTop = scroller.lastElementChild.offsetTop;
            scroller.scrollIntoView();
            // console.log(`AFTER: scrollTop: ${scroller.scrollTop}\t scrollHeight: ${scroller.scrollHeight}`);

        },
        sortBids() {
            this.bids.sort((a, b) => a.bidId - b.bidId);
        },
        handleLeftBidding(userInfo) {
            console.log(userInfo);
        },
        isScrolledToBottom() {
            const scroller = this.$refs.bidScroller;
            return scroller.scrollHeight - scroller.scrollTop === scroller.clientHeight;

        },
        handleReceiveBid(bidInfo) {
            this.bidCount++;
            console.log(bidInfo);
            if (this.bidAmount < bidInfo.bidAmount) {
                this.bidAmount = bidInfo.bidAmount + 0.5;
            }
            if (!this.colorMap.hasOwnProperty(bidInfo.bidder.username)) {
                this.colorMap[bidInfo.bidder.username] = this.colorOptions[this.nextIndexToAssign];
                this.nextIndexToAssign = (this.nextIndexToAssign + 1) % this.colorOptions.length;
            }
            const shouldScroll = this.isScrolledToBottom();
            this.bids.push(bidInfo);
            this.sortBids();
            this.currentPrice = this.bids[this.bids.length - 1].bidAmount;
            if (shouldScroll) this.scrollToEnd();

        },
        getRandomColor() {
            var letters = '0123456789ABCDEF';
            var color = '#';
            for (var i = 0; i < 6; i++) {
                color += letters[Math.floor(Math.random() * 16)];
            }
            return color;
        }

        // getWinningBid: function() {
        //     for (bid in this.bidsCollection) {
        //         if (bid.isWinning == true) {
        //             this.winn
        //         }
        //     }

        // }
    }
})

new Vue({
    el: "#black-bar",
    data: {
        imageSrc: `/account/avatar/${userId}`
    }
})

// new Vue({
//     el: '#category-bar',
//     data: {
//         categories: [
//             { id: 1, title: 'Books' },
//             { id: 2, title: 'Fashion' },
//             { id: 3, title: 'Music' },
//             { id: 4, title: 'Technology' },
//             { id: 5, title: 'Other' }
//         ]
//     }
// })



new Vue({
    el: "footer",
    data: {
        sell_product_text: "Sell New Product",
        sell_product_link: "sell_product.html"
    }
})