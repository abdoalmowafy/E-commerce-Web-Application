// Clicking product quantity in cart page to edit it
var countDisplays = document.getElementsByClassName("count-display");
var countInputs = document.getElementsByClassName("count-input");
for (let i = 0; i < countDisplays.length; i++) {
    countDisplays[i].addEventListener("click", function () {
        countInputs[i].removeAttribute("Hidden");
        countInputs[i].focus();
        this.setAttribute("hidden", "");
    });
}

// Choosing mobile wallet in payment methods option
const storeAddresses = document.getElementById("store-addresses");
const storeRadio = document.getElementById("DeliveryToStoreAddress");
const userAddresses = document.getElementById("user-addresses");
const userRadio = document.getElementById("DeliveryToUserAddress");
const fee = document.getElementById("fee");
const total = document.getElementById("total");
const tel = document.getElementById("identifier");
const radios = document.getElementsByName("paymentMethod");

storeRadio.addEventListener("change", function () {
    storeAddresses.removeAttribute("hidden");
    userAddresses.setAttribute("hidden", "");

    fee.textContent = `${parseFloat(fee.textContent.split(" ")[0]) - 50} EGP`;
    total.textContent = `${parseFloat(total.textContent.split(" ")[0]) - 50} EGP`;
});

userRadio.addEventListener("change", function () {
    userAddresses.removeAttribute("hidden");
    storeAddresses.setAttribute("hidden", "");

    fee.textContent = `${parseFloat(fee.textContent.split(" ")[0]) + 50} EGP`;
    total.textContent = `${parseFloat(total.textContent.split(" ")[0]) + 50} EGP`;
});
   


// Choosing mobile wallet in payment methods option
for (let i = 0; i < radios.length; i++) {
    if (radios[i].id == "MobileWallet") {
        radios[i].addEventListener("change", function () {
            tel.setAttribute("required", "");
            tel.removeAttribute("hidden");

            if (fee.textContent == "60 EGP" || fee.textContent == "10 EGP") {
                fee.textContent = `${parseFloat(fee.textContent.split(" ")[0]) - 10} EGP`;
                total.textContent = `${parseFloat(total.textContent.split(" ")[0]) - 10} EGP`;
            }
        });
    }
    else if (radios[i].id == "COD") {
        radios[i].addEventListener("change", function () {
            tel.removeAttribute("required");
            tel.setAttribute("hidden", "");

            fee.textContent = `${parseFloat(fee.textContent.split(" ")[0]) + 10} EGP`;
            total.textContent = `${parseFloat(total.textContent.split(" ")[0]) + 10} EGP`;
        });
    }
    else {
        radios[i].addEventListener("change", function () {
            tel.removeAttribute("required");
            tel.setAttribute("hidden", "");

            if (fee.textContent == "60 EGP" || fee.textContent == "10 EGP") {
                fee.textContent = `${parseFloat(fee.textContent.split(" ")[0]) - 10} EGP`;
                total.textContent = `${parseFloat(total.textContent.split(" ")[0]) - 10} EGP`;
            }
        });
    }
}
