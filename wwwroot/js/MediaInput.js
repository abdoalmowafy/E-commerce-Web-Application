// Display preview of uploaded media files
document.getElementById("Media").addEventListener("change", function (event) {
    var previewContainer = document.getElementById("media-preview");
    previewContainer.innerHTML = ""; // Clear previous preview

    var files = event.target.files;
    var filePreviews = []; // Array to store file references and their preview elements

    function removeFile(fileToRemove) {
        return function () {
            // Find the index of the clicked file in the array
            var index = filePreviews.findIndex(function (fp) {
                return fp.file === fileToRemove;
            });

            if (index !== -1) {
                // Remove the file and preview element from the array
                var removedFile = filePreviews.splice(index, 1)[0];

                // Remove the preview element from the container
                previewContainer.removeChild(removedFile.preview);

                // Update the input field with the SKU files
                var SKUFiles = filePreviews.map(function (fp) {
                    return fp.file;
                });

                // Create a new FileList object with the SKU files
                var updatedFileList = new DataTransfer();
                SKUFiles.forEach(function (file) {
                    updatedFileList.items.add(file);
                });

                // Assign the new FileList object to the input element
                document.getElementById("Media").files = updatedFileList.files;
            }
        };
    }

    for (let i = 0; i < files.length; i++) {
        var file = files[i];
        var fileReader = new FileReader();

        fileReader.onload = (function (file) {
            return function (e) {
                var previewImage = document.createElement("div");
                previewImage.className = "preview-image col-lg-2";
                var image = document.createElement("img");
                image.src = e.target.result;
                var removeButton = document.createElement("div");
                removeButton.className = "remove-button";
                removeButton.innerText = "x";
                var previewIndex = document.createElement("input");
                previewIndex.type = "number";
                previewIndex.min = 1;
                console.log(i);
                previewIndex.value = i + 1;

                // Store the file and preview element in the array
                var filePreview = {
                    file: file,
                    preview: previewImage
                };
                filePreviews.push(filePreview);

                removeButton.addEventListener("click", removeFile(file));

                previewImage.appendChild(image);
                previewImage.appendChild(removeButton);
                previewImage.appendChild(previewIndex);
                previewContainer.appendChild(previewImage);

                // Open the modal when clicking the preview image
                image.addEventListener("click", function (event) {
                    var modalImage = document.getElementById("modalImage");
                    modalImage = image;
                    // modalImage.src = e.target.result;
                    var previewModal = new bootstrap.Modal(document.getElementById("mediaPreviewModal"));
                    previewModal.show();
                });
            };
        })(file);

        fileReader.readAsDataURL(file);
    }
});
