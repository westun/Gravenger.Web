function saveCard() {
    var savebutt = document.getElementById("save-button");
    savebutt.innerHTML = "Saved";
    if (savebutt.innerHTML = "Saved") { function saveAgain() { savebutt.innerHTML = "Save Postcard"; }; }
    setTimeout(saveAgain, 1000);
    //TO DO: save card data to database
};

function readURL(id) {
    var tile = document.getElementById(id);
    tile.style.display = "inline-block";
    var label = document.getElementById("label-" + id);
    label.className = "labelled"; //hides label while cropping image
    var butts = document.getElementById("buttons-" + id)
//    butts.className = "buttonbox hidden";
    var w = tile.width;
    var h = tile.height;
    var container = document.getElementById("container-" + id);
    var donebutton = document.getElementById("done-button-" + id);
    var basic = new Croppie(document.getElementById(id), { viewport: { width: w, height: h }, showZoomer: false });
    //creates croppie element to resize image
    var file = document.getElementById("file-" + id).files[0];
    var reader = new FileReader();
    reader.onloadend = function () {
        basic.bind({ url: reader.result });
        donebutton.style.visibility = "visible";
    }
    if (file, file.type.match("image.*")) {
        reader.readAsDataURL(file);
    }
    else { alert("Sorry, we don't support whatever format that is. Upload a JPEG, GIF, or PNG!") }

    donebutton.onclick = finalPic; //appends cropped image to tile

    function finalPic() {
        var input = document.getElementById("file-" + id);
        basic.result('base64').then(function (base64) {
            var image = new Image();
            image.src = base64;
            image.setAttribute("style", "z-index: 3");
            image.className = "tile-image"; //makes the image the same size as the container
            label.className = "label labelled"; //makes the tile's label visible on hover
            label.addEventListener("mouseover", showButts); //shows the label's dropdown buttons
            function showButts() {
                butts.style.visibility = "visible";
                butts.addEventListener("mouseleave", function () { butts.style.visibility = "hidden"; });
            }
            container.appendChild(image); //adds image to tile
            //Wes please: ADD IMAGE TO DATABASE

            input.value = null;
            input.addEventListener("change", changePic);

            function changePic() {
                container.removeChild(image);
                input.value = null;
                label.removeEventListener("mouseover", showButts);
                //Wes please: REMOVE IMAGE FROM DATABASE
            };
            function removePic() {
                container.removeChild(image);
                label.className = "label";
                butts.className = "buttonbox hidden";
                label.removeEventListener("mouseover", showButts);
                //Wes please: REMOVE IMAGE FROM DATABASE
            };
            var remove = document.getElementById("remove-" + id);
            var replace = document.getElementById("replace-" + id);

            remove.onclick = removePic;
            replace.onclick = function () { label.click(); }; //calls changePic function
        });
        basic.destroy();
        tile.style.display = "none";

        donebutton.style.visibility = "hidden";
    };

};

//moves Remove&Replace buttons a little lower if the Tile Label is longer than one line
//but, isn't perfect, need to find a way to do this other than textContent.length
function lineBreak() {
    var labelTC = document.getElementsByTagName("label");
    var buttons = document.getElementsByClassName("buttonbox");
    for (i = 0; i < labelTC.length; i++) {
        if (labelTC[i].textContent.length > 30) {
            buttons[i].setAttribute("style", "margin-top: 24px;");
        }
    }
}
lineBreak();
