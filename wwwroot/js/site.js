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


const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))