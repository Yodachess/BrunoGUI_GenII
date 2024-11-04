using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public partial class DonneesBrutesUci : Form
    {
        public DonneesBrutesUci()
        {
            InitializeComponent();
            this.FormClosing += DonneesBrutesUci_FormClosing;      // Gestion du click sur la croix rouge en haut à droite ...
        }
        private void DonneesBrutesUci_FormClosing(object sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true;
            this.Hide();
        }
    }
}
