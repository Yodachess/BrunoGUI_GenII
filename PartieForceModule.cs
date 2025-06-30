// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Stockfish est développé par Bruno COURTOIS.  Copyright © 2024 █
// █ BrunoGUI_Stockfish est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
using System;
using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public partial class PartieForceModule : Form
    {
        public string ChoixCouleur { get; set; }
        public string NomAdversaire { get; set; }
        public int ForceModule { get; set; }
        public int DureeReflexionSeconde { get; set; }
        public bool ForceMaximale { get; set; }
        public PartieForceModule()
        {
            InitializeComponent();
        }
        private void NouvellePartieForceModule_Load(object sender, EventArgs e)
        {
            ModuleJoueNoirs.Checked = true;     // Les Noirs par défaut
            ForceMoteurMaximum.Checked = true;  // Force Maximum par défaut
            DureeReflexionSeconde = 2000;
            ValeurLimiteElo.Value = 1850;       // Valeur par défaut du ELO si pas max
            NomAdversaire = "Bruno";
            TextBoxNomAdvesaire.Text = NomAdversaire;
            ValeurLimiteElo.Enabled = false;    // Désactiver le NumericUpDown par défaut
            TempsReflexion.Enabled = true;

            // Méthodes séparées pour la gestion des événements
            ForceMoteurMaximum.CheckedChanged += ForceMoteurMaximum_CheckedChanged;
            ForceMoteurDefinie.CheckedChanged += ForceMoteurDefinie_CheckedChanged;
            ForceMoteurOk.Click += ForceMoteurOk_Click;
            ForceMoteurAnnuler.Click += ForceMoteurAnnuler_Click;
        }
        private void ForceMoteurMaximum_CheckedChanged(object sender, EventArgs e)
        {
            ValeurLimiteElo.Enabled = !ForceMoteurMaximum.Checked;
        }
        private void ForceMoteurDefinie_CheckedChanged(object sender, EventArgs e)
        {
            ValeurLimiteElo.Enabled = ForceMoteurDefinie.Checked;
        }
        private void ForceMoteurOk_Click(object sender, EventArgs e)
        {
            ChoixCouleur = ModuleJoueBlancs.Checked ? "Blancs" : "Noirs";
            NomAdversaire = TextBoxNomAdvesaire.Text;
            ForceMaximale = ForceMoteurMaximum.Checked;
            ForceModule = (int)ValeurLimiteElo.Value;
            DureeReflexionSeconde = ((int)TempsReflexion.Value) * 1000;
            Console.WriteLine($"Durée Réflexion =  {DureeReflexionSeconde}");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void ForceMoteurAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
