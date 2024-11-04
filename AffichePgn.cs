using System;
using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public partial class AffichePgn : Form
    {
        private AffichePgn afficheZone;
        private string partieFormatPgnIntl;
        private string partieFormatPgnFr;   
        public AffichePgn()
        {
            InitializeComponent();
            afficheZone = this;
        }
        public void AffichePgnDansZone(string contenuPgnIntl, string contenuPgnFr)
        {
            partieFormatPgnIntl = contenuPgnIntl;
            partieFormatPgnFr = contenuPgnFr;
            ZoneAffichage.Text = partieFormatPgnIntl;
        }
        private void AffichePgnIntl_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            afficheZone.ZoneAffichage.Text = partieFormatPgnIntl;
        }
        private void AffichePgnFr_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            afficheZone.ZoneAffichage.Text = partieFormatPgnFr;
        }
        private void ListeNalAfficheNal_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            string contenuNal = "";
            contenuNal = "Notation Algébrique longue\n\n";
            for (int i = 0; i < LogiqueMouvements.ListeCoupsNal.Count; i++)     // Parcours de la liste des FEN
            {
                contenuNal = contenuNal + LogiqueMouvements.ListeCoupsNal[i];
            }
            afficheZone.ZoneAffichage.Text = contenuNal;
        }
        private void ListeFenAffichePgn_Click(object sender, EventArgs e)
        {
            afficheZone.Show();
            string contenuFen = "";
            contenuFen = "Nombre de 1/2 coups = " + LogiqueMouvements.ListeCoupsFen.Count + "\n";
            for (int i = 0; i < LogiqueMouvements.ListeCoupsFen.Count; i++)     // Parcours de la liste des FEN
            {
                contenuFen = contenuFen + "  [" + i + "]: \"" + LogiqueMouvements.ListeCoupsFen[i] + "\"" + " \n";
            }
            afficheZone.ZoneAffichage.Text = contenuFen;
        }
        private void MasqueAffichePgn_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void AffichePgn_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;  // Annule la fermeture de la fenêtre
            this.Hide();      // Masque la fenêtre au lieu de la fermer; 
        } 
    }
}
