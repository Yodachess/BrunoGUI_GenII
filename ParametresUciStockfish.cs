// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'édition de tous les paramètres de Stockfish ...
// └─ Classe "ParametresUciStockfish" 
//              ├─ "ParametresUciStockfish"     (Init)
//              ├─ "ParametresUciStockfish_Load"
//              ├─ "ClearHashButton_Click"  
//              ├─ "ParametresFermer_Click"  
//              └─ "ParametresUciStockfish_FormClosing"


using System;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class ParametresUciStockfish : Form
    {
        private readonly MoteurUci MoteurUci;      // le moteur réglé par cette fenêtre
        public ParametresUciStockfish(MoteurUci moteur)
        {
            InitializeComponent();
            MoteurUci = moteur;
            this.FormClosing += ParametresUciStockfish_FormClosing;     // Gestion du click sur la croix rouge en haut à droite ...
            this.VisibleChanged += ParametresUciStockfish_VisibleChanged;
        }
        private void ParametresUciStockfish_VisibleChanged(object? sender, EventArgs e)
        {   // A chaque affichage, on montre les réglages actuels (variantes, threads, table de hachage)
            if (!Visible)
                return;
            MultiPVUpDown.Value = Math.Clamp(MoteurUci.NombreLignesPV, (int)MultiPVUpDown.Minimum, (int)MultiPVUpDown.Maximum);
            if (MoteurUci.NombreThreads is int threads)
                ThreadsUpDown.Value = Math.Clamp(threads, (int)ThreadsUpDown.Minimum, (int)ThreadsUpDown.Maximum);
            if (MoteurUci.TailleHachageMo is int hachage)
                HashSizeUpDown.Value = Math.Clamp(hachage, (int)HashSizeUpDown.Minimum, (int)HashSizeUpDown.Maximum);
        }
        private void ParametresUciStockfish_Load(object? sender, EventArgs e)
        {   // Affichage des paramêtres dans la console
            MoteurUci.StandardInputDataToUci("uci");
        } 
        private void ClearHashButton_Click(object? sender, EventArgs e)
        {   // Traitement du bouton de vidage des hash tables
            MoteurUci.StandardInputDataToUci("setoption name Clear Hash");
        }
        private void ParametresFermer_Click(object? sender, EventArgs e)
        {   // Passage au moteur des paramètres sélectionnés 
            MoteurUci.StandardInputDataToUci("setoption name Ponder value " + (checkBoxPonder.Checked ? "true" : "false"));
            MoteurUci.DefinitThreads((int)ThreadsUpDown.Value);
            MoteurUci.DefinitHachage((int)HashSizeUpDown.Value);
            MoteurUci.DefinitMultiPV((int)MultiPVUpDown.Value);
            MoteurUci.DefinitNiveau((int)SkillLevelUpDown.Value);      // pour jouer (une analyse se fait au niveau maximal)
            MoteurUci.StandardInputDataToUci("setoption name Move Overhead value " + (int)MoveOverheadUpDown.Value);
            MoteurUci.StandardInputDataToUci("setoption name nodestime value " + (int)NodesTimeUpDown.Value);
            this.Hide();
        }
        private void ParametresUciStockfish_FormClosing(object? sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true; // Annule la fermeture
            this.Hide();      // Masque la fenêtre
        }
    }
}
