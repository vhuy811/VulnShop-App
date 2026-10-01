// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Loi chao theo ten tren URL, vd /?ten=Minh
document.addEventListener("DOMContentLoaded", function () {
    var ten = new URLSearchParams(window.location.search).get("ten");
    var o = document.getElementById("loi-chao");
    if (ten && o) {
        o.textContent = "Xin chao " + ten;
    }
});
