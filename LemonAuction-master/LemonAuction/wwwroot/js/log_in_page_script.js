new Vue({
    el: "#log-in-form",
    data: {
        title: "Log In",
        username_placeholder: "Username",
        password_placeholder: "Password",
        submit_button: "Continue",
        lemonAuction_logo: "images/LemonAuction_Logo.svg",
        create_account_label: "Don't have an account yet? Create one now!",
        create_account_link: "create_account.html",
        username: null,
        password: null
    },
    methods: {
        submitLogin(event) {
            const loginData = {
                Username: this.username,
                Password: this.password
            };
            axios.post('/account/login', loginData)
                .then(response => {
                    var token = response.request.response;
                    window.localStorage.setItem('userId', parseJwt(token)["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]);
                    window.localStorage.setItem('userKey', response.request.response)
                    window.location.href = "/marketplace.html";
                })
                .catch(error => {
                    window.alert(error.response.data.error);

                })
        }
    }
})

function parseJwt(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace('-', '+').replace('_', '/');
    return JSON.parse(window.atob(base64));
};