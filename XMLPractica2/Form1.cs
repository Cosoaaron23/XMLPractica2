using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace XMLPractica2 // <-- ¡Recuerda usar el tuyo!
{
    public partial class Form1 : Form
    {
        private DataGridView miGrid;
        private XmlDocument doc;
        private string rutaArchivo;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Gestor de Usuarios (XML)";
            this.Size = new Size(480, 500);
            this.MinimumSize = new Size(480, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.FromArgb(245, 245, 245);

            try
            {
                this.doc = new XmlDocument();
                this.rutaArchivo = Path.Combine(Application.StartupPath, "Interfaz.xml");
                this.doc.Load(this.rutaArchivo);

                GenerarMenu();
                XmlNodeList nodosControl = this.doc.SelectNodes("/Interfaz/Controles/Control");
                GenerarControles(nodosControl, this);
                CargarDatosEnGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fatal al cargar la interfaz: " + ex.Message);
            }
        }

        private void CargarDatosEnGrid()
        {
            if (this.miGrid == null) return;
            XmlNodeList listaUsuarios = this.doc.SelectNodes("/Interfaz/Datos/Usuarios/Usuario");
            foreach (XmlNode usuario in listaUsuarios)
            {
                this.miGrid.Rows.Add(
                    usuario.Attributes["id"].Value,
                    usuario.Attributes["nombre"].Value,
                    usuario.Attributes["email"].Value
                );
            }
        }

        private void GenerarMenu()
        {
            MenuStrip menuPrincipal = new MenuStrip();
            menuPrincipal.Dock = DockStyle.Top;

            XmlNodeList itemsNivel1 = this.doc.SelectNodes("/Interfaz/MenuPrincipal/MenuItem");
            PoblarMenuItems(itemsNivel1, menuPrincipal.Items);
            this.Controls.Add(menuPrincipal);
        }

        private void PoblarMenuItems(XmlNodeList nodos, ToolStripItemCollection parentCollection)
        {
            foreach (XmlNode nodo in nodos)
            {
                string textoItem = nodo.Attributes["texto"].Value;
                if (textoItem == "-")
                {
                    parentCollection.Add(new ToolStripSeparator());
                    continue;
                }
                ToolStripMenuItem nuevoItem = new ToolStripMenuItem(textoItem);
                nuevoItem.Click += MiManejadorMenuClick;
                parentCollection.Add(nuevoItem);
                XmlNodeList hijos = nodo.SelectNodes("MenuItem");
                if (hijos.Count > 0)
                {
                    PoblarMenuItems(hijos, nuevoItem.DropDownItems);
                }
            }
        }

        private void GenerarControles(XmlNodeList nodos, Control parentContainer)
        {
            foreach (XmlNode nodo in nodos)
            {
                string tipo = nodo["tipo"].InnerText;
                string nombre = nodo["nombre"].InnerText;
                string texto = nodo["texto"].InnerText;
                int posX = int.Parse(nodo["posicionX"].InnerText);
                int posY = int.Parse(nodo["posicionY"].InnerText);
                int ancho = int.Parse(nodo["ancho"].InnerText);
                int alto = int.Parse(nodo["alto"].InnerText);

                Control nuevoControl = null;

                switch (tipo)
                {
                    case "Label":
                        Label newLabel = new Label();
                        if (nombre == "lblBienvenida")
                        {
                            newLabel.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
                        }
                        nuevoControl = newLabel;
                        break;

                    case "Button":
                        Button newButton = new Button();
                        newButton.Click += MiManejadorDeAccionesClick;
                        newButton.FlatStyle = FlatStyle.Flat;
                        newButton.FlatAppearance.BorderSize = 0;
                        nuevoControl = newButton;
                        break;

                    case "Panel":
                        Panel newPanel = new Panel();
                        if (nombre == "pnlButtonContainer")
                        {
                            newPanel.Anchor = AnchorStyles.Top;
                            parentContainer.Resize += (s, e) => {
                                newPanel.Left = (parentContainer.ClientSize.Width - newPanel.Width) / 2;
                            };
                            newPanel.Left = (parentContainer.ClientSize.Width - newPanel.Width) / 2;
                        }
                        else
                        {
                            newPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        }
                        nuevoControl = newPanel;
                        break;

                    case "GroupBox":
                        GroupBox newGroup = new GroupBox();
                        newGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                        nuevoControl = newGroup;
                        break;

                    case "DataGridView":
                        DataGridView newGrid = new DataGridView();
                        newGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                        newGrid.MultiSelect = false;
                        newGrid.AllowUserToAddRows = false;
                        newGrid.AllowUserToDeleteRows = false;

                        // --- MODIFICACIÓN 1: Permitir la edición ---
                        newGrid.ReadOnly = false;

                        newGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                        newGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        newGrid.BackgroundColor = Color.White;
                        newGrid.BorderStyle = BorderStyle.None;
                        newGrid.EnableHeadersVisualStyles = false;
                        newGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 70, 70);
                        newGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                        newGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                        newGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
                        newGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
                        newGrid.RowsDefaultCellStyle.BackColor = Color.White;
                        newGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                        newGrid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 225, 255);
                        newGrid.RowsDefaultCellStyle.SelectionForeColor = Color.Black;
                        newGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                        newGrid.RowHeadersVisible = false;

                        foreach (XmlNode colNodo in nodo.SelectNodes("Columnas/Columna"))
                        {
                            string colNombre = colNodo.Attributes["nombre"].Value;
                            string colHeader = colNodo.Attributes["textoHeader"].Value;
                            newGrid.Columns.Add(colNombre, colHeader);
                            if (colNombre == "colID") newGrid.Columns[colNombre].FillWeight = 15;
                            else if (colNombre == "colNombre") newGrid.Columns[colNombre].FillWeight = 35;
                            else if (colNombre == "colEmail") newGrid.Columns[colNombre].FillWeight = 50;
                        }

                        // --- MODIFICACIÓN 2: Proteger la columna ID ---
                        if (newGrid.Columns["colID"] != null)
                        {
                            newGrid.Columns["colID"].ReadOnly = true;
                            // Poner un fondo gris para que se note que no es editable
                            newGrid.Columns["colID"].DefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
                            newGrid.Columns["colID"].DefaultCellStyle.ForeColor = Color.Black;
                            newGrid.Columns["colID"].DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 210, 210);
                        }

                        this.miGrid = newGrid;
                        nuevoControl = newGrid;
                        break;
                }

                if (nuevoControl == null) continue;
                nuevoControl.Name = nombre;
                nuevoControl.Text = texto;
                nuevoControl.Location = new Point(posX, posY);
                nuevoControl.Size = new Size(ancho, alto);
                if (tipo != "DataGridView" && tipo != "Panel" && tipo != "GroupBox")
                {
                    nuevoControl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                }
                try
                {
                    if (nodo["colorFondo"] != null) nuevoControl.BackColor = ColorTranslator.FromHtml(nodo["colorFondo"].InnerText);
                    if (nodo["colorTexto"] != null) nuevoControl.ForeColor = ColorTranslator.FromHtml(nodo["colorTexto"].InnerText);
                }
                catch (Exception) { /* Ignorar colores inválidos */ }

                parentContainer.Controls.Add(nuevoControl);
                XmlNodeList hijos = nodo.SelectNodes("ControlesHijos/Control");
                if (hijos.Count > 0)
                {
                    GenerarControles(hijos, nuevoControl);
                }
            }
        }

        private string ObtenerSiguienteID()
        {
            int maxID = 0;
            foreach (DataGridViewRow fila in this.miGrid.Rows)
            {
                if (fila.Cells["colID"].Value != null)
                {
                    if (int.TryParse(fila.Cells["colID"].Value.ToString(), out int idActual))
                    {
                        if (idActual > maxID) maxID = idActual;
                    }
                }
            }
            return (maxID + 1).ToString();
        }

        private void MiManejadorDeAccionesClick(object sender, EventArgs e)
        {
            if (this.miGrid == null) return;
            Button botonPresionado = (Button)sender;

            switch (botonPresionado.Name)
            {
                case "btnAnadir":
                    string nuevoID = ObtenerSiguienteID();
                    this.miGrid.Rows.Add(nuevoID, "Nuevo Usuario", "email@nuevo.com");
                    break;

                // --- MODIFICACIÓN 3: Cambiar la lógica del botón "Editar" ---
                case "btnEditar":
                    MessageBox.Show("Puedes editar el Nombre y el Correo directamente en la tabla.\n\nEl ID no se puede modificar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;

                case "btnEliminar":
                    if (this.miGrid.SelectedRows.Count > 0)
                    {
                        string nombre = this.miGrid.SelectedRows[0].Cells["colNombre"].Value.ToString();
                        if (MessageBox.Show($"¿Seguro que quieres eliminar a {nombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            this.miGrid.Rows.Remove(this.miGrid.SelectedRows[0]);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Seleccione una fila para eliminar.");
                    }
                    break;
            }
        }

        private void GuardarDatosEnXML()
        {
            try
            {
                XmlNode nodoUsuarios = this.doc.SelectSingleNode("/Interfaz/Datos/Usuarios");
                if (nodoUsuarios == null) return;

                nodoUsuarios.RemoveAll();

                foreach (DataGridViewRow fila in this.miGrid.Rows)
                {
                    if (fila.IsNewRow) continue;

                    XmlElement nuevoUsuario = this.doc.CreateElement("Usuario");

                    string id = fila.Cells["colID"].Value?.ToString() ?? "0";
                    string nombre = fila.Cells["colNombre"].Value?.ToString() ?? "";
                    string email = fila.Cells["colEmail"].Value?.ToString() ?? "";

                    nuevoUsuario.SetAttribute("id", id);
                    nuevoUsuario.SetAttribute("nombre", nombre);
                    nuevoUsuario.SetAttribute("email", email);

                    nodoUsuarios.AppendChild(nuevoUsuario);
                }

                this.doc.Save(this.rutaArchivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar los datos en el XML: " + ex.Message, "Error de Guardado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GuardarDatosEnXML();
        }

        private void MiManejadorMenuClick(object sender, EventArgs e)
        {
            ToolStripMenuItem itemPresionado = (ToolStripMenuItem)sender;

            if (itemPresionado.Text == "Salir")
            {
                this.Close();
            }
            else if (itemPresionado.Text == "Guardar")
            {
                GuardarDatosEnXML();
                MessageBox.Show("¡Datos guardados con éxito!", "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Has hecho clic en: " + itemPresionado.Text);
            }
        }
    }
}