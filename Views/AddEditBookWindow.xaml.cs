using System;
using System.Linq;
using System.Windows;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Views
{
    public partial class AddEditBookWindow : Window
    {
        private LibraryContext _context;
        private Book _book;
        private bool _isEditMode;

        public AddEditBookWindow(LibraryContext context, int? bookId = null)
        {
            InitializeComponent();
            _context = context;
            _isEditMode = bookId.HasValue;

            try
            {
                // Загружаем авторов
                AuthorComboBox.ItemsSource = _context.Authors.ToList();

                // Загружаем жанры
                GenreComboBox.ItemsSource = _context.Genres.ToList();

                if (_isEditMode && bookId.HasValue)
                {
                    Title = "Редактирование книги";
                    TitleTextBlock.Text = "Редактирование книги";

                    _book = _context.Books.FirstOrDefault(b => b.Id == bookId.Value);

                    if (_book != null)
                    {
                        TitleTextBox.Text = _book.Title;

                        // Выбираем автора
                        foreach (Author a in AuthorComboBox.Items)
                        {
                            if (a.Id == _book.AuthorId)
                            {
                                AuthorComboBox.SelectedItem = a;
                                break;
                            }
                        }

                        // Выбираем жанр
                        foreach (Genre g in GenreComboBox.Items)
                        {
                            if (g.Id == _book.GenreId)
                            {
                                GenreComboBox.SelectedItem = g;
                                break;
                            }
                        }

                        YearTextBox.Text = _book.PublishYear.ToString();
                        ISBNTextBox.Text = _book.ISBN;
                        QuantityTextBox.Text = _book.QuantityInStock.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    MessageBox.Show("Введите название книги", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (AuthorComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите автора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (GenreComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите жанр", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(YearTextBox.Text, out int year) || year < 1800 || year > 2025)
                {
                    MessageBox.Show("Введите корректный год (1800-2025)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 0)
                {
                    MessageBox.Show("Введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_isEditMode)
                {
                    // Редактирование
                    var book = _context.Books.Find(_book.Id);
                    if (book != null)
                    {
                        book.Title = TitleTextBox.Text;
                        book.AuthorId = ((Author)AuthorComboBox.SelectedItem).Id;
                        book.GenreId = ((Genre)GenreComboBox.SelectedItem).Id;
                        book.PublishYear = year;
                        book.ISBN = ISBNTextBox.Text;
                        book.QuantityInStock = quantity;
                    }
                }
                else
                {
                    // Добавление
                    var newBook = new Book
                    {
                        Title = TitleTextBox.Text,
                        AuthorId = ((Author)AuthorComboBox.SelectedItem).Id,
                        GenreId = ((Genre)GenreComboBox.SelectedItem).Id,
                        PublishYear = year,
                        ISBN = ISBNTextBox.Text,
                        QuantityInStock = quantity
                    };
                    _context.Books.Add(newBook);
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}