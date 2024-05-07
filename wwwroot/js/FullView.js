// Share Button
var shareBtn = document.getElementById("share");
shareBtn.addEventListener("mouseover", function () {
    this.firstElementChild.className = "bi bi-share-fill";
});
shareBtn.addEventListener("mouseout", function () {
    this.firstElementChild.className = "bi bi-share";
});


// Main and Minor Images
var mainImg = document.getElementById("mainImg")
var minorImgs = document.getElementsByClassName("minorImg");
for (var i = 0; i < minorImgs.length; i++) {
    minorImgs[i].addEventListener("mouseover", function () {
        mainImg.src = this.src;
    });
}









//var unfill = function () {
//    var mark = this.firstElementChild;
//    if (mark.className.slice(-5) == "-fill") {
//        var newClass = mark.className.slice(0, -5);
//        mark.className = newClass;
//    }
//}

//var fill = function () {
//    this.firstElementChild.className += "-fill";
//}

//// wishlist Buttons
//var wlBtns = document.getElementsByClassName("wishlist");
//for (var i = 0; i < wlBtns.length; i++) {
//    var wlBtn = wlBtns[i].firstElementChild;
//    wlBtn.addEventListener("click", function () {
//        if (this.className == "bi bi-bookmark-heart") {
//            this.className = "bi bi-bookmark-heart-fill";
//        } else {
//            this.className = "bi bi-bookmark-heart";
//        }
//    });
//}
//// Cart Button
//document.getElementById("cart").addEventListener("click", function () {
//    this.className = "btn btn-success";
//    this.innerHTML = "Added &nbsp; <i class=\"bi bi-bag-check\"></i>";
//});

