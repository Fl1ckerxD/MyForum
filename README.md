# Двач-клон на ASP.NET Core 9

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-purple?logo=.net)
![MySQL](https://img.shields.io/badge/MySQL-8.0-blue?logo=mysql)
![EF Core](https://img.shields.io/badge/EF_Core-9.0-green)

MyForum — это веб-приложение, реализующее базовые возможности онлайн-форума. Примерно похоже на [2ch], но без возможности загрузки изображений. Пользователи могут создавать топики, оставлять комментарии, ставить лайки, а администраторы имеют доступ к панели управления для модерации контента.

## 🌟 Основные функции

### Для всех пользователей
- 🔐 **Авторизация/Регистрация** через email и пароль
- 📝 Создание новых тредов с заголовком и содержимым
- 💬 Оставление комментариев к топикам
- 👍 Система лайков под постами

### Для администраторов
- 👑 **Админ-панель** с расширенным управлением:
  - Удаление любых тредов/постов
  - Модерация контента
  - Просмотр активности пользователей

## 🛠 Технологический стек
- Backend: ASP.NET Core
- Database: MySQL 8
- ORM: Entity Framework Core 9
- Кэширование: MemoryCache
- Авторизация: Cookies-based
- Хеширование паролей: PasswordHasher
- Тестирование: xUnit, Moq, InMemoryDatabase
- Оптимизация: GzipCompressionProvider
- AJAX: Fetch API

## 🗄 Архитектура проекта
```
📦 MyForum/
├── 📂 Core/                  # Доменные модели и интерфейсы
│   ├── 📂 DTOs/                # Data Transfer Objects
│   ├── 📂 Entities/            # ORM-сущности
│   └── 📂 Interfaces/          # Сервисные интерфейсы
│
├── 📂 Infrastructure/          # Реализация репозиториев и сервисов
│   ├── 📂 Data/                # Контекст EF + миграции
│   ├── 📂 Repositories/        # Работа с БД
│   └── 📂 Services/            # Бизнес-логика
│
├── 📂 Web/                     # Веб-слои
│   ├── 📂 Controllers/         # MVC контроллеры
│   ├── 📂 Extensions/          # Расширения
│   ├── 📂 Requests/            # DTO входящих запросов
│   ├── 📂 ViewModels/          # Модели представления
│   └── 📂 Views/               # Razor-шаблоны
│
└── 📂 MyForum.Tests/           # Unit-тесты
```

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

## ⚙️ Конфигурация

### Первоначальная настройка админа:
1. Зарегистрируйте обычного пользователя
2. В SQL выполните:
```sql
UPDATE Users
SET Role = 'Admin'
WHERE Email = 'admin@example.com';
```

## 🧪 Тестирование
Тесты находятся в проекте MyForum.Tests. 
Используются:
- xUnit.net – фреймворк тестирования
- Moq – для создания mock-объектов
- InMemoryDatabase – для тестирования уровня работы с данными

## 📬 Контакты

По вопросам сотрудничества:
- Email: mornival@outlook.com
- Telegram: @Fl1cker_0
- Issues: https://github.com/Fl1ckerxD/MyForum/issues
