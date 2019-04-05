using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinqToSql1
{
    /*
    Ekle butonuna basıldığında textBoxlara girilern değerleri kayıt olarak datagridview'a ekleyecek.
    Sil butonuna basıldığında datagridwiew'daki seçili kaydı silecek.
    Kaydet butonuna basıldığında textBoxta yazan değerlere göre datagridviewda seçii satırda update yapacak.
    Ara butonuna basıldığında ProductName'e göre arama yapacak  
    */
    public partial class Form1 : Form
    {
        NorthWindDataContext ctx = new NorthWindDataContext();
        //Facadelara ulaşabilmemiz için NorthWindDataContext'den instance almamız lazım.
        private void ListProducts()
        {
           
            dataGridView1.DataSource = ctx.Products;

            comboKategori.DisplayMember = "CategoryName";
            //Asıl görünmesini istediğim DisplayMember
            comboKategori.ValueMember = "CategoryID";
            //DisplayMember'ı tekilleştiren ValueMember
            comboKategori.DataSource = ctx.Categories;
            //ComboKategori dolduruldu

            comboTedarikci.DisplayMember = "CompanyName";
            comboTedarikci.ValueMember = "SupplierID";
            comboTedarikci.DataSource = ctx.Suppliers;
            //ComboTedarikci dolduruldu

            //---------------------linq expression ile joinleme yaptık--------------
            var query = from prod in ctx.Products
                        join cat in ctx.Categories
                        on prod.CategoryID equals cat.CategoryID

                        join sup in ctx.Suppliers
                        on prod.SupplierID equals sup.SupplierID

                        select new
                        {
                            prod.ProductID,      //anonymous type
                            prod.ProductName,
                            prod.UnitPrice,
                            prod.UnitsInStock,
                            cat.CategoryName,
                            sup.CompanyName,
                            cat.CategoryID,
                            sup.SupplierID
                        };
  //Burda özellikle CategoryID ve SupplierID bana ekleme ve silme butonlarında lazım olacak. Ekleme ve silme işlemlerini bunların IDlerine göre yapıcaz.
//----------------------------------------------------------------------------------------
            
            dataGridView1.DataSource = query;
            //anonymous type'da eklediğimiz alanları datagridview kaynağı olarak ekledik.

            dataGridView1.Columns["SupplierID"].Visible = false;
            dataGridView1.Columns["CategoryID"].Visible = false;
            //Görünürlüğünü kapattık.




        }
        public Form1()
        {
            InitializeComponent();
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            Product p = new Product();
            p.ProductName = txtProductName.Text;
            p.UnitPrice = nudFiyat.Value;
            p.UnitsInStock = Convert.ToInt16(nudStok.Value);
            //Databasedeki unitsInStock'un veri tipi smallint olduğu için Int16.
            p.CategoryID = (int)comboKategori.SelectedValue;
            p.SupplierID = (int)comboTedarikci.SelectedValue;

            ctx.Products.InsertOnSubmit(p);
            //yapılan eklemeyi context'e ekler.
            MessageBox.Show($"SubmitChanges öncesi ProductID={p.ProductID}");
            ctx.SubmitChanges();//değişiklikleri ADO.NET koduna çevirerek database'e yollar.
            //database'e; SubmitChanges'ten sonra  insert eder.
            MessageBox.Show($"SubmitChanges sonrası ProductID={p.ProductID}");
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow==null)
                //currentRow seçili satır
            {
                return;
                // datagridViewda seçtiği satır boşsa yani herhangi bir satır seçilmediyse işlem yapmaz.
            }
            //Önce Context'ten sil, sonra onayla(veritabanından sil)
            int urunID = (int)dataGridView1.CurrentRow.Cells["ProductID"].Value;

            Product p = ctx.Products.SingleOrDefault(urun => urun.ProductID == urunID);
            //SingleOrDefault içine lambda expression yazıyoruz.
            //döndürdüğü şey Product entity o yüzden (Product p=) tanımladık.
            ctx.Products.DeleteOnSubmit(p);
            //yapılan silmeyi Context'tne siler.
            ctx.SubmitChanges();
            //yapılan silme işlemine database'ten siler.

        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            Product p = ctx.Products.SingleOrDefault(urun => urun.ProductID == (int)txtProductName.Tag);
            p.ProductName = txtProductName.Text;
            p.UnitPrice = nudFiyat.Value;
            p.UnitsInStock = Convert.ToInt16(nudStok.Value);
            p.CategoryID = (int)comboKategori.SelectedValue;
            p.SupplierID = (int)comboTedarikci.SelectedValue;

            ctx.SubmitChanges();
            ListProducts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListProducts();
        }

        private void txtAra_TextChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = ctx.Products.Where(x => x.ProductName.Contains(txtAra.Text));
            //Filtreleme yapmamızı sağlar.
            //ProductName'inin içeriği txtAra ile aynı olan ürünleri dataGridView'da göster.
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow r = dataGridView1.CurrentRow;
            txtProductName.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            nudFiyat.Value = Convert.ToDecimal(dataGridView1.CurrentRow.Cells[2].Value);
            nudStok.Value=Convert.ToDecimal(dataGridView1.CurrentRow.Cells[3].Value);
           // comboKategori.SelectedValue = (int)r.Cells["CategoryID"].Value;
            comboTedarikci.SelectedValue = (int)r.Cells["SupplierID"].Value;
            comboKategori.SelectedValue = Convert.ToInt32(dataGridView1.CurrentRow.Cells["CategoryID"].Value);
            txtProductName.Tag = r.Cells["ProductID"].Value;
            //txtProductName'in tag'ine ProductIDyi sakladık.


            /*dataGridView propertiestan CellClick eventi açılcak bu satıra tıkladığımız zaman
             * satırı komple seçmemizi sağlar.
             * 
             * CellClick eventi içinde herhangi bir satıra tıkladığımızda buradaki ProductName,
             * 
             * */

        }
    }
}
