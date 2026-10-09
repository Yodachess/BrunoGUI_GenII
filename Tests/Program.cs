// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘


// Tests de la logique d'échecs, en console (les vérifications elles-mêmes sont dans Scenario.cs) :
//      dotnet run --project Tests               tests rapides
//      dotnet run --project Tests -- --complet  ajoute les perft profonds (~15 s)
// Les mêmes vérifications sont aussi des tests xUnit (VerificationsTests.cs) : dotnet test Tests, ou l'Explorateur de tests.

using System.Collections.Generic;
using System.Linq;

List<Verification> resultats = Scenario.Executer(complet: args.Contains("--complet"));
return resultats.All(v => v.Ok) ? 0 : 1;
