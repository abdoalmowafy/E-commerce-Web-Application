// Clicking product quantity in cart page to edit it
var countDisplays = document.getElementsByClassName("count-display");
var countInputs = document.getElementsByClassName("count-input");
for (let i = 0; i < countDisplays.length; i++) {
    countDisplays[i].addEventListener("click", function () {
        console.log("Show");
        countInputs[i].removeAttribute("Hidden");
        this.setAttribute("hidden", "");
    });
}

// Choosing mobile wallet in payment methods option
const storeAddresses = document.getElementById("store-addresses");
const storeRadio = document.getElementById("DeliveryToStoreAddress");
const userAddresses = document.getElementById("user-addresses");
const userRadio = document.getElementById("DeliveryToUserAddress");

storeRadio.addEventListener("change", function () {
    storeAddresses.removeAttribute("hidden");
    userAddresses.setAttribute("hidden", "");
});

userRadio.addEventListener("change", function () {
    userAddresses.removeAttribute("hidden");
    storeAddresses.setAttribute("hidden", "");
});
   


// Choosing mobile wallet in payment methods option
const tel = document.getElementById("identifier");
const radios = document.getElementsByName("PaymentMethod");

for (let i = 0; i < radios.length; i++) {
    if (radios[i].id != "MobileWallet") {
        radios[i].addEventListener("change", function () {
            tel.setAttribute("hidden", "");
            tel.removeAttribute("required");
        });
    }
    else {
        radios[i].addEventListener("change", function () {
            tel.setAttribute("required", "");
            tel.removeAttribute("hidden");
        });
    }
}
