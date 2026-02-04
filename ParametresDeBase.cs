// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII est développé par Bruno COURTOIS.  Copyright © 2025 █
// █ BrunoGUI_GenII est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'édition des paramètres base de Stockfish ...
// └─ Classe "ParametresDeBase" 
//              ├─ "ParametresDeBase"     (Init)
//              ├─ "BaseBoutonOk_Click"
//              ├─ "BaseBoutonAnnuler_Click"  
//              └─ "ParametresDeBase_FormClosing"

using System;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class ParametresDeBase : Form
    {
        private readonly EchiquierPrincipal interfaceGraphique;     // Stocke la référence de la classe principale
        public ParametresDeBase(EchiquierPrincipal brunoigInstance)
        {
            InitializeComponent();
            interfaceGraphique = brunoigInstance;
            this.FormClosing += ParametresDeBase_FormClosing;   // Gestion du click sur la croix rouge en haut à droite ...
        }
        private void BaseBoutonOk_Click(object sender, EventArgs e)
        {
            PartieForceModule maNouvellePartieForceModule = new PartieForceModule();
            MoteurUci.DefinitLimiteElo(baseEloNumerique.Value.ToString());
            MoteurUci.StandardInputDataToUci("setoption name Threads value " + (int)baseThreadsNumerique.Value);
            MoteurUci.StandardInputDataToUci("setoption name MultiPV value " + (int)baseMultipvNumerique.Value);
            maNouvellePartieForceModule.DureeReflexionSeconde = interfaceGraphique.TrackBarTempsReflexion.Value = ((int)baseReflexionNumerique.Value);
            if (interfaceGraphique.OrdinateurJoueNoir)
                interfaceGraphique.PartieEnCours.BlackElo = interfaceGraphique.EloNoir.Text = baseEloNumerique.Value.ToString();
            else
                interfaceGraphique.PartieEnCours.WhiteElo = interfaceGraphique.EloBlanc.Text = baseEloNumerique.Value.ToString();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
        private void BaseBoutonAnnuler_Click(object sender, EventArgs e)
        {   // Le formulaire ne se ferme jamais, même si l’utilisateur clique sur la croix. Il est simplement caché.
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
        private void ParametresDeBase_FormClosing(object sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true; // Annule la fermeture de la fenêtre
            this.Hide();     // Masque la fenêtre
        }
    }
}
