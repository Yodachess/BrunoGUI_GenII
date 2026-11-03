# Fabrique les PDF de l'aide (Aide\fr et Aide\en) avec Edge, à la racine du dépôt : à chaque publication, et à la main pour les relire.
#   La mise en page du PDF est dans Aide\aide.css (@media print).
$ErrorActionPreference = "Stop"
$racine = Split-Path $PSScriptRoot -Parent
$edge = Join-Path ${env:ProgramFiles(x86)} "Microsoft\Edge\Application\msedge.exe"
if (-not (Test-Path $edge)) { $edge = Join-Path $env:ProgramFiles "Microsoft\Edge\Application\msedge.exe" }
$profil = Join-Path ([IO.Path]::GetTempPath()) ("aide-pdf-" + [guid]::NewGuid())
try {
    foreach ($langue in "fr", "en") {
        $sortie = Join-Path $racine "BrunoGUI_Aide_$langue.pdf"
        if (Test-Path $sortie) { Remove-Item $sortie -Force }
        $page = ([Uri](Join-Path $racine "Aide\$langue\index.html")).AbsoluteUri
        $edgeLance = Start-Process -FilePath $edge -PassThru -WindowStyle Hidden -ArgumentList @(
            "--headless=new", "--disable-gpu", "--no-first-run", "--user-data-dir=`"$profil`"",
            "--no-pdf-header-footer", "--generate-pdf-document-outline", "--print-to-pdf=`"$sortie`"", "`"$page`"")
        if (-not $edgeLance.WaitForExit(120000)) { throw "Edge n'a pas fini le PDF ($langue) en 2 minutes" }
        if (-not (Test-Path $sortie) -or (Get-Item $sortie).Length -lt 100KB) { throw "PDF de l'aide absent ou trop petit ($langue)" }
        Write-Host "$sortie : $([math]::Round((Get-Item $sortie).Length / 1MB, 1)) Mo"
    }
}
finally {
    Remove-Item $profil -Recurse -Force -ErrorAction SilentlyContinue
}
