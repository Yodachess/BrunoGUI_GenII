using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class FenetreAide : Form
    {
        public FenetreAide()
        {
            InitializeComponent();
        }

        private void FenetreAide_Load(object sender, EventArgs e)
        {   // Chargement du fichier d'aide
            ContenuAide.LoadFile("AideBrunoGUI.rtf");
            // Ajouter une petite marge à gauche
            ContenuAide.SelectAll();
            ContenuAide.SelectionIndent = 20; // 20 pixels de marge
            ContenuAide.DeselectAll();         // retire la sélection
        }
    }
}
