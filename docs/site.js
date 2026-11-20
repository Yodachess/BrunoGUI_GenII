// Page d'accueil de BrunoGUI : bouton de téléchargement
(function () {
    var anglais = document.documentElement.lang === "en";

    // Dernière version publiée, lue sur GitHub : le bouton télécharge directement son zip et en affiche le numéro et la taille.
    // Sans réponse (hors ligne, limite de GitHub), le bouton garde son lien vers la page de la dernière version.
    var bouton = document.getElementById("telecharger");
    fetch("https://api.github.com/repos/Yodachess/BrunoGUI_GenII/releases/latest")
        .then(function (r) { return r.ok ? r.json() : null; })
        .then(function (release) {
            if (!release || !release.assets) return;
            var zip = release.assets.filter(function (a) { return /^BrunoGUI_GenII.*\.zip$/i.test(a.name); })[0];
            if (!zip) return;
            var version = release.tag_name.replace(/^v/, "");
            var mo = Math.round(zip.size / 1048576);
            bouton.href = zip.browser_download_url;
            bouton.innerHTML = (anglais ? "Download BrunoGUI " : "Télécharger BrunoGUI ") + version +
                '<span class="taille">' + (anglais ? "zip file, " : "fichier zip, ") + mo + (anglais ? " MB" : " Mo") + "</span>";
        })
        .catch(function () { });
})();
