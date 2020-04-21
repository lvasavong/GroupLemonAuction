var userId = window.localStorage.getItem('userId');
var userToken = window.localStorage.getItem('userKey');

new Vue({
    el: "#black-bar",
    data: {
        imageSrc: `/account/avatar/${userId}`
    }
})

new Vue({
    el: "#form-container",
    data: {
        back_button: "Back",
        marketplace_link: "marketplace.html",
        add_product_label: "NEW PRODUCT",
        productName_label: "Product Name",
        productCategory_label: "Category",
        startingPrice_label: "Starting Price",
        auctionLength_label: "Auction Length",
        productDescription_label: "Description",
        productImage_label: "Product Image",


        selectedFile: null,
        confirmProduct_button: "Confirm Product",

        categories: [
            { name: 'Books' },
            { name: 'Fashion' },
            { name: 'Music' },
            { name: 'Technology' },
            { name: 'Other' }
        ],


        productName: null,
        productCategory: null,
        productStartingPrice: null,
        productDuration: null,
        productDescription: null,
        selectedURL: null
    },
    methods: {
        onFileSelected(event) {
            this.selectedFile = event.target.files[0];
            this.selectedURL = URL.createObjectURL(this.selectedFile);
        },
        submitProduct() {
            var formData = new FormData();
            formData.append('ProductImage', this.selectedFile);
            var productInfo = {
                Name: this.productName,
                Category: this.productCategory,
                StartingPrice: this.productStartingPrice,
                Duration: this.productDuration,
                Description: this.productDescription
            };
            formData.append('ProductInfo', JSON.stringify(productInfo));
            axios.post('/api/bidding/products/add', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data',
                        'Authorization': `Bearer ${localStorage.getItem('userKey')}`
                    }
                })
                .then(function(response) {

                    console.log(response);
                    window.location.href = `/product_detail.html?productId=${response.data.id}`;

                })
                .catch(function(error) {
                    window.alert(error.response.data.error);

                });

        }
    }
})