var userId = window.localStorage.getItem('userId');
var userToken = window.localStorage.getItem('userKey');

Vue.component('category-button', {
    props: ['category'],
    template: `<button @click="$emit('pressed', category.title)">{{ category.title }}</button>`

})


Vue.component('product', {
    props: ['productInfo'],
    data: function() {
        return {
            bidEnd: moment(this.productInfo.biddingEndTime, moment.ISO_8601),
            bidTimeRemaining: null,
            intervalKeeper: null
        }
    },
    template: `
        <a :href="'product_detail.html?productId=' + productInfo.productId">
            <div class="product">
                <div class="product-image">
                    <img v-bind:src="'/api/bidding/products/' + productInfo.productId + '/image'">
                </div>
                <article class="product-information">
                    <h1><span>{{ productInfo.name }} </span></h1>
                    <div>
                        <p class="price"> $ {{ productInfo.currentPrice }} </p>
                        <p class="bids-counter"> {{ productInfo.bidCount }} bids</p>
                        <p class="time-counter"> {{ bidTimeRemaining }} left</p>
                    </div>
                </article>
            </div>
        </a>
        `,
    created: function() {
        this.getTimeLeft();
        this.intervalKeeper = window.setInterval(this.getTimeLeft, 30000);
    },
    destroyed: function() {
        window.clearInterval(this.intervalKeeper);
    },
    methods: {
        getTimeLeft() {
            this.bidTimeRemaining = moment.utc().to(this.bidEnd, true);
        }
    }
})

new Vue({
    el: '#marketplace',
    data: {
        imageSrc: `/account/avatar/${userId}`,
        searchQuery: '',
        selectedCategory: null,
        categories: [
            { id: 1, title: 'Books' },
            { id: 2, title: 'Fashion' },
            { id: 3, title: 'Music' },
            { id: 4, title: 'Technology' },
            { id: 5, title: 'Other' }
        ],
        products: []
    },
    created: function() {
        axios.get('/api/bidding/products?activeOnly=true')
            .then(response => {
                this.products = response.data;
            })
            .catch(error => {
                window.alert(error.response.data.error);

            });

    },
    computed: {
        resultProducts: function() {
            console.log("Filtering");
            return this.products.filter(product => {
                return (this.searchQuery === '' || product.name.toLowerCase().includes(this.searchQuery.toLowerCase())) && (this.selectedCategory === null || this.selectedCategory.toLowerCase() === product.category.toLowerCase());
            });
        }
    }
})


new Vue({
    el: "footer",
    data: {
        sell_product_text: "Sell New Product",
        sell_product_link: "sell_product.html"
    }
})