﻿using System;
using System.Windows.Forms;

namespace CarParkAutomatisation
{
    public partial class FaturaGoster : Form
    {
        public FaturaGoster()
        {
            InitializeComponent();
        }
        
        public FaturaGoster(string aracplaka, string yeri, DateTime gsaati, DateTime csaati, int parksuresi, double toplamucret)
        {
            InitializeComponent();
            plakaLabel.Text += @" "+aracplaka;
            parkLabel.Text += @" " + yeri;
            girisLabel.Text += @" " + gsaati;
            cikisLabel.Text += @" " + csaati;
            sureLabel.Text += @" " + parksuresi + " Dakika";
            ucretLabel.Text += @" " + toplamucret.ToString("F") + " TL";
            
        }  
            

    }
}