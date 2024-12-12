document.getElementById("Media").addEventListener("change", function (event) {
    var previewContainer = document.getElementById("media-preview");
    previewContainer.innerHTML = "";

    var files = event.target.files;
    var filePreviews = [];

    function removeFile(fileToRemove) {
        return function () {
            var index = filePreviews.findIndex(fp => fp.file === fileToRemove);
            if (index !== -1) {
                filePreviews.splice(index, 1);
                updatePreview();
            }
        };
    }

    function swapFiles(index1, index2) {
        if (index1 >= 0 && index2 >= 0 && index1 < filePreviews.length && index2 < filePreviews.length) {
            [filePreviews[index1], filePreviews[index2]] = [filePreviews[index2], filePreviews[index1]];
            updatePreview();
        }
    }

    function updatePreview() {
        previewContainer.innerHTML = "";
        filePreviews.forEach((filePreview, index) => {
            var previewImage = document.createElement("div");
            previewImage.className = "preview-image mx-2 d-grid";
            
            var image = document.createElement("img");
            image.src = filePreview.src;
            
            var removeButton = document.createElement("div");
            removeButton.className = "remove-button";
            removeButton.innerText = "x";
            removeButton.addEventListener("click", removeFile(filePreview.file));
            
            var previewIndex = document.createElement("input");
            previewIndex.type = "number";
            previewIndex.min = 1;
            previewIndex.value = index + 1;
            previewIndex.readOnly = true;
            
            var swapButton = document.createElement("button");
            swapButton.className = "btn btn-primary swap-button";
            swapButton.innerHTML = '<i class="bi bi-arrow-left-right"></i>';
            swapButton.addEventListener("click", function(event) {
                event.preventDefault();
                var swapIndex = parseInt(prompt("Enter the index to swap with:")) - 1;
                swapFiles(index, swapIndex);
            });
            
            previewImage.appendChild(image);
            previewImage.appendChild(removeButton);
            previewImage.appendChild(previewIndex);
            previewImage.appendChild(swapButton);
            previewContainer.appendChild(previewImage);
            
            image.addEventListener("click", function () {
                var modalImage = document.getElementById("modalImage");
                modalImage.src = image.src;
                var previewModal = new bootstrap.Modal(document.getElementById("mediaPreviewModal"));
                previewModal.show();
            });
        });
        
        var updatedFileList = new DataTransfer();
        // filePreviews.forEach(fp => updatedFileList.items.add(fp.file));
        filePreviews.forEach((fp, index) => {
            var extension = fp.file.name.split('.').pop();
            var newFile = new File([fp.file], `${index + 1}.${extension}`, { type: fp.file.type });
            updatedFileList.items.add(newFile);
        });
        document.getElementById("Media").files = updatedFileList.files;
    }

    for (let i = 0; i < files.length; i++) {
        var file = files[i];
        var fileReader = new FileReader();

        fileReader.onload = function (e) {
            filePreviews.push({
                file: file,
                src: e.target.result
            });
            updatePreview();
        };

        fileReader.readAsDataURL(file);
    }
});
