new Vue({
    el: "#create-account-form",
    data: {
        title: "Create New Account",
        username_placeholder: "Username",
        email_placeholder: "email@example.com",
        password_placeholder: "Password",
        submit_button: "Confirm",
        lemonAuction_logo: "images/LemonAuction_Logo.svg",
        create_account_label: "Don't have an account yet? Create one now!",
        userDescription_label: "Upload profile picture",
        selectedFile: null,
        username: null,
        email: null,
        password: null,
        selectedURL: null
    },
    methods: {

        onFileSelected(event) {
            this.selectedFile = event.target.files[0];
            this.selectedURL = URL.createObjectURL(this.selectedFile);
        },
        submitRegistration(event) {
            var formData = new FormData();
            formData.append('Avatar', this.selectedFile);
            var userInfo = {
                Username: this.username,
                Email: this.email,
                Password: this.password,
            };
            formData.append('UserInfo', JSON.stringify(userInfo));
            axios.post('/account/register', formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                })
                .then(function(response) {
                    console.log(response);
                    var token = response.request.response;
                    window.localStorage.setItem('userId', parseJwt(token)["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]);
                    window.localStorage.setItem('userKey', response.request.response)
                    window.location.href = "/marketplace.html";

                })
                .catch(function(error) {
                    window.alert(error.response.data.error);
                });
        }
    }

})

new Vue({
    el: "a",
    data: {
        log_in_page_link: "log_in_page.html"
    }
})

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace('-', '+').replace('_', '/');
    return JSON.parse(window.atob(base64));
};