//var media = document.getElementsByClassName("preview-image");
//for (var i = 0; i < media.length; i++) {
//    var file = media[i];
//    file.addEventListener("click", function () {
//        var modalImage = document.getElementById("modalImage");
//        modalImage.src = file.firstElementChild.src;
//        var previewModal = new bootstrap.Modal(document.getElementById("mediaPreviewModal"));
//        previewModal.show();
//    });
//}

//document.getElementById("search").addEventListener("change", function () {

//});


// Main and Minor Images
var mainImg = document.getElementById("mainImg")
var minorImgs = document.getElementsByClassName("minorImg");
for (var i = 0; i < minorImgs.length; i++) {
    minorImgs[i].addEventListener("mouseover", function () {
        mainImg.src = this.src;
    });
}


// 
var countDisplays = document.getElementsByClassName("count-display");
var countInputs = document.getElementsByClassName("count-input");
for (let i = 0; i < countDisplays.length; i++) {
    countDisplays[i].addEventListener("click", function () {
        console.log("Show");
        countInputs[i].removeAttribute("Hidden");
        this.setAttribute("hidden", "");
    });
}





const tel = document.getElementById("identifier");
const radios = document.getElementsByName("PaymentMethod");

for (let i = 0; i < radios.length; i++) {
    if (i != 2) {
        radios[i].addEventListener("change", function () {
            console.log("unvisable");
            tel.setAttribute("hidden", "");
            tel.removeAttribute("required");
        });
    }
    else {
        radios[i].addEventListener("change", function () {
            console.log("visable");
            tel.setAttribute("required", "");
            tel.removeAttribute("hidden");
        });
    }
}