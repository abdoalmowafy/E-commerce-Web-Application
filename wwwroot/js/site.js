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
