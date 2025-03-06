using FormsTimer = System.Windows.Forms.Timer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using CONTROL_APIS.FORMULARIO;
using System.Drawing.Drawing2D;
using YamlDotNet.Serialization;
using System.Management;
using System.Text;
using System.Collections.Concurrent;

namespace CONTROL_APIS
{
    public partial class Form1 : Form {

        private readonly ConcurrentDictionary<string, Process> procesosAPIs = new ConcurrentDictionary<string, Process>();        // Cambiado a string para manejar nombres de scripts
        private readonly FormsTimer monitorTimer;
        private static readonly string botToken = "7786680952:AAG1epvu8kKvEeocyEDZDof9RLtOmjhNO4I"; // Token de tu bot      
        private static readonly string chatId = "880483483"; // ID del chat de destino
        private bool isVerificando = false;
        private System.Timers.Timer debounceTimer;

        public Form1() {
            InitializeComponent();
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0";
            labelversion.Text = $"Control de APIS - INNOVATED {version}";
            monitorTimer = new FormsTimer { Interval = 3000 };
            monitorTimer.Tick += async (sender, e) => await VerificarTodosLosEstados();
            monitorTimer.Start();
            MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
            EstilizarTabControl(tabControl1);            
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //await SendMessageAsync(message: "HOLA DESDE EL PROYECTO DE CONTROL APIS");
            // Cargar proyectos y controles básicos
            var proyectos = ObtenerProyectos();
            CargarProyectosInicial(proyectos);
            // Verificar el estado de las APIs en segundo plano
            await VerificarTodosLosEstados();
            SetupYamlFileWatcher();            
        }

        public static async Task SendMessageAsync(string message)
        {

            using (var client = new HttpClient())
            {
                // Construir la URL de la API
                string url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                // Crear el cuerpo de la solicitud en formato JSON
                var content = new StringContent($"{{\"chat_id\": \"{chatId}\", \"text\": \"{message}\", \"parse_mode\": \"Markdown\"}}", Encoding.UTF8, "application/json");

                // Enviar la solicitud POST
                var response = await client.PostAsync(url, content);

                // Verificar la respuesta
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Mensaje enviado con éxito.");
                }
                else
                {
                    Console.WriteLine($"Error al enviar el mensaje: {response.StatusCode}");
                }
            }
        }

        private void CargarProyectosInicial(List<Proyecto> proyectos)
        {
            tabControl1.SuspendLayout();

            foreach (var proyecto in proyectos)
            {
                var tabPage = new TabPage(proyecto.nombre);
                var panel = new Panel
                {
                    Size = new Size(800, 600),
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(10, 50, 10, 10)
                };

                var btnIniciarDetener = new Button
                {
                    Text = "Iniciar Todas las APIs",
                    Size = new Size(170, 40),
                    BackColor = Color.Green,
                    ForeColor = Color.White,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Location = new Point(panel.Width - 180, 5),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };

                btnIniciarDetener.FlatAppearance.BorderSize = 0;
              
                btnIniciarDetener.Click += async (s, e) =>
                {
                    btnIniciarDetener.Enabled = false;
                    using (var loadingForm = new loading())
                    {
                        // Mostrar formulario de carga (de manera NO modal)
                        loadingForm.Show(this);

                        try
                        {
                            if (btnIniciarDetener.Text.StartsWith("Iniciar"))
                            {
                                // Envías la lista de apis a un hilo en segundo plano
                                await Task.Run(async () =>
                                {
                                    await IniciarTodasLasApis(proyecto.apis);
                                });
                            }
                            else
                            {
                                await Task.Run(async () =>
                                {
                                    monitorTimer.Stop();
                                    await DetenerTodasLasApis(proyecto.apis);
                                    monitorTimer.Start();
                                });
                            }
                        } //catch
                        finally
                        {
                            loadingForm.Close();
                            btnIniciarDetener.Enabled = true;

                            // Actualizas en la UI
                            await ActualizarEstadoBoton(btnIniciarDetener, proyecto.apis);
                        }
                    }
                };



                panel.Controls.Add(btnIniciarDetener);

                var dgv = CrearDataGridViewEstilo();
                ConfigurarColumnasDataGridView(dgv);
                //AgregarBotonVerConsola(dgv);
                dgv.DataSource = proyecto.apis;
                dgv.Dock = DockStyle.Fill;
                dgv.Margin = new Padding(0, 50, 0, 0);

                panel.Controls.Add(dgv);
                tabPage.Controls.Add(panel);
                tabControl1.TabPages.Add(tabPage);
            }

            tabControl1.ResumeLayout();
        }

        private List<Proyecto> ObtenerProyectos()
        {
            try
            {
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string resourcesPath = Path.Combine(exePath, "Resources");
                string filePath = Path.Combine(resourcesPath, "listaApis.yaml");

                // 🔍 Depuración: Ver la ruta en tiempo de ejecución
                string yamlContent = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder().Build();
                var proyectos = deserializer.Deserialize<Dictionary<string, List<Proyecto>>>(yamlContent);

                return proyectos["proyectos"];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Proyecto>();
            }
        }

        private void EstilizarTabControl(TabControl tabControl)
        {
            // Configuración básica
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            tabControl.ItemSize = new Size(120, 30);
            tabControl.Padding = new Point(10, 2);
            tabControl.BackColor = Color.White;
            tabControl.Appearance = TabAppearance.FlatButtons;

            tabControl.DrawItem += (sender, e) =>
            {
                var g = e.Graphics;
                var tabRect = tabControl.GetTabRect(e.Index);
                bool isSelected = tabControl.SelectedIndex == e.Index;
                var tabPage = tabControl.TabPages[e.Index];

                // Fondo del tab
                using (var backBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(backBrush, tabRect);
                }

                // Dibujar círculo indicador
                bool todasActivas = tabPage.Tag is bool && (bool)tabPage.Tag;
                Color colorCirculo = todasActivas ? Color.FromArgb(13, 174, 117) : Color.FromArgb(244, 67, 54);

                int diametro = 10;
                int espacio = 5;
                Rectangle circuloRect = new Rectangle(
                    tabRect.Left + espacio,
                    tabRect.Top + (tabRect.Height - diametro) / 2,
                    diametro,
                    diametro
                );

                using (var brush = new SolidBrush(colorCirculo))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(brush, circuloRect);
                }

                // Ajustar posición del texto
                Rectangle textoRect = new Rectangle(
                    tabRect.Left + espacio + diametro + espacio,
                    tabRect.Top,
                    tabRect.Width - (espacio * 2 + diametro),
                    tabRect.Height
                );

                Color textColor = Color.Black;
                TextRenderer.DrawText(
                    g,
                    tabPage.Text,
                    new Font("Segoe UI", 9, FontStyle.Bold),
                    textoRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            };

            foreach (TabPage tabPage in tabControl.TabPages)
            {
                tabPage.BackColor = Color.White;
            }
        }

        private DataGridView CrearDataGridViewEstilo() => new DataGridView()
        {
            BackgroundColor = Color.White,
            Dock = DockStyle.Fill,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            ReadOnly = true,
            RowTemplate = { Height = 38 },
            EnableHeadersVisualStyles = false,
            BorderStyle = BorderStyle.None,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(243, 243, 243),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
            },
            ColumnHeadersHeight = 35,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,

            Font = new Font("Segoe UI", 10),
            CellBorderStyle = DataGridViewCellBorderStyle.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.Black
            },
            // ... otras propiedades ...
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,  // 🚫 Des

            RowHeadersVisible = false
        };

        private void ConfigurarColumnasDataGridView(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();

            // 1. Columna Nombre/EndPoint con Ruta y Botón Copiar
            var nombreColumna = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "endPoint",
                HeaderText = "Nombre / EndPoint",
                Width = 500, // Ancho ajustado a 800 píxeles
                DefaultCellStyle = new DataGridViewCellStyle
                {

                    WrapMode = DataGridViewTriState.True,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular)
                }
            };
            nombreColumna.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            nombreColumna.FillWeight = 600; // Ajusta el peso de la columna

            dgv.Columns.Add(nombreColumna);

            // 2. Columna Estado
            var colEstado = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "estado",
                HeaderText = "Estado",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter // Centrar el texto en las celdas
                }
            };
            colEstado.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter; // Centrar el texto en el encabezado
            dgv.Columns.Add(colEstado);

            // 3. Columna Tecnología
            var colTecnologia = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Tecnologia",
                HeaderText = "Tecnología",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter // Centrar el texto en las celdas
                }
            };
            colTecnologia.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter; // Centrar el texto en el encabezado
            dgv.Columns.Add(colTecnologia);

            // 4. Columna Acciones (Botón)
            var accionesCol = new DataGridViewButtonColumn
            {
                HeaderText = "Acciones",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat,
                Width = 150, // Aumentamos el ancho a 150 píxeles
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None, // Evita que se autoajuste
                DefaultCellStyle = new DataGridViewCellStyle
                {

                    Font = new Font("Segoe UI", 9), // Ajusta el tamaño de la fuente
                    Alignment = DataGridViewContentAlignment.MiddleCenter // Centra el texto
                }
            };
            dgv.Columns.Add(accionesCol);
            accionesCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            accionesCol.DefaultCellStyle.Font = new Font("Segoe UI", 8); // Reducir el tamaño de la fuente

            accionesCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter; // Centrar el texto en el encabezado
            // Estilo Encabezados
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // ====================
            // Eventos Personalizados
            // ====================

            // Evento 1: Formatear primera columna (Nombre/EndPoint + Ruta)
            dgv.CellFormatting += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
                var api = (ApisProyecto)dgv.Rows[e.RowIndex].DataBoundItem;

                // Configurar parámetros de truncado
                const int buttonWidth = 60; // Ancho del botón Copiar (ajustado al valor usado en CellPainting)
                var font = dgv.DefaultCellStyle.Font;
                int availableWidth = dgv.Columns[0].Width - buttonWidth - 20; // 20px de margen

                string textoPrincipal = api.EsScript ? api.Nombre : api.endPoint;
                string rutaTexto = api.RutaLocal;

                // Función de truncado inteligente
                string TruncarTexto(string texto, int maxWidth)
                {
                    if (TextRenderer.MeasureText(texto, font).Width <= maxWidth) return texto;

                    string ellipsis = "...";
                    int ellipsisWidth = TextRenderer.MeasureText(ellipsis, font).Width;

                    var finalTexto = texto;
                    while (TextRenderer.MeasureText(finalTexto + ellipsis, font).Width > maxWidth && finalTexto.Length > 3) // Evitar truncar a menos de 3 caracteres
                    {
                        finalTexto = finalTexto.Substring(0, finalTexto.Length - 1);
                    }

                    return finalTexto + ellipsis;
                }

                // Aplicar truncado a ambas líneas
                string textoPrincipalTruncado = TruncarTexto(textoPrincipal, availableWidth);
                string rutaTruncada = TruncarTexto(rutaTexto, availableWidth);

                e.Value = $"{textoPrincipalTruncado}\nRuta: {rutaTruncada}";
            };

            // Evento 2: Dibujar botón Copiar
            dgv.CellPainting += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 0) return;

                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                // Coordenadas del botón (ajustadas para coincidir con el evento CellClick)
                var btnRect = new Rectangle(
                    e.CellBounds.Right - 60, // Ancho del botón: 60px
                    e.CellBounds.Top + (e.CellBounds.Height - 20) / 2, // Centrar verticalmente
                    60, // Ancho del botón
                    20  // Alto del botón
                );

                // Fondo azul
                using (var brush = new SolidBrush(ColorTranslator.FromHtml("#007BFF")))
                {
                    e.Graphics.FillRectangle(brush, btnRect);
                }

                // Texto "Copiar"
                TextRenderer.DrawText(
                    e.Graphics,
                    "Copiar",
                    new Font("Segoe UI", 8, FontStyle.Bold),
                    btnRect,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                e.Handled = true;
            };

            // Evento 3: Click en botón Copiar
            dgv.CellClick += (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 0) return;

                var grid = (DataGridView)sender;
                var cellBounds = grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                var api = (ApisProyecto)grid.Rows[e.RowIndex].DataBoundItem;

                // Verificar clic en el botón (coordenadas ajustadas para coincidir con CellPainting)
                var btnRect = new Rectangle(
                    cellBounds.Right - 60, // Ancho del botón: 60px
                    cellBounds.Top + (cellBounds.Height - 20) / 2, // Centrar verticalmente
                    60, // Ancho del botón
                    20  // Alto del botón
                );

                if (btnRect.Contains(grid.PointToClient(Cursor.Position)))
                {
                    Clipboard.SetText(api.RutaLocal);
                    MessageBox.Show("Ruta copiada al portapeles",
                                  "Éxito",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);
                }
            };
            // Evento 4: Formatear celdas de Estado y Acciones
            dgv.CellFormatting += (sender, e) =>
            {
                if (e.RowIndex < 0) return;
                var api = (ApisProyecto)dgv.Rows[e.RowIndex].DataBoundItem;

                // Columna Estado
                if (e.ColumnIndex == 1)
                {
                    e.CellStyle.ForeColor = api.estado == "Activo"
                        ? Color.FromArgb(13, 174, 117)
                        : Color.Red;
                    e.Value = api.estado == "Activo" ? "Conectado" : "Sin conexión";
                }

                // Columna Acciones
                if (e.ColumnIndex == 3)
                {
                    e.Value = api.estado == "Activo" ? "Detener" : "Iniciar";
                    var cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    cell.Style.BackColor = api.estado == "Activo" ? Color.FromArgb(13, 174, 117) : Color.Red;
                    cell.Style.ForeColor = Color.White;
                }
            };

            // Evento 5: Manejar clic en botones de Acción
            dgv.CellClick += async (sender, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 3) return;
                var grid = (DataGridView)sender;
                var api = (ApisProyecto)grid.Rows[e.RowIndex].DataBoundItem;

                using (var loadingForm = new loading())
                {
                    this.Invoke((MethodInvoker)delegate { loadingForm.Show(this); });

                    if (api.estado == "Inactivo")
                        await IniciarAPI(api);
                    else
                        await DetenerAPI(api);

                    api.estado = await VerificarAPI(api) ? "Activo" : "Inactivo";
                    grid.InvalidateRow(e.RowIndex);
                }
            };

            // Deshabilitar selección de celdas
            dgv.SelectionChanged += (sender, e) =>
            {
                if (dgv.SelectedCells.Count > 0)
                    dgv.ClearSelection();
            };
        }

        private async Task ActualizarEstadoBoton(Button btnIniciarDetener, List<ApisProyecto> apis)
        {
            bool todasActivas = true;
            bool algunaActiva = false;

            foreach (var api in apis)
            {
                var estadoActual = await VerificarAPI(api);
                api.estado = estadoActual ? "Activo" : "Inactivo";

                if (!estadoActual) todasActivas = false;
                if (estadoActual) algunaActiva = true;
            }

            this.Invoke((MethodInvoker)delegate
            {
                btnIniciarDetener.Text = todasActivas ? "Detener Todas las APIs" :
                    algunaActiva ? "Detener Todas las APIs" : "Iniciar Todas las APIs";

                btnIniciarDetener.BackColor = todasActivas ? Color.Red :
                    algunaActiva ? Color.Orange : Color.FromArgb(13, 174, 117);

                btnIniciarDetener.Location = new Point(
                    btnIniciarDetener.Parent.Width - btnIniciarDetener.Width - 10,
                    btnIniciarDetener.Location.Y);
            });
        }
        
        private async Task IniciarTodasLasApis(List<ApisProyecto> apis)
        {
            // Filtrar solo las APIs inactivas
            var apisInactivas = apis.Where(api => api.estado == "Inactivo").ToList();

            //if (apisInactivas.Count == 0)
            //{
            //    MessageBox.Show("Todas las APIs ya están activas.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    return;
            //}
            if (!apisInactivas.Any()) return;


            // Iniciar todas las APIs inactivas
            var tareas = apisInactivas.Select(api => IniciarAPI(api)).ToList();

            // Esperar a que todas las tareas finalicen
            var resultados = await Task.WhenAll(tareas);

            // Verificar si todas las APIs se iniciaron correctamente
            bool todasIniciadas = resultados.All(result => result);

            // Mostrar un solo mensaje resumiendo el estado
            if (todasIniciadas)
            {
                MessageBox.Show("Todas las APIs se iniciaron correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Algunas APIs no se pudieron iniciar. Revise los detalles.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Actualizar el estado de todas las APIs
            await VerificarTodosLosEstados();
        }
        
        private async Task DetenerTodasLasApis(List<ApisProyecto> apis)
        {
            // Mostrar un solo mensaje de confirmación para detener todas las APIs
            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro de detener todas las APIs de este proyecto?",
                "Confirmar detención",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacion != DialogResult.Yes)
            {
                return; // El usuario canceló la detención
            }
            // Filtrar las APIs activas y detenerlas
            var tareas = apis
                .Where(api => api.estado == "Activo")
                .Select(api => DetenerAPI(api))
                .ToList();

            bool[] resultados = await Task.WhenAll(tareas);

            // Verificar si todas las APIs se detuvieron correctamente
            bool todasDetenidas = resultados.All(result => result);

            // Mostrar un mensaje general
            if (todasDetenidas)
            {
                MessageBox.Show("Todas las APIs se han detenido correctamente.", "APIs detenidas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudieron detener todas las APIs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            await VerificarTodosLosEstados(); // Forzar actualización inmediata
        }

        private string GetCommandLine(Process process)
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["CommandLine"]?.ToString();
                }
            }
            return null;
        }

        //private async Task VerificarTodosLosEstados(bool primeraCarga = false)
        //{
        //    Form loadingForm = null;

        //    // Mostrar el formulario de carga si es la primera vez
        //    if (primeraCarga)
        //    {
        //        loadingForm = new Form
        //        {
        //            Text = "Verificando APIs...",
        //            Size = new Size(300, 100),
        //            StartPosition = FormStartPosition.CenterScreen,
        //            FormBorderStyle = FormBorderStyle.FixedDialog,
        //            ControlBox = false
        //        };
        //        var label = new Label
        //        {
        //            Text = "Cargando, por favor espere...",
        //            Dock = DockStyle.Fill,
        //            TextAlign = ContentAlignment.MiddleCenter
        //        };
        //        loadingForm.Controls.Add(label);
        //        loadingForm.Show();
        //    }


        //    // Hacemos toda la verificación en un hilo en segundo plano

        //    var resultados = await Task.Run(() =>
        //    {
        //        var estadosPorTab = new Dictionary<TabPage, Dictionary<ApisProyecto, bool>>();

        //        foreach (TabPage tab in tabControl1.TabPages)
        //        {
        //            var panel = tab.Controls.OfType<Panel>().FirstOrDefault();
        //            var dgv = panel?.Controls.OfType<DataGridView>().FirstOrDefault();
        //            if (dgv?.DataSource is List<ApisProyecto> apis)
        //            {
        //                var resultadoApis = new Dictionary<ApisProyecto, bool>();
        //                foreach (var api in apis)
        //                {
        //                    bool estadoActual = VerificarAPI(api).Result; // O usar .GetAwaiter().GetResult()
        //                    resultadoApis[api] = estadoActual;
        //                }
        //                estadosPorTab[tab] = resultadoApis;
        //            }
        //        }
        //        return estadosPorTab;
        //    });

        //    // 2) Una vez calculados los estados, actualizamos la UI *rápidamente*:
        //    foreach (var kvpTab in resultados)
        //    {
        //        var tab = kvpTab.Key;
        //        var panel = tab.Controls.OfType<Panel>().FirstOrDefault();
        //        var dgv = panel?.Controls.OfType<DataGridView>().FirstOrDefault();

        //        if (dgv?.DataSource is List<ApisProyecto> apis)
        //        {
        //            foreach (var apiEstado in kvpTab.Value)
        //            {
        //                var api = apiEstado.Key;
        //                bool nuevoEstado = apiEstado.Value;
        //                api.estado = nuevoEstado ? "Activo" : "Inactivo";
        //            }
        //            dgv.Invalidate(); // O InvalidateRow para cada fila
        //        }

        //        // Actualizar botón
        //        var btn = panel?.Controls.OfType<Button>().FirstOrDefault();
        //        if (btn != null)
        //            await ActualizarEstadoBoton(btn, (List<ApisProyecto>)dgv.DataSource);

        //        // Actualizar “tag” en la tab para dibujar el “circulito” verde/rojo
        //        tab.Tag = ((List<ApisProyecto>)dgv.DataSource).All(a => a.estado == "Activo");
        //    }

        //    // Forzamos redibujado final
        //    tabControl1.Invalidate();
        //}
        private async Task VerificarTodosLosEstados(bool primeraCarga = false)
        {
            if (isVerificando) return; // Si ya se está verificando, salimos
            isVerificando = true;

            try
            {
                // Si es la primera carga, muestra el formulario de carga
                Form loadingForm = null;
                if (primeraCarga)
                {
                    loadingForm = new Form
                    {
                        Text = "Verificando APIs...",
                        Size = new Size(300, 100),
                        StartPosition = FormStartPosition.CenterScreen,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        ControlBox = false
                    };
                    var label = new Label
                    {
                        Text = "Cargando, por favor espere...",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    loadingForm.Controls.Add(label);
                    loadingForm.Show();
                }

                // Recorrer las pestañas y verificar cada API de forma asíncrona sin bloquear el hilo.
                foreach (TabPage tab in tabControl1.TabPages)
                {
                    var panel = tab.Controls.OfType<Panel>().FirstOrDefault();
                    var dgv = panel?.Controls.OfType<DataGridView>().FirstOrDefault();
                    if (dgv?.DataSource is List<ApisProyecto> apis)
                    {
                        foreach (var api in apis)
                        {
                            // Evita usar .Result: en su lugar, espera la tarea
                            bool estadoActual = await VerificarAPI(api);
                            api.estado = estadoActual ? "Activo" : "Inactivo";
                        }
                        dgv.Invalidate();
                    }

                    // Actualiza el botón y el indicador visual de la pestaña
                    var btn = panel?.Controls.OfType<Button>().FirstOrDefault();
                    if (btn != null)
                        await ActualizarEstadoBoton(btn, (List<ApisProyecto>)dgv.DataSource);
                    tab.Tag = ((List<ApisProyecto>)dgv.DataSource).All(a => a.estado == "Activo");
                }

                tabControl1.Invalidate();
                if (primeraCarga && loadingForm != null)
                    loadingForm.Close();
            }
            finally
            {
                isVerificando = false;
            }
        }
        private async Task<bool> VerificarAPI(ApisProyecto api)
        {
            bool procesoActivo = false;

            if (api.EsScript)
            {
                // Verificar si el proceso ya está registrado y sigue en ejecución
                if (procesosAPIs.TryGetValue(api.Nombre, out var proceso))
                {
                    procesoActivo = !proceso.HasExited;
                }
                else
                {
                    // Buscar procesos activos relacionados con el script
                    var procesos = Process.GetProcessesByName("cmd");
                    foreach (var p in procesos)
                    {
                        try
                        {
                            string cmdLine = GetCommandLine(p);
                            if (cmdLine != null && cmdLine.Contains(api.comandoInicio))
                            {
                                procesosAPIs[api.Nombre] = p;
                                procesoActivo = true;
                                break;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                // Capturar el estado actual y notificar si hay un cambio
                if (procesoActivo)
                {
                    if (api.estado != "Activo" || !api.NotificacionEnviada)
                    {
                        api.estado = "Activo";
                        api.NotificacionEnviada = true;

                        await SendMessageAsync(
                            $"✅ ESTADO ACTUAL: El script '{api.Nombre}' está en ejecución.\n" +
                            $"📂 RutaLocal: {api.RutaLocal}\n" +
                            $"🛠️ Tecnología: {api.Tecnologia}\n" +
                            $"🟢 Estado: {api.estado}"
                        );
                    }
                    return true;
                }
                else
                {
                    if (api.estado != "Inactivo" || !api.NotificacionEnviada)
                    {
                        api.estado = "Inactivo";
                        api.NotificacionEnviada = true;

                        await SendMessageAsync(
                            $"❌ ESTADO ACTUAL: El script '{api.Nombre}' no se encontró o se ha detenido.\n" +
                            $"📂 RutaLocal: {api.RutaLocal}\n" +
                            $"🛠️ Tecnología: {api.Tecnologia}\n" +
                            $"🔴 Estado: {api.estado}"
                        );
                    }
                    return false;
                }
            }
            else
            {
                // Lógica para verificar APIs que no son scripts
                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(api.endPoint);
                        if (response.IsSuccessStatusCode)
                        {
                            if (api.estado != "Activo" || !api.NotificacionEnviada)
                            {
                                api.estado = "Activo";
                                api.NotificacionEnviada = true;

                                await SendMessageAsync(
                                    $"✅ ESTADO ACTUAL: La API '{api.Nombre}' está en ejecución.\n" +
                                    $"🌐 Endpoint: {api.endPoint}\n" +
                                    $"🔌 Puerto: {api.puerto}\n" +
                                    $"🛠️ Tecnología: {api.Tecnologia}\n" +
                                    $"🟢 Estado: {api.estado}"
                                );
                            }
                            return true;
                        }
                        else
                        {
                            if (api.estado != "Inactivo" || !api.NotificacionEnviada)
                            {
                                api.estado = "Inactivo";
                                api.NotificacionEnviada = true;

                                await SendMessageAsync(
                                    $"❌ ESTADO ACTUAL: La API '{api.Nombre}' ha dejado de responder.\n" +
                                    $"🌐 Endpoint: {api.endPoint}\n" +
                                    $"🔌 Puerto: {api.puerto}\n" +
                                    $"🛠️ Tecnología: {api.Tecnologia}\n" +
                                    $"🔴 Estado: {api.estado}"
                                );
                            }
                            return false;
                        }
                    }
                }
                catch
                {
                    if (api.estado != "Inactivo" || !api.NotificacionEnviada)
                    {
                        api.estado = "Inactivo";
                        api.NotificacionEnviada = true;

                        await SendMessageAsync(
                            $"❌ ESTADO ACTUAL: La API '{api.Nombre}' ha dejado de responder.\n" +
                            $"🌐 Endpoint: {api.endPoint}\n" +
                            $"🔌 Puerto: {api.puerto}\n" +
                            $"🛠️ Tecnología: {api.Tecnologia}\n" +
                            $"🔴 Estado: {api.estado}"
                        );
                    }
                    return false;
                }
            }
        }

        private async Task<bool> IniciarAPI(ApisProyecto api)
        {
            if (api.EsScript)
            {
                try
                {
                    if (!Directory.Exists(api.RutaLocal))
                    {
                        MessageBox.Show($"La ruta {api.RutaLocal} no existe.");
                        return false;
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C cd /d \"{api.RutaLocal}\" && {api.comandoInicio}", // Usar /C para cerrar la consola después de ejecutar el comando
                        WorkingDirectory = api.RutaLocal,
                        UseShellExecute = false, // No usar el shell del sistema
                        CreateNoWindow = true    // No crear una ventana de consola visible
                    };

                    var process = new Process { StartInfo = psi };
                    process.Start();

                    // Registrar el proceso en el diccionario
                    procesosAPIs[api.Nombre] = process;
                    api.estado = "Activo"; // Actualizar el estado de la API
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error crítico al iniciar el script: {ex.Message}");
                    return false;
                }
            }
            else
            {
                // Lógica para iniciar APIs no script
                try
                {
                    if (procesosAPIs.ContainsKey(api.puerto.ToString()))
                    {
                        MessageBox.Show($"La API en el puerto {api.puerto} ya está en ejecución.");
                        return false;
                    }

                    if (!Directory.Exists(api.RutaLocal))
                    {
                        MessageBox.Show($"La ruta {api.RutaLocal} no existe.");
                        return false;
                    }

                    var (fileName, arguments) = ParseCommand(api.comandoInicio);

                    if (string.IsNullOrEmpty(fileName))
                    {
                        MessageBox.Show("Comando de inicio no válido");
                        return false;
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        WorkingDirectory = api.RutaLocal,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var process = new Process { StartInfo = psi };
                    process.OutputDataReceived += (s, e) => Debug.WriteLine($"[{api.puerto}] OUTPUT: {e.Data}");
                    process.ErrorDataReceived += (s, e) => Debug.WriteLine($"[{api.puerto}] ERROR: {e.Data}");

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Esperar y verificar
                    await Task.Delay(2000);

                    bool apiIniciada = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (await VerificarAPI(api))
                        {
                            apiIniciada = true;
                            break;
                        }
                        await Task.Delay(3000);
                    }

                    if (apiIniciada)
                    {
                        procesosAPIs[api.puerto.ToString()] = process;
                        return true;
                    }
                    else
                    {
                        process.Kill();
                        MessageBox.Show($"No se pudo iniciar la API en puerto {api.puerto}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error crítico en {api.puerto}: {ex.Message}");
                    return false;
                }
            }
        }

        private (string FileName, string Arguments) ParseCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return (null, null);

            var regex = new System.Text.RegularExpressions.Regex(@"(?<match>""[^""]*""|\S+)");
            var parts = regex.Matches(command)
                            .Cast<System.Text.RegularExpressions.Match>()
                            .Select(m => m.Groups["match"].Value.Trim('"'))
                            .ToList();

            return parts.Count == 0
                ? (null, null)
                : (parts[0], string.Join(" ", parts.Skip(1)));
        }

      
        private async Task<bool> DetenerAPI(ApisProyecto api)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string claveProceso = api.EsScript ? api.Nombre : api.puerto.ToString();
                    bool procesoDetenido = false;

                    // 1. Detener proceso desde el diccionario
                    if (procesosAPIs.TryRemove(claveProceso, out var proceso) && proceso != null && !proceso.HasExited)
                    {
                        proceso.Kill();
                        await Task.Delay(500); // Pequeña espera para asegurar la finalización
                        procesoDetenido = true;
                    }

                    // 2. Buscar y detener procesos relacionados
                    foreach (var proc in Process.GetProcessesByName(api.Nombre))
                    {
                        try
                        {
                            if (!proc.HasExited)
                            {
                                proc.Kill();
                                await Task.Delay(500);
                                procesoDetenido = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al detener proceso secundario {proc.ProcessName}: {ex.Message}");
                        }
                    }

                    // 3. Si no se encontró en procesosAPIs, buscar por puerto (para APIs en ejecución)
                    if (!procesoDetenido && !api.EsScript)
                    {
                        try
                        {
                            string argumentosNetstat = $"-ano | findstr :{api.puerto}";
                            ProcessStartInfo psiNetstat = new ProcessStartInfo("cmd.exe", "/c netstat " + argumentosNetstat)
                            {
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using (Process procesoNetstat = Process.Start(psiNetstat))
                            {
                                string salidaNetstat = await procesoNetstat.StandardOutput.ReadToEndAsync();
                                procesoNetstat.WaitForExit();

                                foreach (string linea in salidaNetstat.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (linea.Contains("LISTENING"))
                                    {
                                        string[] partes = linea.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                        string pid = partes.Last();

                                        // 4. Forzar cierre del proceso por PID
                                        ProcessStartInfo psiKill = new ProcessStartInfo("taskkill", $"/PID {pid} /F /T")
                                        {
                                            RedirectStandardOutput = true,
                                            UseShellExecute = false,
                                            CreateNoWindow = true
                                        };

                                        using (Process procesoKill = Process.Start(psiKill))
                                        {
                                            await procesoKill.StandardOutput.ReadToEndAsync();
                                            procesoKill.WaitForExit();
                                            procesoDetenido = true;
                                        }

                                        // 5. Si es Python, matar procesos hijos (ejemplo: Uvicorn, Flask)
                                        if (api.Tecnologia == "Python")
                                        {
                                            ProcessStartInfo psiUvicorn = new ProcessStartInfo("cmd.exe", $"/c wmic process where (ParentProcessId={pid}) get ProcessId")
                                            {
                                                RedirectStandardOutput = true,
                                                UseShellExecute = false,
                                                CreateNoWindow = true
                                            };

                                            using (Process procesoUvicorn = Process.Start(psiUvicorn))
                                            {
                                                string salidaUvicorn = await procesoUvicorn.StandardOutput.ReadToEndAsync();
                                                procesoUvicorn.WaitForExit();

                                                foreach (string pidHijo in salidaUvicorn.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                                                {
                                                    if (int.TryParse(pidHijo, out int pidHijoNum))
                                                    {
                                                        ProcessStartInfo psiKillHijo = new ProcessStartInfo("taskkill", $"/PID {pidHijoNum} /F")
                                                        {
                                                            RedirectStandardOutput = true,
                                                            UseShellExecute = false,
                                                            CreateNoWindow = true
                                                        };

                                                        using (Process procesoKillHijo = Process.Start(psiKillHijo))
                                                        {
                                                            await procesoKillHijo.StandardOutput.ReadToEndAsync();
                                                            procesoKillHijo.WaitForExit();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inesperado: {ex.Message}");
                        }
                    }

                    return procesoDetenido;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al detener la API '{api.Nombre}': {ex.Message}\n{ex.StackTrace}");
                    return false;
                }
            });
        }


        private void close_Click(object sender, EventArgs e) => Application.Exit();

        private void maxime_Click(object sender, EventArgs e) => WindowState = WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
       
        private void minime_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        public class ConfiguracionApis
        {
            public List<Proyecto> proyectos { get; set; }
        }

        public class Proyecto
        {
            public string nombre { get; set; }
            public List<ApisProyecto> apis { get; set; }
        }

        public class ApisProyecto  {
            public string endPoint { get; set; } 
            public string estado { get; set; }
            public int puerto { get; set; } 
            public string RutaLocal { get; set; }
            public string Tecnologia { get; set; }
            public string comandoInicio { get; set; }
            public bool EsScript { get; set; } 
            public string Nombre { get; set; } 
            public bool NotificacionEnviada { get; set; } = false;
        }

        private bool arrastrando = false;
        private Point puntoInicio;
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (arrastrando) {
                // Calcula la nueva posición del formulario
                Point nuevaPosicion = this.Location;
                nuevaPosicion.X += e.X - puntoInicio.X;
                nuevaPosicion.Y += e.Y - puntoInicio.Y;
                this.Location = nuevaPosicion;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e) {
            // Detecta clic izquierdo para iniciar el movimiento
            if (e.Button == MouseButtons.Left)
            {
                arrastrando = true;
                puntoInicio = new Point(e.X, e.Y);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            // Detiene el movimiento al soltar el botón
            if (e.Button == MouseButtons.Left)
            {
                arrastrando = false;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)  { }


        private FileSystemWatcher yamlWatcher;
        //private void SetupYamlFileWatcher()
        //{
        //    string exePath = AppDomain.CurrentDomain.BaseDirectory;
        //    string resourcesPath = Path.Combine(exePath, "Resources");

        //    yamlWatcher = new FileSystemWatcher(resourcesPath, "listaApis.yaml")
        //    {
        //        NotifyFilter = NotifyFilters.LastWrite
        //    };

        //    yamlWatcher.Changed += (s, e) =>
        //    {
        //        // Espera breve para evitar múltiples triggers
        //        System.Threading.Thread.Sleep(100);
        //        this.Invoke(new Action(() =>
        //        {
        //            // Leer nuevamente el YAML
        //            var nuevosProyectos = ObtenerProyectos();

        //            // Vaciar y reconstruir el TabControl
        //            tabControl1.TabPages.Clear();
        //            CargarProyectosInicial(nuevosProyectos);

        //            // Opcional: forzar una verificación de estados
        //            VerificarTodosLosEstados().ConfigureAwait(false);
        //        }));
        //    };

        //    yamlWatcher.EnableRaisingEvents = true;
        //}
        private void SetupYamlFileWatcher()
        {
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string resourcesPath = Path.Combine(exePath, "Resources");

            yamlWatcher = new FileSystemWatcher(resourcesPath, "listaApis.yaml")
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            yamlWatcher.Changed += (s, e) =>
            {
                // Reinicia el temporizador cada vez que se detecta un cambio
                debounceTimer?.Stop();
                debounceTimer = new System.Timers.Timer(500); // 500 ms de espera
                debounceTimer.Elapsed += (sender, args) =>
                {
                    debounceTimer.Stop();
                    this.Invoke(new Action(() =>
                    {
                        var nuevosProyectos = ObtenerProyectos();
                        tabControl1.TabPages.Clear();
                        CargarProyectosInicial(nuevosProyectos);
                        VerificarTodosLosEstados().ConfigureAwait(false);
                    }));
                };
                debounceTimer.Start();
            };

            yamlWatcher.EnableRaisingEvents = true;
        }
        private void listayaml_Click(object sender, EventArgs e)
        {
            // Ruta completa del archivo YAML
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string resourcesPath = Path.Combine(exePath, "Resources");
            string filePath = Path.Combine(resourcesPath, "listaApis.yaml");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "code",  // Suponiendo que 'code' (VS Code) está en el PATH.
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true
                    // No incluimos CreateNoWindow para evitar forzar la creación de una ventana de CMD
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el archivo YAML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listayaml_Click_1(object sender, EventArgs e)
        {

        }
    }
}