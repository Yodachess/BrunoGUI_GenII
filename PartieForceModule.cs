// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'édition pour nouvellle partie contre Stockfish ...
//      └─ Classe "PartieForceModule" 
//                      ├─ "PartieForceModule"     (Init)
//                      ├─ "NouvellePartieForceModule_Load"
//                      ├─ "ForceMoteurMaximum_CheckedChanged"  
//                      ├─ "ForceMoteurDefinie_CheckedChanged"  
//                      ├─ "ForceMoteurOk_Click"  
//                      └─ "ForceMoteurAnnuler_Click"

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace BrunoGUI_GenII
{
    public partial class PartieForceModule : Form
    {
        // Choix proposés à l'ouverture de la fenêtre (préférences du .ini, puis derniers choix), et choix validés
        public LogiqueMouvements.ColorPiece ChoixCouleur { get; set; } = LogiqueMouvements.ColorPiece.Noir;    // camp joué par le moteur
        public string NomAdversaire { get; set; } = "Bruno";    // nom du joueur humain
        public int ForceModule { get; set; } = 1850;            // Elo du moteur si pas force maximale
        public int DureeReflexionSeconde { get; set; } = 5;
        public bool ForceMaximale { get; set; } = true;
        public Cadence ChoixCadence { get; set; } = Cadence.SansPendule;    // pendule de la partie (sans : temps fixe par coup)

        public PartieForceModule()
        {
            InitializeComponent();
            foreach (Cadence cadence in Cadence.Proposees)
                ListePendule.Items.Add(cadence);
            ListePendule.DrawMode = DrawMode.OwnerDrawFixed;    // étoile dorée des cadences officielles
            ListePendule.DrawItem += DessineCadence;
            this.VisibleChanged += (s, e) => { if (Visible) AfficheChoix(); };
        }
        public static void DessineCadence(object? sender, DrawItemEventArgs e)
        {   // Liste des cadences (ici et sous l'échiquier) dessinée à la main : une étoile dorée devant les cadences officielles
            // de la FIDE (3 + 2, 15 + 10, 90 + 30) ; les autres sont décalées d'autant, pour que les noms restent alignés
            e.DrawBackground();
            ComboBox? liste = sender as ComboBox ?? (sender as Krypton.Toolkit.KryptonComboBox)?.ComboBox;
            if (e.Index < 0 || liste == null || e.Index >= liste.Items.Count)
                return;
            object? element = liste.Items[e.Index];
            Font police = e.Font ?? liste.Font;
            bool selectionne = (e.State & DrawItemState.Selected) != 0;
            Color couleurTexte = selectionne ? SystemColors.HighlightText : SystemColors.WindowText;
            int largeurEtoile = TextRenderer.MeasureText("★", police).Width;
            if (element is Cadence { EstOfficielle: true })
                TextRenderer.DrawText(e.Graphics, "★", police, new Rectangle(e.Bounds.X, e.Bounds.Y, largeurEtoile, e.Bounds.Height),
                    Color.FromArgb(212, 160, 23), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(e.Graphics, element?.ToString() ?? "", police,
                new Rectangle(e.Bounds.X + largeurEtoile, e.Bounds.Y, e.Bounds.Width - largeurEtoile, e.Bounds.Height), couleurTexte,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            e.DrawFocusRectangle();
        }
        private void ListePendule_SelectedIndexChanged(object? sender, EventArgs e)
        {   // La durée de réflexion par coup ne sert que sans pendule (avec une pendule, le moteur gère son temps)
            TempsReflexion.Enabled = ListePendule.SelectedItem is not Cadence cadence || cadence.EstSansPendule;
        }
        private void AfficheChoix()
        {   // Met les contrôles de la fenêtre aux valeurs des propriétés
            ModuleJoueBlancs.Checked = ChoixCouleur == LogiqueMouvements.ColorPiece.Blanc;
            ModuleJoueNoirs.Checked = ChoixCouleur != LogiqueMouvements.ColorPiece.Blanc;
            ForceMoteurMaximum.Checked = ForceMaximale;
            ForceMoteurDefinie.Checked = !ForceMaximale;
            ValeurLimiteElo.Value = Math.Clamp(ForceModule, (int)ValeurLimiteElo.Minimum, (int)ValeurLimiteElo.Maximum);
            ValeurLimiteElo.Enabled = !ForceMaximale;
            TempsReflexion.Value = Math.Clamp(DureeReflexionSeconde, (int)TempsReflexion.Minimum, (int)TempsReflexion.Maximum);
            TextBoxNomAdvesaire.Text = NomAdversaire;
            if (!ListePendule.Items.Contains(ChoixCadence))
                ListePendule.Items.Add(ChoixCadence);       // cadence du .ini absente de la liste proposée
            ListePendule.SelectedItem = ChoixCadence;
            ListePendule_SelectedIndexChanged(ListePendule, EventArgs.Empty);
        }
        private void NouvellePartieForceModule_Load(object? sender, EventArgs e)
        {
            AfficheChoix();

            // Méthodes séparées pour la gestion des événements
            ForceMoteurMaximum.CheckedChanged += ForceMoteurMaximum_CheckedChanged;
            ForceMoteurDefinie.CheckedChanged += ForceMoteurDefinie_CheckedChanged;
            ForceMoteurOk.Click += ForceMoteurOk_Click;
            ForceMoteurAnnuler.Click += ForceMoteurAnnuler_Click;
        }
        private void ForceMoteurMaximum_CheckedChanged(object? sender, EventArgs e)
        {
            ValeurLimiteElo.Enabled = !ForceMoteurMaximum.Checked;
        }
        private void ForceMoteurDefinie_CheckedChanged(object? sender, EventArgs e)
        {
            ValeurLimiteElo.Enabled = ForceMoteurDefinie.Checked;
        }
        private void ForceMoteurOk_Click(object? sender, EventArgs e)
        {
            ChoixCouleur = ModuleJoueBlancs.Checked ? LogiqueMouvements.ColorPiece.Blanc : LogiqueMouvements.ColorPiece.Noir;
            NomAdversaire = TextBoxNomAdvesaire.Text;
            ForceMaximale = ForceMoteurMaximum.Checked;
            ForceModule = (int)ValeurLimiteElo.Value;
            DureeReflexionSeconde = ((int)TempsReflexion.Value);
            if (ListePendule.SelectedItem is Cadence cadence)
                ChoixCadence = cadence;
            Debug.WriteLine($"ForceMoteurOk_Click / Durée Réflexion secondes =  {DureeReflexionSeconde}");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void ForceMoteurAnnuler_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
