using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Views;

namespace LibraryManagement
{
    public partial class MainWindow : Window
    {
        private LibraryContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new LibraryContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем и создаем базу данных если нужно
                _context.Database.EnsureCreated();
                LoadFilterData();
                LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilterData()
        {
            try
            {
                // Загрузка авторов для фильтра (добавляем "Все авторы")
                var authors = _context.Authors.OrderBy(a => a.LastName).ToList();
                authors.Insert(0, new Author { Id = 0, FirstName = "Все", LastName = "авторы" });
                AuthorFilterComboBox.ItemsSource = authors;
                AuthorFilterComboBox.SelectedIndex = 0;

                // Загрузка жанров для фильтра (добавляем "Все жанры")
                var genres = _context.Genres.OrderBy(g => g.Name).ToList();
                genres.Insert(0, new Genre { Id = 0, Name = "Все жанры" });
                GenreFilterComboBox.ItemsSource = genres;
                GenreFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBooks()
        {
            try
            {
                var query = _context.Books
                    .Include(b => b.Author)
                    .Include(b => b.Genre)
                    .AsQueryable();

                // Применение фильтра по автору
                if (AuthorFilterComboBox.SelectedItem is Author selectedAuthor && selectedAuthor.Id > 0)
                {
                    query = query.Where(b => b.AuthorId == selectedAuthor.Id);
                }

                // Применение фильтра по жанру
                if (GenreFilterComboBox.SelectedItem is Genre selectedGenre && selectedGenre.Id > 0)
                {
                    query = query.Where(b => b.GenreId == selectedGenre.Id);
                }

                // Применение поиска по названию
                if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    query = query.Where(b => b.Title.Contains(SearchTextBox.Text));
                }

                var books = query.OrderBy(b => b.Title).ToList();
                BooksDataGrid.ItemsSource = books;

                // Обновляем статистику
                TotalBooksText.Text = _context.Books.Count().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке книг: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadBooks();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadBooks();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            AuthorFilterComboBox.SelectedIndex = 0;
            GenreFilterComboBox.SelectedIndex = 0;
        }

        private void BooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book selectedBook)
            {
                SelectedBookInfo.Text = $"Выбрана книга: {selectedBook.Title} | Автор: {selectedBook.Author?.FullName} | Жанр: {selectedBook.Genre?.Name} | В наличии: {selectedBook.QuantityInStock}";
                SelectedBookInfo.Foreground = System.Windows.Media.Brushes.Black;
                SelectedBookInfo.FontStyle = FontStyles.Normal;
            }
            else
            {
                SelectedBookInfo.Text = "Книга не выбрана";
                SelectedBookInfo.Foreground = System.Windows.Media.Brushes.Gray;
                SelectedBookInfo.FontStyle = FontStyles.Italic;
            }
        }

        private void BooksDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Двойной клик для редактирования
            EditBook_Click(sender, e);
        }

        // Добавление книги
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addEditWindow = new AddEditBookWindow(_context);
                addEditWindow.Owner = this;

                if (addEditWindow.ShowDialog() == true)
                {
                    LoadFilterData(); // Перезагружаем фильтры на случай появления нового автора/жанра
                    LoadBooks();
                    MessageBox.Show("Книга успешно добавлена", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна добавления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Редактирование книги
        private void EditBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BooksDataGrid.SelectedItem is Book selectedBook)
                {
                    var addEditWindow = new AddEditBookWindow(_context, selectedBook.Id);
                    addEditWindow.Owner = this;

                    if (addEditWindow.ShowDialog() == true)
                    {
                        LoadFilterData();
                        LoadBooks();
                        MessageBox.Show("Книга успешно обновлена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите книгу для редактирования",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Удаление книги
        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BooksDataGrid.SelectedItem is Book selectedBook)
                {
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить книгу?\n\nНазвание: {selectedBook.Title}\nАвтор: {selectedBook.Author?.FullName}\nЖанр: {selectedBook.Genre?.Name}\nВ наличии: {selectedBook.QuantityInStock}",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _context.Books.Remove(selectedBook);
                        _context.SaveChanges();
                        LoadBooks();
                        MessageBox.Show("Книга успешно удалена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите книгу для удаления",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}