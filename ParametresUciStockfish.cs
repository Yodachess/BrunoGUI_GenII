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
        public ParametresUciStockfish()
        {
            InitializeComponent();
            this.FormClosing += ParametresUciStockfish_FormClosing;     // Gestion du click sur la croix rouge en haut à droite ...
            this.VisibleChanged += ParametresUciStockfish_VisibleChanged;
        }
        private void ParametresUciStockfish_VisibleChanged(object sender, EventArgs e)
        {   // A chaque affichage, on montre le nombre de variantes actuellement demandé au moteur
            if (Visible)
                MultiPVUpDown.Value = Math.Clamp(MoteurUci.NombreLignesPV, (int)MultiPVUpDown.Minimum, (int)MultiPVUpDown.Maximum);
        }
        private void ParametresUciStockfish_Load(object sender, EventArgs e)
        {   // Affichage des paramêtres dans la console
            MoteurUci.StandardInputDataToUci("uci");
        } 
        private void ClearHashButton_Click(object sender, EventArgs e)
        {   // Traitement du bouton de vidage des hash tables
            MoteurUci.StandardInputDataToUci("setoption name Clear Hash");
        }
        private void ParametresFermer_Click(object sender, EventArgs e)
        {   // Passage au moteur des paramètres sélectionnés 
            MoteurUci.StandardInputDataToUci("setoption name Ponder value " + (checkBoxPonder.Checked ? "true" : "false"));
            MoteurUci.StandardInputDataToUci("setoption name Threads value " + (int)ThreadsUpDown.Value);
            MoteurUci.StandardInputDataToUci("setoption name Hash value " + (int)HashSizeUpDown.Value);
            MoteurUci.DefinitMultiPV((int)MultiPVUpDown.Value);
            MoteurUci.StandardInputDataToUci("setoption name Skill Level value " + (int)SkillLevelUpDown.Value);
            MoteurUci.StandardInputDataToUci("setoption name Move Overhead value " + (int)MoveOverheadUpDown.Value);
            MoteurUci.StandardInputDataToUci("setoption name nodestime value " + (int)NodesTimeUpDown.Value);
            this.Hide();
        }
        private void ParametresUciStockfish_FormClosing(object sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true; // Annule la fermeture
            this.Hide();      // Masque la fenêtre
        }
    }
}
