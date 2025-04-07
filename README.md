# Двач-клон на ASP.NET Core 9

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-purple?logo=.net)
![MySQL](https://img.shields.io/badge/MySQL-8.0-blue?logo=mysql)

Клон популярного имиджборда "Двач". Проект реализован на ASP.NET Core 9 с использованием MySQL.

## 🌟 Основные функции

### Для всех пользователей
- 🔐 **Авторизация/Регистрация** через email и пароль
- 📝 Создание новых тредов с заголовком и содержимым
- 💬 Публикация постов с поддержкой:
  - Текста
- 👍 Система лайков под постами

### Для администраторов
- 🛠 **Админ-панель** с расширенным управлением:
  - Удаление любых тредов/постов
  - Модерация контента
  - Просмотр статистики

## 🛠 Технологический стек

**Backend**:
- ASP.NET Core 9 (MVC)
- Entity Framework Core 9
- MySQL 8.0

**Frontend**:
- Razor Pages
- Bootstrap 5
- HTML5/CSS3

## 🚀 Установка и запуск

### Требования
- .NET 9 SDK
- MySQL 8.0+

### Шаги:
1. Клонируйте репозиторий:
```bash
git clone https://github.com/Fl1ckerxD/MyForum.git
cd MyForum
```

2. Настройте базу данных:
```bash
dotnet ef database update --context MyForumContext
```

3. Заполните конфигурационный файл `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "ForumDatabase": "Server=localhost;Username=root;Password=yourpassword;DataBase=forum"
  }
}
```

4. Запустите приложение:
```bash
dotnet run
```

## 🗄 Структура проекта

```
MyForum/
├── Controllers/        # Контроллеры MVC
├── Models/             # Модели и контекст БД
├── Services/           # DI сервисы
├── wwwroot/            # Статические файлы
└── Views/              # Razor-представления
```

## ⚙️ Конфигурация

### Первоначальная настройка админа:
1. Зарегистрируйте обычного пользователя
2. В SQL выполните:
```sql
UPDATE Users
SET Role = 'Admin'
WHERE Email = 'admin@example.com';
```

## 📬 Контакты

По вопросам сотрудничества:
- Email: mornival@outlook.com
- Telegram: @Fl1cker_0
- Issues: https://github.com/Fl1ckerxD/MyForum/issues
