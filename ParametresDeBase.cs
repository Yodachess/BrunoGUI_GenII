using System;
using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public partial class ParametresDeBase : Form
    {
        private BrunoInterfaceGraphique interfaceGraphique;     // Stocke la référence de la classe principale
        public ParametresDeBase(BrunoInterfaceGraphique brunoigInstance)
        {
            InitializeComponent();
            interfaceGraphique = brunoigInstance;
            this.FormClosing += ParametresDeBase_FormClosing;   // Gestion du click sur la croix rouge en haut à droite ...
        }
        private void BaseBoutonOk_Click(object sender, EventArgs e)
        {
            PartieForceModule maNouvellePartieForceModule = new PartieForceModule();
            MonoMoteurUci.DefinitLimiteElo(baseEloNumerique.Value.ToString());
            MonoMoteurUci.StandardInputDataToUci("setoption name Threads value " + (int)baseThreadsNumerique.Value);
            MonoMoteurUci.StandardInputDataToUci("setoption name MultiPV value " + (int)baseMultipvNumerique.Value);
            maNouvellePartieForceModule.DureeReflexionSeconde = ((int)baseReflexionNumerique.Value) * 1000;
            if (interfaceGraphique.OrdinateurJoueNoir)
                interfaceGraphique.PartieEnCours.BlackElo = interfaceGraphique.EloNoir.Text = baseEloNumerique.Value.ToString();
            else
                interfaceGraphique.PartieEnCours.WhiteElo = interfaceGraphique.EloBlanc.Text = baseEloNumerique.Value.ToString();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
        private void BaseBoutonAnnuler_Click(object sender, EventArgs e)
        {
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
