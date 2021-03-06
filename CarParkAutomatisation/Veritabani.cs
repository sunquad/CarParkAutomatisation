﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using MySql.Data.MySqlClient;

namespace CarParkAutomatisation
{
    public static class Veritabani
    {
        private static string baglantiString = "server=127.0.0.1;uid=root;pwd=120zatura120;database=vtproje";
        private static MySqlConnection baglanti;
        private static MySqlCommand komut;
        private static MySqlDataAdapter adaptor;
        private static MySqlDataReader dataReader;
        private static DataSet dataSet;
        private static string[] uyeOlDizi;
        private static int idTemp;
        private static string sifreTemp;

        //
        private static string sorgu1;
        private static string sorgu2;
        
        // faturagoster icin gerekli degiskenler
        private static string parkyeri;
        
        public static void Baglan()
        {
            try
            {
                baglanti = new MySqlConnection(baglantiString);
                baglanti.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            MessageBox.Show("Veritabanı Bağlantı Durumu: " + baglanti.State);
            
        }

        public static int Personelid // PersonelId için Get Set Property
        {
            get;
            set;
        }

        public static bool PersonelKontrol(string komutString, string[] personelGirisBilgi)
        {
            try
            {
                komut = new MySqlCommand(komutString, baglanti);
                idTemp = Convert.ToInt32(komut.ExecuteScalar());
                if (idTemp != Convert.ToInt32(personelGirisBilgi[0])) return false;
                komutString = "select perSifre from personel where perSifre" + "='" + personelGirisBilgi[1] + "'";
                komut.CommandText = komutString;
                sifreTemp = komut.ExecuteScalar().ToString();
                return sifreTemp==personelGirisBilgi[1];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }
        
        public static bool UyeKontrol(string komutString, string[] uyeGirisBilgi)
        {
            try
            {
                komut = new MySqlCommand(komutString, baglanti);
                idTemp = Convert.ToInt32(komut.ExecuteScalar());
                if (idTemp != Convert.ToInt32(uyeGirisBilgi[0])) return false;
                komutString = "select uyeSifre from uyeler where uyeSifre" + "='" + uyeGirisBilgi[1] + "'";
                komut.CommandText = komutString;
                sifreTemp = komut.ExecuteScalar().ToString();
                return sifreTemp==uyeGirisBilgi[1];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }

        public static int PlakaGetir(string komutString)
        {
            int plakaid = 0;
            
            try
            {
                komut = new MySqlCommand(komutString, baglanti);
                plakaid = Convert.ToInt32(komut.ExecuteScalar());
                return plakaid;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return plakaid;
            }
        }

        public static void PlakaEkle(string komutString, string plaka)
        {
            try
            {
                komut = new MySqlCommand(komutString, baglanti);
                komut.Parameters.Add("@plaka", MySqlDbType.VarChar).Value = plaka.ToUpper();
                komut.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static DataSet Sorgu(string komutString)
        {
            try
            {
                komut = new MySqlCommand(komutString, baglanti);
                adaptor = new MySqlDataAdapter(komut);
                dataSet = new DataSet();
                adaptor.Fill(dataSet, "sorguSonuc");
                return dataSet;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return new DataSet();
            }
            
        }

        public static void KayitOlInsert(string komutString, string parametreler)
        {
            try
            {
                komut = new MySqlCommand(komutString, baglanti);
                uyeOlDizi = parametreler.Split(' ');
                komut.Parameters.Add("@uyeSifre", MySqlDbType.VarChar).Value = uyeOlDizi[0];
                komut.Parameters.Add("@ad", MySqlDbType.VarChar).Value = uyeOlDizi[1];
                komut.Parameters.Add("@soyad", MySqlDbType.VarChar).Value = uyeOlDizi[2];
                komut.Parameters.Add("@telno", MySqlDbType.Int64).Value = Convert.ToInt64(uyeOlDizi[3]);
                komut.Parameters.Add("@uyelikbaslangici", MySqlDbType.DateTime).Value =
                    Convert.ToDateTime(uyeOlDizi[4]+" "+uyeOlDizi[5]);
                komut.Parameters.Add("@plakaId", MySqlDbType.Int32).Value = Convert.ToInt32(uyeOlDizi[6]);
                komut.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.Data);
            }
        }

        public static void Faturalandir(string aracplaka,string yer)
        {
            //plakaId, parkId, girisSaati, ucret
            
            int plaka2 = 0;
            int kisiId = 0;
            double ucretid = 0;
            bool parkyeridurum = true;
            int parkyeridurumint;
            int parkid = 0;
            int cekmeid;
            string sorgu;
            parkyeri = yer;
            
            // plaka id'ye göre max cekmeid al
            //max cekmeid'ye göre parkid al
            //parkid'ye ait olan park yeri durumunu al ve doluysa boşalt
            // Üstteki üç yorum satırı temizlenebilir
            
            //bu fonksiyon, yer seçimi tiklandiginda acilmasi icin her butonun click olayına eklenecektir
            DateTime girisSaati = DateTime.Now;
            sorgu = "select plakaId from plakalar where plaka" + "='"+aracplaka+"'";
            komut = new MySqlCommand(sorgu,baglanti);
            plaka2 = Convert.ToInt32(komut.ExecuteScalar()); // giris yapan aracın plakaId si alindi.
            
            // Önceden çekildiği parkyeri varsa temizleyen kod parçası başlangıcı
            sorgu = "Select * from cekilenaraclar";
            komut = new MySqlCommand(sorgu, baglanti);
            if (komut.ExecuteScalar() != null)
            {
                try
                {
                    komut = new MySqlCommand();
                    komut.Connection = baglanti;
                    sorgu = "select max(cekmeId) from cekilenaraclar where plakaId" + "='" + plaka2 + "'";
                    komut.CommandText = sorgu;
                    cekmeid = Convert.ToInt32(komut.ExecuteScalar());
                    komut.CommandText = "select parkid from cekilenaraclar where cekmeId" + "='" + cekmeid + "'";
                    parkid = Convert.ToInt32(komut.ExecuteScalar());
                    komut.CommandText = "select parkyeridurum from parkyerleri where parkid" + "='" + parkid + "'";
                    parkyeridurumint = Convert.ToByte(komut.ExecuteScalar());
                    if (parkyeridurumint == 1)
                    {
                        komut = new MySqlCommand();
                        komut.Connection = baglanti;
                        komut.CommandText = "update parkyerleri set parkyeridurum=0 where parkId=@parkid";
                        komut.Parameters.Add("@parkid", MySqlDbType.UByte).Value = parkid;
                        komut.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    // Yoksayıldı
                }
            }
            
            // Önceden çekildiği parkyeri varsa temizleyen kod parçası bitişi
            
            komut = new MySqlCommand();
            komut.Connection = baglanti;
            komut.CommandText = "select uyeId from uyeler where plakaId" + "='"+plaka2+"'";
            kisiId = Convert.ToInt32(komut.ExecuteScalar()); // giris yapan arac sahibinin  uyeId si alindi.

            if (kisiId!=0) // kisiye kesilecek ucret belirlendi. 
            {
                komut.CommandText = "select ucretId from ucretlendirmeler where ucretId=1";
                ucretid = Convert.ToInt32(komut.ExecuteScalar());
            }
            else
            {
                komut.CommandText = "select ucretid from ucretlendirmeler where ucretId=2";
                ucretid = Convert.ToInt32(komut.ExecuteScalar());
            }
            
            
            
           
            // park yeri girildi. simdi o park yerine ait id alınacak.
            komut.CommandText = "select parkId from parkyerleri where parkyeri" + "='"+yer+"'";
            parkid = Convert.ToInt32(komut.ExecuteScalar()); // aracın park edildigi id alindi.

            
            
            // toplanan bilgileri o plakaId sayesinde giriscikis tablosuna gir.

            komut.CommandText =
                "insert into giriscikis (plakaId, parkId, girisSaati, ucretId) values(@plakaId, @parkid, @girisSaati, @ucretId);";
            komut.Parameters.Add("@plakaId", MySqlDbType.Int32).Value = plaka2;
            komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = parkid;
            komut.Parameters.Add("@girisSaati", MySqlDbType.DateTime).Value = girisSaati;
            komut.Parameters.Add("@ucretId", MySqlDbType.Int32).Value = ucretid;
            komut.ExecuteNonQuery();
            komut.CommandText = "update parkyerleri set parkyeridurum=@parkyeridurum where parkId=@parkid";
            komut.Parameters.Add("@parkyeridurum", MySqlDbType.TinyBlob).Value = 1;
            komut.ExecuteNonQuery();

        }

        public static void FaturaKes(string sorgu, string aracplaka)
        {
            komut = new MySqlCommand(sorgu,baglanti);
            DateTime girisSaati = Convert.ToDateTime(komut.ExecuteScalar());
            if (girisSaati != DateTime.MinValue)
            {
                DateTime cikisSaati = DateTime.Now; // bu deger alindi, veritabanina girilecek
                int parkSuresi = Convert.ToInt32(cikisSaati.Subtract(girisSaati).TotalMinutes);

                komut.CommandText = "select plakaId from plakalar where plaka=@aracplaka";
                komut.Parameters.Add("@aracplaka", MySqlDbType.VarChar).Value = aracplaka;
                int aracplakaid = Convert.ToInt32(komut.ExecuteScalar());
                komut.CommandText = "select max(faturaId) from girisCikis where plakaId=@aracplakaid";
                komut.Parameters.Add("@aracplakaid", MySqlDbType.Int32).Value = aracplakaid;
                string maxfaturaid = Convert.ToString(komut.ExecuteScalar());
                komut = new MySqlCommand();
                komut.Connection = baglanti;
                komut.CommandText =
                    "select parkid from giriscikis where faturaId=@maxfaturaid";
                komut.Parameters.Add("@maxfaturaid", MySqlDbType.VarChar).Value = maxfaturaid;
                komut.Parameters.Add("@aracplakaid", MySqlDbType.Int32).Value = aracplakaid;
                int parkid = Convert.ToInt32(komut.ExecuteScalar());
                komut.CommandText = "update parkyerleri set parkyeridurum=@parkyeridurum where parkId=@parkid";
                komut.Parameters.Add("@parkyeridurum", MySqlDbType.TinyBlob).Value = 0;
                komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = parkid;
                komut.ExecuteNonQuery();
                
                komut.CommandText = "update girisCikis set cikisSaati=@cikisSaati, parkSuresi=@toplamParkSuresi where faturaId=@maxfaturaid";
                komut.Parameters.Add("@cikisSaati", MySqlDbType.DateTime).Value = cikisSaati;
                komut.Parameters.Add("@toplamParkSuresi", MySqlDbType.Double).Value = parkSuresi;
                //komut.Parameters.Add("@maxfaturaid", MySqlDbType.VarChar).Value = maxfaturaid;
                komut.ExecuteNonQuery();
                 
                komut.CommandText =
                     "select ucret from ucretlendirmeler,girisCikis where girisCikis.faturaId=(select max(faturaId) from girisCikis where plakaId=@aracplakaid and girisCikis.ucretId=ucretlendirmeler.ucretId)";
                double tarife = Convert.ToDouble(komut.ExecuteScalar());
                double ucret = Convert.ToDouble(parkSuresi) / 60 * tarife;
                if (parkSuresi%60>30)
                {
                     ucret += Convert.ToInt32(tarife / 2);
                }
                komut = new MySqlCommand();
                komut.Connection = baglanti;
                komut.CommandText = "select parkyeri from parkyerleri where parkId=@parkid";
                komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = parkid;
                parkyeri = Convert.ToString(komut.ExecuteScalar());
                FaturaGoster faturaGoster = new FaturaGoster(aracplaka,parkyeri,girisSaati,cikisSaati, parkSuresi,ucret);
                faturaGoster.Show();
            }

        }

        public static void AraciCek(string sorgu,int cekilenid,int parkid, int eskiparkid, string sorgu2)
        {
            // Düzenlenen son park yerlerinden vesaire emin olmak için ve çekilmeden önceki park yerini boş hale getiriyor ve yeni park yerini dolu hale getiriyor
            DateTime cekmeSaati = DateTime.Now;
            komut = new MySqlCommand(sorgu2, baglanti);
            string maxfaturaid = Convert.ToString(komut.ExecuteScalar());
            komut.CommandText = "select parkid from giriscikis where faturaId" + "='" + maxfaturaid + "'";
            eskiparkid = Convert.ToInt32(komut.ExecuteScalar());
            komut = new MySqlCommand(sorgu,baglanti);
            komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = parkid;
            komut.Parameters.Add("@faturaid", MySqlDbType.Int32).Value = maxfaturaid;
            komut.ExecuteNonQuery();
            sorgu = "update parkyerleri set parkyeridurum=@parkyeridurum where parkId=@parkid";
            komut = new MySqlCommand(sorgu, baglanti);
            komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = parkid;
            komut.Parameters.Add("@parkyeridurum", MySqlDbType.UByte).Value = 1;
            komut.ExecuteNonQuery();
            sorgu =
                "insert into cekilenaraclar (parkId, plakaId, perId, cekilmeTarihi) values (@parkid, @cekilenid, @perid, @cekilmetarihi)";
            komut = new MySqlCommand(sorgu, baglanti);
            komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = parkid;
            komut.Parameters.Add("@cekilenid", MySqlDbType.Int32).Value = cekilenid;
            komut.Parameters.Add("@perid", MySqlDbType.Int32).Value = Personelid;
            komut.Parameters.Add("@cekilmetarihi", MySqlDbType.DateTime).Value = cekmeSaati;
            komut.ExecuteNonQuery();
            sorgu = "update parkyerleri set parkyeridurum=@parkyeridurum where parkId=@parkid";
            komut = new MySqlCommand(sorgu, baglanti);
            komut.Parameters.Add("@parkid", MySqlDbType.Int32).Value = eskiparkid;
            komut.Parameters.Add("@parkyeridurum", MySqlDbType.UByte).Value = 0;
            komut.ExecuteNonQuery();

        }

        public static int ParkIdGetir(string sorgu)
        {
            komut = new MySqlCommand(sorgu,baglanti);
            return Convert.ToInt32(komut.ExecuteScalar());

        }
        

        

    }
}