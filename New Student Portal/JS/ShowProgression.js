var modal, loading;
function ShowProgress() {
    modal = document.createElement("DIV");
    modal.className = "modalLoader";
    document.body.appendChild(modal);
    loading = document.getElementsByClassName("loading")[0];
    loading.style.display = "block";
    var top = Math.max(window.innerHeight / 2 - loading.offsetHeight / 2, 0);
    var left = Math.max(window.innerWidth / 2 - loading.offsetWidth / 2, 0);
    loading.style.top = top + "px";
    loading.style.left = left + "px";
};
function HideProgress() {
    document.body.removeChild(modal);
    //loading.style.display = "none";
    $("#divLoader").hide();
}