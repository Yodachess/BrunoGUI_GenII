using System;
using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public partial class ParametresUciStockfish : Form
    {
        public int MultiPV { get; private set; }
        public ParametresUciStockfish()
        {
            InitializeComponent();
            MultiPV = 3;
            this.FormClosing += ParametresUciStockfish_FormClosing;     // Gestion du click sur la croix rouge en haut à droite ...
        }
        private void ParametresUciStockfish_Load(object sender, EventArgs e)
        {   // Affichage des paramêtres dans la console
            MonoMoteurUci.StandardInputDataToUci("uci");
        } 
        private void ClearHashButton_Click(object sender, EventArgs e)
        {   // Traitement du bouton de vidage des hash tables
            MonoMoteurUci.StandardInputDataToUci("setoption name Clear Hash");
        }
        private void ParametresFermer_Click(object sender, EventArgs e)
        {   // Passage au moteur des paramètres sélectionnés 
            MonoMoteurUci.StandardInputDataToUci("setoption name Ponder " + (bool)checkBoxPonder.Checked);
            MonoMoteurUci.StandardInputDataToUci("setoption name Threads value " + (int)ThreadsUpDown.Value);
            MonoMoteurUci.StandardInputDataToUci("setoption name Hash value " + (int)HashSizeUpDown.Value);
            MonoMoteurUci.StandardInputDataToUci("setoption name MultiPV value " + (int)MultiPVUpDown.Value);
            MonoMoteurUci.StandardInputDataToUci("setoption name Skill Level value " + (int)SkillLevelUpDown.Value);
            MonoMoteurUci.StandardInputDataToUci("setoption name Move Overhead value " + (int)MoveOverheadUpDown.Value);
            MonoMoteurUci.StandardInputDataToUci("setoption name nodestime value " + (int)NodesTimeUpDown.Value);
            this.Hide();
        }
        private void ParametresUciStockfish_FormClosing(object sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true; // Annule la fermeture
            this.Hide();      // Masque la fenêtre
        }
    }
}
