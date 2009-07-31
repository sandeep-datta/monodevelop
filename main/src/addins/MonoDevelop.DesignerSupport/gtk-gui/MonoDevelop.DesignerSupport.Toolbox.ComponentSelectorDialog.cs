// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.DesignerSupport.Toolbox {
    
    
    internal partial class ComponentSelectorDialog {
        
        private Gtk.VBox vbox2;
        
        private Gtk.HBox hbox1;
        
        private Gtk.Label label1;
        
        private Gtk.ComboBox comboType;
        
        private Gtk.VSeparator vseparator1;
        
        private Gtk.Button button24;
        
        private Gtk.ScrolledWindow scrolledwindow1;
        
        private Gtk.TreeView listView;
        
        private Gtk.CheckButton checkGroupByCat;
        
        private Gtk.Button buttonCancel;
        
        private Gtk.Button buttonOk;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget MonoDevelop.DesignerSupport.Toolbox.ComponentSelectorDialog
            this.Name = "MonoDevelop.DesignerSupport.Toolbox.ComponentSelectorDialog";
            this.Title = Mono.Unix.Catalog.GetString("Toolbox Item Selector");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            // Internal child MonoDevelop.DesignerSupport.Toolbox.ComponentSelectorDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Name = "dialog1_VBox";
            w1.BorderWidth = ((uint)(2));
            // Container child dialog1_VBox.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            this.vbox2.BorderWidth = ((uint)(6));
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 6;
            // Container child hbox1.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.Xalign = 0F;
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Type of component:");
            this.hbox1.Add(this.label1);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox1[this.label1]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.comboType = Gtk.ComboBox.NewText();
            this.comboType.Name = "comboType";
            this.hbox1.Add(this.comboType);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.hbox1[this.comboType]));
            w3.Position = 1;
            // Container child hbox1.Gtk.Box+BoxChild
            this.vseparator1 = new Gtk.VSeparator();
            this.vseparator1.Name = "vseparator1";
            this.hbox1.Add(this.vseparator1);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.hbox1[this.vseparator1]));
            w4.Position = 2;
            w4.Expand = false;
            w4.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.button24 = new Gtk.Button();
            this.button24.CanFocus = true;
            this.button24.Name = "button24";
            this.button24.UseUnderline = true;
            // Container child button24.Gtk.Container+ContainerChild
            Gtk.Alignment w5 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w6 = new Gtk.HBox();
            w6.Spacing = 2;
            // Container child GtkHBox1.Gtk.Container+ContainerChild
            Gtk.Image w7 = new Gtk.Image();
            w7.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", Gtk.IconSize.Menu, 16);
            w6.Add(w7);
            // Container child GtkHBox1.Gtk.Container+ContainerChild
            Gtk.Label w9 = new Gtk.Label();
            w9.LabelProp = Mono.Unix.Catalog.GetString("Add Assembly...");
            w9.UseUnderline = true;
            w6.Add(w9);
            w5.Add(w6);
            this.button24.Add(w5);
            this.hbox1.Add(this.button24);
            Gtk.Box.BoxChild w13 = ((Gtk.Box.BoxChild)(this.hbox1[this.button24]));
            w13.Position = 3;
            w13.Expand = false;
            w13.Fill = false;
            this.vbox2.Add(this.hbox1);
            Gtk.Box.BoxChild w14 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
            w14.Position = 0;
            w14.Expand = false;
            w14.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.scrolledwindow1 = new Gtk.ScrolledWindow();
            this.scrolledwindow1.CanFocus = true;
            this.scrolledwindow1.Name = "scrolledwindow1";
            this.scrolledwindow1.ShadowType = ((Gtk.ShadowType)(1));
            // Container child scrolledwindow1.Gtk.Container+ContainerChild
            this.listView = new Gtk.TreeView();
            this.listView.CanFocus = true;
            this.listView.Name = "listView";
            this.scrolledwindow1.Add(this.listView);
            this.vbox2.Add(this.scrolledwindow1);
            Gtk.Box.BoxChild w16 = ((Gtk.Box.BoxChild)(this.vbox2[this.scrolledwindow1]));
            w16.Position = 1;
            // Container child vbox2.Gtk.Box+BoxChild
            this.checkGroupByCat = new Gtk.CheckButton();
            this.checkGroupByCat.CanFocus = true;
            this.checkGroupByCat.Name = "checkGroupByCat";
            this.checkGroupByCat.Label = Mono.Unix.Catalog.GetString("Group by component category");
            this.checkGroupByCat.DrawIndicator = true;
            this.checkGroupByCat.UseUnderline = true;
            this.vbox2.Add(this.checkGroupByCat);
            Gtk.Box.BoxChild w17 = ((Gtk.Box.BoxChild)(this.vbox2[this.checkGroupByCat]));
            w17.Position = 2;
            w17.Expand = false;
            w17.Fill = false;
            w1.Add(this.vbox2);
            Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(w1[this.vbox2]));
            w18.Position = 0;
            // Internal child MonoDevelop.DesignerSupport.Toolbox.ComponentSelectorDialog.ActionArea
            Gtk.HButtonBox w19 = this.ActionArea;
            w19.Name = "dialog1_ActionArea";
            w19.Spacing = 10;
            w19.BorderWidth = ((uint)(5));
            w19.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.buttonCancel = new Gtk.Button();
            this.buttonCancel.CanDefault = true;
            this.buttonCancel.CanFocus = true;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseStock = true;
            this.buttonCancel.UseUnderline = true;
            this.buttonCancel.Label = "gtk-cancel";
            this.AddActionWidget(this.buttonCancel, -6);
            Gtk.ButtonBox.ButtonBoxChild w20 = ((Gtk.ButtonBox.ButtonBoxChild)(w19[this.buttonCancel]));
            w20.Expand = false;
            w20.Fill = false;
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.buttonOk = new Gtk.Button();
            this.buttonOk.CanDefault = true;
            this.buttonOk.CanFocus = true;
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseStock = true;
            this.buttonOk.UseUnderline = true;
            this.buttonOk.Label = "gtk-ok";
            w19.Add(this.buttonOk);
            Gtk.ButtonBox.ButtonBoxChild w21 = ((Gtk.ButtonBox.ButtonBoxChild)(w19[this.buttonOk]));
            w21.Position = 1;
            w21.Expand = false;
            w21.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 642;
            this.DefaultHeight = 433;
            this.Show();
            this.comboType.Changed += new System.EventHandler(this.OnComboTypeChanged);
            this.button24.Clicked += new System.EventHandler(this.OnButton24Clicked);
            this.checkGroupByCat.Clicked += new System.EventHandler(this.OnCheckbutton1Clicked);
            this.buttonOk.Clicked += new System.EventHandler(this.OnButtonOkClicked);
        }
    }
}
