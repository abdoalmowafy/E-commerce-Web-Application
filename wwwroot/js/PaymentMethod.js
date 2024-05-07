const tel = document.getElementById("identifier");
const radio = document.getElementById("PaymentMethod3");
radio.addEventListener("change", function () {
    if (this.checked) {
        tel.removeAttribute("hidden");
        tel.setAttribute("required", "");
    } else {
        tel.setAttribute("hidden", "true");
        tel.removeAttribute("required");
    }
});