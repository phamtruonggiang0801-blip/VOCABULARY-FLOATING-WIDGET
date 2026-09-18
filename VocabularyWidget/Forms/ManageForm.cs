using System.ComponentModel;
using VocabularyWidget.Models;
using VocabularyWidget.Services;

namespace VocabularyWidget.Forms;

public class ManageForm : Form
{
    private readonly DataService _dataService;
    private readonly BindingList<WordItem> _binding;
    private readonly DataGridView _grid;
    private readonly TextBox _txtWord;
    private readonly TextBox _txtDefinition;

    public event Action<List<WordItem>>? WordsChanged;

    public ManageForm(DataService dataService)
    {
        _dataService = dataService;
        _binding = new BindingList<WordItem>(_dataService.LoadWords());

        Text = "Quản lý từ vựng";
        Size = new Size(720, 460);
        MinimumSize = new Size(560, 360);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 245, 247);
        Font = new Font("Segoe UI", 9);

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            DataSource = _binding
        };
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(WordItem.Word),
            HeaderText = "Từ vựng",
            FillWeight = 30
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(WordItem.Definition),
            HeaderText = "Định nghĩa",
            FillWeight = 50
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(WordItem.ReviewCount),
            HeaderText = "Ôn",
            FillWeight = 10,
            ReadOnly = true
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(WordItem.CorrectCount),
            HeaderText = "Đúng",
            FillWeight = 10,
            ReadOnly = true
        });
        _grid.CellEndEdit += (_, _) => Persist();

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 108, Padding = new Padding(12) };

        _txtWord = new TextBox { Width = 160, Location = new Point(12, 12) };
        var placeholderWord = new Label
        {
            Text = "Từ mới",
            AutoSize = true,
            Location = new Point(12, 36),
            ForeColor = Color.Gray
        };
        _txtDefinition = new TextBox { Width = 280, Location = new Point(184, 12) };
        var placeholderDef = new Label
        {
            Text = "Định nghĩa",
            AutoSize = true,
            Location = new Point(184, 36),
            ForeColor = Color.Gray
        };

        var btnAdd = MakeButton("Thêm", 476, 10);
        btnAdd.Click += (_, _) => AddWord();

        var btnDelete = MakeButton("Xóa", 580, 10);
        btnDelete.Click += (_, _) => DeleteSelected();

        var btnImport = MakeButton("Import CSV/TXT", 12, 58);
        btnImport.Width = 150;
        btnImport.Click += (_, _) => ImportFile();

        var btnSave = MakeButton("Lưu", 476, 58);
        btnSave.Click += (_, _) => Persist();

        var btnClose = MakeButton("Đóng", 580, 58);
        btnClose.Click += (_, _) => Close();

        bottom.Controls.AddRange(new Control[]
        {
            _txtWord, placeholderWord, _txtDefinition, placeholderDef,
            btnAdd, btnDelete, btnImport, btnSave, btnClose
        });

        Controls.Add(_grid);
        Controls.Add(bottom);
        AcceptButton = btnAdd;
    }

    public void ReloadFromDisk()
    {
        _binding.Clear();
        foreach (var word in _dataService.LoadWords())
        {
            _binding.Add(word);
        }
    }

    private static Button MakeButton(string text, int x, int y)
    {
        return new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(96, 32),
            FlatStyle = FlatStyle.System
        };
    }

    private void AddWord()
    {
        string word = _txtWord.Text.Trim();
        string definition = _txtDefinition.Text.Trim();
        if (word.Length == 0 || definition.Length == 0)
        {
            MessageBox.Show(this, "Nhập cả từ và định nghĩa.", "Thiếu dữ liệu",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _binding.Add(new WordItem { Word = word, Definition = definition });
        _txtWord.Clear();
        _txtDefinition.Clear();
        _txtWord.Focus();
        Persist();
    }

    private void DeleteSelected()
    {
        if (_grid.CurrentRow?.DataBoundItem is not WordItem item)
        {
            return;
        }

        _binding.Remove(item);
        Persist();
    }

    private void ImportFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Từ vựng (*.csv;*.txt)|*.csv;*.txt|CSV (*.csv)|*.csv|Text (*.txt)|*.txt|Tất cả (*.*)|*.*",
            Title = "Import từ vựng"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        string text = File.ReadAllText(dialog.FileName);
        var imported = WordImportService.Parse(text, dialog.FileName);
        if (imported.Count == 0)
        {
            MessageBox.Show(this, "Không đọc được dòng nào. Dùng word,definition hoặc word | definition.",
                "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existing = new HashSet<string>(
            _binding.Select(w => w.Word.Trim().ToLowerInvariant()),
            StringComparer.Ordinal);
        int added = 0;
        foreach (var item in imported)
        {
            string key = item.Word.Trim().ToLowerInvariant();
            if (existing.Add(key))
            {
                _binding.Add(item);
                added++;
            }
        }

        Persist();
        MessageBox.Show(this, $"Đã thêm {added} từ (bỏ qua trùng).", "Import",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void Persist()
    {
        var list = _binding.ToList();
        _dataService.SaveWords(list);
        WordsChanged?.Invoke(list);
    }
}
